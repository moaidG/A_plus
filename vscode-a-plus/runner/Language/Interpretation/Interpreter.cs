using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using A_.Platform;

namespace A_
{
    class ModuleScope
    {
        public string Path;
        public Dictionary<string, object> Vars = new Dictionary<string, object>();
        public Dictionary<string, FuncDefNode> Funcs = new Dictionary<string, FuncDefNode>();
        public Dictionary<string, ClassDefNode> Classes = new Dictionary<string, ClassDefNode>();
    }

    partial class Interpreter
    {
        public Action<string> Output { get; set; }
        public string BaseDir { get; set; } = ".";
        public IUIRuntime UIRuntime { get; set; }

        private Dictionary<string, object> vars = new Dictionary<string, object>();
        private Dictionary<string, string> varTypes = new Dictionary<string, string>();
        private Dictionary<string, FuncDefNode> funcs = new Dictionary<string, FuncDefNode>();
        private Dictionary<string, ClassDefNode> classes = new Dictionary<string, ClassDefNode>();
        private Dictionary<string, Dictionary<string, object>> staticFields = new Dictionary<string, Dictionary<string, object>>();
        private Dictionary<string, IntPtr> _externLibs = new Dictionary<string, IntPtr>();
        private Dictionary<string, (IntPtr lib, string name, List<string> parameters)> externs = new Dictionary<string, (IntPtr, string, List<string>)>();
        private HashSet<string> ExportedVars = new HashSet<string>();
        private Dictionary<string, ModuleScope> _modules = new Dictionary<string, ModuleScope>();
        private HashSet<string> _loading = new HashSet<string>();
        private string _currentClass = null;
        private List<string> _callStack = new List<string>();
        public string StdLibPath { get; set; } = null;
        public DapDebugger Debugger { get; set; }

        public void InterpretFile(string path)
        {
            if (!System.IO.File.Exists(path)) return;
            string code = System.IO.File.ReadAllText(path);
            code = Shorthand.Convert(code);
            var tokens = new Lexer(code).Tokenize();
            var parser = new Parser(tokens);
            var ast = parser.Parse();
            BaseDir = System.IO.Path.GetDirectoryName(System.IO.Path.GetFullPath(path)) ?? ".";
            Visit(ast);
        }

        void PushCall(string name) => _callStack.Add(name);
        void PopCall() { if (_callStack.Count > 0) _callStack.RemoveAt(_callStack.Count - 1); }

        string StackTrace() => _callStack.Count > 0 ? "\nتتبع الاستدعاء:\n  " + string.Join("\n  ", _callStack.Select((c, i) => $"#{i + 1} {c}")) : "";

        public void Clear()
        {
            vars.Clear(); varTypes.Clear(); funcs.Clear(); classes.Clear();
            staticFields.Clear(); _currentClass = null; _callStack.Clear();
            ExportedVars.Clear(); _modules.Clear(); _loading.Clear();
            if (UIRuntime != null) { UIRuntime.Dispose(); }
        }

        public void LoadStdLib()
        {
            if (string.IsNullOrEmpty(StdLibPath) || !System.IO.Directory.Exists(StdLibPath)) return;
            string savedDir = BaseDir;
            foreach (var file in System.IO.Directory.GetFiles(StdLibPath, "*.a"))
            {
                try
                {
                    string code = System.IO.File.ReadAllText(file);
                    if (string.IsNullOrWhiteSpace(code)) continue;
                    code = Shorthand.Convert(code);
                    var lex = new Lexer(code);
                    var toks = lex.Tokenize();
                    if (toks.Count == 0) continue;
                    var parser = new Parser(toks);
                    var ast = parser.Parse();
                    BaseDir = System.IO.Path.GetDirectoryName(System.IO.Path.GetFullPath(file));
                    Visit(ast);
                    BaseDir = savedDir;
                }
                catch (Exception ex) { System.Console.Error.WriteLine($"[StdLib] {System.IO.Path.GetFileName(file)}: {ex.Message}"); }
            }
        }

        string NodePos(Node n) => n != null && n.Line > 0 ? $" (سطر {n.Line}, عمود {n.Column})" : "";
        string Err(Node n, string msg) => $"خطأ{NodePos(n)}: {msg}{StackTrace()}";
        string FormatValue(object value)
        {
            if (value is List<object> list)
                return "[" + string.Join(", ", list.Select(FormatValue)) + "]";
            if (value is Dictionary<string, object> dict && dict.ContainsKey("__class"))
                return "<" + dict["__class"] + ">";
            if (value is Dictionary<string, object> enumDict && enumDict.ContainsKey("__enum"))
                return "<enum " + enumDict["__enum"] + ">";
            return value?.ToString() ?? "";
        }

        Node ToLiteralNode(object value)
        {
            if (value is double d) return new NumberNode(d);
            if (value is int i) return new NumberNode(i);
            if (value is string s) return new StringNode(s);
            if (value is bool b) return new BoolNode(b);
            return new StringNode(value?.ToString() ?? "");
        }
        bool IsTrue(object value)
        {
            if (value is double d) return d != 0;
            if (value is int i) return i != 0;
            if (value is string s) return !string.IsNullOrEmpty(s);
            if (value is bool b) return b;
            if (value is List<object> lst) return lst.Count > 0;
            return value != null;
        }
        string GetTypeName(object value)
        {
            if (value is double || value is int) return "number";
            if (value is string) return "string";
            if (value is bool) return "bool";
            if (value is Dictionary<string, object>) return "object";
            if (value is List<object>) return "array";
            return "unknown";
        }

        void EnsureWindow()
        {
            if (UIRuntime == null)
                throw new Exception("UIRuntime غير مهيأ");
            UIRuntime.EnsureWindow();
            if (!vars.ContainsKey("__root"))
            {
                var rp = UIRuntime.RootPanel;
                if (rp != null)
                    vars["__root"] = rp;
            }
        }

        void RestoreShadowed(List<string> parameters, Dictionary<string, object> saved)
        {
            foreach (var p in parameters)
                if (saved.ContainsKey(p)) vars[p] = saved[p];
                else vars.Remove(p);
        }
        void RestoreMethodVars(List<string> parameters, Dictionary<string, object> saved, bool hadSelf)
        {
            foreach (var p in parameters)
                if (saved.ContainsKey(p)) vars[p] = saved[p];
                else vars.Remove(p);
            if (saved.ContainsKey("self")) vars["self"] = saved["self"];
            else vars.Remove("self");
        }

        object CallExtern(string name, List<Node> args)
        {
            if (!externs.ContainsKey(name))
                throw new Exception($"Extern function not found: {name}");
            var (lib, funcName, parameters) = externs[name];
            if (lib == IntPtr.Zero)
                throw new Exception($"DLL not loaded for extern '{name}'");
            ValidateExternFunc(funcName, args.Count > 0 ? args[0] : null);
            IntPtr pFunc = IntPtr.Zero;
            try { pFunc = NativeLibrary.GetExport(lib, funcName); }
            catch { throw new Exception($"Cannot find function '{funcName}' in DLL"); }

            var argValues = new List<object>();
            foreach (var a in args) argValues.Add(Visit(a));

            int paramCount = Math.Min(argValues.Count, parameters.Count);
            var nativeArgs = new IntPtr[paramCount];
            var stringsToFree = new List<IntPtr>();
            try
            {
                for (int i = 0; i < paramCount; i++)
                {
                    if (argValues[i] is string s)
                    {
                        nativeArgs[i] = Marshal.StringToHGlobalAuto(s);
                        stringsToFree.Add(nativeArgs[i]);
                    }
                    else
                        nativeArgs[i] = (IntPtr)Convert.ToInt32(argValues[i]);
                }
                object result;
                switch (paramCount)
                {
                    case 0: result = Marshal.GetDelegateForFunctionPointer<Func0>(pFunc)(); break;
                    case 1: result = Marshal.GetDelegateForFunctionPointer<Func1>(pFunc)(nativeArgs[0]); break;
                    case 2: result = Marshal.GetDelegateForFunctionPointer<Func2>(pFunc)(nativeArgs[0], nativeArgs[1]); break;
                    case 3: result = Marshal.GetDelegateForFunctionPointer<Func3>(pFunc)(nativeArgs[0], nativeArgs[1], nativeArgs[2]); break;
                    case 4: result = Marshal.GetDelegateForFunctionPointer<Func4>(pFunc)(nativeArgs[0], nativeArgs[1], nativeArgs[2], nativeArgs[3]); break;
                    case 5: result = Marshal.GetDelegateForFunctionPointer<Func5>(pFunc)(nativeArgs[0], nativeArgs[1], nativeArgs[2], nativeArgs[3], nativeArgs[4]); break;
                    case 6: result = Marshal.GetDelegateForFunctionPointer<Func6>(pFunc)(nativeArgs[0], nativeArgs[1], nativeArgs[2], nativeArgs[3], nativeArgs[4], nativeArgs[5]); break;
                    case 7: result = Marshal.GetDelegateForFunctionPointer<Func7>(pFunc)(nativeArgs[0], nativeArgs[1], nativeArgs[2], nativeArgs[3], nativeArgs[4], nativeArgs[5], nativeArgs[6]); break;
                    case 8: result = Marshal.GetDelegateForFunctionPointer<Func8>(pFunc)(nativeArgs[0], nativeArgs[1], nativeArgs[2], nativeArgs[3], nativeArgs[4], nativeArgs[5], nativeArgs[6], nativeArgs[7]); break;
                    default: throw new Exception("Extern with >8 params not supported");
                }
                return Convert.ToDouble(result);
            }
            finally
            {
                foreach (var ptr in stringsToFree)
                    Marshal.FreeHGlobal(ptr);
            }
        }

        object CallDotNetMethod(List<Node> args)
        {
            if (args.Count < 2)
                throw new Exception("dotnet(typeName, methodName, ...) needs 2+ args");
            string typeName = Visit(args[0])?.ToString();
            string memberName = Visit(args[1])?.ToString();
            if (string.IsNullOrEmpty(typeName) || string.IsNullOrEmpty(memberName))
                throw new Exception("Type and method names required");
            ValidateDotNetAccess(typeName, memberName, args.Count > 0 ? args[0] : null);
            var argValues = new List<object>();
            for (int i = 2; i < args.Count; i++)
                argValues.Add(Visit(args[i]));

