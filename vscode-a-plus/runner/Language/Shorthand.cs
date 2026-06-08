using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace A_
{
    static class Shorthand
    {
        private static int _xamlCounter = 0;
        private static int _xamlEvCounter = 0;

        public static string Convert(string code)
        {
            _xamlCounter = 0;
            _xamlEvCounter = 0;
            code = ConvertAPlusXml(code);
            code = ConvertAPlusNewInline(code);
            code = ConvertAPlusNoNew(code);
            code = ConvertXamlEvents(code);
            code = ConvertXaml(code);
            code = ConvertArabicShort(code);
            code = ConvertPrint(code);
            code = ConvertIfElseBraces(code);
            code = ConvertIfElseCompact(code);
            code = ConvertIf(code);
            code = ConvertWhile(code);
            code = ConvertIfParenLess(code);
            code = ConvertWhileParenLess(code);
            code = ConvertFor(code);
            code = ConvertIncrementDecrement(code);
            code = Regex.Replace(code, @"<!--.*?-->", "", RegexOptions.Singleline);
            code = Regex.Replace(code, @"</\w+\s*>", "");
            code = code.Replace(@"<---", "");
            return code;
        }

        static Dictionary<string, string> _arabicTags = new OverwriteDictionary<string, string>
        {
            {"شبكة", "Grid"}, {"لوحة_مكدسة", "StackPanel"}, {"مكدسة", "StackPanel"},
            {"لوحة_ملتفة", "WrapPanel"}, {"ملتفة", "WrapPanel"},
            {"لوحة_إرساء", "DockPanel"}, {"إرساء", "DockPanel"},
            {"لوحة_رسم", "Canvas"}, {"رسم", "Canvas"},
            {"شبكة_موحدة", "UniformGrid"}, {"صندوق_عرض", "Viewbox"},
            {"حدود", "Border"}, {"متصفح_تمرير", "ScrollViewer"}, {"تمرير", "ScrollViewer"},
            {"نص", "TextBlock"}, {"صندوق_نص", "TextBox"}, {"صندوق_نص_غني", "RichTextBox"},
            {"تسمية", "Label"}, {"نص_غني", "RichTextBox"},
            {"زر", "Button"}, {"زر_تكرار", "RepeatButton"}, {"زر_تبديل", "ToggleButton"},
            {"مربع_اختيار", "CheckBox"}, {"زر_خيار", "RadioButton"},
            {"صندوق_قائمة", "ListBox"}, {"عرض_قائمة", "ListView"},
            {"صندوق_مدمج", "ComboBox"}, {"عرض_شجري", "TreeView"},
            {"قائمة", "Menu"}, {"عنصر_قائمة", "MenuItem"},
            {"قائمة_سياق", "ContextMenu"}, {"تحكم_تبويب", "TabControl"}, {"تبويب", "TabItem"},
            {"صورة", "Image"}, {"img", "Image"},
            {"عنصر_وسائط", "MediaElement"}, {"وسائط", "MediaElement"},
            {"منظور3d", "Viewport3D"},             {"وسائط_فيديو", "MediaElement"}, {"وسائط_صوت", "MediaElement"},
            {"فيديو", "Video"}, {"صوت", "Sound"},
            {"منزلق", "Slider"}, {"شريط_تقدم", "ProgressBar"},
            {"منتقي_تاريخ", "DatePicker"}, {"تقويم", "Calendar"},
            {"صندوق_كلمة_سر", "PasswordBox"},
            {"مستطيل", "Rectangle"}, {"قطع_ناقص", "Ellipse"}, {"ناقص", "Ellipse"},
            {"خط", "Line"}, {"مضلع", "Polygon"}, {"خط_متعدد", "Polyline"}, {"مسار", "Path"},
            {"نافذة", "Window"},
        };

        static Dictionary<string, string> _arabicAttrs = new OverwriteDictionary<string, string>
        {
            {"المحتوى", "Content"}, {"النص", "Text"}, {"العرض", "Width"},
            {"الارتفاع", "Height"}, {"الهامش", "Margin"}, {"الحشوة", "Padding"},
            {"نقر", "Click"}, {"تحميل", "Loaded"}, {"المصدر", "Source"},
            {"القيمة", "Value"}, {"الحد_الأدنى", "Minimum"}, {"الحد_الأقصى", "Maximum"},
            {"الخلفية", "Background"}, {"الأمامية", "Foreground"},
            {"محدد", "IsChecked"}, {"مفعل", "IsEnabled"}, {"رؤية", "Visibility"},
            {"الاسم", "Name"}, {"المعرف", "Name"},
            {"دخول_فأرة", "MouseEnter"}, {"خروج_فأرة", "MouseLeave"},
            {"ضغط_فأرة", "MouseDown"}, {"رفع_فأرة", "MouseUp"}, {"تحريك_فأرة", "MouseMove"},
            {"ضغط_مفتاح", "KeyDown"}, {"رفع_مفتاح", "KeyUp"},
            {"اكتسب_تركيز", "GotFocus"}, {"فقد_تركيز", "LostFocus"},
            {"تحديث_تخطيط", "LayoutUpdated"},
            {"تغير_نص", "TextChanged"}, {"تغير_اختيار", "SelectionChanged"},
            {"تغير_قيمة", "ValueChanged"}, {"تغير_حجم", "SizeChanged"},
            {"تم_تحديد", "Checked"}, {"تم_إلغاء", "Unchecked"}, {"غير_محدد", "Indeterminate"},
            {"إفلات", "Drop"}, {"سحب_فوق", "DragOver"},
            {"src", "Source"},
            {"أمر", "Command"}, {"وسيط_أمر", "CommandParameter"},
            {"مصدر", "Source"}, {"توجيه", "Orientation"},
            {"زخرفة", "TextDecoration"}, {"محاذاة", "HorizontalAlignment"},
            {"محاذاة_عمودية", "VerticalAlignment"}, {"حجم_الخط", "FontSize"},
            {"نوع_الخط", "FontFamily"}, {"غامق", "FontWeight"},
            {"مائل", "FontStyle"}, {"لون", "Foreground"},
            {"لون_الخلفية", "Background"}, {"سمك_الحدود", "BorderThickness"},
            {"لون_الحدود", "BorderBrush"}, {"تعبئة", "Fill"}, {"حد", "Stroke"},
        };

        static string TranslateArabicXaml(string code)
        {
            foreach (var kv in _arabicTags)
            {
                code = code.Replace("<" + kv.Key + " ", "<" + kv.Value + " ");
                code = code.Replace("<" + kv.Key + ">", "<" + kv.Value + ">");
                code = code.Replace("</" + kv.Key + ">", "</" + kv.Value + ">");
                code = code.Replace("<" + kv.Key + "/>", "<" + kv.Value + "/>");
                code = code.Replace("<" + kv.Key + "\n", "<" + kv.Value + "\n");
            }
            foreach (var kv in _arabicAttrs)
                code = Regex.Replace(code, "(" + kv.Key + ")\\s*=", kv.Value + "=");
            return code;
        }

        static HashSet<string> _knownUiTypes = new HashSet<string>
        {
            "grid", "stackpanel", "wrappanel", "dockpanel", "canvas", "uniformgrid",
            "viewbox", "border", "scrollviewer",
            "textblock", "textbox", "richtextbox", "label",
            "button", "repeatbutton", "togglebutton", "checkbox", "radiobutton",
            "listbox", "listview", "combobox", "treeview", "menu", "menuitem",
            "contextmenu", "tabcontrol", "tabitem",
            "image", "img", "video", "sound", "mediaelement", "viewport3d",
            "slider", "progressbar", "datepicker", "calendar", "passwordbox",
            "rectangle", "ellipse", "line", "polygon", "polyline", "path",
            "window",
        };

        static HashSet<string> _propAttrs = new HashSet<string>
        {
            "id", "name", "content", "text", "width", "height",
            "margin", "padding", "fontsize", "fontfamily", "foreground", "background",
            "horizontalalignment", "verticalalignment", "source", "src", "autoplay", "controls", "loop", "value", "minimum", "maximum",
            "ischecked", "isselected", "isenabled", "visibility", "opacity",
        };

        static bool IsKnownUiType(string name) => _knownUiTypes.Contains(name.ToLower());
        static bool IsPropAttr(string name) => _propAttrs.Contains(name.ToLower());

        static bool IsIdentifier(string s)
        {
            if (string.IsNullOrEmpty(s)) return false;
            if (!char.IsLetter(s[0]) && s[0] != '_') return false;
            for (int i = 1; i < s.Length; i++)
                if (!char.IsLetterOrDigit(s[i]) && s[i] != '_') return false;
            return true;
        }

        static bool IsXamlNamespace(XAttribute attr) =>
            attr.Name.NamespaceName == "http://schemas.microsoft.com/winfx/2006/xaml";

        static string EscapeStr(string s) =>
            s.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", "\\r");

        static string ConvertAPlusXml(string code)
        {
            // Form 1: a+ xml varname = <component attrs>content</component>
            // Uses manual scan with depth tracking to extract the full XML block
            {
                var sb = new StringBuilder();
                int pos = 0;
                while (pos < code.Length)
                {
                    var m = Regex.Match(code.Substring(pos), @"^a\+\s+(?:xml|new\s+dev)\s+(\w+)\s*=\s*<(\w+)([\s>][^>]*)?>");
                    if (!m.Success) { sb.Append(code[pos]); pos++; continue; }
                    string varName = m.Groups[1].Value;
                    string tagName = m.Groups[2].Value;
                    int tagStart = pos + m.Value.IndexOf('<');  // start of actual XML
                    int afterOpen = pos + m.Length;
                    // Self-closing check — look back from the '>'
                    int gtPos = pos + m.Value.LastIndexOf('>');
                    if (gtPos > 0 && code[gtPos - 1] == '/')
                    {
                        string singleXml = code.Substring(tagStart, afterOpen - tagStart);
                        string xr = ConvertXaml(singleXml);
                        var vm = Regex.Match(xr, @"__xaml_(\d+)");
                        string xv = vm.Success ? $"__xaml_{vm.Groups[1].Value}" : "__xaml_0";
                        sb.Append(xr);
                        sb.AppendLine($"let {varName} = {xv}");
                        pos = afterOpen;
                        continue;
                    }
                    // Depth-track to matching </tagName> (case-insensitive)
                    int depth = 1, j = afterOpen;
                    bool inStr = false; char sq = '"';
                    while (j < code.Length && depth > 0)
                    {
                        char c = code[j];
                        if (inStr) { if (c == sq) inStr = false; j++; continue; }
                        if (c == '"' || c == '\'') { inStr = true; sq = c; j++; continue; }
                        if (c == '<')
                        {
                            if (j + 3 < code.Length && code[j + 1] == '!' && code[j + 2] == '-' && code[j + 3] == '-')
                            {
                                int end = code.IndexOf("-->", j + 4);
                                if (end >= 0) j = end + 3; else j++;
                            }
                            else if (j + 1 < code.Length && code[j + 1] == '/')
                            {
                                depth--;
                                while (j < code.Length && code[j] != '>') j++;
                                j++;
                            }
                            else if (j + 1 < code.Length && char.IsLetter(code[j + 1]))
                            {
                                int te = code.IndexOf('>', j + 1);
                                if (te > j && te - 1 > j && code[te - 1] == '/')
                                    { /* self-closing, depth unchanged */ }
                                else
                                    depth++;
                                while (j < code.Length && code[j] != '>') j++;
                                j++;
                            }
                            else j++;
                        }
                        else j++;
                    }
                    string fullXml = code.Substring(tagStart, j - tagStart);
                    string xamlResult = ConvertXaml(fullXml);
                    var firstVar = Regex.Match(xamlResult, @"__xaml_(\d+)");
                    string outerVar = firstVar.Success ? $"__xaml_{firstVar.Groups[1].Value}" : "__xaml_0";
                    sb.Append(xamlResult);
                    sb.AppendLine($"let {varName} = {outerVar}");
                    pos = j;
                }
                code = sb.ToString();
            }

            // Form 2: a+ xml varname = { <xml inline> } — manual extraction with brace depth
            {
                var sb2 = new StringBuilder();
                int i2 = 0;
                while (i2 < code.Length)
                {
                    var match = Regex.Match(code.Substring(i2), @"a\+\s+(?:xml|new\s+dev)\s+(\w+)\s*=\s*\{");
                    if (!match.Success || match.Index != 0) { sb2.Append(code[i2]); i2++; continue; }
                    string varName = match.Groups[1].Value;
                    int start = i2 + match.Length;
                    int depth = 1, j = start;
                    bool inStr2 = false; char strChar2 = '"';
                    while (j < code.Length && depth > 0)
                    {
                        char c2 = code[j];
                        if (inStr2) { if (c2 == strChar2) inStr2 = false; j++; continue; }
                        if (c2 == '"' || c2 == '\'') { inStr2 = true; strChar2 = c2; j++; continue; }
                        if (c2 == '{') depth++;
                        else if (c2 == '}') depth--;
                        if (depth > 0 || (depth == 0 && c2 != '}')) j++;
                        else break;
                    }
                    string xmlContent = code.Substring(start, j - start).Trim();
                    i2 = j + 1;
                    xmlContent = ConvertXamlEvents(xmlContent);
                    string xamlResult = ConvertXaml(xmlContent);
                    var firstVar = Regex.Match(xamlResult, @"__xaml_(\d+)");
                    string outerVar = firstVar.Success ? $"__xaml_{firstVar.Groups[1].Value}" : "__xaml_0";
                    sb2.Append(xamlResult);
                    sb2.AppendLine($"let {varName} = {outerVar}");
                }
                code = sb2.ToString();
            }
            return code;
        }

        static string ConvertXamlEvents(string code)
        {
            // Preprocess anonymous function event handlers: onclick=()=>{...}
            var sb = new StringBuilder();
            int i = 0;
            var evFunctions = new List<string>();
            while (i < code.Length)
            {
                var m = Regex.Match(code.Substring(i), @"(\w[\w.-]*)\s*=\s*\(\)\s*=>\s*\{");
                if (!m.Success || m.Index != 0) { sb.Append(code[i]); i++; continue; }
                string eventName = m.Groups[1].Value;
                string funcName = $"__xaml_ev_{_xamlEvCounter++}";
                int start = i + m.Length;
                int depth = 1, pos = start;
                bool inStr = false; char strChar = '"';
                while (pos < code.Length && depth > 0)
                {
                    char c = code[pos];
                    if (inStr) { if (c == strChar) inStr = false; pos++; continue; }
                    if (c == '"' || c == '\'') { inStr = true; strChar = c; pos++; continue; }
                    if (c == '{') depth++;
                    else if (c == '}') depth--;
                    if (depth > 0) pos++;
                }
                string body = code.Substring(start, pos - start);
                sb.Append($"{eventName}={funcName}");
                evFunctions.Add($"func {funcName}() {{ {body} }}");
                i = pos + 1;
            }
            if (evFunctions.Count > 0)
                code = string.Join("\n", evFunctions) + "\n" + sb.ToString();
            else
                code = sb.ToString();
            return code;
        }

        static bool IsInFuncBody(string code, int pos)
        {
            // Scan backward from pos to find if we're inside a function body
            int braceDepth = 0;
            bool inStr = false;
            char strChar = '"';
            for (int i = pos - 1; i >= 0; i--)
            {
                char c = code[i];
                if (inStr) { if (c == strChar) inStr = false; continue; }
                if (c == '"' || c == '\'') { inStr = true; strChar = c; continue; }
                if (c == '}') braceDepth--;
                else if (c == '{') braceDepth++;
                if (c == '{' && braceDepth > 0)
                {
                    // Scan further back for 'func' or 'دالة' keyword
                    int searchStart = i - 30;
                    if (searchStart < 0) searchStart = 0;
                    string before = code.Substring(searchStart, i - searchStart);
                    if (Regex.IsMatch(before, @"\b(func|دالة)\s+\w+\s*\("))
                        return true;
                }
            }
            return false;
        }

        public static string ConvertAxmlFile(string filePath)
        {
            string code = System.IO.File.ReadAllText(filePath);
            return Convert(code);
        }

        public static string ResolveIncludesForWeb(string code, string baseDir)
        {
            return Regex.Replace(code, @"include\s+""([^""]+)""(\s+from\s+""([^""]+)"")?", m =>
            {
                string path = m.Groups[1].Value;
                string from = m.Groups[3].Success ? m.Groups[3].Value : null;
                string fullPath;
                if (from != null)
                {
                    string prefix;
                    if (from.Length == 1 && char.IsLetter(from[0]))
                        prefix = from + ":\\";
                    else if (from.EndsWith(":"))
                        prefix = from + "\\";
                    else if (System.IO.Path.IsPathRooted(from))
                        prefix = from;
                    else
                        prefix = System.IO.Path.Combine(baseDir, from);
                    fullPath = System.IO.Path.Combine(prefix, path);
                }
                else
                    fullPath = System.IO.Path.Combine(baseDir, path);
                if (System.IO.File.Exists(fullPath))
                {
                    string included = System.IO.File.ReadAllText(fullPath);
                    return included;
                }
                foreach (var sp in new[] {
                    System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "stdlib", path),
                    System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "stdlib", path),
                    System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "stdlib", path),
                    System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "stdlib", path),
                })
                {
                    string p = System.IO.Path.GetFullPath(sp);
                    if (System.IO.File.Exists(p)) { return System.IO.File.ReadAllText(p); }
                }
                return m.Value;
            });
        }

        static string ConvertXaml(string code)
        {
            code = TranslateArabicXaml(code);
            var sb = new StringBuilder();
            int i = 0;
            while (i < code.Length)
            {
                if (code[i] == '<' && i + 1 < code.Length && char.IsLetter(code[i + 1]))
                {
                    int start = i;
                    bool inFunc = IsInFuncBody(code, i);
                    i = ExtractXamlElement(code, i);
                    string xamlBlock = code.Substring(start, i - start);
                    try
                    {
                        string fixedXaml = Regex.Replace(xamlBlock, @"<[^>]+>", m =>
                        {
                            string tag = m.Value;
                            tag = Regex.Replace(tag, @"([\w.-]+)\s*=\s*(\{Binding\s+[^}]+\})",
                                m2 => $"{m2.Groups[1].Value}=\"{m2.Groups[2].Value}\"");
                            tag = Regex.Replace(tag, @"(\w[\w.-]*)\s*=\s*([a-zA-Z_]\w*)(?=[\s>\/])",
                                m2 => $"{m2.Groups[1].Value}=\"__XAMLVAR__{m2.Groups[2].Value}\"");
                            tag = Regex.Replace(tag, @"(\w[\w.-]*)\s*=\s*([^""'>/\s]+)(?=[\s>\/])",
                                m2 => {
                                    string val = m2.Groups[2].Value;
                                    if (val.StartsWith("\"") || val.StartsWith("'"))
                                        return m2.Value;
                                    return $"{m2.Groups[1].Value}=\"{val}\"";
                                });
                            return tag;
                        });
                        string xhtml = "<Root xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\">" + fixedXaml + "</Root>";
                        var doc = XDocument.Parse(xhtml);
                        string aplus = ConvertXElement(doc.Root, null, inFunc);
                        sb.Append(aplus);
                    }
                    catch (Exception ex)
                    {
                        sb.AppendLine($"# XAML error: {ex.Message}");
                        sb.AppendLine(xamlBlock);
                    }
                }
                else
                {
                    sb.Append(code[i]);
                    i++;
                }
            }
            return sb.ToString();
        }

        static int ExtractXamlElement(string code, int start)
        {
            int depth = 0, i = start;
            bool inStr = false;
            char strChar = '"';
            while (i < code.Length)
            {
                char c = code[i];
                if (inStr) { if (c == strChar) inStr = false; i++; continue; }
                if (c == '"' || c == '\'') { inStr = true; strChar = c; i++; continue; }
                if (c == '<')
                {
                    if (i + 1 < code.Length && code[i + 1] == '/') { depth--; i = SkipToEndOfTag(code, i); if (depth == 0) break; }
                    else if (i + 1 < code.Length && char.IsLetter(code[i + 1])) { depth++; i = SkipToEndOfTag(code, i); if (IsSelfClosingTag(code, i)) { depth--; if (depth == 0) break; } }
                    else i++;
                }
                else i++;
            }
            return i;
        }

        static int SkipToEndOfTag(string code, int start)
        {
            bool inStr = false;
            char sq = '"';
            for (int i = start; i < code.Length; i++)
            {
                if (inStr) { if (code[i] == sq) inStr = false; continue; }
                if (code[i] == '"' || code[i] == '\'') { inStr = true; sq = code[i]; continue; }
                if (code[i] == '>') return i + 1;
            }
            return code.Length;
        }

        static bool IsSelfClosingTag(string code, int end)
        {
            for (int i = end - 2; i >= 0 && i > end - 6; i--)
                if (code[i] == '/') return true;
            return false;
        }

        static string ConvertXElement(XElement elem, string parentVar, bool inFunction = false)
        {
            if (elem.Name.LocalName == "Root")
            {
                var sb = new StringBuilder();
                foreach (var child in elem.Elements())
                    sb.Append(ConvertXElement(child, parentVar, inFunction));
                return sb.ToString();
            }
            string tag = elem.Name.LocalName;
            string varName = $"__xaml_{_xamlCounter++}";
            var result = new StringBuilder();

            bool hasBody = elem.HasElements;
            if (!hasBody && !string.IsNullOrWhiteSpace(elem.Value))
            {
                foreach (var attr in elem.Attributes())
                {
                    if (IsXamlNamespace(attr)) continue;
                    string an = attr.Name.LocalName;
                    string av = attr.Value;
                    string en = an.StartsWith("on") && an.Length > 2
                        ? char.ToUpper(an[2]) + an.Substring(3) : an;
                    if (!IsPropAttr(en) && !av.StartsWith("{") && !av.EndsWith("}")) { hasBody = true; break; }
                }
            }

            string xName = null;
            foreach (var attr in elem.Attributes())
            {
                string an = attr.Name.LocalName;
                if ((IsXamlNamespace(attr) && an == "Name") ||
                    (!IsXamlNamespace(attr) && (an == "Name" || an == "الاسم" || an == "المعرف")))
                    xName = attr.Value;
            }

            if (IsKnownUiType(tag))
            {
                result.AppendLine($"a+ new {tag} {varName}");
                foreach (var attr in elem.Attributes())
                {
                    if (IsXamlNamespace(attr)) continue;
                    string an = attr.Name.LocalName;
                    string av = attr.Value;
                    string en = an.StartsWith("on") && an.Length > 2
                        ? char.ToUpper(an[2]) + an.Substring(3) : an;
                    bool isEvent = av.StartsWith("{") && av.EndsWith("}") && !av.StartsWith("{Binding ");
                    if (av.StartsWith("{Binding ") && av.EndsWith("}"))
                        result.AppendLine($"{varName}.{en} => {av.Substring(9, av.Length - 10).Trim()}");
                    else if (av.StartsWith("__XAMLVAR__"))
                        result.AppendLine($"{varName}.{en} => {av.Substring("__XAMLVAR__".Length)}");
                    else if (isEvent)
                        result.AppendLine($"{varName}.{en} => {av.Substring(1, av.Length - 2)}");
                    else
                        result.AppendLine($"{varName}.{en} = \"{EscapeStr(av)}\"");
                }
                if (xName != null && IsIdentifier(xName))
                    result.AppendLine($"let {xName} = {varName}");
                if (hasBody)
                {
                    foreach (var n in elem.Nodes())
                    {
                        if (n is XText textNode)
                        {
                            string txt = textNode.Value.Trim();
                            if (txt.Length > 0)
                                result.AppendLine(txt);
                        }
                        else if (n is XElement child)
                            result.Append(ConvertXElement(child, varName));
                    }
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(elem.Value))
                    {
                        string txt = elem.Value.Trim();
                        if (txt.Length > 0)
                            result.AppendLine($"{varName}.content = \"{EscapeStr(txt)}\"");
                    }
                    foreach (var child in elem.Elements())
                        result.Append(ConvertXElement(child, varName));
                }
                if (inFunction && parentVar == null)
                    result.AppendLine($"return {varName}");
                else
                    result.AppendLine($"{(parentVar ?? "__root")}.add({varName})");
            }
            else
            {
                var callArgs = new List<string>();
                foreach (var attr in elem.Attributes())
                {
                    if (IsXamlNamespace(attr)) continue;
                    string an = attr.Name.LocalName;
                    string av = attr.Value;
                    if (double.TryParse(av, System.Globalization.NumberStyles.Any,
                        System.Globalization.CultureInfo.InvariantCulture, out _))
                        callArgs.Add($"{an}={av}");
                    else if (av.StartsWith("__XAMLVAR__"))
                        callArgs.Add($"{an}={av.Substring("__XAMLVAR__".Length)}");
                    else if (av.StartsWith("{Binding ") && av.EndsWith("}"))
                        callArgs.Add($"{an}={av.Substring(9, av.Length - 10).Trim()}");
                    else
                        callArgs.Add($"{an}=\"{EscapeStr(av)}\"");
                }
                result.AppendLine($"let {varName} = {tag}({string.Join(", ", callArgs)})");
                if (xName != null && IsIdentifier(xName))
                    result.AppendLine($"let {xName} = {varName}");
                if (hasBody)
                {
                    foreach (var n in elem.Nodes())
                    {
                        if (n is XText textNode)
                        {
                            string txt = textNode.Value.Trim();
                            if (txt.Length > 0) result.AppendLine(txt);
                        }
                        else if (n is XElement child)
                            result.Append(ConvertXElement(child, varName));
                    }
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(elem.Value))
                    {
                        string txt = elem.Value.Trim();
                        if (txt.Length > 0)
                            result.AppendLine($"{varName}.content = \"{EscapeStr(txt)}\"");
                    }
                    foreach (var child in elem.Elements())
                        result.Append(ConvertXElement(child, varName));
                }
                if (inFunction && parentVar == null)
                    result.AppendLine($"return {varName}");
                else
                    result.AppendLine($"{(parentVar ?? "__root")}.add({varName})");
            }
            return result.ToString();
        }

        static string ConvertArabicShort(string code)
        {
            code = Regex.Replace(code, @"(^|[^\w\u0600-\u06FF])م>\s+(?=[\w\u0600-\u06FF])", "$1let ");
            code = Regex.Replace(code, @"(^|[^\w\u0600-\u06FF])ط>\s*(?="")", "$1print(");
            code = Regex.Replace(code, @"ط>\s*(\w)", "print($1");
            code = Regex.Replace(code, @"(^|[^\w\u0600-\u06FF])لو>\s+", "$1if(");
            code = Regex.Replace(code, @"(^|[^\w\u0600-\u06FF])ت>\s+", "$1while(");
            return code;
        }

        static string ConvertPrint(string code)
        {
            code = Regex.Replace(code, @"\bprint>""([^""]*)""\s*;?\s*", "print(\"$1\")\n");
            code = Regex.Replace(code, @"\bاطبع>""([^""]*)""\s*;?\s*", "print(\"$1\")\n");
            code = Regex.Replace(code, @"\bط>""([^""]*)""\s*;?\s*", "print(\"$1\")\n");
            return code;
        }

        static string ConvertIfParenLess(string code)
        {
            code = Regex.Replace(code, @"\b(if|لو|إذا)\s+(?!\()(.+?)\s*\{", m => $"{m.Groups[1].Value}({m.Groups[2].Value.Trim()}) {{");
            return code;
        }

        static string ConvertWhileParenLess(string code)
        {
            code = Regex.Replace(code, @"\b(while|طالما|بينما|ت)\s+(?!\()(.+?)\s*\{", m => $"{m.Groups[1].Value}({m.Groups[2].Value.Trim()}) {{");
            return code;
        }

        static string ConvertIf(string code)
        {
            code = Regex.Replace(code, @"\?>([^(]+?)_\s*\{?", m => $"if({m.Groups[1].Value.Trim()}) {{");
            code = Regex.Replace(code, @"\bif>([^(]+?)_\s*\{?", m => $"if({m.Groups[1].Value.Trim()}) {{");
            code = Regex.Replace(code, @"\bإذا>([^(]+?)_\s*\{?", m => $"if({m.Groups[1].Value.Trim()}) {{");
            code = Regex.Replace(code, @"\bلو>([^(]+?)_\s*\{?", m => $"if({m.Groups[1].Value.Trim()}) {{");
            return code;
        }

        static string ConvertIfElseCompact(string code) =>
            Regex.Replace(code, @"\?>([^_]+)_\s*([^;{}]+?)_\s*([^;{}]+?)\s*;",
                m => $"if({m.Groups[1].Value.Trim()}) {{ {m.Groups[2].Value.Trim()} }} else {{ {m.Groups[3].Value.Trim()} }}");

        static string ConvertIfElseBraces(string code) =>
            Regex.Replace(code, @"\}\s*_\s*\{", "} else {");

        static string ConvertWhile(string code)
        {
            code = Regex.Replace(code, @"\bwhile>([^(]+?)_\s*\{?", m => $"while({m.Groups[1].Value.Trim()}) {{");
            code = Regex.Replace(code, @"\bطالما>([^(]+?)_\s*\{?", m => $"while({m.Groups[1].Value.Trim()}) {{");
            code = Regex.Replace(code, @"\bبينما>([^(]+?)_\s*\{?", m => $"while({m.Groups[1].Value.Trim()}) {{");
            code = Regex.Replace(code, @"\bت>([^(]+?)_\s*\{?", m => $"while({m.Groups[1].Value.Trim()}) {{");
            return code;
        }

        static string ConvertFor(string code)
        {
            code = Regex.Replace(code,
                @"\bfor>(\w+\s*=\s*\d+)\s*>\s*([^>]+?)\s*>\s*\*\s*(\d+)\s*_\s*\{?",
                m => {
                    string v = m.Groups[1].Value.Trim().Split('=')[0].Trim();
                    string inc = m.Groups[2].Value.Trim();
                    return $"for({m.Groups[1].Value.Trim()}; {v} < {m.Groups[3].Value}; {(inc.Contains("=") ? inc : $"{v} = {inc}")}) {{";
                });
            code = Regex.Replace(code,
                @"\bfor>(\w+\s*=\s*\d+)\s*>\s*([^>]+?)\s*>\s*([^>]+?)\s*_\s*\{?",
                m => {
                    string v = m.Groups[1].Value.Trim().Split('=')[0].Trim();
                    string inc = m.Groups[2].Value.Trim();
                    return $"for({m.Groups[1].Value.Trim()}; {m.Groups[3].Value.Trim()}; {(inc.Contains("=") ? inc : $"{v} = {inc}")}) {{";
                });
            return code;
        }

        static string ConvertIncrementDecrement(string code)
        {
            code = Regex.Replace(code, @"(?m)^\s*([A-Za-z_\u0600-\u06FF][\w\u0600-\u06FF]*)\s*\+\+\s*;?\s*$",
                m => $"{m.Groups[1].Value} += 1");
            code = Regex.Replace(code, @"(?m)^\s*([A-Za-z_\u0600-\u06FF][\w\u0600-\u06FF]*)\s*--\s*;?\s*$",
                m => $"{m.Groups[1].Value} -= 1");
            code = Regex.Replace(code, @"([;(]\s*)([A-Za-z_\u0600-\u06FF][\w\u0600-\u06FF]*)\s*\+\+(\s*[;)])",
                m => $"{m.Groups[1].Value}{m.Groups[2].Value} += 1{m.Groups[3].Value}");
            code = Regex.Replace(code, @"([;(]\s*)([A-Za-z_\u0600-\u06FF][\w\u0600-\u06FF]*)\s*--(\s*[;)])",
                m => $"{m.Groups[1].Value}{m.Groups[2].Value} -= 1{m.Groups[3].Value}");
            return code;
        }

        static string ConvertAPlusNoNew(string code)
        {
            code = Regex.Replace(code, @"a\+\s+(?!new\s)([\w\u0600-\u06FF]+)\s+([\w\u0600-\u06FF]+)", "a+ new $1 $2");
            return code;
        }

        static string ConvertAPlusNewInline(string code)
        {
            code = Regex.Replace(code, @"a\+\s+new\s+(\w+)\s+(\w+)\s*=\s*<\w+\s+([^>]+?)\s*/?\s*>",
                m => {
                    string className = m.Groups[1].Value;
                    string varName = m.Groups[2].Value;
                    string attrs = m.Groups[3].Value.Trim();
                    var sb = new StringBuilder();
                    sb.AppendLine($"a+ new {className} {varName}");
                    var attrMatches = Regex.Matches(attrs, @"(\w[\w.-]*)\s*=\s*(?:""([^""]*)""|(\S+))");
                    foreach (Match attr in attrMatches)
                    {
                        string val = attr.Groups[2].Success ? attr.Groups[2].Value : attr.Groups[3].Value;
                        sb.AppendLine($"{varName}.{attr.Groups[1].Value} = \"{EscapeStr(val)}\"");
                    }
                    return sb.ToString().TrimEnd();
                });
            return code;
        }
    }

    class OverwriteDictionary<TKey, TValue> : Dictionary<TKey, TValue>
    {
        public new void Add(TKey key, TValue value) => this[key] = value;
    }
}
