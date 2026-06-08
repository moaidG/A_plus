// ============================================================
// A+ Web Runtime — Full Language Interpreter
// Supports: let, print, if/else, while, for, func, return,
//           arrays, string methods, pipe, arithmetic, comparisons,
//           Arabic keywords, inline XAML/XML, shorthand syntax
// ============================================================
(function () {
    'use strict';

    // ============================================================
    // 1. Token Types & Keyword Map (from checker.js)
    // ============================================================
    const TT = Object.freeze({
        LET: 1, PRINT: 2, IF: 3, ELSE: 4, WHILE: 5, FOR: 6,
        FUNCTION: 7, RETURN: 8, CLASS: 9, NEW: 10, SELF: 11,
        PUBLIC: 12, PRIVATE: 13, STATIC: 14, ABSTRACT: 15, INTERFACE: 16,
        IDENTIFIER: 17, NUMBER: 18, STRING: 19,
        EQUALS: 20, PLUS: 21, MINUS: 22, STAR: 23, SLASH: 24, PERCENT: 55,
        PLUS_EQUALS: 56, MINUS_EQUALS: 57, STAR_EQUALS: 58, SLASH_EQUALS: 59,
        LPAREN: 25, RPAREN: 26, LBRACE: 27, RBRACE: 28, DOT: 29,
        GREATER: 30, LESS: 31, DOUBLE_EQUALS: 32, NOT_EQUALS: 33,
        LESS_EQUALS: 34, GREATER_EQUALS: 35,
        AND: 36, OR: 37, NOT: 60, BANG: 61,
        COMMA: 38, SEMICOLON: 39, COLON: 40, QUESTION: 62,
        LBRACKET: 43, RBRACKET: 44, INCLUDE: 45, FROM: 46, IN: 63, EOF: 47,
        TRUE: 41, FALSE: 42,
        THROW: 48, TRY: 49, CATCH: 50, FINALLY: 69,
        BREAK: 64, CONTINUE: 65, SWITCH: 66, CASE: 67, DEFAULT: 68,
        A_PLUS: 51, ARROW: 52, SHOW: 53, EXTERN: 54,
        EXTENDS: 70, EXPORT: 71, IMPORT: 72, AS: 73,
        ASYNC: 74, AWAIT: 75, GO: 76, SPAWN: 77,
        NIL: 78, PIPE: 79, REGEX: 81, DEBUGGER: 80,
        VIEWPORT3D: 82, MODELVISUAL3D: 83, MODEL3DGROUP: 84, GEOMETRYMODEL3D: 85, MESHGEOMETRY3D: 86,
        DIRECTIONALLIGHT: 87, AMBIENTLIGHT: 88, POINTLIGHT: 89, SPOTLIGHT: 90,
        PERSPECTIVECAMERA: 91, ORTHOGRAPHICCAMERA: 92,
        MEDIAELEMENT: 93
    });

    const KW = {
        'let': TT.LET, 'م': TT.LET, 'متغير': TT.LET,
        'print': TT.PRINT, 'ط': TT.PRINT, 'اطبع': TT.PRINT,
        'if': TT.IF, 'لو': TT.IF, 'إذا': TT.IF,
        'while': TT.WHILE, 'ت': TT.WHILE, 'طالما': TT.WHILE,
        'for': TT.FOR, 'لكل': TT.FOR,
        'else': TT.ELSE, 'وإلا': TT.ELSE,
        'func': TT.FUNCTION, 'function': TT.FUNCTION, 'دالة': TT.FUNCTION,
        'return': TT.RETURN, 'ارجع': TT.RETURN, 'عود': TT.RETURN, 'رجع': TT.RETURN,
        'and': TT.AND, 'و': TT.AND,
        'or': TT.OR, 'أو': TT.OR,
        'true': TT.TRUE, 'false': TT.FALSE,
        'صحيح': TT.TRUE, 'خاطئ': TT.FALSE,
        'class': TT.CLASS, 'صنف': TT.CLASS, 'فئة': TT.CLASS,
        'new': TT.NEW, 'جديد': TT.NEW,
        'self': TT.SELF, 'this': TT.SELF, 'نفس': TT.SELF,
        'هذا': TT.SELF,
        'public': TT.PUBLIC, 'عام': TT.PUBLIC,
        'private': TT.PRIVATE, 'خاص': TT.PRIVATE,
        'static': TT.STATIC, 'ثابت': TT.STATIC,
        'abstract': TT.ABSTRACT, 'مجرد': TT.ABSTRACT,
        'interface': TT.INTERFACE, 'واجهة': TT.INTERFACE,
        'include': TT.INCLUDE, 'استدعاء': TT.INCLUDE, 'ضم': TT.INCLUDE,
        'from': TT.FROM, 'من': TT.FROM,
        'throw': TT.THROW, 'ارم': TT.THROW,
        'try': TT.TRY, 'حاول': TT.TRY, 'محاولة': TT.TRY,
        'catch': TT.CATCH, 'التقط': TT.CATCH, 'القط': TT.CATCH,
        'show': TT.SHOW, 'اعرض': TT.SHOW,
        'extern': TT.EXTERN, 'خارجي': TT.EXTERN,
        'not': TT.NOT, 'ليس': TT.NOT,
        'break': TT.BREAK, 'continue': TT.CONTINUE,
        'توقف': TT.BREAK, 'كسر': TT.BREAK, 'استمر': TT.CONTINUE,
        'switch': TT.SWITCH, 'اختيار': TT.SWITCH,
        'case': TT.CASE, 'حالة': TT.CASE,
        'default': TT.DEFAULT, 'افتراضي': TT.DEFAULT,
        'in': TT.IN, 'في': TT.IN,
        'extends': TT.EXTENDS, 'يرث': TT.EXTENDS,
        'finally': TT.FINALLY, 'وأخيراً': TT.FINALLY, 'أخيرا': TT.FINALLY,
        'بينما': TT.WHILE,
        'export': TT.EXPORT, 'صدر': TT.EXPORT,
        'import': TT.IMPORT, 'استورد': TT.IMPORT,
        'as': TT.AS, 'كـ': TT.AS,
        'async': TT.ASYNC, 'غيرمتزامن': TT.ASYNC,
        'await': TT.AWAIT, 'انتظر': TT.AWAIT,
        'go': TT.GO, 'انطلق': TT.GO,
        'spawn': TT.SPAWN, 'أنشئ': TT.SPAWN,
        'nil': TT.NIL, 'عدم': TT.NIL,
        'debugger': TT.DEBUGGER, 'مصحح': TT.DEBUGGER,
        'viewport3d': TT.VIEWPORT3D, 'منظور3d': TT.VIEWPORT3D,
        'mediaelement': TT.MEDIAELEMENT, 'وسائط': TT.MEDIAELEMENT, 'عنصر_وسائط': TT.MEDIAELEMENT,
    };

    const OP_MAP = {
        '=': TT.EQUALS, '+': TT.PLUS, '-': TT.MINUS,
        '*': TT.STAR, '/': TT.SLASH, '%': TT.PERCENT,
        '(': TT.LPAREN, ')': TT.RPAREN,
        '{': TT.LBRACE, '}': TT.RBRACE,
        ',': TT.COMMA, ';': TT.SEMICOLON, ':': TT.COLON,
        '.': TT.DOT,
        '>': TT.GREATER, '<': TT.LESS,
        '[': TT.LBRACKET, ']': TT.RBRACKET,
        '?': TT.QUESTION, '|': TT.PIPE,
    };

    const TYPE_NAMES = {};
    for (const k in TT) TYPE_NAMES[TT[k]] = k;

    // ============================================================
    // 2. Arabic Tag/Attr Maps (from checker.js)
    // ============================================================
    const arabicTags = {
        'شبكة': 'Grid', 'لوحة_مكدسة': 'StackPanel', 'مكدسة': 'StackPanel',
        'لوحة_ملتفة': 'WrapPanel', 'ملتفة': 'WrapPanel',
        'لوحة_إرساء': 'DockPanel', 'إرساء': 'DockPanel',
        'لوحة_رسم': 'Canvas', 'رسم': 'Canvas',
        'شبكة_موحدة': 'UniformGrid', 'صندوق_عرض': 'Viewbox',
        'حدود': 'Border', 'متصفح_تمرير': 'ScrollViewer', 'تمرير': 'ScrollViewer',
        'نص': 'TextBlock', 'صندوق_نص': 'TextBox', 'صندوق_نص_غني': 'RichTextBox',
        'تسمية': 'Label', 'نص_غني': 'RichTextBox',
        'زر': 'Button', 'زر_تكرار': 'RepeatButton', 'زر_تبديل': 'ToggleButton',
        'مربع_اختيار': 'CheckBox', 'زر_خيار': 'RadioButton',
        'صندوق_قائمة': 'ListBox', 'عرض_قائمة': 'ListView',
        'صندوق_مدمج': 'ComboBox', 'عرض_شجري': 'TreeView',
        'قائمة': 'Menu', 'عنصر_قائمة': 'MenuItem',
        'قائمة_سياق': 'ContextMenu', 'تحكم_تبويب': 'TabControl', 'تبويب': 'TabItem',
        'صورة': 'Image', 'عنصر_وسائط': 'MediaElement', 'وسائط': 'MediaElement',
        'منزلق': 'Slider', 'شريط_تقدم': 'ProgressBar',
        'منتقي_تاريخ': 'DatePicker', 'تقويم': 'Calendar',
        'صندوق_كلمة_سر': 'PasswordBox',
        'مستطيل': 'Rectangle', 'قطع_ناقص': 'Ellipse', 'ناقص': 'Ellipse',
        'خط': 'Line', 'مضلع': 'Polygon', 'خط_متعدد': 'Polyline', 'مسار': 'Path',
        'نافذة': 'Window',
    };

    const arabicAttrs = {
        'المحتوى': 'Content', 'النص': 'Text', 'العرض': 'Width',
        'الارتفاع': 'Height', 'الهامش': 'Margin', 'الحشوة': 'Padding',
        'نقر': 'Click', 'تحميل': 'Loaded', 'المصدر': 'Source',
        'القيمة': 'Value', 'الحد_الأدنى': 'Minimum', 'الحد_الأقصى': 'Maximum',
        'الخلفية': 'Background', 'الأمامية': 'Foreground',
        'محدد': 'IsChecked', 'مفعل': 'IsEnabled', 'رؤية': 'Visibility',
        'الاسم': 'Name', 'المعرف': 'Name',
        'دخول_فأرة': 'MouseEnter', 'خروج_فأرة': 'MouseLeave',
        'ضغط_فأرة': 'MouseDown', 'رفع_فأرة': 'MouseUp', 'تحريك_فأرة': 'MouseMove',
        'ضغط_مفتاح': 'KeyDown', 'رفع_مفتاح': 'KeyUp',
        'اكتسب_تركيز': 'GotFocus', 'فقد_تركيز': 'LostFocus',
        'تحديث_تخطيط': 'LayoutUpdated',
        'تغير_نص': 'TextChanged', 'تغير_اختيار': 'SelectionChanged',
        'تغير_قيمة': 'ValueChanged', 'تغير_حجم': 'SizeChanged',
        'تم_تحديد': 'Checked', 'تم_إلغاء': 'Unchecked', 'غير_محدد': 'Indeterminate',
        'إفلات': 'Drop', 'سحب_فوق': 'DragOver',
        'أمر': 'Command', 'وسيط_أمر': 'CommandParameter',
        'مصدر': 'Source', 'توجيه': 'Orientation',
        'زخرفة': 'TextDecoration', 'محاذاة': 'HorizontalAlignment',
        'محاذاة_عمودية': 'VerticalAlignment', 'حجم_الخط': 'FontSize',
        'نوع_الخط': 'FontFamily', 'غامق': 'FontWeight',
        'مائل': 'FontStyle', 'لون': 'Foreground',
        'لون_الخلفية': 'Background', 'سمك_الحدود': 'BorderThickness',
        'لون_الحدود': 'BorderBrush', 'تعبئة': 'Fill', 'حد': 'Stroke',
    };

    // ============================================================
    // 3. Preprocessing: XAML/Shorthand Conversion (from checker.js)
    // ============================================================
    let __xamlCounter = 0;

    function translateArabicXamlJs(code) {
        for (const [ar, en] of Object.entries(arabicTags)) {
            code = code.split('<' + ar + ' ').join('<' + en + ' ');
            code = code.split('<' + ar + '>').join('<' + en + '>');
            code = code.split('</' + ar + '>').join('</' + en + '>');
            code = code.split('<' + ar + '/>').join('<' + en + '/>');
            code = code.split('<' + ar + '\n').join('<' + en + '\n');
        }
        for (const [ar, en] of Object.entries(arabicAttrs)) {
            code = code.split(ar + '=').join(en + '=');
        }
        return code;
    }

    function parseSimpleXml(xml) {
        let root = { tag: '#root', attrs: {}, children: [], text: '' };
        let stack = [root];
        let i = 0;
        while (i < xml.length) {
            if (xml[i] === '<') {
                if (i + 1 < xml.length && xml[i + 1] === '/') {
                    let closeEnd = xml.indexOf('>', i);
                    stack.pop();
                    i = closeEnd + 1;
                } else if (i + 1 < xml.length && /[_a-zA-Z\u0600-\u06FF]/.test(xml[i + 1])) {
                    let tagEnd = xml.indexOf('>', i);
                    let tagContent = xml.substring(i + 1, tagEnd);
                    let parts = tagContent.split(/\s+/);
                    let tagName = parts[0];
                    let attrs = {};
                    let attrRe = /(\w[\w.-]*)\s*=\s*(?:"([^"]*)"|'([^']*)'|([^"'>\s]+))/g;
                    let m;
                    while ((m = attrRe.exec(tagContent)) !== null) attrs[m[1]] = m[2] || m[3] || m[4];
                    let selfClose = tagContent.endsWith('/') || tagContent.trim().endsWith('/');
                    let node = { tag: tagName, attrs: attrs, children: [], text: '' };
                    stack[stack.length - 1].children.push(node);
                    if (!selfClose) stack.push(node);
                    i = tagEnd + 1;
                } else i++;
            } else {
                let nextTag = xml.indexOf('<', i);
                let text = nextTag >= 0 ? xml.substring(i, nextTag) : xml.substring(i);
                text = text.trim();
                if (text && stack.length > 0) stack[stack.length - 1].text += text;
                i = nextTag >= 0 ? nextTag : xml.length;
            }
        }
        return root;
    }

    const knownUiTypes = new Set([
        'grid', 'stackpanel', 'wrappanel', 'dockpanel', 'canvas', 'uniformgrid',
        'viewbox', 'border', 'scrollviewer',
        'textblock', 'textbox', 'richtextbox', 'label',
        'button', 'repeatbutton', 'togglebutton', 'checkbox', 'radiobutton',
        'listbox', 'listview', 'combobox', 'treeview', 'menu', 'menuitem',
        'contextmenu', 'tabcontrol', 'tabitem',
        'image', 'mediaelement', 'viewport3d',
        'slider', 'progressbar', 'datepicker', 'calendar', 'passwordbox',
        'rectangle', 'ellipse', 'line', 'polygon', 'polyline', 'path',
        'window',
    ]);

    // Property attributes (NOT events)
    const propAttrs = new Set(['id','name','content','text','width','height',
        'margin','padding','fontsize','fontfamily','foreground','background',
        'horizontalalignment','verticalalignment','source','value','minimum','maximum',
        'ischecked','isselected','isenabled','visibility','opacity']);

    function isIdentifier(s) { return /^[A-Za-z_\u0600-\u06FF][\w\u0600-\u06FF]*$/.test(s); }
    function isPropAttr(s) { return propAttrs.has(s.toLowerCase()); }
    function normalizeEvent(an) {
        if (an.startsWith('on') && an.length > 2)
            return an[2].toUpperCase() + an.substring(3);
        return an;
    }
    function isInFuncBodyJs(code, pos) {
        let braceDepth = 0, inStrF = false, scF = '"';
        for (let i = pos - 1; i >= 0; i--) {
            let c = code[i];
            if (inStrF) { if (c === scF) inStrF = false; continue; }
            if (c === '"' || c === "'") { inStrF = true; scF = c; continue; }
            if (c === '}') braceDepth--;
            else if (c === '{') braceDepth++;
            if (c === '{' && braceDepth > 0) {
                let searchStart = Math.max(0, i - 30);
                let before = code.substring(searchStart, i);
                if (/\b(func|دالة)\s+\w+\s*\(/.test(before)) return true;
            }
        }
        return false;
    }

    function convertXamlJs(code) {
        code = translateArabicXamlJs(code);
        let result = '';
        let i = 0;
        while (i < code.length) {
            if (code[i] === '=') {
                let j = i + 1;
                while (j < code.length && (code[j] === ' ' || code[j] === '\t')) j++;
                if (j < code.length && code[j] === '<' && j + 1 < code.length && /[_a-zA-Z\u0600-\u06FF]/.test(code[j + 1])) {
                    result += '\n';
                    i = j;
                    continue;
                }
            }
            if (code[i] === '<' && i + 1 < code.length && /[_a-zA-Z\u0600-\u06FF]/.test(code[i + 1])) {
                let start = i;
                let inFunc = isInFuncBodyJs(code, i);
                let depth = 0, inStr = false, sq = '"';
                while (i < code.length) {
                    let c = code[i];
                    if (inStr) { if (c === sq) inStr = false; i++; continue; }
                    if (c === '"' || c === "'") { inStr = true; sq = c; i++; continue; }
                    if (c === '<') {
                        if (i + 1 < code.length && code[i + 1] === '/') { depth--; let te = code.indexOf('>', i); i = te + 1; if (depth <= 0) break; }
                        else if (i + 1 < code.length && /[_a-zA-Z\u0600-\u06FF]/.test(code[i + 1])) { depth++; let te = code.indexOf('>', i); let tc = code.substring(i + 1, te); if (tc.endsWith('/') || tc.trim().endsWith('/')) depth--; i = te + 1; if (depth <= 0) break; }
                        else i++;
                    } else i++;
                }
                let xamlBlock = code.substring(start, i);
                try {
                    let parsed = parseSimpleXml(xamlBlock);
                    result += xamlNodeToAplus(parsed, null, inFunc);
                } catch (e) {
                    result += xamlBlock;
                }
            } else {
                result += code[i];
                i++;
            }
        }
        return result;
    }

    function xamlNodeToAplus(node, parentVar, inFunc) {
        if (node.tag === '#root') {
            let r = '';
            for (let child of node.children) r += xamlNodeToAplus(child, parentVar, inFunc);
            return r;
        }
        let v = '__xaml_' + (__xamlCounter++);
        let r = '';
        const isUi = knownUiTypes.has(node.tag.toLowerCase());
        if (isUi) {
            r += 'a+ new ' + node.tag + ' ' + v + '\n';
            for (let [an, av] of Object.entries(node.attrs)) {
                let en = normalizeEvent(an);
                let isEvent = isIdentifier(av) && en[0] === en[0].toUpperCase() && !isPropAttr(en);
                if (isEvent) {
                    r += v + '.' + en + ' => ' + av + '\n';
                } else if (/^\d+(\.\d+)?$/.test(av)) {
                    r += v + '.' + en + ' = ' + av + '\n';
                } else {
                    r += v + '.' + en + ' = "' + av.replace(/"/g, '\\"') + '"\n';
                }
            }
            for (let child of node.children) r += xamlNodeToAplus(child, v, inFunc);
            if (node.children.length === 0 && node.text.trim())
                r += v + '.content = "' + node.text.trim().replace(/"/g, '\\"') + '"\n';
            if (inFunc && !parentVar)
                r += 'return ' + v + '\n';
            else if (parentVar)
                r += parentVar + '.add(' + v + ')\n';
            else
                r += '__root.add(' + v + ')\n';
        } else {
            let callArgs = [];
            for (let [an, av] of Object.entries(node.attrs)) {
                let en = normalizeEvent(an);
                let isEvent = isIdentifier(av) && en[0] === en[0].toUpperCase() && !isPropAttr(en);
                if (isEvent) {
                    callArgs.push(en + '=>' + av);
                } else if (/^\d+(\.\d+)?$/.test(av)) {
                    callArgs.push(en + '=' + av);
                } else {
                    callArgs.push(en + '="' + av.replace(/"/g, '\\"') + '"');
                }
            }
            r += 'let ' + v + ' = ' + node.tag + '(' + callArgs.join(', ') + ')\n';
            if (node.children.length === 0 && node.text.trim())
                r += v + '.content = "' + node.text.trim().replace(/"/g, '\\"') + '"\n';
            for (let child of node.children) r += xamlNodeToAplus(child, v, inFunc);
            if (inFunc && !parentVar)
                r += 'return ' + v + '\n';
            else if (parentVar)
                r += parentVar + '.add(' + v + ')\n';
            else
                r += '__root.add(' + v + ')\n';
        }
        return r;
    }

    function convertAPlusNewInlineJs(code) {
        return code.replace(/a\+\s+new\s+(\w+)\s+(\w+)\s*=\s*<\w+\s+([^>]+?)\s*\/?\s*>/g,
            function(m, className, varName, attrs) {
                let r = 'a+ new ' + className + ' ' + varName + '\n';
                let attrRe = /(\w[\w.-]*)\s*=\s*(?:"([^"]*)"|'([^']*)'|(\S+))/g;
                let am;
                while ((am = attrRe.exec(attrs)) !== null) {
                    let val = am[2] || am[3] || am[4] || '';
                    r += varName + '.' + am[1] + ' = "' + val.replace(/"/g, '\\"') + '"\n';
                }
                return r;
            });
    }

    function convertAPlusXmlJs(code) {
        // Form 1: a+ xml varname = <Component args>
        code = code.replace(/a\+\s+xml\s+(\w+)\s*=\s*<(\w+)\s*([^>]*)>\s*/g,
            function(m, varName, compName, attrs) {
                let args = [];
                let attrRe = /(\w[\w.-]*)\s*=\s*(?:"([^"]*)"|'([^']*)'|(\S+))/g;
                let am;
                while ((am = attrRe.exec(attrs)) !== null) {
                    let val = am[2] || am[3] || am[4] || '';
                    args.push('"' + val.replace(/"/g, '\\"') + '"');
                }
                return 'let ' + varName + ' = ' + compName + '(' + args.join(', ') + ')\n__root.add(' + varName + ')\n';
            });
        // Form 2: a+ xml varname = { <xml inline> }
        let sb2 = '', i2 = 0;
        while (i2 < code.length) {
            let m2 = code.substring(i2).match(/a\+\s+xml\s+(\w+)\s*=\s*\{/);
            if (!m2 || m2.index !== 0) { sb2 += code[i2]; i2++; continue; }
            let vn = m2[1], start2 = i2 + m2[0].length;
            let depth = 1, j = start2, inStr2 = false, sc2 = '"';
            while (j < code.length && depth > 0) {
                let c2 = code[j];
                if (inStr2) { if (c2 === sc2) inStr2 = false; j++; continue; }
                if (c2 === '"' || c2 === "'") { inStr2 = true; sc2 = c2; j++; continue; }
                if (c2 === '{') depth++; else if (c2 === '}') depth--;
                if (depth > 0 || (depth === 0 && c2 !== '}')) j++; else break;
            }
            let xmlC = code.substring(start2, j - start2).trim();
            let xamlR = convertXamlJs(xmlC);
            let maxNum = -1, lastVar = '__xaml_0';
            let vmRe = /__xaml_(\d+)/g;
            let vm;
            while ((vm = vmRe.exec(xamlR)) !== null) {
                let n = parseInt(vm[1]);
                if (n > maxNum) { maxNum = n; lastVar = '__xaml_' + n; }
            }
            sb2 += xamlR + 'let ' + vn + ' = ' + lastVar + '\n';
            i2 = j + 1;
        }
        return sb2;
    }

    function convertXamlEventsJs(code) {
        let sb = '', i = 0, evC = 0, evFuncs = [];
        while (i < code.length) {
            let m = code.substring(i).match(/(\w[\w.-]*)\s*=\s*\(\)\s*=>\s*\{/);
            if (!m || m.index !== 0) { sb += code[i]; i++; continue; }
            let evName = m[1], fnName = '__xaml_ev_' + (evC++);
            let start2 = i + m[0].length;
            let depth = 1, pos = start2, inStr = false, sc = '"';
            while (pos < code.length && depth > 0) {
                let c = code[pos];
                if (inStr) { if (c === sc) inStr = false; pos++; continue; }
                if (c === '"' || c === "'") { inStr = true; sc = c; pos++; continue; }
                if (c === '{') depth++; else if (c === '}') depth--;
                if (depth > 0) pos++;
            }
            let body = code.substring(start2, pos - start2);
            sb += evName + '=' + fnName;
            evFuncs.push('func ' + fnName + '() { ' + body + ' }');
            i = pos + 1;
        }
        return evFuncs.length ? evFuncs.join('\n') + '\n' + sb : sb;
    }

    function shorthandConvert(code) {
        __xamlCounter = 0;
        code = convertAPlusNewInlineJs(code);
        code = convertAPlusXmlJs(code);
        code = convertXamlEventsJs(code);
        code = convertXamlJs(code);
        code = code.replace(/(^|[^\w\u0600-\u06FF])م>\s+(?=[\w\u0600-\u06FF])/g, '$1let ');
        code = code.replace(/(^|[^\w\u0600-\u06FF])ط>\s*(?=")/g, '$1print(');
        code = code.replace(/ط>\s*(\w)/g, 'print($1');
        code = code.replace(/(^|[^\w\u0600-\u06FF])لو>\s+/g, '$1if(');
        code = code.replace(/(^|[^\w\u0600-\u06FF])ت>\s+/g, '$1while(');
        code = code.replace(/\bprint>"([^"]*)"\s*;?\s*/g, 'print("$1")\n');
        code = code.replace(/\bاطبع>"([^"]*)"\s*;?\s*/g, 'print("$1")\n');
        code = code.replace(/\bط>"([^"]*)"\s*;?\s*/g, 'print("$1")\n');
        code = code.replace(/\}\s*_\s*\{/g, '} else {');
        code = code.replace(/\?>([^_]+)_\s*([^;{}]+?)_\s*([^;{}]+?)\s*;/g, function (m, cond, b1, b2) {
            return 'if(' + cond.trim() + ') { ' + b1.trim() + ' } else { ' + b2.trim() + ' }';
        });
        code = code.replace(/\?>([^(]+?)_\s*\{?/g, function (m, cond) { return 'if(' + cond.trim() + ') {'; });
        code = code.replace(/\bif>([^(]+?)_\s*\{?/g, function (m, cond) { return 'if(' + cond.trim() + ') {'; });
        code = code.replace(/\bإذا>([^(]+?)_\s*\{?/g, function (m, cond) { return 'if(' + cond.trim() + ') {'; });
        code = code.replace(/\bلو>([^(]+?)_\s*\{?/g, function (m, cond) { return 'if(' + cond.trim() + ') {'; });
        code = code.replace(/\bwhile>([^(]+?)_\s*\{?/g, function (m, cond) { return 'while(' + cond.trim() + ') {'; });
        code = code.replace(/\bطالما>([^(]+?)_\s*\{?/g, function (m, cond) { return 'while(' + cond.trim() + ') {'; });
        code = code.replace(/\bبينما>([^(]+?)_\s*\{?/g, function (m, cond) { return 'while(' + cond.trim() + ') {'; });
        code = code.replace(/^\s*([A-Za-z_\u0600-\u06FF][\w\u0600-\u06FF]*)\s*\+\+\s*;?\s*$/gm, '$1 += 1');
        code = code.replace(/^\s*([A-Za-z_\u0600-\u06FF][\w\u0600-\u06FF]*)\s*--\s*;?\s*$/gm, '$1 -= 1');
        code = code.replace(/([;(]\s*)([A-Za-z_\u0600-\u06FF][\w\u0600-\u06FF]*)\s*\+\+(\s*[;)])/g, '$1$2 += 1$3');
        code = code.replace(/([;(]\s*)([A-Za-z_\u0600-\u06FF][\w\u0600-\u06FF]*)\s*--(\s*[;)])/g, '$1$2 -= 1$3');
        code = code.replace(/<---/g, '');
        return code;
    }

    // ============================================================
    // 4. Tokenizer
    // ============================================================
    function tokenize(code, errors) {
        const tokens = [];
        let pos = 0, line = 1, col = 1;

        function addTok(type, value) {
            tokens.push({ type: type, value: value, line: line, col: col });
        }

        while (pos < code.length) {
            const c = code[pos];

            if (c === '\n') { line++; col = 1; pos++; continue; }
            if (c === '\r') { pos++; continue; }
            if (c === ' ' || c === '\t') { pos++; col++; continue; }

            if ((c === '/' && pos + 1 < code.length && code[pos + 1] === '/') || c === '#') {
                while (pos < code.length && code[pos] !== '\n') pos++;
                continue;
            }

            if (/[a-zA-Z\u0600-\u06FF_]/.test(c)) {
                const wStart = pos;
                while (pos < code.length && /[a-zA-Z0-9\u0600-\u06FF_]/.test(code[pos])) pos++;
                const word = code.substring(wStart, pos);
                col += pos - wStart;
                const type = KW[word];
                if (type !== undefined) {
                    addTok(type, word);
                } else if (word === 'a' && pos < code.length && code[pos] === '+') {
                    pos++; col++;
                    addTok(TT.A_PLUS, 'a+');
                } else {
                    addTok(TT.IDENTIFIER, word);
                }
                continue;
            }

            if (/\d/.test(c)) {
                const nStart = pos;
                let hasDot = false;
                while (pos < code.length && /[0-9.]/.test(code[pos])) {
                    if (code[pos] === '.') {
                        if (hasDot) { errors.push('Invalid number at line ' + line); break; }
                        hasDot = true;
                    }
                    pos++;
                }
                col += pos - nStart;
                addTok(TT.NUMBER, code.substring(nStart, pos));
                continue;
            }

            if (c === '"') {
                pos++; col++;
                let str = "";
                while (pos < code.length && code[pos] !== '"') {
                    if (code[pos] === '\\' && pos + 1 < code.length) {
                        const next = code[pos + 1];
                        if (next === '"') { str += '"'; pos += 2; col += 2; continue; }
                        if (next === '\\') { str += '\\'; pos += 2; col += 2; continue; }
                        if (next === 'n') { str += '\n'; pos += 2; col += 2; continue; }
                        if (next === 't') { str += '\t'; pos += 2; col += 2; continue; }
                        if (next === 'r') { str += '\r'; pos += 2; col += 2; continue; }
                    }
                    str += code[pos]; pos++; col++;
                }
                if (pos >= code.length) { errors.push('Unclosed string at line ' + line); continue; }
                addTok(TT.STRING, str);
                pos++; col++;
                continue;
            }

            if (c === '=' && pos + 1 < code.length && code[pos + 1] === '=') {
                addTok(TT.DOUBLE_EQUALS, '=='); pos += 2; col += 2; continue;
            }
            if (c === '=' && pos + 1 < code.length && code[pos + 1] === '>') {
                addTok(TT.ARROW, '=>'); pos += 2; col += 2; continue;
            }
            if (c === '+' && pos + 1 < code.length && code[pos + 1] === '=') {
                addTok(TT.PLUS_EQUALS, '+='); pos += 2; col += 2; continue;
            }
            if (c === '-' && pos + 1 < code.length && code[pos + 1] === '=') {
                addTok(TT.MINUS_EQUALS, '-='); pos += 2; col += 2; continue;
            }
            if (c === '*' && pos + 1 < code.length && code[pos + 1] === '=') {
                addTok(TT.STAR_EQUALS, '*='); pos += 2; col += 2; continue;
            }
            if (c === '/' && pos + 1 < code.length && code[pos + 1] === '=') {
                addTok(TT.SLASH_EQUALS, '/='); pos += 2; col += 2; continue;
            }
            if (c === '>' && pos + 1 < code.length && code[pos + 1] === '=') {
                addTok(TT.GREATER_EQUALS, '>='); pos += 2; col += 2; continue;
            }
            if (c === '<' && pos + 1 < code.length && code[pos + 1] === '=') {
                addTok(TT.LESS_EQUALS, '<='); pos += 2; col += 2; continue;
            }
            if (c === '!' && pos + 1 < code.length && code[pos + 1] === '=') {
                addTok(TT.NOT_EQUALS, '!='); pos += 2; col += 2; continue;
            }
            if (c === '!') {
                addTok(TT.BANG, '!'); pos++; col++; continue;
            }

            const t = OP_MAP[c];
            if (t !== undefined) {
                addTok(t, c); pos++; col++; continue;
            }

            errors.push('Unknown character: ' + c + ' at line ' + line);
            pos++; col++;
        }

        addTok(TT.EOF, '');
        return tokens;
    }

    // ============================================================
    // 5. AST Parser
    // ============================================================
    class Parser {
        constructor(tokens, errors) {
            this.tokens = tokens;
            this.errors = errors;
            this.pos = 0;
        }

        current() { return this.pos < this.tokens.length ? this.tokens[this.pos] : null; }
        peek(n) { return this.pos + n < this.tokens.length ? this.tokens[this.pos + n] : null; }

        eat(type, msg) {
            const t = this.current();
            if (!t || t.type !== type) {
                this.errors.push(msg || 'Expected ' + type + ' at line ' + (t ? t.line : '?'));
                return null;
            }
            this.pos++;
            return t;
        }

        eatName(msg) {
            const t = this.current();
            if (!t || t.type !== TT.IDENTIFIER) {
                this.errors.push(msg || 'Expected identifier');
                return null;
            }
            this.pos++;
            return t;
        }

        syncToStatement() {
            const stmtTokens = new Set([
                TT.LET, TT.PRINT, TT.IF, TT.WHILE, TT.FOR,
                TT.FUNCTION, TT.RETURN, TT.BREAK, TT.CONTINUE,
                TT.CLASS, TT.ABSTRACT, TT.INTERFACE, TT.SHOW, TT.A_PLUS,
                TT.EXTERN, TT.RBRACE, TT.EOF,
                TT.EXPORT, TT.IMPORT, TT.ASYNC, TT.GO, TT.SPAWN, TT.DEBUGGER,
                TT.VIEWPORT3D, TT.MEDIAELEMENT
            ]);
            while (this.current() && !stmtTokens.has(this.current().type)) this.pos++;
        }

        // === Main parse entry ===
        parseProgram() {
            const stmts = [];
            while (this.current() && this.current().type !== TT.EOF) {
                const s = this.parseStatement();
                if (s) stmts.push(s);
                else this.pos++;
            }
            return stmts;
        }

        // === Statement dispatch ===
        parseStatement() {
            const c = this.current();
            if (!c) return null;
            try {
                if (c.type === TT.LET) return this.parseAssignment();
                if (c.type === TT.PRINT) return this.parsePrint();
                if (c.type === TT.IF) return this.parseIf();
                if (c.type === TT.WHILE) return this.parseWhile();
                if (c.type === TT.FOR) return this.parseFor();
                if (c.type === TT.FUNCTION) return this.parseFuncDef();
                if (c.type === TT.RETURN) return this.parseReturn();
                if (c.type === TT.BREAK) { this.eat(TT.BREAK); return { type: 'break' }; }
                if (c.type === TT.CONTINUE) { this.eat(TT.CONTINUE); return { type: 'continue' }; }
                if (c.type === TT.INCLUDE) { this.eat(TT.INCLUDE); this.eat(TT.STRING); return null; }
                if (c.type === TT.LBRACE) return this.parseBlock();
                if (c.type === TT.SHOW) { return this.parseShow(); }
                if (c.type === TT.A_PLUS) { return this.parseAPlus(); }
                if (c.type === TT.DEBUGGER) { this.eat(TT.DEBUGGER); return { type: 'debugger' }; }
                if (c.type === TT.THROW) { this.eat(TT.THROW); this.parseExpression(); return null; }
                if (c.type === TT.TRY) { return this.parseTry(); }
                if (c.type === TT.SWITCH) { return this.parseSwitch(); }
                if (c.type === TT.SEMICOLON) { this.eat(TT.SEMICOLON); return null; }

                // Re-assignment: x = expr
                if ((c.type === TT.IDENTIFIER) && this.peek(1) && [
                    TT.EQUALS, TT.PLUS_EQUALS, TT.MINUS_EQUALS, TT.STAR_EQUALS, TT.SLASH_EQUALS
                ].includes(this.peek(1).type)) {
                    return this.parseReAssignment();
                }

                // Arrow bind: obj.event => handler
                if (c.type === TT.IDENTIFIER && this.peek(1) && this.peek(1).type === TT.DOT &&
                    this.peek(2) && this.peek(2).type === TT.IDENTIFIER &&
                    this.peek(3) && this.peek(3).type === TT.ARROW) {
                    return this.parseArrowBind();
                }

                // Member set: obj.member = expr
                if ((c.type === TT.IDENTIFIER || c.type === TT.SELF) && this.peek(1) && this.peek(1).type === TT.DOT &&
                    this.peek(2) && this.peek(2).type === TT.IDENTIFIER &&
                    this.peek(3) && this.peek(3).type === TT.EQUALS) {
                    return this.parseMemberSet();
                }

                // Array index: arr[idx] or arr[idx] = val
                if ((c.type === TT.IDENTIFIER) && this.peek(1) && this.peek(1).type === TT.LBRACKET) {
                    return this.parseArrayIndex();
                }

                // Expression statement
                if (c.type === TT.IDENTIFIER || c.type === TT.NUMBER || c.type === TT.STRING ||
                    c.type === TT.TRUE || c.type === TT.FALSE || c.type === TT.NIL ||
                    c.type === TT.LPAREN || c.type === TT.NEW || c.type === TT.SELF ||
                    c.type === TT.MINUS || c.type === TT.PLUS || c.type === TT.NOT || c.type === TT.BANG ||
                    c.type === TT.LBRACKET) {
                    const expr = this.parseExpression();
                    return { type: 'expr', expr: expr };
                }

                // Export / Import / Async / Go / Spawn / Class / Interface / Extern
                if (c.type === TT.EXPORT) { this.eat(TT.EXPORT); return this.parseStatement(); }
                if (c.type === TT.IMPORT) { this.skipImport(); return null; }
                if (c.type === TT.ASYNC) { this.eat(TT.ASYNC); return this.parseFuncDef(); }
                if (c.type === TT.GO) { this.eat(TT.GO); var e = this.parseExpression(); return { type: 'expr', expr: e }; }
                if (c.type === TT.SPAWN) { this.skipSpawn(); return null; }
                if (c.type === TT.CLASS) { return this.parseClassDef(); }
                if (c.type === TT.ABSTRACT && this.peek(1) && this.peek(1).type === TT.CLASS) { this.eat(TT.ABSTRACT); this.eat(TT.CLASS); return this.parseClassDef(); }
                if (c.type === TT.INTERFACE) { this.eat(TT.INTERFACE); return this.parseClassDef(); }
                if (c.type === TT.EXTERN) { this.skipExtern(); return null; }
                if (c.type === TT.VIEWPORT3D) { this.eat(TT.VIEWPORT3D); this.eat(TT.NEW); this.eatName(); return null; }
                if (c.type === TT.MEDIAELEMENT) { this.eat(TT.MEDIAELEMENT); this.eat(TT.NEW); this.eatName(); return null; }

                this.errors.push('Unknown statement at line ' + (c.line || '?'));
                this.pos++;
                this.syncToStatement();
                return null;
            } catch (e) {
                this.syncToStatement();
                return null;
            }
        }

        parseAssignment() {
            this.eat(TT.LET);
            const nameTok = this.eatName();
            const name = nameTok ? nameTok.value : '?';
            let typeAnnot = null;
            if (this.current() && this.current().type === TT.COLON) {
                this.eat(TT.COLON);
                typeAnnot = this.eatName() ? this.current() && this.current().value : null;
            }
            let init = null;
            if (this.current() && this.current().type === TT.EQUALS) {
                this.eat(TT.EQUALS);
                init = this.parseExpression();
            }
            return { type: 'let', name: name, value: init };
        }

        parseReAssignment() {
            const nameTok = this.eatName();
            const opTok = this.current();
            this.pos++;
            const value = this.parseExpression();
            const op = opTok ? opTok.value : '=';
            if (op === '=') return { type: 'assign', name: nameTok.value, value: value };
            return { type: 'assignOp', name: nameTok.value, op: op, value: value };
        }

        parsePrint() {
            this.eat(TT.PRINT);
            this.eat(TT.LPAREN);
            const arg = (this.current() && this.current().type !== TT.RPAREN) ? this.parseExpression() : null;
            this.eat(TT.RPAREN);
            return { type: 'print', arg: arg };
        }

        parseShow() {
            this.eat(TT.SHOW);
            if (this.current() && this.current().type === TT.LPAREN) {
                this.eat(TT.LPAREN);
                if (this.current() && this.current().type !== TT.RPAREN) this.parseExpression();
                this.eat(TT.RPAREN);
            }
            while (this.current() && this.current().type === TT.COMMA) {
                this.eat(TT.COMMA);
                while (this.current() && ![TT.COMMA, TT.SEMICOLON, TT.RBRACE, TT.EOF].includes(this.current().type)) this.pos++;
            }
            return null;
        }

        parseIf() {
            this.eat(TT.IF);
            this.eat(TT.LPAREN);
            const cond = this.parseExpression();
            this.eat(TT.RPAREN);
            const thenBlock = this.parseBlock_or_Statement();
            let elseBlock = null;
            if (this.current() && this.current().type === TT.ELSE) {
                this.eat(TT.ELSE);
                elseBlock = this.parseBlock_or_Statement();
            }
            return { type: 'if', condition: cond, then: thenBlock, else: elseBlock };
        }

        parseWhile() {
            this.eat(TT.WHILE);
            this.eat(TT.LPAREN);
            const cond = this.parseExpression();
            this.eat(TT.RPAREN);
            const body = this.parseBlock_or_Statement();
            return { type: 'while', condition: cond, body: body };
        }

        parseFor() {
            this.eat(TT.FOR);
            this.eat(TT.LPAREN);

            // For-in: let x in expr
            if (this.current() && [TT.IDENTIFIER].includes(this.current().type) &&
                this.peek(1) && this.peek(1).type === TT.IN) {
                const nameTok = this.eatName();
                this.eat(TT.IN);
                const iterable = this.parseExpression();
                this.eat(TT.RPAREN);
                const body = this.parseBlock_or_Statement();
                return { type: 'forIn', name: nameTok.value, iterable: iterable, body: body };
            }
            if (this.current() && this.current().type === TT.LET &&
                this.peek(2) && this.peek(2).type === TT.IN) {
                this.eat(TT.LET);
                const nameTok = this.eatName();
                this.eat(TT.IN);
                const iterable = this.parseExpression();
                this.eat(TT.RPAREN);
                const body = this.parseBlock_or_Statement();
                return { type: 'forIn', name: nameTok.value, iterable: iterable, body: body };
            }

            // C-style for
            const init = (this.current() && this.current().type !== TT.SEMICOLON) ? this.parseForInit() : null;
            this.eat(TT.SEMICOLON);
            const cond = (this.current() && this.current().type !== TT.SEMICOLON) ? this.parseExpression() : null;
            this.eat(TT.SEMICOLON);
            let inc = null;
            if (this.current() && this.current().type !== TT.RPAREN) {
                inc = this.parseForInc();
            }
            this.eat(TT.RPAREN);
            const body = this.parseBlock_or_Statement();
            return { type: 'for', init: init, condition: cond, inc: inc, body: body };
        }

        parseForInit() {
            const c = this.current();
            if (!c) return null;
            if (c.type === TT.LET || c.type === TT.IDENTIFIER) {
                if (c.type === TT.LET) {
                    this.eat(TT.LET);
                    const nameTok = this.eatName();
                    const name = nameTok ? nameTok.value : '?';
                    let init = null;
                    if (this.current() && this.current().type === TT.EQUALS) {
                        this.eat(TT.EQUALS);
                        init = this.parseExpression();
                    }
                    return { type: 'let', name: name, value: init };
                }
            }
            const expr = this.parseExpression();
            return { type: 'expr', expr: expr };
        }

        parseForInc() {
            const c = this.current();
            if (!c) return null;
            if ((c.type === TT.IDENTIFIER) && this.peek(1) && [
                TT.EQUALS, TT.PLUS_EQUALS, TT.MINUS_EQUALS, TT.STAR_EQUALS, TT.SLASH_EQUALS
            ].includes(this.peek(1).type)) {
                const nameTok = this.eatName();
                const opTok = this.current();
                this.pos++;
                const value = this.parseExpression();
                const op = opTok ? opTok.value : '=';
                if (op === '=') return { type: 'assign', name: nameTok.value, value: value };
                return { type: 'assignOp', name: nameTok.value, op: op, value: value };
            }
            const expr = this.parseExpression();
            return { type: 'expr', expr: expr };
        }

        parseFuncDef() {
            this.eat(TT.FUNCTION);
            const nameTok = this.eatName();
            const name = nameTok ? nameTok.value : '?';
            this.eat(TT.LPAREN);
            const params = [];
            if (this.current() && this.current().type !== TT.RPAREN) {
                let p = this.eatName();
                if (p) params.push(p.value);
                while (this.current() && this.current().type === TT.COMMA) {
                    this.eat(TT.COMMA);
                    p = this.eatName();
                    if (p) params.push(p.value);
                }
            }
            this.eat(TT.RPAREN);
            const body = this.parseBlock_or_Statement();
            return { type: 'func', name: name, params: params, body: body };
        }

        parseReturn() {
            this.eat(TT.RETURN);
            let value = null;
            if (this.current() && this.current().type !== TT.SEMICOLON &&
                this.current().type !== TT.RBRACE && this.current().type !== TT.EOF) {
                value = this.parseExpression();
            }
            return { type: 'return', value: value };
        }

        parseBlock() {
            this.eat(TT.LBRACE);
            const stmts = [];
            while (this.current() && this.current().type !== TT.RBRACE && this.current().type !== TT.EOF) {
                const s = this.parseStatement();
                if (s) stmts.push(s);
            }
            this.eat(TT.RBRACE);
            return { type: 'block', statements: stmts };
        }

        parseBlock_or_Statement() {
            if (this.current() && this.current().type === TT.LBRACE) return this.parseBlock();
            const s = this.parseStatement();
            return { type: 'block', statements: s ? [s] : [] };
        }

        parseMemberSet() {
            let obj;
            if (this.current() && this.current().type === TT.SELF) {
                this.eat(TT.SELF);
                obj = { type: 'ident', name: 'self' };
            } else {
                const t = this.eatName();
                obj = { type: 'ident', name: t.value };
            }
            this.eat(TT.DOT);
            const memberTok = this.eatName();
            this.eat(TT.EQUALS);
            const value = this.parseExpression();
            return { type: 'memberSet', object: obj, member: memberTok.value, value: value };
        }

        parseArrayIndex() {
            const nameTok = this.eatName();
            const indices = [];
            while (this.current() && this.current().type === TT.LBRACKET) {
                this.eat(TT.LBRACKET);
                indices.push(this.parseExpression());
                this.eat(TT.RBRACKET);
            }
            if (this.current() && this.current().type === TT.EQUALS) {
                this.eat(TT.EQUALS);
                const value = this.parseExpression();
                return { type: 'arraySet', name: nameTok.value, indices: indices, value: value };
            }
            return { type: 'expr', expr: { type: 'arrayGet', name: nameTok.value, indices: indices } };
        }

        parseArrowBind() {
            const objTok = this.eatName();
            this.eat(TT.DOT);
            const evtTok = this.eatName();
            this.eat(TT.ARROW);
            const handlerTok = this.eatName();
            if (this.current() && this.current().type === TT.LPAREN) {
                this.eat(TT.LPAREN);
                if (this.current() && this.current().type !== TT.RPAREN) this.parseExpression();
                this.eat(TT.RPAREN);
            }
            return { type: 'arrowBind', object: objTok.value, event: evtTok.value, handler: handlerTok.value };
        }

        parseAPlus() {
            this.eat(TT.A_PLUS);
            this.eat(TT.NEW);
            const classTok = this.eatName();
            const varTok = this.eatName();
            return { type: 'aplus', className: classTok.value, varName: varTok.value };
        }

        parseTry() {
            this.eat(TT.TRY);
            const body = this.parseBlock_or_Statement();
            let catchVar = null, catchBody = null;
            if (this.current() && this.current().type === TT.CATCH) {
                this.eat(TT.CATCH);
                if (this.current() && this.current().type === TT.LPAREN) {
                    this.eat(TT.LPAREN);
                    const t = this.eatName();
                    if (t) catchVar = t.value;
                    this.eat(TT.RPAREN);
                }
                catchBody = this.parseBlock_or_Statement();
            }
            let finallyBody = null;
            if (this.current() && this.current().type === TT.FINALLY) {
                this.eat(TT.FINALLY);
                finallyBody = this.parseBlock_or_Statement();
            }
            return { type: 'try', body: body, catchVar: catchVar, catchBody: catchBody, finallyBody: finallyBody };
        }

        parseSwitch() {
            this.eat(TT.SWITCH);
            this.eat(TT.LPAREN);
            const expr = this.parseExpression();
            this.eat(TT.RPAREN);
            this.eat(TT.LBRACE);
            const cases = [];
            let defaultCase = null;
            while (this.current() && this.current().type !== TT.RBRACE && this.current().type !== TT.EOF) {
                if (this.current().type === TT.CASE) {
                    this.eat(TT.CASE);
                    const val = this.parseExpression();
                    this.eat(TT.COLON);
                    const stmts = [];
                    while (this.current() && ![TT.CASE, TT.DEFAULT, TT.RBRACE, TT.EOF].includes(this.current().type)) {
                        const s = this.parseStatement();
                        if (s) stmts.push(s);
                    }
                    cases.push({ value: val, statements: stmts });
                } else if (this.current().type === TT.DEFAULT) {
                    this.eat(TT.DEFAULT);
                    this.eat(TT.COLON);
                    const stmts = [];
                    while (this.current() && ![TT.CASE, TT.DEFAULT, TT.RBRACE, TT.EOF].includes(this.current().type)) {
                        const s = this.parseStatement();
                        if (s) stmts.push(s);
                    }
                    defaultCase = { statements: stmts };
                } else break;
            }
            this.eat(TT.RBRACE);
            return { type: 'switch', expr: expr, cases: cases, defaultCase: defaultCase };
        }

        parseClassDef() {
            const nameTok = this.eatName();
            const name = nameTok ? nameTok.value : '?';
            let parent = null;
            if (this.current() && this.current().type === TT.SLASH) {
                this.eat(TT.SLASH);
                const pTok = this.eatName();
                if (pTok) parent = pTok.value;
            }
            this.eat(TT.LBRACE);
            const fields = [], methods = [];
            while (this.current() && this.current().type !== TT.RBRACE && this.current().type !== TT.EOF) {
                let access = 'public';
                while (this.current() && [TT.PUBLIC, TT.PRIVATE, TT.STATIC, TT.ABSTRACT].includes(this.current().type)) {
                    access = this.current().value;
                    this.pos++;
                }
                if (this.current() && this.current().type === TT.LET) {
                    this.eat(TT.LET);
                    const fTok = this.eatName();
                    let fVal = null;
                    if (this.current() && this.current().type === TT.EQUALS) {
                        this.eat(TT.EQUALS);
                        fVal = this.parseExpression();
                    }
                    fields.push({ name: fTok.value, value: fVal, access: access });
                } else if (this.current() && this.current().type === TT.FUNCTION) {
                    this.eat(TT.FUNCTION);
                    const mTok = this.eatName();
                    const mName = mTok ? mTok.value : '?';
                    this.eat(TT.LPAREN);
                    const params = [];
                    if (this.current() && this.current().type !== TT.RPAREN) {
                        let p = this.eatName();
                        if (p) params.push(p.value);
                        while (this.current() && this.current().type === TT.COMMA) {
                            this.eat(TT.COMMA);
                            p = this.eatName();
                            if (p) params.push(p.value);
                        }
                    }
                    this.eat(TT.RPAREN);
                    const mBody = this.parseBlock_or_Statement();
                    methods.push({ name: mName, params: params, body: mBody, access: access });
                }
            }
            this.eat(TT.RBRACE);
            return { type: 'class', name: name, parent: parent, fields: fields, methods: methods };
        }

        skipImport() {
            this.eat(TT.IMPORT);
            if (this.current() && this.current().type === TT.STRING) {
                this.eat(TT.STRING);
                if (this.current() && this.current().type === TT.AS) { this.eat(TT.AS); this.eatName(); }
            } else {
                if (this.current() && this.current().type === TT.LBRACE) {
                    this.eat(TT.LBRACE);
                    while (this.current() && this.current().type !== TT.RBRACE) { this.pos++; }
                    this.eat(TT.RBRACE);
                } else {
                    this.eatName();
                    while (this.current() && this.current().type === TT.COMMA) { this.eat(TT.COMMA); this.eatName(); }
                }
                if (this.current() && this.current().type === TT.FROM) { this.eat(TT.FROM); this.eat(TT.STRING); }
            }
        }

        skipSpawn() {
            this.eat(TT.SPAWN);
            this.eat(TT.FUNCTION);
            this.eatName();
            this.eat(TT.LPAREN);
            if (this.current() && this.current().type !== TT.RPAREN) {
                this.eatName();
                while (this.current() && this.current().type === TT.COMMA) { this.eat(TT.COMMA); this.eatName(); }
            }
            this.eat(TT.RPAREN);
            if (this.current() && this.current().type === TT.LBRACE) this.parseBlock();
            else this.parseExpression();
        }

        skipExtern() {
            this.eat(TT.EXTERN);
            this.eat(TT.FUNCTION);
            this.eatName();
            this.eat(TT.LPAREN);
            if (this.current() && this.current().type !== TT.RPAREN) {
                this.eatName();
                while (this.current() && this.current().type === TT.COMMA) { this.eat(TT.COMMA); this.eatName(); }
            }
            this.eat(TT.RPAREN);
            this.eat(TT.FROM);
            this.eat(TT.STRING);
        }

        // ============================================================
        // Expression parsing (precedence climbing)
        // ============================================================
        parseExpression() {
            let expr = this.parsePipe();
            if (this.current() && this.current().type === TT.QUESTION) {
                this.eat(TT.QUESTION);
                const thenExpr = this.parsePipe();
                this.eat(TT.COLON);
                const elseExpr = this.parsePipe();
                expr = { type: 'ternary', condition: expr, then: thenExpr, else: elseExpr };
            }
            return expr;
        }

        parsePipe() {
            let expr = this.parseOr();
            while (this.current() && this.current().type === TT.PIPE) {
                this.eat(TT.PIPE);
                const right = this.parseOr();
                expr = { type: 'pipe', left: expr, right: right };
            }
            return expr;
        }

        parseOr() {
            let expr = this.parseAnd();
            while (this.current() && this.current().type === TT.OR) {
                this.eat(TT.OR);
                const right = this.parseAnd();
                expr = { type: 'binary', op: 'or', left: expr, right: right };
            }
            return expr;
        }

        parseAnd() {
            let expr = this.parseComparison();
            while (this.current() && this.current().type === TT.AND) {
                this.eat(TT.AND);
                const right = this.parseComparison();
                expr = { type: 'binary', op: 'and', left: expr, right: right };
            }
            return expr;
        }

        parseComparison() {
            let expr = this.parseAddSubtract();
            if (this.current() && [TT.GREATER, TT.LESS, TT.DOUBLE_EQUALS, TT.NOT_EQUALS, TT.LESS_EQUALS, TT.GREATER_EQUALS].includes(this.current().type)) {
                const op = this.current().value;
                this.pos++;
                const right = this.parseAddSubtract();
                expr = { type: 'binary', op: op, left: expr, right: right };
            }
            return expr;
        }

        parseAddSubtract() {
            let expr = this.parseTerm();
            while (this.current() && (this.current().type === TT.PLUS || this.current().type === TT.MINUS)) {
                const op = this.current().value;
                this.pos++;
                const right = this.parseTerm();
                expr = { type: 'binary', op: op, left: expr, right: right };
            }
            return expr;
        }

        parseTerm() {
            let expr = this.parseUnary();
            while (this.current() && (this.current().type === TT.STAR || this.current().type === TT.SLASH || this.current().type === TT.PERCENT)) {
                const op = this.current().value;
                this.pos++;
                const right = this.parseUnary();
                expr = { type: 'binary', op: op, left: expr, right: right };
            }
            return expr;
        }

        parseUnary() {
            if (this.current() && (this.current().type === TT.MINUS || this.current().type === TT.PLUS || this.current().type === TT.NOT || this.current().type === TT.BANG)) {
                const op = this.current().value;
                this.pos++;
                const operand = this.parseUnary();
                return { type: 'unary', op: op, operand: operand };
            }
            return this.parseCall();
        }

        parseCall() {
            let expr = this.parsePrimary();
            while (true) {
                if (this.current() && this.current().type === TT.LPAREN) {
                    this.eat(TT.LPAREN);
                    const args = [];
                    if (this.current() && this.current().type !== TT.RPAREN) {
                        args.push(this.parseArg());
                        while (this.current() && this.current().type === TT.COMMA) {
                            this.eat(TT.COMMA);
                            args.push(this.parseArg());
                        }
                    }
                    this.eat(TT.RPAREN);
                    expr = { type: 'call', callee: expr, args: args };
                } else if (this.current() && this.current().type === TT.DOT) {
                    this.eat(TT.DOT);
                    const memberTok = this.eatName();
                    if (this.current() && this.current().type === TT.LPAREN) {
                        this.eat(TT.LPAREN);
                        const args = [];
                        if (this.current() && this.current().type !== TT.RPAREN) {
                            args.push(this.parseArg());
                            while (this.current() && this.current().type === TT.COMMA) {
                                this.eat(TT.COMMA);
                                args.push(this.parseArg());
                            }
                        }
                        this.eat(TT.RPAREN);
                        expr = { type: 'call', callee: { type: 'dot', object: expr, member: memberTok.value }, args: args };
                    } else {
                        expr = { type: 'dot', object: expr, member: memberTok.value };
                    }
                } else if (this.current() && this.current().type === TT.LBRACKET) {
                    this.eat(TT.LBRACKET);
                    const index = this.parseExpression();
                    this.eat(TT.RBRACKET);
                    expr = { type: 'index', object: expr, index: index };
                } else {
                    break;
                }
            }
            return expr;
        }

        parseArg() {
            if (this.current() && [TT.IDENTIFIER].includes(this.current().type) &&
                this.peek(1) && this.peek(1).type === TT.EQUALS) {
                const nameTok = this.eatName();
                this.eat(TT.EQUALS);
                const val = this.parseExpression();
                return { type: 'namedArg', name: nameTok.value, value: val };
            }
            return this.parseExpression();
        }

        parsePrimary() {
            const t = this.current();
            if (!t) return { type: 'nil' };

            if (t.type === TT.NUMBER) { this.pos++; return { type: 'number', value: parseFloat(t.value) }; }
            if (t.type === TT.STRING) { this.pos++; return { type: 'string', value: t.value }; }
            if (t.type === TT.TRUE) { this.pos++; return { type: 'bool', value: true }; }
            if (t.type === TT.FALSE) { this.pos++; return { type: 'bool', value: false }; }
            if (t.type === TT.NIL) { this.pos++; return { type: 'nil' }; }
            if (t.type === TT.SELF) { this.pos++; return { type: 'ident', name: 'self' }; }

            if (t.type === TT.NEW) {
                this.eat(TT.NEW);
                const cTok = this.eatName();
                const cName = cTok ? cTok.value : '?';
                this.eat(TT.LPAREN);
                const args = [];
                if (this.current() && this.current().type !== TT.RPAREN) {
                    args.push(this.parseExpression());
                    while (this.current() && this.current().type === TT.COMMA) {
                        this.eat(TT.COMMA);
                        args.push(this.parseExpression());
                    }
                }
                this.eat(TT.RPAREN);
                return { type: 'new', className: cName, args: args };
            }

            if (t.type === TT.IDENTIFIER) {
                this.pos++;
                return { type: 'ident', name: t.value };
            }

            if (t.type === TT.LPAREN) {
                this.eat(TT.LPAREN);
                const expr = this.parseExpression();
                this.eat(TT.RPAREN);
                return expr;
            }

            if (t.type === TT.LBRACKET) {
                this.eat(TT.LBRACKET);
                const elements = [];
                if (this.current() && this.current().type !== TT.RBRACKET) {
                    elements.push(this.parseExpression());
                    while (this.current() && this.current().type === TT.COMMA) {
                        this.eat(TT.COMMA);
                        elements.push(this.parseExpression());
                    }
                }
                this.eat(TT.RBRACKET);
                return { type: 'array', elements: elements };
            }

            this.errors.push('Unexpected expression at line ' + (t.line || '?'));
            this.pos++;
            return { type: 'nil' };
        }
    }

    // ============================================================
    // 6. Interpreter
    // ============================================================
    class ReturnSignal extends Error { constructor(val) { super('return'); this.value = val; } }
    class BreakSignal extends Error { constructor() { super('break'); } }
    class ContinueSignal extends Error { constructor() { super('continue'); } }

    class Environment {
        constructor(parent) {
            this.parent = parent;
            this.vars = {};
            this.funcs = {};
            this.classes = {};
        }

        defineVar(name, value) { this.vars[name] = value; }

        getVar(name) {
            if (name in this.vars) return this.vars[name];
            if (this.parent) return this.parent.getVar(name);
            throw new Error('Variable not defined: ' + name);
        }

        setVar(name, value) {
            if (name in this.vars) { this.vars[name] = value; return; }
            if (this.parent) { this.parent.setVar(name, value); return; }
            throw new Error('Variable not defined: ' + name);
        }

        defineFunc(name, func) { this.funcs[name] = func; }

        getFunc(name) {
            if (name in this.funcs) return this.funcs[name];
            if (this.parent) return this.parent.getFunc(name);
            throw new Error('Function not defined: ' + name);
        }

        defineClass(name, cls) { this.classes[name] = cls; }

        getClass(name) {
            if (name in this.classes) return this.classes[name];
            if (this.parent) return this.parent.getClass(name);
            throw new Error('Class not defined: ' + name);
        }

        hasVar(name) {
            if (name in this.vars) return true;
            if (this.parent) return this.parent.hasVar(name);
            return false;
        }

        child() { return new Environment(this); }
    }

    class AObject {
        constructor(clsName, fields, methods) {
            this._className = clsName;
            this._fields = {};
            for (const f of fields) this._fields[f.name] = f.value;
            this._methods = {};
            for (const m of methods) this._methods[m.name] = m;
        }
    }

    // WebUIElement — lightweight UI container for the web runtime
    class WebUIElement {
        constructor(tag) {
            this._tag = tag;
            this._fields = {};
            this._children = [];
            this._methods = {};
        }
        add(child) { this._children.push(child); return this; }
        toString() { return '<' + this._tag + '>'; }
    }

    class Interpreter {
        constructor(printFn, inputFn) {
            this.globalEnv = new Environment(null);
            this.env = this.globalEnv;
            this.printFn = printFn || console.log;
            this.inputFn = inputFn || null;
            this.inLoop = false;

            // Built-in functions
            const g = this.globalEnv;
            g.defineFunc('len', function (s) { return s.length; });
            g.defineFunc('str', function (v) { return String(v); });
            g.defineFunc('num', function (v) { return Number(v); });
            g.defineFunc('int', function (v) { return parseInt(v, 10); });
            g.defineFunc('type', function (v) { return typeof v; });
            g.defineFunc('wait', function () { return null; });
            g.defineFunc('readNumber', function () { return 0; });
            g.defineFunc('input', function () { return ''; });
        }

        interpret(stmts) {
            let result = null;
            for (const stmt of stmts) {
                result = this.exec(stmt);
            }
            return result;
        }

        exec(stmt) {
            if (!stmt) return null;
            switch (stmt.type) {
                case 'let': return this.execLet(stmt);
                case 'assign': return this.execAssign(stmt);
                case 'assignOp': return this.execAssignOp(stmt);
                case 'print': return this.execPrint(stmt);
                case 'if': return this.execIf(stmt);
                case 'while': return this.execWhile(stmt);
                case 'for': return this.execFor(stmt);
                case 'forIn': return this.execForIn(stmt);
                case 'func': return this.execFuncDef(stmt);
                case 'return': throw new ReturnSignal(this.evalExpr(stmt.value));
                case 'break': throw new BreakSignal();
                case 'continue': throw new ContinueSignal();
                case 'block': return this.execBlock(stmt);
                case 'expr': return this.evalExpr(stmt.expr);
                case 'memberSet': return this.execMemberSet(stmt);
                case 'arraySet': return this.execArraySet(stmt);
                case 'arrowBind': return this.execArrowBind(stmt);
                case 'aplus': {
                    const elem = new WebUIElement(stmt.className);
                    this.env.defineVar(stmt.varName, elem);
                    return elem;
                }
                case 'debugger': return null;
                case 'try': return this.execTry(stmt);
                case 'switch': return this.execSwitch(stmt);
                case 'class': return this.execClassDef(stmt);
                default: return null;
            }
        }

        execLet(stmt) {
            const val = stmt.value ? this.evalExpr(stmt.value) : null;
            this.env.defineVar(stmt.name, val);
            return val;
        }

        execAssign(stmt) {
            const val = this.evalExpr(stmt.value);
            if (this.env.hasVar(stmt.name)) {
                this.env.setVar(stmt.name, val);
            } else {
                this.env.defineVar(stmt.name, val);
            }
            return val;
        }

        execAssignOp(stmt) {
            const curr = this.env.getVar(stmt.name);
            const val = this.evalExpr(stmt.value);
            let result;
            switch (stmt.op) {
                case '+=': result = curr + val; break;
                case '-=': result = curr - val; break;
                case '*=': result = curr * val; break;
                case '/=': result = curr / val; break;
                default: result = val;
            }
            this.env.setVar(stmt.name, result);
            return result;
        }

        execPrint(stmt) {
            const val = stmt.arg ? this.evalExprAsString(stmt.arg) : '';
            this.printFn(val);
            return val;
        }

        execIf(stmt) {
            const cond = this.evalExpr(stmt.condition);
            if (cond) {
                return this.execBlock(stmt.then);
            } else if (stmt.else) {
                return this.execBlock(stmt.else);
            }
            return null;
        }

        execWhile(stmt) {
            const prev = this.inLoop;
            this.inLoop = true;
            try {
                while (this.evalExpr(stmt.condition)) {
                    try {
                        this.execBlock(stmt.body);
                    } catch (e) {
                        if (e instanceof BreakSignal) break;
                        if (e instanceof ContinueSignal) continue;
                        throw e;
                    }
                }
            } finally {
                this.inLoop = prev;
            }
            return null;
        }

        execFor(stmt) {
            const prev = this.inLoop;
            this.inLoop = true;
            try {
                if (stmt.init) this.exec(stmt.init);
                while (stmt.condition ? this.evalExpr(stmt.condition) : true) {
                    try {
                        this.execBlock(stmt.body);
                    } catch (e) {
                        if (e instanceof BreakSignal) break;
                        if (e instanceof ContinueSignal) { if (stmt.inc) this.exec(stmt.inc); continue; }
                        throw e;
                    }
                    if (stmt.inc) this.exec(stmt.inc);
                }
            } finally {
                this.inLoop = prev;
            }
            return null;
        }

        execForIn(stmt) {
            const iterable = this.evalExpr(stmt.iterable);
            const prev = this.inLoop;
            this.inLoop = true;
            try {
                for (let i = 0; i < iterable.length; i++) {
                    this.env.defineVar(stmt.name, iterable[i]);
                    try {
                        this.execBlock(stmt.body);
                    } catch (e) {
                        if (e instanceof BreakSignal) break;
                        if (e instanceof ContinueSignal) continue;
                        throw e;
                    }
                }
            } finally {
                this.inLoop = prev;
            }
            return null;
        }

        execFuncDef(stmt) {
            const interp = this;
            const fn = function (...args) {
                const prevEnv = interp.env;
                const funcEnv = interp.globalEnv.child();
                interp.env = funcEnv;
                for (let i = 0; i < stmt.params.length; i++) {
                    funcEnv.defineVar(stmt.params[i], i < args.length ? args[i] : null);
                }
                try {
                    interp.execBlock(stmt.body);
                    return null;
                } catch (e) {
                    if (e instanceof ReturnSignal) return e.value;
                    throw e;
                } finally {
                    interp.env = prevEnv;
                }
            };
            this.env.defineFunc(stmt.name, fn);
            this.env.defineVar(stmt.name, fn);
            return fn;
        }

        execBlock(block) {
            if (!block || !block.statements) return null;
            let result = null;
            for (const stmt of block.statements) {
                result = this.exec(stmt);
            }
            return result;
        }

        execMemberSet(stmt) {
            const obj = this.evalExpr(stmt.object);
            const val = this.evalExpr(stmt.value);
            if (typeof obj === 'object' && obj !== null) {
                if (obj._fields) { obj._fields[stmt.member] = val; }
                else { obj[stmt.member] = val; }
            }
            return val;
        }

        execArraySet(stmt) {
            const arr = this.env.getVar(stmt.name);
            let target = arr;
            for (let i = 0; i < stmt.indices.length - 1; i++) {
                target = target[this.evalExpr(stmt.indices[i])];
            }
            const lastIdx = this.evalExpr(stmt.indices[stmt.indices.length - 1]);
            target[lastIdx] = this.evalExpr(stmt.value);
            return target[lastIdx];
        }

        execArrowBind(stmt) {
            let obj;
            try {
                obj = this.env.getVar(stmt.object);
            } catch (e) {
                if (stmt.object === '__root') {
                    obj = new WebUIElement('Root');
                    this.env.defineVar('__root', obj);
                } else {
                    throw e;
                }
            }
            let handler;
            try { handler = this.env.getFunc(stmt.handler); }
            catch (e) { handler = this.env.getVar(stmt.handler); }
            if (typeof obj === 'object' && obj !== null) {
                if (obj._methods) obj._methods[stmt.event] = handler;
                else obj[stmt.event] = handler;
            }
            return handler;
        }

        execTry(stmt) {
            try {
                this.execBlock(stmt.body);
            } catch (e) {
                if (stmt.catchVar && stmt.catchBody) {
                    this.env.defineVar(stmt.catchVar, e.message || String(e));
                    this.execBlock(stmt.catchBody);
                }
            } finally {
                if (stmt.finallyBody) this.execBlock(stmt.finallyBody);
            }
            return null;
        }

        execSwitch(stmt) {
            const val = this.evalExpr(stmt.expr);
            let matched = false;
            for (const c of stmt.cases) {
                const cv = this.evalExpr(c.value);
                if (val === cv) {
                    for (const s of c.statements) this.exec(s);
                    matched = true;
                    break;
                }
            }
            if (!matched && stmt.defaultCase) {
                for (const s of stmt.defaultCase.statements) this.exec(s);
            }
            return null;
        }

        execClassDef(stmt) {
            const cls = { name: stmt.name, parent: stmt.parent, fields: stmt.fields, methods: stmt.methods };
            this.env.defineClass(stmt.name, cls);
            this.env.defineVar(stmt.name, cls);
            return cls;
        }

        // ============================================================
        // Expression evaluation
        // ============================================================
        evalExpr(node) {
            if (!node) return null;
            switch (node.type) {
                case 'number': return node.value;
                case 'string': return node.value;
                case 'bool': return node.value;
                case 'nil': return null;
                case 'ident': return this.evalIdent(node);
                case 'binary': return this.evalBinary(node);
                case 'unary': return this.evalUnary(node);
                case 'call': return this.evalCall(node);
                case 'dot': return this.evalDot(node);
                case 'index': return this.evalIndex(node);
                case 'array': return this.evalArray(node);
                case 'new': return this.evalNew(node);
                case 'ternary': return this.evalTernary(node);
                case 'pipe': return this.evalPipe(node);
                case 'namedArg': return this.evalExpr(node.value);
                default: return null;
            }
        }

        evalExprAsString(node) {
            const v = this.evalExpr(node);
            if (v === null || v === undefined) return '';
            return String(v);
        }

        evalIdent(node) {
            if (node.name === 'self') return this.env;
            if (node.name === '__root') {
                try { return this.env.getVar('__root'); }
                catch (e) {
                    const root = new WebUIElement('Root');
                    this.env.defineVar('__root', root);
                    return root;
                }
            }
            try { return this.env.getVar(node.name); }
            catch (e) {
                try { return this.env.getFunc(node.name); }
                catch (e2) { throw new Error('Name not found: ' + node.name); }
            }
        }

        evalBinary(node) {
            const left = this.evalExpr(node.left);
            const right = this.evalExpr(node.right);
            switch (node.op) {
                case '+': return left + right;
                case '-': return left - right;
                case '*': return left * right;
                case '/': return left / right;
                case '%': return left % right;
                case '>': return left > right;
                case '<': return left < right;
                case '==': return left === right;
                case '!=': return left !== right;
                case '>=': return left >= right;
                case '<=': return left <= right;
                case 'and': return left && right;
                case 'or': return left || right;
                default: return null;
            }
        }

        evalUnary(node) {
            const operand = this.evalExpr(node.operand);
            switch (node.op) {
                case '-': return -operand;
                case '+': return +operand;
                case '!':
                case 'not': return !operand;
                default: return operand;
            }
        }

        evalCall(node) {
            const callee = this.evalExpr(node.callee);
            const args = node.args.map(a => this.evalExpr(a));
            // Handle method calls with object context
            if (node.callee.type === 'dot') {
                const obj = this.evalExpr(node.callee.object);
                const methodName = node.callee.member;
                if (obj && typeof obj[methodName] === 'function') return obj[methodName](...args);
                if (obj && obj._methods && obj._methods[methodName]) {
                    const m = obj._methods[methodName];
                    const interp = this;
                    const fn2 = function (...margs) {
                        const prevEnv = interp.env;
                        const funcEnv = interp.globalEnv.child();
                        funcEnv.defineVar('self', obj);
                        interp.env = funcEnv;
                        for (let i = 0; i < m.params.length; i++) {
                            funcEnv.defineVar(m.params[i], i < margs.length ? margs[i] : null);
                        }
                        try {
                            interp.execBlock(m.body);
                            return null;
                        } catch (e) {
                            if (e instanceof ReturnSignal) return e.value;
                            throw e;
                        } finally {
                            interp.env = prevEnv;
                        }
                    };
                    return fn2(...args);
                }
                return null;
            }
            if (typeof callee === 'function') return callee(...args);
            throw new Error('Cannot call: ' + typeof callee);
        }

        evalDot(node) {
            const obj = this.evalExpr(node.object);
            if (obj === null || obj === undefined) throw new Error('Cannot access member of null');
            if (typeof obj === 'string') {
                if (node.member === 'len') return obj.length;
                const fn = obj[node.member];
                if (typeof fn === 'function') return fn.bind(obj);
                return obj[node.member];
            }
            if (typeof obj === 'number' || typeof obj === 'boolean') {
                return obj[node.member];
            }
            if (typeof obj === 'object') {
                if (Array.isArray(obj) && node.member === 'len') return obj.length;
                if (obj._fields && obj._fields[node.member] !== undefined) return obj._fields[node.member];
                if (obj._methods && obj._methods[node.member]) return obj._methods[node.member];
                return obj[node.member];
            }
            return obj[node.member];
        }

        evalIndex(node) {
            const obj = this.evalExpr(node.object);
            const index = this.evalExpr(node.index);
            return obj[index];
        }

        evalArray(node) {
            return node.elements.map(e => this.evalExpr(e));
        }

        evalNew(node) {
            try {
                const cls = this.env.getClass(node.className);
                const instance = new AObject(cls.name, cls.fields, cls.methods);
                // Run constructor if exists
                if (instance._methods['init']) {
                    const m = instance._methods['init'];
                    const interp = this;
                    const prevEnv = interp.env;
                    const funcEnv = interp.globalEnv.child();
                    funcEnv.defineVar('self', instance);
                    interp.env = funcEnv;
                    for (let i = 0; i < m.params.length; i++) {
                        funcEnv.defineVar(m.params[i], i < node.args.length ? this.evalExpr(node.args[i]) : null);
                    }
                    try { interp.execBlock(m.body); }
                    catch (e) { if (!(e instanceof ReturnSignal)) throw e; }
                    finally { interp.env = prevEnv; }
                }
                return instance;
            } catch (e) {
                // Try JS built-in constructor
                const args = node.args.map(a => this.evalExpr(a));
                const Ctor = this.env.getVar(node.className);
                if (typeof Ctor === 'function') return new Ctor(...args);
                throw e;
            }
        }

        evalTernary(node) {
            const cond = this.evalExpr(node.condition);
            return cond ? this.evalExpr(node.then) : this.evalExpr(node.else);
        }

        evalPipe(node) {
            const left = this.evalExpr(node.left);
            const right = node.right;
            if (right.type === 'call') {
                const args = right.args.map(a => this.evalExpr(a));
                const fn = this.evalExpr(right.callee);
                if (typeof fn === 'function') return fn(left, ...args);
            } else {
                const fn = this.evalExpr(right);
                if (typeof fn === 'function') return fn(left);
            }
            return left;
        }
    }

    // ============================================================
    // 7. Entry Point
    // ============================================================
    globalThis.aplusRun = function (code, callbacks) {
        const printFn = (callbacks && callbacks.print) || console.log;
        const inputFn = (callbacks && callbacks.input) || null;

        // Step 1: Preprocess
        code = shorthandConvert(code);

        // Step 2: Tokenize
        const errors = [];
        const tokens = tokenize(code, errors);
        if (tokens.length === 0) {
            printFn('[A+ Error] Empty program');
            return;
        }

        // Step 3: Parse
        const parser = new Parser(tokens, errors);
        const stmts = parser.parseProgram();

        // Step 4: Interpret
        const interp = new Interpreter(printFn, inputFn);
        try {
            interp.interpret(stmts);
        } catch (e) {
            if (e instanceof ReturnSignal) { /* top-level return */ }
            else if (e instanceof BreakSignal) { /* top-level break */ }
            else if (e instanceof ContinueSignal) { /* top-level continue */ }
            else {
                printFn('[A+ Error] ' + e.message);
            }
        }
    };

    // REPL evaluation (single expression)
    globalThis.aplusEval = function (code) {
        const errors = [];
        const tokens = tokenize(code, errors);
        if (tokens.length === 0) return null;
        const parser = new Parser(tokens, errors);
        const stmts = parser.parseProgram();
        const interp = new Interpreter(function () {}, null);
        try {
            const result = interp.interpret(stmts);
            return result !== undefined ? result : null;
        } catch (e) {
            throw new Error(e.message);
        }
    };

    // Run a file by URL (for fetch-based loading)
    globalThis.aplusRunFile = async function (url) {
        const resp = await fetch(url);
        const code = await resp.text();
        globalThis.aplusRun(code);
    };

})();