            var type = Type.GetType(typeName, false);
            if (type == null)
            {
                foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
                {
                    type = asm.GetType(typeName);
                    if (type != null) break;
                }
            }
            if (type == null)
                throw new Exception($"Type not found: {typeName}");

            try
            {
                var prop = type.GetProperty(memberName);
                if (prop != null && argValues.Count == 0)
                    return ConvertNetValue(prop.GetValue(null));

                var method = type.GetMethod(memberName, argValues.Select(a => a?.GetType() ?? typeof(object)).ToArray());
                if (method == null)
                {
                    var methods = type.GetMethods().Where(m => m.Name == memberName && m.IsStatic).ToArray();
                    if (methods.Length == 0)
                        throw new Exception($"Static method not found: {typeName}.{memberName}");
                    method = methods.FirstOrDefault(m => m.GetParameters().Length == argValues.Count) ?? methods[0];
                }
                return ConvertNetValue(method.Invoke(null, argValues.ToArray()));
            }
            catch (Exception ex)
            {
                throw new Exception($"Error calling {typeName}.{memberName}: {ex.InnerException?.Message ?? ex.Message}");
            }
        }

        object ConvertNetValue(object val)
        {
            if (val == null) return 0.0;
            if (val is int i) return (double)i;
            if (val is double d) return d;
            if (val is float f) return (double)f;
            if (val is long l) return (double)l;
            if (val is string s) return s;
            if (val is Array arr)
            {
                var list = new List<object>();
                foreach (var item in arr) list.Add(ConvertNetValue(item));
                return list;
            }
            return val.ToString();
        }

        object JsonToAplus(System.Text.Json.JsonElement elem)
        {
            switch (elem.ValueKind)
            {
                case System.Text.Json.JsonValueKind.String: return elem.GetString() ?? "";
                case System.Text.Json.JsonValueKind.Number: return elem.GetDouble();
                case System.Text.Json.JsonValueKind.True: return 1.0;
                case System.Text.Json.JsonValueKind.False: return 0.0;
                case System.Text.Json.JsonValueKind.Null: return 0.0;
                case System.Text.Json.JsonValueKind.Object:
                    var dict = new Dictionary<string, object>();
                    foreach (var prop in elem.EnumerateObject())
                        dict[prop.Name] = JsonToAplus(prop.Value);
                    return dict;
                case System.Text.Json.JsonValueKind.Array:
                    var list = new List<object>();
                    foreach (var item in elem.EnumerateArray())
                        list.Add(JsonToAplus(item));
                    return list;
                default: return 0.0;
            }
        }

        void ShowError(string message)
        {
            Output?.Invoke($"[خطأ] {message}");
            UIRuntime?.AddErrorText(message);
        }

        bool IsEventMember(string member)
        {
            string l = member.ToLower();
            return l == "click" || l == "loaded" || l == "mouseenter" || l == "mouseleave" ||
                   l == "mousedown" || l == "mouseup" || l == "mousemove" ||
                   l == "keydown" || l == "keyup" || l == "gotfocus" || l == "lostfocus" ||
                   l == "layoutupdated" || l == "textchanged" || l == "selectionchanged" ||
                   l == "valuechanged" || l == "sizechanged" ||
                   l == "checked" || l == "unchecked" || l == "indeterminate" ||
                   l == "drop" || l == "dragover" ||
                   l == "نقر" || l == "تحميل" || l == "دخول_فأرة" || l == "خروج_فأرة" ||
                   l == "ضغط_فأرة" || l == "رفع_فأرة" || l == "تحريك_فأرة" ||
                   l == "ضغط_مفتاح" || l == "رفع_مفتاح" ||
                   l == "اكتسب_تركيز" || l == "فقد_تركيز" ||
                   l == "تحديث_تخطيط" || l == "تغير_نص" || l == "تغير_اختيار" ||
                   l == "تغير_قيمة" || l == "تغير_حجم" ||
                   l == "تم_تحديد" || l == "تم_إلغاء" || l == "غير_محدد" ||
                   l == "إفلات" || l == "سحب_فوق" ||
                   l == "mediaended" || l == "mediafailed" || l == "mediaopened" ||
                   l == "انتهى_الوسائط" || l == "فشل_الوسائط" || l == "فتح_الوسائط";
        }

        void ReapplyBindings()
        {
            // Property bindings are now handled by the runtime timer
        }

    string InferNodeType(Node node)
    {
        if (node is BoolNode) return "bool";
        if (node is NumberNode) return "number";
        if (node is StringNode) return "string";
        return GetTypeName(Visit(node));
    }

    bool IsKnownUIElement(string name)
    {
        switch (name.ToLower())
        {
            case "grid": case "stackpanel": case "wrappanel": case "dockpanel":
            case "canvas": case "uniformgrid": case "viewbox": case "border":
            case "scrollviewer": case "textblock": case "textbox": case "richtextbox":
            case "label": case "button": case "repeatbutton": case "togglebutton":
            case "checkbox": case "radiobutton": case "listbox": case "listview":
            case "combobox": case "treeview": case "menu": case "menuitem":
            case "contextmenu": case "tabcontrol": case "tabitem":
            case "image": case "img": case "video": case "sound": case "mediaelement":
            case "slider": case "progressbar": case "datepicker": case "calendar":
            case "passwordbox": case "rectangle": case "ellipse": case "line":
            case "polygon": case "polyline": case "path": case "window":
            case "viewport3d":
                return true;
            default:
                return false;
        }
    }

    public object Visit(Node node)
    {
        if (node is StringNode s) return s.Value;
        if (node is NumberNode n) return n.Value;
        if (node is BoolNode b) return b.Value ? 1.0 : 0.0;
            if (node is VariableNode v)
            {
                if (!vars.ContainsKey(v.Name))
                {
                    if (v.Name == "__root" && UIRuntime != null)
                    {
                        EnsureWindow();
                        return vars["__root"];
                    }
                    if (vars.ContainsKey("self") && vars["self"] is Dictionary<string, object> selfObj && selfObj.ContainsKey(v.Name))
                        return selfObj[v.Name];
                    if (classes.ContainsKey(v.Name)) return staticFields[v.Name];
                    if (funcs.ContainsKey(v.Name)) return v.Name;
                    throw new Exception(Err(v, $"المتغير '{v.Name}' غير موجود"));
                }
                return vars[v.Name];
            }
            if (node is BlockNode block)
            {
                foreach (var stmt in block.Statements)
                    if (stmt is FuncDefNode f) funcs[f.Name] = f;
                object result = 0;
                foreach (var stmt in block.Statements) result = Visit(stmt);
                return result;
            }

            if (node is AddNode add)
            {
                var left = Visit(add.Left);
                var right = Visit(add.Right);
                if (left is string || right is string) return FormatValue(left) + FormatValue(right);
                return Convert.ToDouble(left) + Convert.ToDouble(right);
            }
            if (node is SubtractNode sub) return Convert.ToDouble(Visit(sub.Left)) - Convert.ToDouble(Visit(sub.Right));
            if (node is MultiplyNode mul) return Convert.ToDouble(Visit(mul.Left)) * Convert.ToDouble(Visit(mul.Right));
            if (node is DivideNode div) return Convert.ToDouble(Visit(div.Left)) / Convert.ToDouble(Visit(div.Right));
            if (node is ModuloNode mod) return Convert.ToDouble(Visit(mod.Left)) % Convert.ToDouble(Visit(mod.Right));
            if (node is GreaterNode gt) return Convert.ToDouble(Visit(gt.Left)) > Convert.ToDouble(Visit(gt.Right)) ? 1.0 : 0.0;
            if (node is LessNode lt) return Convert.ToDouble(Visit(lt.Left)) < Convert.ToDouble(Visit(lt.Right)) ? 1.0 : 0.0;
            if (node is EqualNode eq) return Visit(eq.Left).Equals(Visit(eq.Right)) ? 1.0 : 0.0;
            if (node is NotEqualNode ne) return !Visit(ne.Left).Equals(Visit(ne.Right)) ? 1.0 : 0.0;
            if (node is LeNode le) return Convert.ToDouble(Visit(le.Left)) <= Convert.ToDouble(Visit(le.Right)) ? 1.0 : 0.0;
            if (node is GeNode ge) return Convert.ToDouble(Visit(ge.Left)) >= Convert.ToDouble(Visit(ge.Right)) ? 1.0 : 0.0;
            if (node is AndNode and)
            {
                double left = Convert.ToDouble(Visit(and.Left));
                return left != 0 ? Convert.ToDouble(Visit(and.Right)) != 0 ? 1.0 : 0.0 : 0.0;
            }
            if (node is OrNode or)
            {
                double left = Convert.ToDouble(Visit(or.Left));
                return left != 0 ? 1.0 : Convert.ToDouble(Visit(or.Right)) != 0 ? 1.0 : 0.0;
            }

            if (node is PipeNode pipe)
            {
                object leftVal = Visit(pipe.Left);
                if (pipe.Right is FuncCallNode fcn)
                {
                    var allArgs = new List<Node> { ToLiteralNode(leftVal) };
                    allArgs.AddRange(fcn.Arguments);
                    return Visit(new FuncCallNode(fcn.Name, allArgs));
                }
                if (pipe.Right is VariableNode vn)
                {
                    if (funcs.ContainsKey(vn.Name))
                        return Visit(new FuncCallNode(vn.Name, new List<Node> { ToLiteralNode(leftVal) }));
                    try { return Visit(new FuncCallNode(vn.Name, new List<Node> { ToLiteralNode(leftVal) })); }
                    catch { /* fall through */ }
                }
                return Visit(pipe.Right);
            }

