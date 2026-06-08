using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace A_
{
    class WebCodeGen
    {
        static Dictionary<string, string> _htmlTagMap = new(StringComparer.OrdinalIgnoreCase)
        {
            ["button"] = "button",
            ["stackpanel"] = "div",
            ["grid"] = "div",
            ["wrappanel"] = "div",
            ["dockpanel"] = "div",
            ["canvas"] = "div",
            ["uniformgrid"] = "div",
            ["scrollviewer"] = "div",
            ["border"] = "div",
            ["textblock"] = "span",
            ["textbox"] = "input",
            ["richtextbox"] = "textarea",
            ["label"] = "span",
            ["image"] = "img",
            ["img"] = "img",
            ["slider"] = "input",
            ["progressbar"] = "progress",
            ["checkbox"] = "input",
            ["radiobutton"] = "input",
            ["listbox"] = "select",
            ["listview"] = "div",
            ["combobox"] = "select",
            ["window"] = "div",
            ["viewport3d"] = "div",
            ["mediaelement"] = "video",
            ["video"] = "video",
            ["sound"] = "audio",
            ["menuitem"] = "div",
            ["menu"] = "div",
        };

        static Dictionary<string, string> _htmlAttrMap = new(StringComparer.OrdinalIgnoreCase)
        {
            ["text"] = "textContent",
            ["content"] = "textContent",
            ["width"] = "_styleWidth",
            ["height"] = "_styleHeight",
            ["fontsize"] = "_styleFontSize",
            ["fontfamily"] = "_styleFontFamily",
            ["foreground"] = "_styleColor",
            ["background"] = "_styleBackground",
            ["horizontalalignment"] = "_styleTextAlign",
            ["margin"] = "_styleMargin",
            ["padding"] = "_stylePadding",
            ["opacity"] = "_styleOpacity",
            ["source"] = "src",
            ["value"] = "_attrValue",
            ["minimum"] = "_attrMin",
            ["maximum"] = "_attrMax",
            ["placeholder"] = "placeholder",
            ["class"] = "_className",
            ["id"] = "_attrId",
            ["name"] = "_attrName",
            ["visible"] = "_styleDisplay",
            ["enabled"] = "_attrDisabled",
            ["tooltip"] = "title",
            ["tabindex"] = "tabIndex",
            ["orientation"] = "_styleFlexDirection",
            ["position"] = "currentTime",
            ["speed"] = "playbackRate",
            ["volume"] = "volume",
            ["muted"] = "muted",
        };

        static Dictionary<string, string> _htmlEventMap = new(StringComparer.OrdinalIgnoreCase)
        {
            ["click"] = "click",
            ["doubleclick"] = "dblclick",
            ["mousedown"] = "mousedown",
            ["mouseup"] = "mouseup",
            ["mouseenter"] = "mouseenter",
            ["mouseleave"] = "mouseleave",
            ["mousemove"] = "mousemove",
            ["keydown"] = "keydown",
            ["keyup"] = "keyup",
            ["keypress"] = "keypress",
            ["focus"] = "focus",
            ["blur"] = "blur",
            ["change"] = "change",
            ["input"] = "input",
            ["submit"] = "submit",
            ["load"] = "load",
            ["scroll"] = "scroll",
            ["resize"] = "resize",
            ["touchstart"] = "touchstart",
            ["touchend"] = "touchend",
            ["touchmove"] = "touchmove",
            ["contextmenu"] = "contextmenu",
            ["valuechanged"] = "input",
        };

        static HashSet<string> _skipProperties = new(StringComparer.OrdinalIgnoreCase)
        {
            "width", "height", "fontsize", "fontfamily", "foreground", "background",
            "horizontalalignment", "margin", "padding", "opacity",
            "source", "value", "minimum", "maximum", "placeholder",
            "class", "id", "name", "visible", "enabled", "tooltip", "tabindex",
            "orientation",
        };

        static string MemberToJsProperty(string member)
        {
            if (member == "__bgImage")
                return "style.backgroundImage";
            if (_htmlAttrMap.TryGetValue(member, out string mapped))
            {
                if (mapped.StartsWith("_style"))
                    return "style." + char.ToLower(mapped[6]) + mapped.Substring(7);
                if (mapped.StartsWith("_attr"))
                    return char.ToLower(mapped[5]) + mapped.Substring(6);
                return mapped;
            }
            return member;
        }

        string MemberToJsGetter(Node obj, string member)
        {
            if (member == "__bgImage")
                return "style.backgroundImage";
            string varName = GetVarName(obj);
            if (_htmlAttrMap.TryGetValue(member, out string mapped))
            {
                // Input elements use .value instead of .textContent
                if (mapped == "textContent" && varName != null &&
                    _elementMap.TryGetValue(varName, out var info) && info.HtmlTag == "input")
                    return "value";
                if (mapped.StartsWith("_style"))
                    return "style." + char.ToLower(mapped[6]) + mapped.Substring(7);
                if (mapped.StartsWith("_attr"))
                    return char.ToLower(mapped[5]) + mapped.Substring(6);
                return mapped;
            }
            return member;
        }

        class UiElementInfo
        {
            public string VarName;
            public string ClassName;
            public string HtmlTag = "div";
            public string ParentVar;
            public Dictionary<string, string> Properties = new(StringComparer.OrdinalIgnoreCase);
            public bool IsCheckable;
            public bool IsSlider;
        }

        class EventBindingInfo
        {
            public string VarName;
            public string EventName;
            public string HandlerName;
        }

        readonly List<UiElementInfo> _elements = new();
        readonly Dictionary<string, UiElementInfo> _elementMap = new(StringComparer.OrdinalIgnoreCase);
        readonly List<EventBindingInfo> _eventBindings = new();
        readonly List<(string VarName, string Property, string ValueName)> _staticPropBindings = new();
        readonly Dictionary<string, string> _fnJs = new();
        readonly Dictionary<string, List<string>> _fnParams = new();
        readonly HashSet<string> _declaredVars = new(StringComparer.OrdinalIgnoreCase);
        List<string> _topLevelJs = new();
        readonly Dictionary<string, string> _topLevelVars = new(StringComparer.OrdinalIgnoreCase);
        bool _inFunctionBody;
        int _conditionalDepth;
        int _timerCounter;
        string _appName;

        public void Generate(Node ast, string outputName, string targetDir)
        {
            _appName = outputName;
            // Register __root as the #app container div
            _elementMap["__root"] = new UiElementInfo
            {
                VarName = "__root",
                ClassName = "div",
                HtmlTag = "div"
            };
            _elements.Add(_elementMap["__root"]);

            VisitStatements(ast is BlockNode blk ? blk.Statements : new List<Node> { ast });

            PostProcessElements();
            string appJs = GenerateAppJs();
            string html = GenerateHtml(outputName);
            string runtimeJs = GenerateRuntimeJs();

            string escapedName = outputName.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");
            string htmlPath = Path.Combine(targetDir, "index.html");
            File.WriteAllText(htmlPath, html);
            File.WriteAllText(Path.Combine(targetDir, "app.js"), appJs);
            File.WriteAllText(Path.Combine(targetDir, "runtime.js"), runtimeJs);
            Console.WriteLine($"[web] Web app generated at: {targetDir}");
            Console.WriteLine("[web] Open index.html in a browser to run.");
        }

        void VisitStatements(List<Node> stmts)
        {
            foreach (var stmt in stmts)
                VisitStatement(stmt);
        }

        void VisitStatement(Node node)
        {
            if (node == null) return;
            switch (node)
            {
                case APlusNewNode apn:
                    VisitAPlusNew(apn);
                    break;
                case APlusXmlNode axn:
                    VisitAPlusXml(axn);
                    break;
                case XmlElementNode xen:
                    VisitXmlElement(xen);
                    break;
                case MemberSetNode ms:
                    VisitMemberSet(ms);
                    break;
                case FuncCallNode fc:
                    VisitFuncCall(fc);
                    break;
                case FuncDefNode fd:
                    VisitFuncDef(fd);
                    break;
                case AssignNode an:
                    VisitAssign(an);
                    break;
                case BlockNode blk:
                    VisitStatements(blk.Statements);
                    break;
                case IfNode ifn:
                    VisitIf(ifn);
                    break;
                case WhileNode wn:
                    VisitWhile(wn);
                    break;
                case ForNode fn:
                    VisitFor(fn);
                    break;
                case ForInNode fin:
                    VisitForIn(fin);
                    break;
                case ReturnNode rn:
                    VisitReturn(rn);
                    break;
                case PrintNode pn:
                    VisitPrint(pn);
                    break;
                case ShowNode sn:
                    if (_inFunctionBody || _conditionalDepth > 0)
                    {
                        _topLevelJs.Add("__init();");
                    }
                    else
                    {
                        // At top level: __init() is already called by DOMContentLoaded handler
                        // If show() has a function argument, register it to be called
                        if (sn.Args.Count > 0 && sn.Args[0] is VariableNode showFn)
                        {
                            if (_fnJs.ContainsKey(showFn.Name))
                                _topLevelVars["__showFn"] = showFn.Name;
                        }
                    }
                    break;
                case VariableNode vn:
                    break;
                case TernaryNode tn:
                    break;
                case PipeNode pipe:
                    break;
                case UnaryNode un:
                    break;
                case SelfNode sn:
                    break;
                case NumberNode nn:
                    break;
                case StringNode sn:
                    break;
                case BoolNode bn:
                    break;
                case NilNode niln:
                    break;
                case NewNode newn:
                    break;
                case IncludeNode inc:
                    break;
                case ArrayLiteralNode al:
                    break;
                case ObjectLiteralNode ol:
                    break;
                case ArrayGetNode ag:
                    break;
                case ArraySetNode asn:
                    break;
                case MemberGetNode mg:
                    if (_inFunctionBody || _conditionalDepth > 0)
                    {
                        string mgo = NodeToJsString(mg.Object);
                        string mgm = mg.Member.ToLower();
                        if (mgm == "stop")
                            _topLevelJs.Add($"({mgo}.pause(), {mgo}.currentTime = 0);");
                        else if (mgm == "play" || mgm == "pause" || mgm == "load")
                            _topLevelJs.Add($"{mgo}.{mgm}();");
                        else
                            _topLevelJs.Add($"{mgo}.{mgm};");
                    }
                    break;
                case MethodCallNode mc:
                    VisitFuncCall(mc);
                    break;
                case ArrowBindNode ab:
                    if (_inFunctionBody || _conditionalDepth > 0)
                    {
                        bool isImgWrapper = _elementMap.TryGetValue(ab.VarName, out var imgInfo) && imgInfo.ClassName == "ImageWrapper";
                        if (isImgWrapper)
                        {
                            string lowerProp = ab.EventName.ToLower();
                            if (lowerProp == "source" || lowerProp == "src")
                            {
                                _topLevelJs.Add($"{ab.VarName}.style.backgroundImage = 'url(' + {ab.FuncName} + ')';");
                                break;
                            }
                        }
                        if (_htmlEventMap.ContainsKey(ab.EventName))
                        {
                            string evt = _htmlEventMap[ab.EventName];
                            _topLevelJs.Add($"{ab.VarName}.on{evt} = {ab.FuncName};");
                        }
                        else
                        {
                            string jsProp = MemberToJsProperty(ab.EventName);
                            _topLevelJs.Add($"{ab.VarName}.{jsProp} = {ab.FuncName};");
                        }
                    }
                    else
                    {
                        if (_htmlEventMap.ContainsKey(ab.EventName))
                        {
                            _eventBindings.Add(new EventBindingInfo { VarName = ab.VarName, EventName = ab.EventName, HandlerName = ab.FuncName });
                        }
                        else
                        {
                            string elemName = ab.VarName;
                            if (_topLevelVars.TryGetValue(elemName, out string resolved))
                                elemName = resolved;
                            _staticPropBindings.Add((elemName, ab.EventName, ab.FuncName));
                        }
                    }
                    break;
                default:
                    break;
            }
        }

        void VisitAPlusNew(APlusNewNode node)
        {
            bool isImg = node.ClassName.Equals("image", StringComparison.OrdinalIgnoreCase) ||
                         node.ClassName.Equals("img", StringComparison.OrdinalIgnoreCase);
            string tag = _htmlTagMap.TryGetValue(node.ClassName, out string t) ? t : "div";
            bool isCheckable = node.ClassName.Equals("checkbox", StringComparison.OrdinalIgnoreCase) ||
                               node.ClassName.Equals("radiobutton", StringComparison.OrdinalIgnoreCase);
            bool isSlider = node.ClassName.Equals("slider", StringComparison.OrdinalIgnoreCase);
            string inputType = node.ClassName.Equals("radiobutton", StringComparison.OrdinalIgnoreCase) ? "radio" :
                               node.ClassName.Equals("checkbox", StringComparison.OrdinalIgnoreCase) ? "checkbox" :
                               node.ClassName.Equals("slider", StringComparison.OrdinalIgnoreCase) ? "range" : null;
            var info = new UiElementInfo
            {
                VarName = node.VarName,
                ClassName = node.ClassName,
                HtmlTag = tag,
                IsCheckable = isCheckable,
                IsSlider = isSlider,
            };
            if (inputType != null) info.Properties["_inputType"] = inputType;
            _elementMap[node.VarName] = info;

            if (_inFunctionBody || _conditionalDepth > 0)
            {
                // Dynamic element: emit JS creation, don't add to static _elements
                // Use div for img elements since HTML img cannot have children
                string createTag = isImg ? "div" : tag;
                _topLevelJs.Add($"let {node.VarName} = document.createElement('{createTag}');");
                _topLevelJs.Add($"{node.VarName}.id = '{node.VarName}';");
                if (inputType != null)
                    _topLevelJs.Add($"{node.VarName}.type = '{inputType}';");
                // Mark img elements so property bindings use backgroundImage
                if (isImg)
                {
                    info.HtmlTag = "div";
                    info.ClassName = "ImageWrapper";
                }
                return;
            }

            _elements.Add(info);
            if (!_declaredVars.Contains(node.VarName))
                _declaredVars.Add(node.VarName);
        }

        void VisitAPlusXml(APlusXmlNode node)
        {
            var xe = node.Element;
            string tag = _htmlTagMap.TryGetValue(xe.TagName, out string t) ? t : "div";
            var info = new UiElementInfo
            {
                VarName = node.VarName,
                ClassName = xe.TagName,
                HtmlTag = tag,
            };
            foreach (var attr in xe.Attributes)
            {
                if (attr.Name.StartsWith("on"))
                {
                    string evt = attr.Name.Substring(2);
                    if (attr.Value is FuncDefNode fn)
                    {
                        string handlerName = $"__handler_{_eventBindings.Count}";
                        _fnJs[handlerName] = FuncBodyToJs(handlerName, new List<string>(), fn.Body);
                        _fnParams[handlerName] = new List<string>();
                        _eventBindings.Add(new EventBindingInfo { VarName = node.VarName, EventName = evt, HandlerName = handlerName });
                    }
                    else if (attr.Value is VariableNode vn)
                    {
                        _eventBindings.Add(new EventBindingInfo { VarName = node.VarName, EventName = evt, HandlerName = vn.Name });
                    }
                }
                else
                {
                    string val = GetStaticStringValue(attr.Value) ?? NodeToJsString(attr.Value);
                    info.Properties[attr.Name] = val;
                }
            }
            _elementMap[node.VarName] = info;
            _elements.Add(info);
            if (!_declaredVars.Contains(node.VarName))
                _declaredVars.Add(node.VarName);
        }

        void VisitXmlElement(XmlElementNode node)
        {
            string tag = _htmlTagMap.TryGetValue(node.TagName, out string t) ? t : "div";
            string tempVar = $"__xml_{_elements.Count}";
            _topLevelJs.Add($"let {tempVar} = document.createElement('{tag}');");
            _topLevelJs.Add($"{tempVar}.id = '{tempVar}';");
            foreach (var attr in node.Attributes)
            {
                if (attr.Name.StartsWith("on"))
                {
                    string evt = attr.Name.Substring(2);
                    string htmlEv = _htmlEventMap.TryGetValue(evt.ToLower(), out string he) ? he : evt.ToLower();
                    if (attr.Value is FuncDefNode fn)
                    {
                        string handlerName = $"__handler_{_eventBindings.Count}";
                        _fnJs[handlerName] = FuncBodyToJs(handlerName, new List<string>(), fn.Body);
                        _fnParams[handlerName] = new List<string>();
                        _topLevelJs.Add($"{tempVar}.on{htmlEv} = {handlerName};");
                    }
                    else if (attr.Value is VariableNode vn)
                    {
                        _topLevelJs.Add($"{tempVar}.on{htmlEv} = {vn.Name};");
                    }
                }
                else
                {
                    string jsVal = NodeToJsString(attr.Value);
                    string jsProp = MemberToJsProperty(attr.Name);
                    if (jsProp.StartsWith("style.") && attr.Value is NumberNode)
                        jsVal += " + 'px'";
                    _topLevelJs.Add($"{tempVar}.{jsProp} = {jsVal};");
                }
            }
        }

        void VisitMemberSet(MemberSetNode node)
        {
            string varName = GetVarName(node.Object);
            if (varName == null) return;

            // Resolve variable alias (e.g., btnPlay -> __xaml_3) for element lookup
            string elemName = varName;
            if (_topLevelVars.TryGetValue(elemName, out string resolved))
                elemName = resolved;

            string member = node.Member;

            // Check if it's an event binding: btn.Click = handlerName()
            if (IsEventBinding(node, out string eventHandler))
            {
                if (_conditionalDepth > 0)
                {
                    string htmlEv = _htmlEventMap.TryGetValue(member.ToLower(), out string he) ? he : member.ToLower();
                    _topLevelJs.Add($"{varName}.on{htmlEv} = {eventHandler};");
                }
                else
                {
                    _eventBindings.Add(new EventBindingInfo { VarName = elemName, EventName = member, HandlerName = eventHandler });
                }
                return;
            }

            if (_elementMap.TryGetValue(elemName, out var info))
            {
                if (_inFunctionBody || _conditionalDepth > 0)
                {
                    string jsProp = MemberToJsProperty(member);
                    string jsVal = NodeToJsString(node.Value);
                    if (jsProp.StartsWith("style.") && node.Value is NumberNode)
                        jsVal += " + 'px'";
                    _topLevelJs.Add($"{NodeToJsString(node.Object)}.{jsProp} = {jsVal};");
                }
                else
                {
                    string val = GetStaticStringValue(node.Value) ?? NodeToJsString(node.Value);
                    info.Properties[member] = val;
                }
            }
            else if (_inFunctionBody || _conditionalDepth > 0)
            {
                string jsProp = MemberToJsProperty(member);
                string jsVal = NodeToJsString(node.Value);
                if (jsProp.StartsWith("style.") && node.Value is NumberNode)
                    jsVal += " + 'px'";
                _topLevelJs.Add($"{NodeToJsString(node.Object)}.{jsProp} = {jsVal};");
            }
        }

        bool IsEventBinding(MemberSetNode node, out string handlerName)
        {
            handlerName = null;
            string member = node.Member;
            string lowerMember = member.ToLower();
            // Handle inline arrow function: btn.Click = () => { ... }
            if (node.Value is FuncDefNode arrowFn)
            {
                handlerName = $"__xaml_ev_{_eventBindings.Count}";
                _fnJs[handlerName] = FuncBodyToJs(handlerName, arrowFn.Parameters, arrowFn.Body);
                _fnParams[handlerName] = arrowFn.Parameters;
                return true;
            }
            // Only treat as event if the member is a known HTML event, or if value is a function call
            if (_htmlEventMap.ContainsKey(lowerMember))
            {
                if (node.Value is FuncCallNode fcn)
                {
                    handlerName = fcn.Name;
                    return true;
                }
                if (node.Value is VariableNode vn)
                {
                    handlerName = vn.Name;
                    return true;
                }
            }
            // For uppercase members not in event map, only treat as event if value is a function call
            if (member.Length > 0 && char.IsUpper(member[0]) && node.Value is FuncCallNode fcn2)
            {
                handlerName = fcn2.Name;
                return true;
            }
            return false;
        }

        void VisitFuncCall(FuncCallNode node)
        {
            if (node.Name.Equals("add", StringComparison.OrdinalIgnoreCase))
                return;
            if (node.Name.Equals("timer", StringComparison.OrdinalIgnoreCase) || node.Name.Equals("مؤقت", StringComparison.OrdinalIgnoreCase))
            {
                string ms = NodeToJsString(node.Arguments[0]);
                string callback;
                if (node.Arguments[1] is FuncDefNode fn)
                {
                    string fnName = $"__timer_cb_{_timerCounter++}";
                    _fnParams[fnName] = fn.Parameters;
                    _fnJs[fnName] = FuncBodyToJs(fnName, fn.Parameters, fn.Body);
                    callback = fnName;
                }
                else
                {
                    callback = NodeToJsString(node.Arguments[1]);
                }
                _topLevelJs.Add($"setInterval(function() {{ {callback}(); }}, {ms});");
                return;
            }
            if (_inFunctionBody || _conditionalDepth > 0)
            {
                string args = string.Join(", ", node.Arguments.Select(a => NodeToJsString(a)));
                _topLevelJs.Add($"{node.Name}({args});");
            }
        }

        void VisitFuncCall(MethodCallNode node)
        {
            string objName = GetVarName(node.Object);
            if (objName == null) return;

            if (node.Method.Equals("add", StringComparison.OrdinalIgnoreCase) && node.Arguments.Count > 0)
            {
                string childName = GetVarName(node.Arguments[0]);
                if (childName != null)
                {
                    if (_elementMap.TryGetValue(childName, out var childInfo))
                    {
                        if (_inFunctionBody || _conditionalDepth > 0)
                        {
                            _topLevelJs.Add($"{objName}.appendChild({childName});");
                        }
                        else
                        {
                            childInfo.ParentVar = objName;
                        }
                    }
                    else
                    {
                        // Child is a runtime DOM element (returned from component function)
                        // Resolve alias if needed
                        string resolvedChild = childName;
                        if (_topLevelVars.TryGetValue(resolvedChild, out string resolvedAlias))
                            resolvedChild = resolvedAlias;
                        if (_inFunctionBody || _conditionalDepth > 0)
                            _topLevelJs.Add($"{objName}.appendChild({resolvedChild});");
                        else
                            _topLevelJs.Add($"{objName}.appendChild({resolvedChild});");
                    }
                }
            }
            else if (_inFunctionBody || _conditionalDepth > 0)
            {
                if (node.Method.Equals("stop", StringComparison.OrdinalIgnoreCase))
                {
                    string objStr = NodeToJsString(node.Object);
                    _topLevelJs.Add($"({objStr}.pause(), {objStr}.currentTime = 0);");
                }
                else
                {
                    string args = string.Join(", ", node.Arguments.Select(a => NodeToJsString(a)));
                    _topLevelJs.Add($"{NodeToJsString(node.Object)}.{node.Method}({args});");
                }
            }
        }

        void VisitFuncDef(FuncDefNode node)
        {
            _fnParams[node.Name] = node.Parameters;
            _fnJs[node.Name] = FuncBodyToJs(node.Name, node.Parameters, node.Body);
        }

        void VisitAssign(AssignNode node)
        {
            string jsVal = NodeToJsString(node.Value);
            if (_inFunctionBody || _conditionalDepth > 0)
            {
                string decl = node.IsLet ? "let " : "";
                _topLevelJs.Add($"{decl}{node.Name} = {jsVal};");
            }
            else if (!_declaredVars.Contains(node.Name))
            {
                _declaredVars.Add(node.Name);
                _topLevelVars[node.Name] = jsVal;
            }
            else
            {
                _topLevelJs.Add($"{node.Name} = {jsVal};");
            }
        }

        void PostProcessElements()
        {
            var imgWrappers = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var elem in _elements.ToList())
            {
                bool hasChildren = _elements.Any(c => c.ParentVar == elem.VarName);
                if (hasChildren && (elem.HtmlTag == "img"))
                {
                    elem.HtmlTag = "div";
                    elem.ClassName = "ImageWrapper";
                    imgWrappers.Add(elem.VarName);
                    if (elem.Properties.TryGetValue("source", out string src))
                    {
                        elem.Properties.Remove("source");
                        elem.Properties["__bgImage"] = src;
                    }
                }
            }
            // Fix up static prop bindings for image wrappers
            for (int i = 0; i < _staticPropBindings.Count; i++)
            {
                var (varName, prop, valName) = _staticPropBindings[i];
                if (_elementMap.TryGetValue(varName, out var info) && info.ClassName == "ImageWrapper")
                {
                    string lowerProp = prop.ToLower();
                    if (lowerProp == "source" || lowerProp == "src")
                        _staticPropBindings[i] = (varName, "__bgImage", valName);
                }
            }
            // Fix up dynamic JS for image wrappers (already generated in _topLevelJs)
            for (int i = 0; i < _topLevelJs.Count; i++)
            {
                string stmt = _topLevelJs[i];
                foreach (var vn in imgWrappers)
                {
                    string pattern = $"{vn}.src = ";
                    if (stmt.Contains(pattern))
                    {
                        string valName = stmt.Substring(stmt.IndexOf(pattern) + pattern.Length).TrimEnd(';');
                        _topLevelJs[i] = $"{vn}.style.backgroundImage = 'url(' + {valName} + ')';";
                    }
                }
            }
        }

        string ProcessConditionalBody(Node body)
        {
            var saved = _topLevelJs;
            _topLevelJs = new List<string>();
            _conditionalDepth++;
            VisitStatement(body);
            _conditionalDepth--;
            string result = string.Join("\n", _topLevelJs.Select(l => "    " + l));
            _topLevelJs = saved;
            return result;
        }

        void VisitIf(IfNode node)
        {
            string cond = NodeToJsString(node.Condition);
            string body = ProcessConditionalBody(node.Body);
            string elseBody = node.ElseBody != null ? ProcessConditionalBody(node.ElseBody) : null;
            if (elseBody != null)
                _topLevelJs.Add($"if ({cond}) {{\n{body}\n}} else {{\n{elseBody}\n}}");
            else
                _topLevelJs.Add($"if ({cond}) {{\n{body}\n}}");
        }

        void VisitWhile(WhileNode node)
        {
            string cond = NodeToJsString(node.Condition);
            string body = ProcessConditionalBody(node.Body);
            _topLevelJs.Add($"while ({cond}) {{\n{body}\n}}");
        }

        void VisitFor(ForNode node)
        {
            string init = NodeToJsString(node.Init);
            string cond = NodeToJsString(node.Condition);
            string inc = NodeToJsString(node.Increment);
            string body = ProcessConditionalBody(node.Body);
            _topLevelJs.Add($"for ({init}; {cond}; {inc}) {{\n{body}\n}}");
        }

        void VisitForIn(ForInNode node)
        {
            string iterable = NodeToJsString(node.Iterable);
            string body = ProcessConditionalBody(node.Body);
            _topLevelJs.Add($"for (let {node.VarName} of {iterable}) {{\n{body}\n}}");
        }

        void VisitReturn(ReturnNode node)
        {
            string val = NodeToJsString(node.Value);
            _topLevelJs.Add($"return {val};");
        }

        void VisitPrint(PrintNode node)
        {
            string val = NodeToJsString(node.Value);
            _topLevelJs.Add($"__print({val});");
        }

        string FuncBodyToJs(string name, List<string> parameters, Node body)
        {
            var saved = _topLevelJs;
            _topLevelJs = new List<string>();
            bool savedInFunc = _inFunctionBody;
            _inFunctionBody = true;
            VisitStatement(body);
            _inFunctionBody = savedInFunc;
            string bodyJs = string.Join("\n", _topLevelJs.Select(l => "    " + l));
            _topLevelJs = saved;
            string p = string.Join(", ", parameters);
            return $"function {name}({p}) {{\n{bodyJs}\n}}";
        }

        string BlockToJsString(Node node)
        {
            if (node is BlockNode blk)
            {
                var parts = new List<string>();
                foreach (var stmt in blk.Statements)
                    parts.Add(StatementToJsString(stmt));
                return "\n" + string.Join("\n", parts.Select(l => "  " + l)) + "\n";
            }
            return "\n  " + StatementToJsString(node) + "\n";
        }

        string StatementToJsString(Node node)
        {
            switch (node)
            {
                case AssignNode an:
                    string val = NodeToJsString(an.Value);
                    return $"{an.Name} = {val};";
                case MemberSetNode ms:
                    string objS = NodeToJsString(ms.Object);
                    string v = NodeToJsString(ms.Value);
                    return $"{objS}.{ms.Member} = {v};";
                case FuncCallNode fc:
                    string args = string.Join(", ", fc.Arguments.Select(a => NodeToJsString(a)));
                    return $"{fc.Name}({args});";
                case MethodCallNode mc:
                    string mobj = NodeToJsString(mc.Object);
                    string margs = string.Join(", ", mc.Arguments.Select(a => NodeToJsString(a)));
                    if (mc.Method.Equals("stop", StringComparison.OrdinalIgnoreCase))
                        return $"({mobj}.pause(), {mobj}.currentTime = 0);";
                    return $"{mobj}.{mc.Method}({margs});";
                case IfNode ifn:
                    string cond = NodeToJsString(ifn.Condition);
                    string body = BlockToJsString(ifn.Body);
                    string elseB = ifn.ElseBody != null ? BlockToJsString(ifn.ElseBody) : null;
                    return elseB != null ? $"if ({cond}) {{{body}}} else {{{elseB}}}" : $"if ({cond}) {{{body}}}";
                case WhileNode wn:
                    string wcond = NodeToJsString(wn.Condition);
                    string wbody = BlockToJsString(wn.Body);
                    return $"while ({wcond}) {{{wbody}}}";
                case ForNode fn:
                    string finit = NodeToJsString(fn.Init);
                    string fcond = NodeToJsString(fn.Condition);
                    string finc = NodeToJsString(fn.Increment);
                    string fbody = BlockToJsString(fn.Body);
                    return $"for ({finit}; {fcond}; {finc}) {{{fbody}}}";
                case ReturnNode rn:
                    string rval = NodeToJsString(rn.Value);
                    return $"return {rval};";
                case PrintNode pn:
                    string pval = NodeToJsString(pn.Value);
                    return $"__print({pval});";
                case MemberGetNode mg:
                    string mgo = NodeToJsString(mg.Object);
                    string mgm = mg.Member.ToLower();
                    if (mgm == "stop")
                        return $"({mgo}.pause(), {mgo}.currentTime = 0);";
                    if (mgm == "play" || mgm == "pause" || mgm == "load")
                        return $"{mgo}.{mgm}();";
                    return $"{mgo}.{mgm};";
                case VariableNode vn:
                    return $"{vn.Name};";
                case BreakNode:
                    return "break;";
                case ContinueNode:
                    return "continue;";
                case ThrowNode tn:
                    return $"throw {NodeToJsString(tn.Value)};";
                case BlockNode blk:
                    return "{\n" + string.Join("\n", blk.Statements.Select(s => "  " + StatementToJsString(s))) + "\n}";
                default:
                    return NodeToJsString(node) + ";";
            }
        }

        string NodeToJsString(Node node)
        {
            if (node == null) return "null";
            switch (node)
            {
                case NumberNode nn:
                    return nn.Value.ToString(System.Globalization.CultureInfo.InvariantCulture);
                case StringNode sn:
                    return "\"" + EscapeJsStr(sn.Value) + "\"";
                case BoolNode bn:
                    return bn.Value ? "true" : "false";
                case NilNode niln:
                    return "null";
                case VariableNode vn:
                    return vn.Name;
                case SelfNode sn:
                    return "this";
                case AddNode ad:
                    return $"({NodeToJsString(ad.Left)} + {NodeToJsString(ad.Right)})";
                case SubtractNode sb:
                    return $"({NodeToJsString(sb.Left)} - {NodeToJsString(sb.Right)})";
                case MultiplyNode ml:
                    return $"({NodeToJsString(ml.Left)} * {NodeToJsString(ml.Right)})";
                case DivideNode dv:
                    return $"({NodeToJsString(dv.Left)} / {NodeToJsString(dv.Right)})";
                case ModuloNode md:
                    return $"({NodeToJsString(md.Left)} % {NodeToJsString(md.Right)})";
                case EqualNode eq:
                    return $"({NodeToJsString(eq.Left)} === {NodeToJsString(eq.Right)})";
                case NotEqualNode ne:
                    return $"({NodeToJsString(ne.Left)} !== {NodeToJsString(ne.Right)})";
                case GreaterNode gt:
                    return $"({NodeToJsString(gt.Left)} > {NodeToJsString(gt.Right)})";
                case LessNode ls:
                    return $"({NodeToJsString(ls.Left)} < {NodeToJsString(ls.Right)})";
                case GeNode ge:
                    return $"({NodeToJsString(ge.Left)} >= {NodeToJsString(ge.Right)})";
                case LeNode le:
                    return $"({NodeToJsString(le.Left)} <= {NodeToJsString(le.Right)})";
                case AndNode an:
                    return $"({NodeToJsString(an.Left)} && {NodeToJsString(an.Right)})";
                case OrNode or:
                    return $"({NodeToJsString(or.Left)} || {NodeToJsString(or.Right)})";
                case UnaryNode un:
                    if (un.Op == "NOT")
                        return $"(!{NodeToJsString(un.Operand)})";
                    return $"(-{NodeToJsString(un.Operand)})";
                case PostfixNode pn:
                    return $"({NodeToJsString(pn.Operand)}{pn.Op})";
                case FuncCallNode fc:
                    string args = string.Join(", ", fc.Arguments.Select(a => NodeToJsString(a)));
                    return $"{fc.Name}({args})";
                case MethodCallNode mc:
                    string mobj = NodeToJsString(mc.Object);
                    string margs = string.Join(", ", mc.Arguments.Select(a => NodeToJsString(a)));
                    return $"{mobj}.{mc.Method}({margs})";
                case MemberGetNode mg:
                    string obj = NodeToJsString(mg.Object);
                    string getterProp = MemberToJsGetter(mg.Object, mg.Member);
                    return $"{obj}.{getterProp}";
                case TernaryNode tn:
                    return $"({NodeToJsString(tn.Condition)} ? {NodeToJsString(tn.TrueExpr)} : {NodeToJsString(tn.FalseExpr)})";
                case ArrayLiteralNode al:
                    return "[" + string.Join(", ", al.Elements.Select(e => NodeToJsString(e))) + "]";
                case ObjectLiteralNode ol:
                    return "{" + string.Join(", ", ol.Fields.Select(f => $"{f.Key}: {NodeToJsString(f.Value)}")) + "}";
                case ArrayGetNode ag:
                    return $"{NodeToJsString(ag.Object)}[{NodeToJsString(ag.Index)}]";
                case PipeNode pn:
                    string left = NodeToJsString(pn.Left);
                    string right = NodeToJsString(pn.Right);
                    return $"{right}({left})";
                case NewNode nn:
                    string newArgs = string.Join(", ", nn.Arguments.Select(a => NodeToJsString(a)));
                    return $"new {nn.ClassName}({newArgs})";
                case NamedArgNode nan:
                    return NodeToJsString(nan.Value);
                case AssignNode an:
                    if (an.Value != null)
                        return $"{an.Name} = {NodeToJsString(an.Value)}";
                    return an.Name;
                case RegexNode rn:
                    return $"/{rn.Pattern}/{rn.Flags}";
                default:
                    return "null";
            }
        }

        static string GetStaticStringValue(Node node)
        {
            if (node is StringNode sn)
                return sn.Value;
            if (node is NumberNode nn)
                return nn.Value.ToString(System.Globalization.CultureInfo.InvariantCulture);
            if (node is BoolNode bn)
                return bn.Value ? "true" : "false";
            if (node is NilNode)
                return "";
            return null;
        }

        static string FormatCssValue(string val)
        {
            var parts = val.Split(' ');
            for (int i = 0; i < parts.Length; i++)
                if (int.TryParse(parts[i], System.Globalization.NumberStyles.Integer,
                    System.Globalization.CultureInfo.InvariantCulture, out _))
                    parts[i] = parts[i] + "px";
            return string.Join(" ", parts);
        }

        static string EscapeJsStr(string s)
        {
            return s.Replace("\\", "\\\\")
                    .Replace("\"", "\\\"")
                    .Replace("\n", "\\n")
                    .Replace("\r", "\\r")
                    .Replace("\t", "\\t");
        }

        static string GetVarName(Node node)
        {
            if (node is VariableNode vn) return vn.Name;
            return null;
        }

        string GenerateHtml(string outputName)
        {
            var html = new StringBuilder();
            string escapedName = outputName.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");
            html.AppendLine("<!DOCTYPE html>");
            html.AppendLine("<html lang=\"en\">");
            html.AppendLine("<head>");
            html.AppendLine("    <meta charset=\"UTF-8\">");
            html.AppendLine("    <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">");
            html.AppendLine($"    <title>{escapedName} - A+ App</title>");
            html.AppendLine("    <style>");
            html.AppendLine("        * { box-sizing: border-box; margin: 0; padding: 0; }");
            html.AppendLine("        body { display: flex; flex-direction: column; align-items: center; font-family: 'Segoe UI', system-ui, sans-serif; background: #f5f5f5; min-height: 100vh; }");
            html.AppendLine("        #app { width: 100%; display: flex; flex-direction: column; align-items: center; }");
            html.AppendLine("        .error { color: #ef4444; }");
            html.AppendLine("    </style>");
            html.AppendLine("</head>");
            html.AppendLine("<body>");
            html.AppendLine("    <div id=\"app\">");
            html.AppendLine(GenerateUiHtml("    "));
            html.AppendLine("    </div>");
            html.AppendLine("    <div id=\"output\" style=\"display:none\"></div>");
            html.AppendLine("    <script src=\"app.js\"></script>");
            html.AppendLine("</body>");
            html.AppendLine("</html>");
            return html.ToString();
        }

        string GenerateUiHtml(string indent)
        {
            // Build tree from parent references
            // __root is the implicit container — treat elements with no parent or parent __root as roots
            var roots = _elements.Where(e => e.ParentVar == null).ToList();
            if (roots.Count == 0 && _elements.Count > 0)
                roots.Add(_elements[0]);

            var sb = new StringBuilder();
            foreach (var root in roots)
                BuildElementHtml(root, indent, sb);
            return sb.ToString();
        }

        void BuildElementHtml(UiElementInfo elem, string indent, StringBuilder sb)
        {
            string id = elem.VarName;
            string tag = elem.HtmlTag;
            string cls = elem.ClassName.ToLower();

            // Special handling for input types
            if (elem.IsCheckable || elem.IsSlider || tag == "input" && cls == "textbox")
            {
                sb.Append(indent);
                sb.Append($"<{tag} id=\"{id}\"");
                if (elem.Properties.TryGetValue("_inputType", out string iType))
                    sb.Append($" type=\"{iType}\"");
                else if (tag == "input")
                    sb.Append(" type=\"text\"");
                sb.Append(GenerateAttributes(elem));
                if (elem.Properties.TryGetValue("checked", out string chk) && chk == "true")
                    sb.Append(" checked");
                string inputVal = null;
                if (elem.Properties.TryGetValue("text", out string tv))
                    inputVal = tv;
                else if (elem.Properties.TryGetValue("content", out string cv))
                    inputVal = cv;
                else if (elem.Properties.TryGetValue("value", out string vv))
                    inputVal = vv;
                if (inputVal != null)
                    sb.Append($" value=\"{EscapeHtmlAttr(inputVal)}\"");
                if (elem.Properties.TryGetValue("placeholder", out string ph))
                    sb.Append($" placeholder=\"{EscapeHtmlAttr(ph)}\"");
                if (elem.Properties.TryGetValue("source", out string src))
                    sb.Append($" src=\"{EscapeHtmlAttr(src)}\"");
                bool hasChildren = _elements.Any(c => c.ParentVar == elem.VarName);
                if (hasChildren)
                {
                    sb.AppendLine(">");
                    foreach (var child in _elements.Where(c => c.ParentVar == elem.VarName))
                        BuildElementHtml(child, indent + "    ", sb);
                    sb.AppendLine($"{indent}</{tag}>");
                }
                else
                {
                    sb.AppendLine(" />");
                }
                return;
            }

            // Image tag
            if (tag == "img")
            {
                sb.Append(indent);
                sb.Append($"<{tag} id=\"{id}\"");
                string imgSrc = null;
                if (elem.Properties.TryGetValue("source", out imgSrc) || elem.Properties.TryGetValue("src", out imgSrc))
                    sb.Append($" src=\"{EscapeHtmlAttr(imgSrc)}\"");
                sb.Append(GenerateAttributes(elem));
                sb.AppendLine(" />");
                return;
            }

            // Progress
            if (tag == "progress")
            {
                sb.Append(indent);
                sb.Append($"<{tag} id=\"{id}\"");
                if (elem.Properties.TryGetValue("value", out string val))
                    sb.Append($" value=\"{EscapeHtmlAttr(val)}\"");
                if (elem.Properties.TryGetValue("maximum", out string max))
                    sb.Append($" max=\"{EscapeHtmlAttr(max)}\"");
                sb.AppendLine("></progress>");
                return;
            }

            // Video/MediaElement
            if (tag == "video")
            {
                sb.Append(indent);
                sb.Append($"<{tag} id=\"{id}\"");
                string videoSrc = null;
                if (elem.Properties.TryGetValue("source", out videoSrc) || elem.Properties.TryGetValue("src", out videoSrc))
                    sb.Append($" src=\"{EscapeHtmlAttr(videoSrc)}\"");
                sb.Append(GenerateAttributes(elem));
                sb.AppendLine($"></{tag}>");
                return;
            }

            // Default: div/span/button
            sb.Append(indent);
            sb.Append($"<{tag} id=\"{id}\"");
            sb.Append(GenerateAttributes(elem));

            // Content
            string content = null;
            if (elem.Properties.TryGetValue("text", out string t))
                content = t;
            else if (elem.Properties.TryGetValue("content", out string c))
                content = c;

            bool hasKids = _elements.Any(c => c.ParentVar == elem.VarName);

            if (content != null && !hasKids && tag != "input")
            {
                sb.AppendLine($">{EscapeHtml(content)}</{tag}>");
            }
            else if (hasKids)
            {
                sb.AppendLine(">");
                foreach (var child in _elements.Where(c => c.ParentVar == elem.VarName))
                    BuildElementHtml(child, indent + "    ", sb);
                sb.AppendLine($"{indent}</{tag}>");
            }
            else
            {
                sb.AppendLine($"></{tag}>");
            }
        }

        string GenerateAttributes(UiElementInfo elem)
        {
            var sb = new StringBuilder();
            // Style attributes
            var styles = new List<string>();
            foreach (var kv in elem.Properties)
            {
                string key = kv.Key;
                string val = kv.Value;
                if (key == "_inputType" || key == "_attrValue" || key == "_attrMin" || key == "_attrMax"
                    || key == "_attrId" || key == "_attrName" || key == "_attrDisabled"
                    || key == "_styleDisplay" || key == "_className") continue;

                if (key.Equals("width", StringComparison.OrdinalIgnoreCase))
                    styles.Add($"width: {val}px");
                else if (key.Equals("height", StringComparison.OrdinalIgnoreCase))
                    styles.Add($"height: {val}px");
                else if (key.Equals("fontsize", StringComparison.OrdinalIgnoreCase))
                    styles.Add($"font-size: {val}px");
                else if (key.Equals("fontfamily", StringComparison.OrdinalIgnoreCase))
                    styles.Add($"font-family: {val}");
                else if (key.Equals("foreground", StringComparison.OrdinalIgnoreCase))
                    styles.Add($"color: {val}");
                else if (key.Equals("background", StringComparison.OrdinalIgnoreCase))
                    styles.Add($"background-color: {val}");
                else if (key.Equals("horizontalalignment", StringComparison.OrdinalIgnoreCase))
                    styles.Add($"text-align: {val.ToLower()}");
                else if (key == "__bgImage")
                    styles.Add($"background-image: url({val})");
                else if (key.Equals("margin", StringComparison.OrdinalIgnoreCase))
                    styles.Add($"margin: {FormatCssValue(val)}");
                else if (key.Equals("padding", StringComparison.OrdinalIgnoreCase))
                    styles.Add($"padding: {FormatCssValue(val)}");
                else if (key.Equals("opacity", StringComparison.OrdinalIgnoreCase))
                    styles.Add($"opacity: {val}");
            }
            // Add flex display for container elements
            string cls = elem.ClassName?.ToLower();
            if (cls == "stackpanel" || cls == "grid" || cls == "wrappanel" || cls == "dockpanel" ||
                cls == "canvas" || cls == "uniformgrid" || cls == "scrollviewer" || cls == "border" || cls == "menu")
            {
                styles.Insert(0, "display: flex");
                string dir = "column";
                if (elem.Properties.TryGetValue("orientation", out string orient) && orient.ToLower() == "horizontal")
                    dir = "row";
                styles.Insert(1, $"flex-direction: {dir}");
            }

            if (styles.Count > 0)
                sb.Append($" style=\"{string.Join("; ", styles)}\"");

            // Event handlers
            foreach (var ev in _eventBindings.Where(e => e.VarName == elem.VarName))
            {
                string htmlEv = _htmlEventMap.TryGetValue(ev.EventName.ToLower(), out string he) ? he : ev.EventName.ToLower();
                sb.Append($" on{htmlEv}=\"{ev.HandlerName}(event)\"");
            }

            if (elem.ClassName != null && !elem.ClassName.Equals("div", StringComparison.OrdinalIgnoreCase))
                sb.Append($" data-aplus-class=\"{elem.ClassName}\"");

            return sb.ToString();
        }

        string GenerateAppJs()
        {
            var js = new StringBuilder();

            // Print function
            js.AppendLine("function __print(msg) {");
            js.AppendLine("    var out = document.getElementById('output');");
            js.AppendLine("    if (out) out.style.display = 'block';");
            js.AppendLine("    var div = document.createElement('div');");
            js.AppendLine("    div.textContent = String(msg);");
            js.AppendLine("    out.appendChild(div);");
            js.AppendLine("}");
            js.AppendLine();

            // Top-level variables
            foreach (var kv in _topLevelVars)
                js.AppendLine($"let {kv.Key} = {kv.Value};");
            js.AppendLine();

            // Declare UI element variables as let (they'll be initialized by DOM queries)
            foreach (var elem in _elements)
            {
                if (!_topLevelVars.ContainsKey(elem.VarName))
                    js.AppendLine($"let {elem.VarName};");
            }
            js.AppendLine();

            // Init function to get DOM references and apply static property bindings
            js.AppendLine("function __init() {");
            foreach (var elem in _elements)
                js.AppendLine($"    {elem.VarName} = document.getElementById('{elem.VarName}');");
            foreach (var (varName, property, valueName) in _staticPropBindings)
            {
                string jsProp = MemberToJsProperty(property);
                string jsVal = valueName;
                if (property == "__bgImage")
                    jsVal = $"'url(' + {valueName} + ')'";
                js.AppendLine($"    {varName}.{jsProp} = {jsVal};");
            }
            js.AppendLine("}");
            js.AppendLine();

            // Transpiled functions
            foreach (var kv in _fnJs)
                js.AppendLine(kv.Value + "\n");

            // Init and run
            js.AppendLine("// Initialize");
            js.AppendLine("document.addEventListener('DOMContentLoaded', function() {");
            js.AppendLine("    __init();");
            // Top-level statements (executed after DOM is ready)
            if (_topLevelJs.Count > 0)
            {
                js.AppendLine("    // Top-level logic");
                foreach (var stmt in _topLevelJs)
                    js.AppendLine("    " + stmt);
            }
            js.AppendLine("    // Print app name");
            js.AppendLine($"    __print('=== {EscapeJsStr(_appName)} ===');");
            js.AppendLine("});");

            return js.ToString();
        }

        string GenerateRuntimeJs()
        {
            // Minimal runtime for any A+ features not transpiled
            return "// Minimal A+ Web Runtime\n"
                + "// All code is transpiled to native JS in app.js\n"
                + "// This file exists for compatibility with existing tooling.\n";
        }

        static string EscapeHtml(string s)
        {
            if (s == null) return "";
            return s.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");
        }

        static string EscapeHtmlAttr(string s)
        {
            if (s == null) return "";
            return s.Replace("&", "&amp;").Replace("\"", "&quot;").Replace("<", "&lt;").Replace(">", "&gt;");
        }
    }
}