            if (node is AssignNode assign)
            {
                object value = assign.Value != null ? Visit(assign.Value) : 0.0;
                var concreteTypes = new HashSet<string> { "number", "string", "bool", "object", "array", "any", "void" };
                if (assign.TypeName != null && concreteTypes.Contains(assign.TypeName))
                {
                    string actualType = assign.Value != null ? InferNodeType(assign.Value) : "number";
                    if (actualType != assign.TypeName)
                        throw new Exception(Err(assign, $"خطأ نوع: '{assign.Name}' معرف كـ {assign.TypeName} لكن القيمة من نوع {actualType}"));
                    varTypes[assign.Name] = assign.TypeName;
                }
                else if (varTypes.ContainsKey(assign.Name))
                {
                    string newType = assign.Value != null ? InferNodeType(assign.Value) : "number";
                    if (varTypes[assign.Name] != newType)
                        throw new Exception(Err(assign, $"لا يمكن تغيير نوع المتغير '{assign.Name}' من {varTypes[assign.Name]} إلى {newType}"));
                }
                vars[assign.Name] = value;
                return value;
            }
            if (node is ArrayDestructureNode adn)
            {
                object arrVal = Visit(adn.Value);
                if (arrVal is List<object> lst)
                {
                    for (int i = 0; i < adn.Names.Count; i++)
                        vars[adn.Names[i]] = i < lst.Count ? lst[i] : 0.0;
                }
                else if (arrVal is string str)
                {
                    for (int i = 0; i < adn.Names.Count; i++)
                        vars[adn.Names[i]] = i < str.Length ? str[i].ToString() : "";
                }
                return arrVal;
            }
            if (node is ObjectDestructureNode odn)
            {
                object objVal = Visit(odn.Value);
                if (objVal is Dictionary<string, object> dct)
                {
                    foreach (var nm in odn.Names)
                        vars[nm] = dct.TryGetValue(nm, out var vv) ? vv : 0.0;
                }
                return objVal;
            }
            if (node is RegexNode rn)
            {
                if (rn.RegexObj == null)
                    throw new Exception(Err(node, $"تعبير منتظم غير صالح: /{rn.Pattern}/{rn.Flags}"));
                return rn;
            }
            if (node is PrintNode print)
            {
                object value = Visit(print.Value);
                Output?.Invoke(FormatValue(value));
                return value;
            }

            if (node is IfNode ifNode)
            {
                if (IsTrue(Visit(ifNode.Condition))) return Visit(ifNode.Body);
                if (ifNode.ElseBody != null) return Visit(ifNode.ElseBody);
                return 0;
            }
            if (node is WhileNode whileNode)
            {
                while (IsTrue(Visit(whileNode.Condition)))
                {
                    try { Visit(whileNode.Body); }
                    catch (BreakException) { break; }
                    catch (ContinueException) { }
                }
                return 0;
            }
            if (node is ForNode forNode)
            {
                if (forNode.Init != null) Visit(forNode.Init);
                while (forNode.Condition == null || IsTrue(Visit(forNode.Condition)))
                {
                    try { Visit(forNode.Body); }
                    catch (BreakException) { break; }
                    catch (ContinueException) { if (forNode.Increment != null) Visit(forNode.Increment); continue; }
                    if (forNode.Increment != null) Visit(forNode.Increment);
                }
                return 0;
            }
            if (node is ForInNode forIn)
            {
                var iterable = Visit(forIn.Iterable);
                if (iterable is List<object> list)
                {
                    for (int i = 0; i < list.Count; i++)
                    {
                        vars[forIn.VarName] = list[i];
                        try { Visit(forIn.Body); }
                        catch (BreakException) { break; }
                        catch (ContinueException) { }
                    }
                }
                else if (iterable is string str)
                {
                    for (int i = 0; i < str.Length; i++)
                    {
                        vars[forIn.VarName] = str[i].ToString();
                        try { Visit(forIn.Body); }
                        catch (BreakException) { break; }
                        catch (ContinueException) { }
                    }
                }
                return 0;
            }
            if (node is BreakNode) throw new BreakException();
            if (node is ContinueNode) throw new ContinueException();
            if (node is TernaryNode tern)
                return IsTrue(Visit(tern.Condition)) ? Visit(tern.TrueExpr) : Visit(tern.FalseExpr);
            if (node is SwitchNode sw)
            {
                object val = Visit(sw.Value);
                foreach (var c in sw.Cases)
                {
                    if (val.Equals(Visit(c.Value)))
                        try { return Visit(c.Body); } catch (BreakException) { return 0; }
                }
                if (sw.DefaultBody != null)
                    try { return Visit(sw.DefaultBody); } catch (BreakException) { return 0; }
                return 0;
            }
            if (node is UnaryNode un)
            {
                double val = Convert.ToDouble(Visit(un.Operand));
                if (un.Op == "NOT") return (val != 0) ? 0.0 : 1.0;
                if (un.Op == "NEGATE") return -val;
                throw new Exception(Err(un, $"عملية أحادية غير معروفة: {un.Op}"));
            }

            if (node is PostfixNode postfixNode)
            {
                object val = Visit(postfixNode.Operand);
                if (postfixNode.Operand is VariableNode vn && vars.ContainsKey(vn.Name))
                {
                    double d = Convert.ToDouble(val);
                    vars[vn.Name] = postfixNode.Op == "++" ? d + 1 : d - 1;
                }
                return val;
            }

            if (node is FuncDefNode funcDef)
            {
                funcs[funcDef.Name] = funcDef;
                return funcDef.Name;
            }
            if (node is ReturnNode ret)
            {
                object value = ret.Value != null ? Visit(ret.Value) : 0;
                throw new ReturnException(value);
            }
            if (node is ExternDefNode extDef)
            {
                if (!_externLibs.ContainsKey(extDef.DllName))
                {
                    try
                    {
                        ValidateExternDll(extDef.DllName, extDef);
                        _externLibs[extDef.DllName] = NativeLibrary.Load(extDef.DllName);
                    }
                    catch { _externLibs[extDef.DllName] = IntPtr.Zero; }
                }
                externs[extDef.Name] = (_externLibs[extDef.DllName], extDef.Name, extDef.Parameters);
                return extDef.Name;
            }
            if (node is FuncCallNode call)
            {
                if (call.Name == "help" || call.Name == "مساعدة")
                {
                    if (call.Arguments.Count != 1)
                        throw new Exception(Err(call, "help/مساعدة تحتاج وسيط واحد"));
                    object target = call.Arguments[0] is VariableNode vn ? vn.Name : Visit(call.Arguments[0]);
                    string name = target?.ToString() ?? "";
                    if (funcs.ContainsKey(name) && funcs[name].Doc != null) return funcs[name].Doc;
                    if (classes.ContainsKey(name) && classes[name].Doc != null) return classes[name].Doc;
                    return $"لا يوجد توثيق لـ '{name}'";
                }
                if (call.Name == "typeof" || call.Name == "نوع")
                {
                    if (call.Arguments.Count != 1)
                        throw new Exception(Err(call, "typeof/نوع تحتاج وسيط واحد"));
                    return GetTypeName(Visit(call.Arguments[0]));
                }
                if (call.Name == "platform" || call.Name == "منصة")
                {
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return "windows";
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) return "linux";
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) return "macos";
                    return "other";
                }
                if (call.Name == "dotnet" || call.Name == "نت")
                    return CallDotNetMethod(call.Arguments);

                if (call.Name == "input" || call.Name == "اقرأ")
                {
                    string prompt = call.Arguments.Count > 0 ? Visit(call.Arguments[0])?.ToString() ?? "" : "";
                    return UIRuntime.ReadInput(prompt);
                }

                if (call.Name == "jsonParse" || call.Name == "تحليلJson")
                {
                    string json = call.Arguments.Count > 0 ? Visit(call.Arguments[0])?.ToString() ?? "{}" : "{}";
                    try
                    {
                        using var doc = System.Text.Json.JsonDocument.Parse(json);
                        return JsonToAplus(doc.RootElement);
                    }
                    catch (Exception ex) { throw new Exception(Err(call, $"خطأ في JSON: {ex.Message}")); }
                }
                if (call.Name == "regexMatch" || call.Name == "تطابقRegex")
                {
                    string input = call.Arguments.Count > 0 ? Visit(call.Arguments[0])?.ToString() ?? "" : "";
                    string pattern = call.Arguments.Count > 1 ? Visit(call.Arguments[1])?.ToString() ?? "" : "";
                    var match = System.Text.RegularExpressions.Regex.Match(input, pattern);
                    return match.Success ? 1.0 : 0.0;
                }
                if (call.Name == "regexFind" || call.Name == "بحثRegex")
                {
                    string input = call.Arguments.Count > 0 ? Visit(call.Arguments[0])?.ToString() ?? "" : "";
                    string pattern = call.Arguments.Count > 1 ? Visit(call.Arguments[1])?.ToString() ?? "" : "";
                    var match = System.Text.RegularExpressions.Regex.Match(input, pattern);
                    if (match.Success)
                    {
                        var result = new List<object>();
                        foreach (System.Text.RegularExpressions.Group g in match.Groups)
                            result.Add(g.Value);
                        return result;
                    }
                    return new List<object>();
                }
                if (call.Name == "regexReplace" || call.Name == "استبدلRegex")
                {
                    string input = call.Arguments.Count > 0 ? Visit(call.Arguments[0])?.ToString() ?? "" : "";
                    string pattern = call.Arguments.Count > 1 ? Visit(call.Arguments[1])?.ToString() ?? "" : "";
                    string replacement = call.Arguments.Count > 2 ? Visit(call.Arguments[2])?.ToString() ?? "" : "";
                    return System.Text.RegularExpressions.Regex.Replace(input, pattern, replacement);
                }
                if (call.Name == "httpGet" || call.Name == "طلبHttpGet")
                {
                    string url = call.Arguments.Count > 0 ? Visit(call.Arguments[0])?.ToString() ?? "" : "";
                    ValidateUri(url, call);
                    try
                    {
                        using var client = new System.Net.Http.HttpClient();
                        client.Timeout = TimeSpan.FromSeconds(15);
                        var response = client.GetStringAsync(url).GetAwaiter().GetResult();
                        return response;
                    }
                    catch (Exception ex) { throw new Exception(Err(call, $"خطأ في HTTP: {ex.Message}")); }
                }

                if (call.Name == "readFile" || call.Name == "اقرأملف")
                {
                    string path = call.Arguments.Count > 0 ? Visit(call.Arguments[0])?.ToString() ?? "" : "";
                    ValidateFilePath(path, call);
                    try { return System.IO.File.ReadAllText(path); }
                    catch (Exception ex) { throw new Exception(Err(call, $"خطأ في قراءة الملف: {ex.Message}")); }
                }
                if (call.Name == "writeFile" || call.Name == "اكتبملف")
                {
                    string path = call.Arguments.Count > 0 ? Visit(call.Arguments[0])?.ToString() ?? "" : "";
                    string content = call.Arguments.Count > 1 ? Visit(call.Arguments[1])?.ToString() ?? "" : "";
                    ValidateFilePath(path, call);
                    try { System.IO.File.WriteAllText(path, content); return 1.0; }
                    catch (Exception ex) { throw new Exception(Err(call, $"خطأ في كتابة الملف: {ex.Message}")); }
                }
                if (call.Name == "fileExists" || call.Name == "ملفموجود")
                {
                    string path = call.Arguments.Count > 0 ? Visit(call.Arguments[0])?.ToString() ?? "" : "";
                    ValidateFilePath(path, call);
                    return System.IO.File.Exists(path) ? 1.0 : 0.0;
                }
                if (call.Name == "appendFile" || call.Name == "أضفلملف")
                {
                    string path = call.Arguments.Count > 0 ? Visit(call.Arguments[0])?.ToString() ?? "" : "";
                    string content = call.Arguments.Count > 1 ? Visit(call.Arguments[1])?.ToString() ?? "" : "";
                    ValidateFilePath(path, call);
                    try { System.IO.File.AppendAllText(path, content); return 1.0; }
                    catch (Exception ex) { throw new Exception(Err(call, $"خطأ في الإضافة للملف: {ex.Message}")); }
                }
                if (call.Name == "deleteFile" || call.Name == "احذفملف")
                {
                    string path = call.Arguments.Count > 0 ? Visit(call.Arguments[0])?.ToString() ?? "" : "";
                    ValidateFilePath(path, call);
                    try { System.IO.File.Delete(path); return 1.0; }
                    catch (Exception ex) { throw new Exception(Err(call, $"خطأ في حذف الملف: {ex.Message}")); }
                }

                if (call.Name == "trace" || call.Name == "تتبع")
                {
                    string msg = call.Arguments.Count > 0 ? FormatValue(Visit(call.Arguments[0])) : "";
                    Output?.Invoke($"[تتبع] {msg}");
                    return 0;
                }
                if (call.Name == "sleep" || call.Name == "أنتظر")
                {
                    int delayMs = call.Arguments.Count > 0 ? Convert.ToInt32(Visit(call.Arguments[0])) : 0;
                    System.Threading.Tasks.Task.Delay(delayMs).Wait();
                    return 0;
                }
                if (call.Name == "timer" || call.Name == "مؤقت")
                {
                    int intervalMs = call.Arguments.Count > 0 ? Convert.ToInt32(Visit(call.Arguments[0])) : 1000;
                    if (call.Arguments.Count > 1)
                    {
                        if (call.Arguments[1] is VariableNode timerFn)
                        {
                            string fnName = timerFn.Name;
                            UIRuntime.StartTimer(intervalMs, () =>
                            {
                                try { Visit(new FuncCallNode(fnName, new List<Node>())); }
                                catch (Exception ex) { UIRuntime.ShowError(ex.Message); }
                            });
                        }
                        else if (call.Arguments[1] is FuncDefNode arrowFn)
                        {
                            UIRuntime.StartTimer(intervalMs, () =>
                            {
                                try { Visit(arrowFn.Body); }
                                catch (Exception ex) { UIRuntime.ShowError(ex.Message); }
                            });
                        }
                    }
                    return 0;
                }
                if (call.Name == "stopTimer" || call.Name == "إيقاف_مؤقت")
                {
                    UIRuntime.StopTimer();
                    return 0;
                }
                if (call.Name == "timestamp" || call.Name == "الوقت")
                {
                    return (double)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                }

                if (call.Name == "str" || call.Name == "نص")
                {
                    if (call.Arguments.Count == 0) return "";
                    return FormatValue(Visit(call.Arguments[0]));
                }
                if (call.Name == "int" || call.Name == "رقم_صحيح")
                {
                    if (call.Arguments.Count == 0) return 0.0;
                    return (double)(int)Convert.ToDouble(Visit(call.Arguments[0]));
                }
                if (call.Name == "consoleClear" || call.Name == "مسحشاشة")
                {
                    try { Console.Clear(); } catch { /* ignore if no console */ }
                    return 0;
                }
                if (call.Name == "consoleSetCursor" || call.Name == "حددموقعمؤشر")
                {
                    try
                    {
                        int x = call.Arguments.Count > 0 ? Convert.ToInt32(Visit(call.Arguments[0])) : 0;
                        int y = call.Arguments.Count > 1 ? Convert.ToInt32(Visit(call.Arguments[1])) : 0;
                        Console.SetCursorPosition(x, y);
                    }
                    catch { /* ignore if no console */ }
                    return 0;
                }
                if (call.Name == "consoleColor" || call.Name == "لوننص")
                {
                    string color = call.Arguments.Count > 0 ? (Visit(call.Arguments[0])?.ToString() ?? "white").ToLower() : "white";
                    var map = new Dictionary<string, ConsoleColor>
                    {
                        ["black"] = ConsoleColor.Black, ["blue"] = ConsoleColor.Blue,
                        ["cyan"] = ConsoleColor.Cyan, ["darkblue"] = ConsoleColor.DarkBlue,
                        ["darkcyan"] = ConsoleColor.DarkCyan, ["darkgray"] = ConsoleColor.DarkGray,
                        ["darkgreen"] = ConsoleColor.DarkGreen, ["darkmagenta"] = ConsoleColor.DarkMagenta,
                        ["darkred"] = ConsoleColor.DarkRed, ["darkyellow"] = ConsoleColor.DarkYellow,
                        ["gray"] = ConsoleColor.Gray, ["green"] = ConsoleColor.Green,
                        ["magenta"] = ConsoleColor.Magenta, ["red"] = ConsoleColor.Red,
                        ["white"] = ConsoleColor.White, ["yellow"] = ConsoleColor.Yellow
                    };
                    if (map.ContainsKey(color)) Console.ForegroundColor = map[color];
                    return 0;
                }

                if (!funcs.ContainsKey(call.Name))
                {
                    if (externs.ContainsKey(call.Name))
                        return CallExtern(call.Name, call.Arguments);
                    throw new Exception(Err(call, $"الدالة '{call.Name}' غير موجودة"));
                }

                var func = funcs[call.Name];
                var positionalValues = new List<object>();
                var namedValues = new Dictionary<string, object>();
                foreach (var arg in call.Arguments)
                {
                    if (arg is NamedArgNode named)
                        namedValues[named.Name] = Visit(named.Value);
                    else
                        positionalValues.Add(Visit(arg));
                }
                var saved = new Dictionary<string, object>();
                foreach (var p in func.Parameters)
                    if (vars.ContainsKey(p)) saved[p] = vars[p];

                for (int i = 0; i < positionalValues.Count && i < func.Parameters.Count; i++)
                    vars[func.Parameters[i]] = positionalValues[i];
                foreach (var kv in namedValues)
                    if (func.Parameters.Contains(kv.Key))
                        vars[kv.Key] = kv.Value;
                for (int i = positionalValues.Count; i < func.Parameters.Count; i++)
                {
                    string pname = func.Parameters[i];
                    if (!vars.ContainsKey(pname))
                    {
                        if (func.Defaults.ContainsKey(pname))
                            vars[pname] = Visit(func.Defaults[pname]);
                        else
                            throw new Exception(Err(call, $"الوسيط '{pname}' مطلوب في استدعاء '{call.Name}'"));
                    }
                }
                var concreteTypes = new HashSet<string> { "number", "string", "bool", "object", "array", "any", "void" };
                if (func.ParamTypes != null && func.ParamTypes.Count > 0)
                {
                    foreach (var p in func.Parameters)
                    {
                        if (func.ParamTypes.TryGetValue(p, out var expectedType) && concreteTypes.Contains(expectedType))
                        {
                            string actualType = GetTypeName(vars[p]);
                            if (actualType != expectedType)
                                throw new Exception(Err(call, $"خطأ نوع: الوسيط '{p}' في '{call.Name}' يتوقع {expectedType} لكن القيمة من نوع {actualType}"));
                        }
                    }
                }
                if (func.IsAsync)
                {
                    var task = System.Threading.Tasks.Task.Run(() =>
                    {
                        PushCall(call.Name + "()");
                        try { Visit(func.Body); }
                        catch (ReturnException ex) { PopCall(); RestoreShadowed(func.Parameters, saved); return ex.Value; }
                        PopCall(); RestoreShadowed(func.Parameters, saved); return (object)0;
                    });
                    return task;
                }
                PushCall(call.Name + "()");
                try { Visit(func.Body); }
                catch (ReturnException ex)
                {
                    PopCall();
                    RestoreShadowed(func.Parameters, saved);
                    return ex.Value;
                }
                PopCall();
                RestoreShadowed(func.Parameters, saved);
                return 0;
            }

            if (node is ClassDefNode cls)
            {
                classes[cls.Name] = cls;
                var classDict = new Dictionary<string, object> { ["__class"] = cls.Name };
                staticFields[cls.Name] = classDict;

                if (cls.ParentName != null && classes.ContainsKey(cls.ParentName))
                {
                    var parent = classes[cls.ParentName];
                    foreach (var pm in parent.Methods)
                        if (!cls.Methods.Exists(m => m.Name == pm.Name))
                            cls.Methods.Add(pm);
                }
                var allFields = new List<FieldDefNode>();
                if (cls.ParentName != null && classes.ContainsKey(cls.ParentName))
                {
                    var parent = classes[cls.ParentName];
                    foreach (var pf in parent.Fields)
                        if (!allFields.Exists(f => f.Name == pf.Name))
                            allFields.Add(pf);
                }
                foreach (var field in cls.Fields)
                    if (!allFields.Exists(f => f.Name == field.Name))
                        allFields.Add(field);
                foreach (var f in allFields)
                    if ((f.Flags & AccessFlags.Static) != 0 && !classDict.ContainsKey(f.Name))
                        classDict[f.Name] = 0.0;
                cls.Fields = allFields;
                vars[cls.Name] = classDict;
                return cls.Name;
            }
            if (node is NewNode newObj)
            {
                string cn = newObj.ClassName.ToLower();
                if (cn == "camera" || cn == "كاميرا")
                {
                    double x = newObj.Arguments.Count > 0 ? Convert.ToDouble(Visit(newObj.Arguments[0])) : 0;
                    double y = newObj.Arguments.Count > 1 ? Convert.ToDouble(Visit(newObj.Arguments[1])) : 0;
                    double z = newObj.Arguments.Count > 2 ? Convert.ToDouble(Visit(newObj.Arguments[2])) : 5;
                    var cam = UIRuntime?.Create3DObject("camera", x, y, z);
                    if (cam != null) return cam;
                    // Fallback: store as typed wrapper
                    return new UIElementHandle(new { Type = "camera", X = x, Y = y, Z = z }, "camera");
                }
                if (cn == "light" || cn == "ضوء")
                {
                    double x = newObj.Arguments.Count > 0 ? Convert.ToDouble(Visit(newObj.Arguments[0])) : 0;
                    double y = newObj.Arguments.Count > 1 ? Convert.ToDouble(Visit(newObj.Arguments[1])) : 0;
                    double power = newObj.Arguments.Count > 2 ? Convert.ToDouble(Visit(newObj.Arguments[2])) : 1;
                    var light = UIRuntime?.Create3DObject("light", x, y, power);
                    if (light != null) return light;
                    return new UIElementHandle(new { Type = "light", X = x, Y = y, Power = power }, "light");
                }
                if (cn == "object" || cn == "كائن")
                {
                    double x = newObj.Arguments.Count > 0 ? Convert.ToDouble(Visit(newObj.Arguments[0])) : 0;
                    double y = newObj.Arguments.Count > 1 ? Convert.ToDouble(Visit(newObj.Arguments[1])) : 0;
                    string path = newObj.Arguments.Count > 2 ? (Visit(newObj.Arguments[2])?.ToString() ?? "") : "";
                    var obj3d = UIRuntime?.Create3DObject("object", x, y, 0);
                    if (obj3d != null) return obj3d;
                    return new UIElementHandle(new { Type = "object", X = x, Y = y, Path = path }, "object");
                }
                if (!classes.ContainsKey(newObj.ClassName))
                    throw new Exception(Err(newObj, $"class '{newObj.ClassName}' غير موجود"));
                var clsDef = classes[newObj.ClassName];
                if (clsDef.IsAbstract)
                    throw new Exception(Err(newObj, $"لا يمكن إنشاء كائن من class مجرد '{newObj.ClassName}'"));
                if (clsDef.IsInterface)
                    throw new Exception(Err(newObj, $"لا يمكن إنشاء كائن من interface '{newObj.ClassName}'"));
                var instance = new Dictionary<string, object> { ["__class"] = newObj.ClassName };
                var instanceFields = clsDef.Fields.Where(f => (f.Flags & AccessFlags.Static) == 0).ToList();
                foreach (var field in instanceFields)
                    instance[field.Name] = 0.0;
                for (int i = 0; i < newObj.Arguments.Count && i < instanceFields.Count; i++)
                    instance[instanceFields[i].Name] = Visit(newObj.Arguments[i]);
                bool hadSelf = vars.ContainsKey("self");
                object savedSelf = hadSelf ? vars["self"] : null;
                vars["self"] = instance;
                if (clsDef.InitBody != null)
                {
                    Visit(clsDef.InitBody);
                    if (clsDef.InitBody is BlockNode initBodyBlock)
                        foreach (var stmt in initBodyBlock.Statements)
                            if (stmt is AssignNode an)
                                instance[an.Name] = vars[an.Name];
                }
                if (hadSelf) vars["self"] = savedSelf;
                else vars.Remove("self");
                return instance;
            }
            if (node is SelfNode sn)
            {
                if (!vars.ContainsKey("self")) throw new Exception(Err(node, "self خارج الدالة"));
                return vars["self"];
            }

            if (node is MemberGetNode mg)
            {
                var obj = Visit(mg.Object);
                if (obj is UIElementHandle uih && UIRuntime != null)
                {
                    var result = UIRuntime.GetProperty(uih, mg.Member);
                    if (result != null) return result;
                    throw new Exception(Err(mg, $"الخاصية '{mg.Member}' غير موجودة في {uih.TypeName}"));
                }
                if (obj is Dictionary<string, object> dict)
                {
                    string cn = dict.ContainsKey("__class") ? dict["__class"] as string : null;
                    if (cn != null && classes.ContainsKey(cn))
                    {
                        var clsDef = classes[cn];
                        var field = clsDef.Fields.Find(f => f.Name == mg.Member);
                        if (field != null && (field.Flags & AccessFlags.Private) != 0 && _currentClass != cn)
                            throw new Exception(Err(mg, $"الحقل '{mg.Member}' خاص في class {cn}"));
                        if ((field?.Flags & AccessFlags.Static) != 0 && staticFields.ContainsKey(cn) && staticFields[cn].ContainsKey(mg.Member))
                            return staticFields[cn][mg.Member];
                    }
                    if (!dict.ContainsKey(mg.Member))
                        throw new Exception(Err(mg, $"الحقل '{mg.Member}' غير موجود"));
                    return dict[mg.Member];
                }
                if (obj is string strVal)
                {
                    if (mg.Member.ToLower() == "len" || mg.Member == "طول" || mg.Member.ToLower() == "length" || mg.Member == "الطول")
                        return (double)strVal.Length;
                }
                if (obj is List<object> list)
                {
                    if (mg.Member.ToLower() == "len" || mg.Member == "طول")
                        return (double)list.Count;
                    if (mg.Member.ToLower() == "length" || mg.Member == "الطول")
                        return (double)list.Count;
                }
                throw new Exception(Err(mg, "الوصول إلى عضو في غير كائن"));
            }
            if (node is MemberSetNode ms)
            {
                var obj = Visit(ms.Object);
                if (obj is UIElementHandle uih && UIRuntime != null)
                {
                    string member = ms.Member;
                    if (member.Equals("content", StringComparison.OrdinalIgnoreCase)) member = "Content";
                    if (member.Equals("text", StringComparison.OrdinalIgnoreCase)) member = "Text";
                    if (member.Equals("id", StringComparison.OrdinalIgnoreCase)) member = "Name";
                    if (member.Equals("source", StringComparison.OrdinalIgnoreCase) || member.Equals("src", StringComparison.OrdinalIgnoreCase)) member = "Source";
                    if (member.ToLower() == "cam") member = "Camera";
                    if (member.ToLower() == "ligt" || member.ToLower() == "lig") member = "Light";
                    if (member.ToLower() == "objet") member = "Object";

                    if (uih.TypeName.ToLower() == "mediaelement" || uih.TypeName.ToLower() == "video" || uih.TypeName.ToLower() == "sound")
                    {
                        if (member.ToLower() == "speed") member = "SpeedRatio";
                        if (member.ToLower() == "muted") member = "IsMuted";
                        if (member.ToLower() == "volume") member = "Volume";
                        if (member.ToLower() == "balance") member = "Balance";
                        if (member.ToLower() == "position") member = "Position";
                    }

                    string fn = null;
                    if (ms.Value is FuncCallNode fcn) fn = fcn.Name;
                    else if (ms.Value is VariableNode vn) fn = vn.Name;
                    else if (ms.Value is FuncDefNode arrowFn && IsEventMember(member))
                    {
                        UIRuntime.BindEvent(uih, member, () =>
                        {
                            try { Visit(arrowFn.Body); }
                            catch (Exception ex) { UIRuntime.ShowError(ex.Message); }
                        });
                        return 0;
                    }

                    if (fn != null && IsEventMember(member))
                    {
                        UIRuntime.BindEvent(uih, member, () =>
                        {
                            try { Visit(new FuncCallNode(fn, new List<Node>())); }
                            catch (Exception ex) { UIRuntime.ShowError(ex.Message); }
                        });
                        return 0;
                    }
                    object value = Visit(ms.Value);
                    if (member == "Source" && value is string srcPath && !string.IsNullOrEmpty(srcPath) &&
                        !srcPath.StartsWith("http://") && !srcPath.StartsWith("https://") &&
                        !System.IO.Path.IsPathRooted(srcPath))
                        value = System.IO.Path.GetFullPath(System.IO.Path.Combine(BaseDir, srcPath));
                    UIRuntime.SetProperty(uih, member, value);
                    return 0;
                }
                if (obj is Dictionary<string, object> dict)
                {
                    object value = Visit(ms.Value);
                    if (dict.ContainsKey("__class") && dict["__class"] is string cn && classes.ContainsKey(cn))
                    {
                        var clsDef = classes[cn];
                        var field = clsDef.Fields.Find(f => f.Name == ms.Member);
                        if (field != null && (field.Flags & AccessFlags.Private) != 0 && _currentClass != cn)
                            throw new Exception(Err(ms, $"الحقل '{ms.Member}' خاص في class {cn}"));
                        if (field != null && (field.Flags & AccessFlags.Static) != 0)
                        {
                            staticFields[cn][ms.Member] = value;
                            return value;
                        }
                        if (field != null || clsDef.Methods.Exists(m => m.Name == ms.Member))
                        {
                            dict[ms.Member] = value;
                            return value;
                        }
                        throw new Exception(Err(ms, $"الحقل '{ms.Member}' غير موجود في class {cn}"));
                    }
                    dict[ms.Member] = value;
                    return value;
                }
                throw new Exception(Err(ms, "تعيين عضو في غير كائن"));
            }

            if (node is MethodCallNode mc)
            {
                var obj = Visit(mc.Object);

                if (obj is string str)
                {
                    if (mc.Method == "len" || mc.Method == "طول") return (double)str.Length;
                    if ((mc.Method == "sub" || mc.Method == "اقتطع") && mc.Arguments.Count >= 1)
                    {
                        int start = Convert.ToInt32(Visit(mc.Arguments[0]));
                        int length = mc.Arguments.Count > 1 ? Convert.ToInt32(Visit(mc.Arguments[1])) : str.Length - start;
                        return str.Substring(start, Math.Min(length, str.Length - start));
                    }
                    if (mc.Method == "contains" || mc.Method == "يحتوي")
                        return str.Contains(Visit(mc.Arguments[0])?.ToString() ?? "") ? 1.0 : 0.0;
                    if (mc.Method == "replace" || mc.Method == "استبدل")
                        return str.Replace(Visit(mc.Arguments[0])?.ToString() ?? "", Visit(mc.Arguments[1])?.ToString() ?? "");
                    if (mc.Method == "upper" || mc.Method == "كبير") return str.ToUpper();
                    if (mc.Method == "lower" || mc.Method == "صغير") return str.ToLower();
                    if (mc.Method == "trim" || mc.Method == "قص") return str.Trim();
                    if (mc.Method == "indexOf" || mc.Method == "موقع")
                        return (double)str.IndexOf(Visit(mc.Arguments[0])?.ToString() ?? "");
                    if (mc.Method == "split" || mc.Method == "قسم")
                    {
                        char sep = (Visit(mc.Arguments[0])?.ToString() ?? " ")[0];
                        var parts = str.Split(sep);
                        var resultList = new List<object>();
                        foreach (var p in parts) resultList.Add(p);
                        return resultList;
                    }
                    if (mc.Method == "forEach" || mc.Method == "لكل")
                    {
                        string fnName = mc.Arguments[0] is VariableNode vn ? vn.Name : (Visit(mc.Arguments[0])?.ToString() ?? "");
                        for (int ci = 0; ci < str.Length; ci++)
                        {
                            vars["it"] = str[ci].ToString();
                            Visit(new FuncCallNode(fnName, new List<Node>()));
                        }
                        return 0;
                    }
                    if (mc.Method == "startsWith" || mc.Method == "يبدأ")
                        return str.StartsWith(Visit(mc.Arguments[0])?.ToString() ?? "") ? 1.0 : 0.0;
                    if (mc.Method == "endsWith" || mc.Method == "ينتهي")
                        return str.EndsWith(Visit(mc.Arguments[0])?.ToString() ?? "") ? 1.0 : 0.0;
                    if (mc.Method == "padLeft" || mc.Method == "املأيسار")
                    {
                        int totalWidth = Convert.ToInt32(Visit(mc.Arguments[0]));
                        char padChar = mc.Arguments.Count > 1 ? (Visit(mc.Arguments[1])?.ToString() ?? " ")[0] : ' ';
                        return str.PadLeft(totalWidth, padChar);
                    }
                    if (mc.Method == "padRight" || mc.Method == "املأيمين")
                    {
                        int totalWidth = Convert.ToInt32(Visit(mc.Arguments[0]));
                        char padChar = mc.Arguments.Count > 1 ? (Visit(mc.Arguments[1])?.ToString() ?? " ")[0] : ' ';
                        return str.PadRight(totalWidth, padChar);
                    }
                    if (mc.Method == "repeat" || mc.Method == "كرر")
                    {
                        int count = Convert.ToInt32(Visit(mc.Arguments[0]));
                        var sb = new System.Text.StringBuilder();
                        for (int ri = 0; ri < count; ri++) sb.Append(str);
                        return sb.ToString();
                    }
                    if (mc.Method == "format" || mc.Method == "نسق")
                    {
                        var args = new object[mc.Arguments.Count];
                        for (int ai = 0; ai < mc.Arguments.Count; ai++)
                            args[ai] = Visit(mc.Arguments[ai]);
                        return string.Format(str, args);
                    }
                    throw new Exception(Err(mc, $"الدالة '{mc.Method}' غير معروفة للنص"));
                }
                if (obj is List<object> list)
                {
                    if (mc.Method == "set" || mc.Method == "تعيين")
                    {
                        int idx = Convert.ToInt32(Visit(mc.Arguments[0]));
                        list[idx] = Visit(mc.Arguments[1]);
                        return list[idx];
                    }
                    if (mc.Method == "push" || mc.Method == "add" || mc.Method == "أضف")
                    {
                        list.Add(Visit(mc.Arguments[0]));
                        return list[list.Count - 1];
                    }
                    if (mc.Method == "pop" || mc.Method == "احذف")
                    {
                        if (list.Count == 0) throw new Exception(Err(mc, "المصفوفة فارغة"));
                        int last = list.Count - 1;
                        var val = list[last];
                        list.RemoveAt(last);
                        return val;
                    }
                    if (mc.Method == "get" || mc.Method == "احصل")
                    {
                        int idx = Convert.ToInt32(Visit(mc.Arguments[0]));
                        return list[idx];
                    }
                    if (mc.Method == "len" || mc.Method == "طول") return (double)list.Count;
                    if (mc.Method == "join" || mc.Method == "انضم")
                    {
                        string sep = mc.Arguments.Count > 0 ? (Visit(mc.Arguments[0])?.ToString() ?? "") : "";
                        return string.Join(sep, list);
                    }
                    if (mc.Method == "forEach" || mc.Method == "لكل")
                    {
                        string fnName = mc.Arguments[0] is VariableNode vn ? vn.Name : (Visit(mc.Arguments[0])?.ToString() ?? "");
                        foreach (var item in list)
                        {
                            vars["it"] = item;
                            Visit(new FuncCallNode(fnName, new List<Node>()));
                        }
                        return 0;
                    }
                    if (mc.Method == "map" || mc.Method == "حول")
                    {
                        string fnName = mc.Arguments[0] is VariableNode vn ? vn.Name : (Visit(mc.Arguments[0])?.ToString() ?? "");
                        var result = new List<object>();
                        foreach (var item in list)
                        {
                            vars["it"] = item;
                            result.Add(Visit(new FuncCallNode(fnName, new List<Node>())));
                        }
                        return result;
                    }
                    if (mc.Method == "filter" || mc.Method == "صفي")
                    {
                        string fnName = mc.Arguments[0] is VariableNode vn ? vn.Name : (Visit(mc.Arguments[0])?.ToString() ?? "");
                        var result = new List<object>();
                        foreach (var item in list)
                        {
                            vars["it"] = item;
                            double val = Convert.ToDouble(Visit(new FuncCallNode(fnName, new List<Node>())));
                            if (val != 0) result.Add(item);
                        }
                        return result;
                    }
                    if (mc.Method == "reduce" || mc.Method == "اختزل")
                    {
                        string fnName = mc.Arguments[0] is VariableNode vn ? vn.Name : (Visit(mc.Arguments[0])?.ToString() ?? "");
                        object acc = mc.Arguments.Count > 1 ? Visit(mc.Arguments[1]) : 0;
                        int idx = 0;
                        foreach (var item in list)
                        {
                            vars["acc"] = acc;
                            vars["it"] = item;
                            vars["idx"] = (double)idx;
                            acc = Visit(new FuncCallNode(fnName, new List<Node>()));
                            idx++;
                        }
                        return acc;
                    }
                    if (mc.Method == "sort" || mc.Method == "رتب")
                    {
                        list.Sort((a, b) => (a?.ToString() ?? "").CompareTo(b?.ToString() ?? ""));
                        return list;
                    }
                    if (mc.Method == "reverse" || mc.Method == "عكس")
                    {
                        list.Reverse();
                        return list;
                    }
                    throw new Exception(Err(mc, $"الدالة '{mc.Method}' غير معروفة للمصفوفة"));
                }
                if (obj is RegexNode reg)
                {
                    if (mc.Method == "test" || mc.Method == "اختبر")
                    {
                        string input = mc.Arguments.Count > 0 ? Visit(mc.Arguments[0])?.ToString() ?? "" : "";
                        return reg.RegexObj.IsMatch(input) ? 1.0 : 0.0;
                    }
                    if (mc.Method == "match" || mc.Method == "طابق")
                    {
                        string input = mc.Arguments.Count > 0 ? Visit(mc.Arguments[0])?.ToString() ?? "" : "";
                        var m = reg.RegexObj.Match(input);
                        if (!m.Success) return new List<object>();
                        var result = new List<object>();
                        foreach (System.Text.RegularExpressions.Group g in m.Groups)
                            result.Add(g.Value);
                        return result;
                    }
                    if (mc.Method == "replace" || mc.Method == "استبدل")
                    {
                        string input = mc.Arguments.Count > 0 ? Visit(mc.Arguments[0])?.ToString() ?? "" : "";
                        string replacement = mc.Arguments.Count > 1 ? Visit(mc.Arguments[1])?.ToString() ?? "" : "";
                        return reg.RegexObj.Replace(input, replacement);
                    }
                    throw new Exception(Err(mc, $"الدالة '{mc.Method}' غير معروفة للتعبير المنتظم"));
                }
                if (obj is UIElementHandle uih && UIRuntime != null)
                {
                    if (mc.Method == "add" || mc.Method == "أضف")
                    {
                        for (int ai = 0; ai < mc.Arguments.Count; ai++)
                        {
                            var child = Visit(mc.Arguments[ai]);
                            if (child is UIElementHandle childHandle)
                                UIRuntime.AddChild(uih, childHandle);
                        }
                        return 0;
                    }
                    if (mc.Method == "remove" || mc.Method == "احذف")
                    {
                        var child = Visit(mc.Arguments[0]);
                        if (child is UIElementHandle childHandle)
                            UIRuntime.RemoveChild(uih, childHandle);
                        return 0;
                    }
                    if (uih.TypeName.ToLower() == "mediaelement" || uih.TypeName.ToLower() == "video" || uih.TypeName.ToLower() == "sound")
                    {
                        if (mc.Method == "play" || mc.Method == "شغل") { UIRuntime.CallMethod(uih, "Play"); return 0; }
                        if (mc.Method == "pause" || mc.Method == "أوقف") { UIRuntime.CallMethod(uih, "Pause"); return 0; }
                        if (mc.Method == "stop" || mc.Method == "إيقاف") { UIRuntime.CallMethod(uih, "Stop"); return 0; }
                        if (mc.Method == "speed" || mc.Method == "سرعة") { if (mc.Arguments.Count > 0) UIRuntime.SetProperty(uih, "SpeedRatio", Convert.ToDouble(Visit(mc.Arguments[0]))); return 0; }
                        if (mc.Method == "volume" || mc.Method == "صوت") { if (mc.Arguments.Count > 0) UIRuntime.SetProperty(uih, "Volume", Convert.ToDouble(Visit(mc.Arguments[0]))); return 0; }
                        if (mc.Method == "mute" || mc.Method == "كتم") { UIRuntime.SetProperty(uih, "IsMuted", true); return 0; }
                        if (mc.Method == "unmute" || mc.Method == "الغاء_الكتم") { UIRuntime.SetProperty(uih, "IsMuted", false); return 0; }
                        if (mc.Method == "fullscreen" || mc.Method == "كامل") { UIRuntime.ToggleFullscreen(uih); return 0; }
                        if (mc.Method == "zoom" || mc.Method == "تكبير") { if (mc.Arguments.Count > 0) UIRuntime.SetProperty(uih, "ZoomFactor", Convert.ToDouble(Visit(mc.Arguments[0]))); return 0; }
                    }
                    throw new Exception(Err(mc, $"الدالة '{mc.Method}' غير معروفة للعنصر"));
                }
                if (obj is Dictionary<string, object> instance)
                {
                    if (!instance.ContainsKey("__class"))
                    {
                        if (instance.TryGetValue(mc.Method, out var val) && val is FuncDefNode fn)
                        {
                            var savedMod = new Dictionary<string, object>();
                            foreach (var p in fn.Parameters)
                                if (vars.ContainsKey(p)) savedMod[p] = vars[p];
                            for (int i = 0; i < fn.Parameters.Count; i++)
                                vars[fn.Parameters[i]] = i < mc.Arguments.Count ? Visit(mc.Arguments[i]) : 0;
                            PushCall(mc.Method + "(mod)");
                            try { Visit(fn.Body); }
                            catch (ReturnException ex) { PopCall(); RestoreMethodVars(fn.Parameters, savedMod, false); return ex.Value; }
                            PopCall();
                            RestoreMethodVars(fn.Parameters, savedMod, false);
                            return 0;
                        }
                        throw new Exception(Err(mc, $"الدالة '{mc.Method}' غير موجودة في الوحدة"));
                    }
                    string cn = instance["__class"] as string;
                    if (!classes.ContainsKey(cn))
                        throw new Exception(Err(mc, $"class '{cn}' غير موجود"));
                    var clsDef = classes[cn];
                    var method = clsDef.Methods.Find(m => m.Name == mc.Method);
                    if (method == null)
                        throw new Exception(Err(mc, $"الدالة '{mc.Method}' غير موجودة في class {cn}"));
                    if ((method.Flags & AccessFlags.Private) != 0 && _currentClass != cn)
                        throw new Exception(Err(mc, $"الدالة '{mc.Method}' خاصة في class {cn}"));
                    if (mc.Arguments.Count != method.Parameters.Count)
                        throw new Exception(Err(mc, $"Argument mismatch in {cn}.{mc.Method}: expected {method.Parameters.Count}, got {mc.Arguments.Count}"));

                    var saved = new Dictionary<string, object>();
                    foreach (var p in method.Parameters)
                        if (vars.ContainsKey(p)) saved[p] = vars[p];
                    string savedClass = _currentClass;
                    _currentClass = cn;
                    bool hadSelf = vars.ContainsKey("self");
                    if (hadSelf) saved["self"] = vars["self"];

                    for (int i = 0; i < method.Parameters.Count; i++)
                        vars[method.Parameters[i]] = Visit(mc.Arguments[i]);
                    if ((method.Flags & AccessFlags.Static) != 0)
                        vars["self"] = staticFields[cn];
                    else
                        vars["self"] = instance;

                    PushCall(cn + "." + mc.Method + "()");
                    try { Visit(method.Body); }
                    catch (ReturnException ex)
                    {
                        PopCall();
                        _currentClass = savedClass;
                        RestoreMethodVars(method.Parameters, saved, hadSelf);
                        return ex.Value;
                    }
                    PopCall();
                    _currentClass = savedClass;
                    RestoreMethodVars(method.Parameters, saved, hadSelf);
                    return 0;
                }
                throw new Exception(Err(mc, "استدعاء دالة على غير كائن"));
            }

            if (node is APlusXmlNode ax)
            {
                EnsureWindow();
                var elem = Visit(ax.Element);
                UIRuntime.RegisterElement(ax.VarName, elem as UIElementHandle);
                if (elem != null) vars[ax.VarName] = elem;
                return elem;
            }
            if (node is XmlElementNode xe)
            {
                EnsureWindow();
                // Component function call
                if (funcs.ContainsKey(xe.TagName) && !IsKnownUIElement(xe.TagName))
                {
                    var func = funcs[xe.TagName] as FuncDefNode;
                    if (func != null)
                    {
                        var args = new List<Node>();
                        foreach (var param in func.Parameters)
                        {
                            var attr = xe.Attributes.FirstOrDefault(a =>
                                string.Equals(a.Name, param, StringComparison.OrdinalIgnoreCase));
                            if (attr.Value != null)
                                args.Add(attr.Value);
                            else if (func.Defaults.ContainsKey(param))
                                args.Add(func.Defaults[param]);
                            else
                                throw new Exception(Err(xe, $"الوسيط '{param}' مفقود للمكون '{xe.TagName}'"));
                        }
                        var callNode = new FuncCallNode(xe.TagName, args);
                        object result = Visit(callNode);
                        foreach (var child in xe.Children)
                        {
                            var ch = Visit(child);
                            if (result is UIElementHandle parent && ch is UIElementHandle childElem)
                                UIRuntime.AddChild(parent, childElem);
                        }
                        return result;
                    }
                }
                // Create UI element
                var elem = UIRuntime.CreateElement(xe.TagName);
                foreach (var attr in xe.Attributes)
                {
                    string an = attr.Name;
                    if (an.StartsWith("on"))
                    {
                        string evt = an.Substring(2);
                        if (evt == "click") evt = "Click";
                        else if (evt == "input" || evt == "change") evt = "ValueChanged";
                        if (attr.Value is FuncDefNode fn)
                        {
                            var capturedFn = fn;
                            UIRuntime.BindEvent(elem, evt, () =>
                            {
                                try
                                {
                                    Visit(capturedFn.Body);
                                    ReapplyBindings();
                                }
                                catch (Exception ex) { UIRuntime.ShowError(ex.Message); }
                            });
                        }
                        else if (attr.Value != null)
                        {
                            object val = Visit(attr.Value);
                            if (val is string fnName && funcs.ContainsKey(fnName))
                            {
                                UIRuntime.BindEvent(elem, evt, () =>
                                {
                                    try
                                    {
                                        Visit(new FuncCallNode(fnName, new List<Node>()));
                                        ReapplyBindings();
                                    }
                                    catch (Exception ex) { UIRuntime.ShowError(ex.Message); }
                                });
                            }
                        }
                    }
                    else
                    {
                        string pn = an;
                        if (pn == "content" || pn == "text") pn = "Content";
                        if (pn == "id") pn = "Name";
                        if (pn == "src") pn = "Source";
                        object val = attr.Value != null ? Visit(attr.Value) : null;
                        UIRuntime.SetProperty(elem, pn, val);
                    }
                }
                foreach (var child in xe.Children)
                {
                    var ch = Visit(child);
                    if (ch is UIElementHandle childElem)
                        UIRuntime.AddChild(elem, childElem);
                }
                return elem;
            }
            if (node is APlusNewNode ap)
            {
                EnsureWindow();
                var elem = UIRuntime.CreateElement(ap.ClassName);
                UIRuntime.RegisterElement(ap.VarName, elem);
                vars[ap.VarName] = elem;
                return elem;
            }
            if (node is ArrowBindNode ab)
            {
                var elem = UIRuntime.GetElement(ab.VarName);
                if (elem == null)
                    throw new Exception(Err(ab, $"عنصر UI '{ab.VarName}' غير موجود"));
                string pn = ab.EventName;
                if (pn == "content" || pn == "text") pn = "Content";
                if (pn == "id") pn = "Name";

                // Try property binding (with timer-based reapply)
                object val = Visit(new VariableNode(ab.FuncName));
                UIRuntime.SetProperty(elem, pn, val);

                // Also try event binding
                UIRuntime.BindEvent(elem, pn, () =>
                {
                    try
                    {
                        Visit(new FuncCallNode(ab.FuncName, new List<Node>()));
                        ReapplyBindings();
                    }
                    catch (Exception ex) { UIRuntime.ShowError(ex.Message); }
                });
                return 0;
            }
            if (node is ShowNode showNode)
            {
                EnsureWindow();
                bool isWisMode = false;
                string loopFunc = null;
                ClassDefNode loopClassDef = null;
                if (showNode.Args.Count > 1 && showNode.Args[1] is VariableNode pv && pv.Name.ToLower() == "wis")
                    isWisMode = true;
                if (showNode.Args.Count > 0)
                {
                    object firstVal = Visit(showNode.Args[0]);
                    if (firstVal is string fnName && funcs.ContainsKey(fnName))
                    {
                        Visit(new FuncCallNode(fnName, new List<Node>()));
                        if (isWisMode) loopFunc = fnName;
                    }
                    else if (firstVal is Dictionary<string, object> dict && dict.ContainsKey("__class"))
                    {
                        string cn = dict["__class"] as string;
                        if (cn != null && classes.ContainsKey(cn))
                        {
                            var clsDef = classes[cn];
                            var savedFuncs = new Dictionary<string, FuncDefNode>();
                            foreach (var m in clsDef.Methods)
                            {
                                if (funcs.ContainsKey(m.Name))
                                    savedFuncs[m.Name] = funcs[m.Name];
                                funcs[m.Name] = m;
                            }
                            bool hadSelf = vars.ContainsKey("self");
                            object savedSelf = hadSelf ? vars["self"] : null;
                            var instance = new Dictionary<string, object> { ["__class"] = cn };
                            foreach (var field in clsDef.Fields)
                                instance[field.Name] = 0.0;
                            vars["self"] = instance;
                            if (clsDef.InitBody != null)
                            {
                                Visit(clsDef.InitBody);
                                if (isWisMode) { loopClassDef = clsDef; }
                            }
                            if (hadSelf) vars["self"] = savedSelf;
                            else vars.Remove("self");
                            foreach (var m in clsDef.Methods)
                            {
                                if (savedFuncs.ContainsKey(m.Name))
                                    funcs[m.Name] = savedFuncs[m.Name];
                                else
                                    funcs.Remove(m.Name);
                            }
                        }
                    }
                }
                UIRuntime.ShowWindow();
                return 0;
            }

            if (node is ThrowNode thr)
            {
                object value = Visit(thr.Value);
                throw new ThrowException(value);
            }
            if (node is TryNode tryNode)
            {
                try { return Visit(tryNode.TryBody); }
                catch (ThrowException ex)
                {
                    if (tryNode.CatchVar != null)
                        vars[tryNode.CatchVar] = ex.Value;
                    if (tryNode.CatchBody != null)
                        return Visit(tryNode.CatchBody);
                    throw;
                }
                finally
                {
                    if (tryNode.FinallyBody != null) Visit(tryNode.FinallyBody);
                }
            }

            if (node is ExportNode exp)
            {
                object result = Visit(exp.Declaration);
                if (exp.Declaration is AssignNode an && an.IsExported)
                    ExportedVars.Add(an.Name);
                else if (exp.Declaration is FuncDefNode fn && fn.IsExported)
                    ExportedVars.Add(fn.Name);
                else if (exp.Declaration is ClassDefNode cl && cl.IsExported)
                    ExportedVars.Add(cl.Name);
                return result;
            }

            if (node is ImportNode imp)
            {
                if (imp.From == null) return 0;
                string fullPath = System.IO.Path.Combine(BaseDir, imp.From);
                if (!System.IO.File.Exists(fullPath))
                {
                    string withA = fullPath + ".a";
                    string withAPlus = fullPath + ".a+";
                    if (System.IO.File.Exists(withA)) fullPath = withA;
                    else if (System.IO.File.Exists(withAPlus)) fullPath = withAPlus;
                }
                if (!System.IO.File.Exists(fullPath))
                    throw new Exception($"الملف غير موجود: {imp.From}");
                string normalized = System.IO.Path.GetFullPath(fullPath);

                if (_loading.Contains(normalized))
                    throw new Exception($"استيراد دائري (circular import) في '{imp.From}'");

                if (!_modules.TryGetValue(normalized, out var module))
                {
                    _loading.Add(normalized);
                    string code = System.IO.File.ReadAllText(fullPath);
                    if (string.IsNullOrWhiteSpace(code)) { _loading.Remove(normalized); return 0; }
                    code = Shorthand.Convert(code);
                    var lex = new Lexer(code);
                    var toks = lex.Tokenize();
                    if (toks.Count == 0) { _loading.Remove(normalized); return 0; }
                    var parser = new Parser(toks);
                    var ast = parser.Parse();

                    var modInterp = new Interpreter { Output = Output, BaseDir = System.IO.Path.GetDirectoryName(fullPath), UIRuntime = UIRuntime };
                    modInterp.Visit(ast);

                    module = new ModuleScope { Path = normalized };
                    foreach (var kv in modInterp.vars)
                        if (modInterp.ExportedVars.Contains(kv.Key)) module.Vars[kv.Key] = kv.Value;
                    foreach (var kv in modInterp.funcs)
                        if (kv.Value.IsExported) module.Funcs[kv.Key] = kv.Value;
                    foreach (var kv in modInterp.classes)
                        if (kv.Value.IsExported) module.Classes[kv.Key] = kv.Value;
                    _modules[normalized] = module;
                    _loading.Remove(normalized);
                }

                if (imp.Names != null && imp.Names.Count > 0)
                {
                    foreach (var nm in imp.Names)
                    {
                        if (module.Vars.ContainsKey(nm)) vars[nm] = module.Vars[nm];
                        else if (module.Funcs.ContainsKey(nm)) funcs[nm] = module.Funcs[nm];
                        else if (module.Classes.ContainsKey(nm)) classes[nm] = module.Classes[nm];
                    }
                }
                else if (imp.AsAlias != null)
                {
                    var modObj = new Dictionary<string, object>();
                    foreach (var kv in module.Vars) modObj[kv.Key] = kv.Value;
                    foreach (var kv in module.Funcs) modObj[kv.Key] = kv.Value;
                    foreach (var kv in module.Classes) modObj[kv.Key] = kv.Value;
                    vars[imp.AsAlias] = modObj;
                }
                else
                {
                    foreach (var kv in module.Vars) vars[kv.Key] = kv.Value;
                    foreach (var kv in module.Funcs) funcs[kv.Key] = kv.Value;
                    foreach (var kv in module.Classes) classes[kv.Key] = kv.Value;
                }
                return 0;
            }

            if (node is NilNode)
            {
                return 0.0;
            }

            if (node is AwaitNode awaitNode)
            {
                var result = Visit(awaitNode.Expr);
                if (result is System.Threading.Tasks.Task task)
                {
                    task.GetAwaiter().GetResult();
                    if (task is System.Threading.Tasks.Task<object> taskObj)
                        return taskObj.Result;
                    return 0;
                }
                return result;
            }

            if (node is GoNode goNode)
            {
                System.Threading.Tasks.Task.Run(() =>
                {
                    try { Visit(goNode.Expr); }
                    catch (Exception ex) { Output?.Invoke($"[خطأ في go] {ex.Message}"); }
                });
                return 0;
            }

            if (node is SpawnNode spawnNode)
            {
                funcs[spawnNode.Name] = new FuncDefNode(spawnNode.Name, spawnNode.Parameters, spawnNode.Body) { IsAsync = true };
                return spawnNode.Name;
            }

            if (node is DebuggerNode)
            {
                var locals = string.Join(", ", vars.Where(kv => !kv.Key.StartsWith("__") && kv.Key != "self").Select(kv => $"{kv.Key}={FormatValue(kv.Value)}"));
                Output?.Invoke($"[مصحح] سطر {node.Line}, عمود {node.Column} | المتغيرات: {locals}");
                if (Debugger != null)
                {
                    var localList = vars.Where(kv => !kv.Key.StartsWith("__") && kv.Key != "self")
                        .Select(kv => new Dictionary<string, string> { { "name", kv.Key }, { "value", FormatValue(kv.Value) } })
                        .ToList();
                    Debugger.PauseAtBreakpoint(node.Line, node.Column, localList);
                }
                return 0;
            }

            if (node is IncludeNode inc)
            {
                string fullPath;
                if (inc.From != null)
                {
                    string prefix;
                    if (inc.From.Length == 1 && char.IsLetter(inc.From[0]))
                        prefix = inc.From + ":\\";
                    else if (inc.From.EndsWith(":"))
                        prefix = inc.From + "\\";
                    else if (System.IO.Path.IsPathRooted(inc.From))
                        prefix = inc.From;
                    else
                        prefix = System.IO.Path.Combine(BaseDir, inc.From);
                    fullPath = System.IO.Path.Combine(prefix, inc.Path);
                }
                else
                    fullPath = System.IO.Path.Combine(BaseDir, inc.Path);
                if (!System.IO.File.Exists(fullPath))
                {
                    // Fallback: search in stdlib directories
                    var packageBase = System.IO.Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                        ".aplus", "packages");
                    string[] stdlibPaths = {
                        System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "stdlib", inc.Path),
                        System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "stdlib", inc.Path),
                        System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "stdlib", inc.Path),
                        System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "stdlib", inc.Path),
                        System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "stdlib", inc.Path),
                    };
                    if (System.IO.Directory.Exists(packageBase))
                    {
                        foreach (var pkgDir in System.IO.Directory.GetDirectories(packageBase))
                        {
                            string pkgName = System.IO.Path.GetFileName(pkgDir);
                            stdlibPaths = stdlibPaths.Append(System.IO.Path.Combine(pkgDir, inc.Path)).ToArray();
                        }
                    }
                    foreach (var sp in stdlibPaths)
                    {
                        string p = System.IO.Path.GetFullPath(sp);
                        if (System.IO.File.Exists(p)) { fullPath = p; break; }
                    }
                }
                string code = System.IO.File.ReadAllText(fullPath);
                if (string.IsNullOrWhiteSpace(code)) return 0;
                if (fullPath.EndsWith(".axml", StringComparison.OrdinalIgnoreCase))
                    code = Shorthand.ConvertAxmlFile(fullPath);
                else
                    code = Shorthand.Convert(code);
                var lex = new Lexer(code);
                var toks = lex.Tokenize();
                if (toks.Count == 0) return 0;
                var parser = new Parser(toks);
                var ast = parser.Parse();
                Visit(ast);
                return 0;
            }

            if (node is ArrayLiteralNode al)
            {
                var list = new List<object>();
                foreach (var elem in al.Elements)
                    list.Add(Visit(elem));
                return list;
            }
            if (node is ObjectLiteralNode ol)
            {
                var dict = new Dictionary<string, object>();
                foreach (var (key, val) in ol.Fields)
                    dict[key] = Visit(val);
                return dict;
            }
            if (node is ArrayGetNode ag)
            {
                var obj = Visit(ag.Object);
                var idxVal = Visit(ag.Index);
                int idx = Convert.ToInt32(idxVal);
                if (obj is List<object> list) return list[idx];
                throw new Exception(Err(ag, "ليس مصفوفة"));
            }
            if (node is ArraySetNode asn)
            {
                var obj = Visit(asn.Object);
                var idxVal = Visit(asn.Index);
                var value = Visit(asn.Value);
                int idx = Convert.ToInt32(idxVal);
                if (obj is List<object> list) { list[idx] = value; return value; }
                throw new Exception(Err(asn, "ليس مصفوفة"));
            }

            throw new Exception(Err(node, $"عقدة غير معروفة: {node.GetType().Name}"));
        }
    }

    delegate int Func0();
    delegate int Func1(IntPtr a1);
    delegate int Func2(IntPtr a1, IntPtr a2);
    delegate int Func3(IntPtr a1, IntPtr a2, IntPtr a3);
    delegate int Func4(IntPtr a1, IntPtr a2, IntPtr a3, IntPtr a4);
    delegate int Func5(IntPtr a1, IntPtr a2, IntPtr a3, IntPtr a4, IntPtr a5);
    delegate int Func6(IntPtr a1, IntPtr a2, IntPtr a3, IntPtr a4, IntPtr a5, IntPtr a6);
    delegate int Func7(IntPtr a1, IntPtr a2, IntPtr a3, IntPtr a4, IntPtr a5, IntPtr a6, IntPtr a7);
    delegate int Func8(IntPtr a1, IntPtr a2, IntPtr a3, IntPtr a4, IntPtr a5, IntPtr a6, IntPtr a7, IntPtr a8);

    class ReturnException : Exception { public object Value; public ReturnException(object value) { Value = value; } }
    class ThrowException : Exception { public object Value; public ThrowException(object value) { Value = value; } }
    class BreakException : Exception { }
    class ContinueException : Exception { }
}
