const TokenType = Object.freeze({
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
    EXTENDS: 70,
    // Module System
    EXPORT: 71, IMPORT: 72, AS: 73,
    // Async/Await
    ASYNC: 74, AWAIT: 75, GO: 76, SPAWN: 77,
    // Type System
    NIL: 78, PIPE: 79, REGEX: 81,
    // Debugger
    DEBUGGER: 80,
    // 3D and Media Elements
    VIEWPORT3D: 82, MODELVISUAL3D: 83, MODEL3DGROUP: 84, GEOMETRYMODEL3D: 85, MESHGEOMETRY3D: 86,
    DIRECTIONALLIGHT: 87, AMBIENTLIGHT: 88, POINTLIGHT: 89, SPOTLIGHT: 90,
    PERSPECTIVECAMERA: 91, ORTHOGRAPHICCAMERA: 92,
    MEDIAELEMENT: 93
});

const KW = {
    'let': TokenType.LET, 'م': TokenType.LET, 'متغير': TokenType.LET,
    'print': TokenType.PRINT, 'ط': TokenType.PRINT, 'اطبع': TokenType.PRINT,
    'if': TokenType.IF, 'لو': TokenType.IF, 'إذا': TokenType.IF,
    'while': TokenType.WHILE, 'ت': TokenType.WHILE, 'طالما': TokenType.WHILE,
    'for': TokenType.FOR, 'لكل': TokenType.FOR,
    'else': TokenType.ELSE, 'وإلا': TokenType.ELSE,
    'func': TokenType.FUNCTION, 'function': TokenType.FUNCTION, 'دالة': TokenType.FUNCTION,
    'return': TokenType.RETURN, 'ارجع': TokenType.RETURN, 'عود': TokenType.RETURN, 'رجع': TokenType.RETURN,
    'and': TokenType.AND, 'و': TokenType.AND,
    'or': TokenType.OR, 'أو': TokenType.OR,
    'true': TokenType.TRUE, 'false': TokenType.FALSE,
    'صحيح': TokenType.TRUE, 'خاطئ': TokenType.FALSE,
    'class': TokenType.CLASS, 'صنف': TokenType.CLASS, 'فئة': TokenType.CLASS,
    'new': TokenType.NEW, 'جديد': TokenType.NEW,
    'self': TokenType.SELF, 'this': TokenType.SELF, 'نفس': TokenType.SELF,
    'هذا': TokenType.SELF,
    'public': TokenType.PUBLIC, 'عام': TokenType.PUBLIC,
    'private': TokenType.PRIVATE, 'خاص': TokenType.PRIVATE,
    'static': TokenType.STATIC, 'ثابت': TokenType.STATIC,
    'abstract': TokenType.ABSTRACT, 'مجرد': TokenType.ABSTRACT,
    'interface': TokenType.INTERFACE, 'واجهة': TokenType.INTERFACE,
    'include': TokenType.INCLUDE, 'استدعاء': TokenType.INCLUDE, 'ضم': TokenType.INCLUDE,
    'from': TokenType.FROM, 'من': TokenType.FROM,
    'throw': TokenType.THROW, 'ارم': TokenType.THROW,
    'try': TokenType.TRY, 'حاول': TokenType.TRY, 'محاولة': TokenType.TRY,
    'catch': TokenType.CATCH, 'التقط': TokenType.CATCH, 'القط': TokenType.CATCH,
    'show': TokenType.SHOW, 'اعرض': TokenType.SHOW,
    'extern': TokenType.EXTERN, 'خارجي': TokenType.EXTERN,
    'not': TokenType.NOT, 'ليس': TokenType.NOT,
    'break': TokenType.BREAK, 'continue': TokenType.CONTINUE,
    'توقف': TokenType.BREAK, 'كسر': TokenType.BREAK, 'استمر': TokenType.CONTINUE,
    'switch': TokenType.SWITCH, 'اختيار': TokenType.SWITCH,
    'case': TokenType.CASE, 'حالة': TokenType.CASE,
    'default': TokenType.DEFAULT, 'افتراضي': TokenType.DEFAULT,
    'in': TokenType.IN, 'في': TokenType.IN,
    'extends': TokenType.EXTENDS, 'يرث': TokenType.EXTENDS,
    'finally': TokenType.FINALLY, 'وأخيراً': TokenType.FINALLY, 'أخيرا': TokenType.FINALLY,
    'بينما': TokenType.WHILE,
    // Module System
    'export': TokenType.EXPORT, 'صدر': TokenType.EXPORT,
    'import': TokenType.IMPORT, 'استورد': TokenType.IMPORT,
    'as': TokenType.AS, 'كـ': TokenType.AS,
    // Async/Await
    'async': TokenType.ASYNC, 'غيرمتزامن': TokenType.ASYNC,
    'await': TokenType.AWAIT, 'انتظر': TokenType.AWAIT,
    'go': TokenType.GO, 'انطلق': TokenType.GO,
    'spawn': TokenType.SPAWN, 'أنشئ': TokenType.SPAWN,
    // Type System
    'nil': TokenType.NIL, 'عدم': TokenType.NIL,
    // Debugger
    'debugger': TokenType.DEBUGGER, 'مصحح': TokenType.DEBUGGER,
    // 3D and Media Elements
    'viewport3d': TokenType.VIEWPORT3D, 'منظور3d': TokenType.VIEWPORT3D,
    'mediaelement': TokenType.MEDIAELEMENT, 'وسائط': TokenType.MEDIAELEMENT, 'عنصر_وسائط': TokenType.MEDIAELEMENT,
};

let __xamlCounter = 0;

function isUpper(c) { return c >= 'A' && c <= 'Z'; }

const arabicTags = {
    // Layout
    'شبكة': 'Grid', 'لوحة_مكدسة': 'StackPanel', 'مكدسة': 'StackPanel',
    'لوحة_ملتفة': 'WrapPanel', 'ملتفة': 'WrapPanel',
    'لوحة_إرساء': 'DockPanel', 'إرساء': 'DockPanel',
    'لوحة_رسم': 'Canvas', 'رسم': 'Canvas',
    'شبكة_موحدة': 'UniformGrid', 'صندوق_عرض': 'Viewbox',
    'حدود': 'Border', 'متصفح_تمرير': 'ScrollViewer', 'تمرير': 'ScrollViewer',
    // Text
    'نص': 'TextBlock', 'صندوق_نص': 'TextBox', 'صندوق_نص_غني': 'RichTextBox',
    'تسمية': 'Label', 'نص_غني': 'RichTextBox',
    // Buttons
    'زر': 'Button', 'زر_تكرار': 'RepeatButton', 'زر_تبديل': 'ToggleButton',
    'مربع_اختيار': 'CheckBox', 'زر_خيار': 'RadioButton',
    // Lists
    'صندوق_قائمة': 'ListBox', 'عرض_قائمة': 'ListView',
    'صندوق_مدمج': 'ComboBox', 'عرض_شجري': 'TreeView',
    'قائمة': 'Menu', 'عنصر_قائمة': 'MenuItem',
    'قائمة_سياق': 'ContextMenu', 'تحكم_تبويب': 'TabControl', 'تبويب': 'TabItem',
    // Image
    'صورة': 'Image', 'عنصر_وسائط': 'MediaElement', 'وسائط': 'MediaElement',
    // Input
    'منزلق': 'Slider', 'شريط_تقدم': 'ProgressBar',
    'منتقي_تاريخ': 'DatePicker', 'تقويم': 'Calendar',
    'صندوق_كلمة_سر': 'PasswordBox',
    // Shapes
    'مستطيل': 'Rectangle', 'قطع_ناقص': 'Ellipse', 'ناقص': 'Ellipse',
    'خط': 'Line', 'مضلع': 'Polygon', 'خط_متعدد': 'Polyline', 'مسار': 'Path',
    // Window
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



function translateArabicXamlJs(code) {
    // Replace Arabic tag names
    for (const [ar, en] of Object.entries(arabicTags)) {
        code = code.replaceAll('<' + ar + ' ', '<' + en + ' ');
        code = code.replaceAll('<' + ar + '>', '<' + en + '>');
        code = code.replaceAll('</' + ar + '>', '</' + en + '>');
        code = code.replaceAll('<' + ar + '/>', '<' + en + '/>');
        code = code.replaceAll('<' + ar + '\n', '<' + en + '\n');
    }
    // Replace Arabic attribute names
    for (const [ar, en] of Object.entries(arabicAttrs)) {
        code = code.replaceAll(ar + '=', en + '=');
    }
    return code;
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
            i = extractXmlElement(code, i);
            let xamlBlock = code.substring(start, i);
            try {
                let parsed = parseSimpleXml(xamlBlock);
                let generated = xamlNodeToAplus(parsed, null);
                result += generated;
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

function extractXmlElement(code, start) {
    let depth = 0, i = start, inStr = false, sq = '"';
    while (i < code.length) {
        let c = code[i];
        if (inStr) { if (c === sq) inStr = false; i++; continue; }
        if (c === '"' || c === "'") { inStr = true; sq = c; i++; continue; }
        if (c === '<') {
            if (i + 1 < code.length && code[i + 1] === '/') { depth--; i = skipXmlTag(code, i); if (depth <= 0) break; }
            else if (i + 1 < code.length && /[_a-zA-Z\u0600-\u06FF]/.test(code[i + 1])) { depth++; i = skipXmlTag(code, i); if (isSelfClosingXml(code, i)) { depth--; if (depth <= 0) break; } }
            else i++;
        } else i++;
    }
    return i;
}

function skipXmlTag(code, start) {
    let inStr = false, sq = '"';
    for (let i = start; i < code.length; i++) {
        if (inStr) { if (code[i] === sq) inStr = false; continue; }
        if (code[i] === '"' || code[i] === "'") { inStr = true; sq = code[i]; continue; }
        if (code[i] === '>') return i + 1;
    }
    return code.length;
}

function isSelfClosingXml(code, end) {
    for (let i = end - 2; i >= 0 && i > end - 6; i--)
        if (code[i] === '/') return true;
    return false;
}

function parseSimpleXml(xml) {
    // returns {tag, attrs:{}, children:[], text:''}
    let root = { tag: '#root', attrs: {}, children: [], text: '' };
    let stack = [root];
    let i = 0;
    while (i < xml.length) {
        if (xml[i] === '<') {
            if (i + 1 < xml.length && xml[i + 1] === '/') {
                // closing tag
                let closeEnd = xml.indexOf('>', i);
                stack.pop();
                i = closeEnd + 1;
            } else if (i + 1 < xml.length && /[_a-zA-Z\u0600-\u06FF]/.test(xml[i + 1])) {
                // opening tag
                let tagEnd = xml.indexOf('>', i);
                let tagContent = xml.substring(i + 1, tagEnd);
                let parts = tagContent.split(/\s+/);
                let tagName = parts[0];
                let attrs = {};
                // Parse attrs: name="val" OR name=val (unquoted)
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
            // text content
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

function xamlNodeToAplus(node, parentVar) {
    if (node.tag === '#root') {
        let r = '';
        for (let child of node.children)
            r += xamlNodeToAplus(child, parentVar);
        return r;
    }
    let v = `__xaml_${__xamlCounter++}`;
    let r = '';
    const isUi = knownUiTypes.has(node.tag.toLowerCase());
    if (isUi) {
        r += `a+ new ${node.tag} ${v}\n`;
        for (let [an, av] of Object.entries(node.attrs)) {
            if (isUpper(an[0]) && /[a-zA-Z]/.test(av[0]) && !/^(id|name|content|text|width|height|margin|padding|fontsize|fontfamily|foreground|background|horizontalalignment|verticalalignment|source|value|minimum|maximum|ischecked|isselected|isenabled|visibility|opacity)$/i.test(an))
                r += `${v}.${an} => ${av}\n`;
            else
                r += `${v}.${an} = "${av.replace(/"/g, '\\"')}"\n`;
        }
        for (let child of node.children)
            r += xamlNodeToAplus(child, v);
        if (node.children.length === 0 && node.text.trim())
            r += `${v}.content = "${node.text.trim().replace(/"/g, '\\"')}"\n`;
        if (parentVar) r += `${parentVar}.add(${v})\n`;
    } else {
        // Function component
        let callArgs = [];
        for (let [an, av] of Object.entries(node.attrs)) {
            if (/^\d+(\.\d+)?$/.test(av))
                callArgs.push(`${an}=${av}`);
            else
                callArgs.push(`${an}="${av.replace(/"/g, '\\"')}"`);
        }
        r += `let ${v} = ${node.tag}(${callArgs.join(', ')})\n`;
        if (node.children.length === 0 && node.text.trim())
            r += `${v}.content = "${node.text.trim().replace(/"/g, '\\"')}"\n`;
        for (let child of node.children)
            r += xamlNodeToAplus(child, v);
        if (parentVar) r += `${parentVar}.add(${v})\n`;
    }
    return r;
}

const OP_MAP = {
    '=': TokenType.EQUALS, '+': TokenType.PLUS, '-': TokenType.MINUS,
    '*': TokenType.STAR, '/': TokenType.SLASH, '%': TokenType.PERCENT,
    '(': TokenType.LPAREN, ')': TokenType.RPAREN,
    '{': TokenType.LBRACE, '}': TokenType.RBRACE,
    ',': TokenType.COMMA, ';': TokenType.SEMICOLON, ':': TokenType.COLON,
    '.': TokenType.DOT,
    '>': TokenType.GREATER, '<': TokenType.LESS,
    '[': TokenType.LBRACKET, ']': TokenType.RBRACKET,
    '?': TokenType.QUESTION, '|': TokenType.PIPE,
};

const TYPE_NAMES = {};
for (const [k, v] of Object.entries(TokenType)) TYPE_NAMES[v] = k;

function shorthandConvert(code) {
    __xamlCounter = 0;
    code = convertXamlJs(code);
    code = code.replace(/(^|[^\w\u0600-\u06FF])م>\s+(?=[\w\u0600-\u06FF])/g, '$1let ');
    code = code.replace(/(^|[^\w\u0600-\u06FF])ط>\s*(?=")/g, '$1print(');
    code = code.replace(/ط>\s*(\w)/g, 'print($1');
    code = code.replace(/(^|[^\w\u0600-\u06FF])لو>\s+/g, '$1if(');
    code = code.replace(/(^|[^\w\u0600-\u06FF])ت>\s+/g, '$1while(');
    code = code.replace(/\bprint>"([^"]*)"\s*;?\s*/g, 'print("$1")\n');
    code = code.replace(/\}\s*_\s*\{/g, '} else {');
    code = code.replace(/\?>([^_]+)_\s*([^;{}]+?)_\s*([^;{}]+?)\s*;/g, (m, cond, b1, b2) =>
        `if(${cond.trim()}) { ${b1.trim()} } else { ${b2.trim()} }`);
    code = code.replace(/\?>([^(]+?)_\s*\{?/g, (m, cond) => `if(${cond.trim()}) {`);
    code = code.replace(/\bif>([^(]+?)_\s*\{?/g, (m, cond) => `if(${cond.trim()}) {`);
    code = code.replace(/\bwhile>([^(]+?)_\s*\{?/g, (m, cond) => `while(${cond.trim()}) {`);
    code = code.replace(/\bfor>(\w+\s*=\s*\d+)\s*>\s*([^>]+?)\s*>\s*\*\s*(\d+)\s*_\s*\{?/g, (m, init, inc, count) => {
        const varName = init.split('=')[0].trim();
        const increment = inc.includes('=') ? inc.trim() : `${varName} = ${inc.trim()}`;
        return `for(${init.trim()}; ${varName} < ${count}; ${increment}) {`;
    });
    code = code.replace(/\bfor>(\w+\s*=\s*\d+)\s*>\s*([^>]+?)\s*>\s*([^>]+?)\s*_\s*\{?/g, (m, init, inc, cond) => {
        const varName = init.split('=')[0].trim();
        const increment = inc.includes('=') ? inc.trim() : `${varName} = ${inc.trim()}`;
        return `for(${init.trim()}; ${cond.trim()}; ${increment}) {`;
    });
    code = code.replace(/^\s*([A-Za-z_\u0600-\u06FF][\w\u0600-\u06FF]*)\s*\+\+\s*;?\s*$/gm, '$1 += 1');
    code = code.replace(/^\s*([A-Za-z_\u0600-\u06FF][\w\u0600-\u06FF]*)\s*--\s*;?\s*$/gm, '$1 -= 1');
    code = code.replace(/([;(]\s*)([A-Za-z_\u0600-\u06FF][\w\u0600-\u06FF]*)\s*\+\+(\s*[;)])/g, '$1$2 += 1$3');
    code = code.replace(/([;(]\s*)([A-Za-z_\u0600-\u06FF][\w\u0600-\u06FF]*)\s*--(\s*[;)])/g, '$1$2 -= 1$3');
    code = code.replace(/<---/g, '');
    return code;
}

function checkCode(code) {
    const errors = [];
    code = code.replace(/^\uFEFF/, '');
    code = shorthandConvert(code);
    const tokens = tokenize(code, errors);
    if (tokens.length === 0) return errors;
    const parser = new Parser(tokens, errors);
    parser.parse();
    return errors;
}

function tokenize(code, errors) {
    const tokens = [];
    let pos = 0, line = 1, col = 1;

    function addTok(type, value) {
        tokens.push({ type, value, line, col, endCol: col + (value ? value.length : 1) - 1 });
    }

    while (pos < code.length) {
        const c = code[pos];
        const startCol = col;

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
                addTok(TokenType.A_PLUS, 'a+');
            } else {
                addTok(TokenType.IDENTIFIER, word);
            }
            continue;
        }

        if (/\d/.test(c)) {
            const nStart = pos;
            let hasDot = false;
            let bad = false;
            while (pos < code.length && /[0-9.]/.test(code[pos])) {
                if (code[pos] === '.') {
                    if (hasDot) {
                        errors.push({ line, column: pos + 1 - (pos - nStart), message: 'رقم عشري غير صالح' });
                        bad = true;
                        break;
                    }
                    hasDot = true;
                }
                pos++;
            }
            col += pos - nStart;
            if (!bad) addTok(TokenType.NUMBER, code.substring(nStart, pos));
            continue;
        }

        if (c === '"') {
            pos++; col++;
            const sStart = pos;
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
            if (pos >= code.length) {
                errors.push({ line, column: col, message: 'النص غير مغلق' });
                continue;
            }
            addTok(TokenType.STRING, str);
            pos++; col++;
            continue;
        }

        if (c === '=' && pos + 1 < code.length && code[pos + 1] === '=') {
            addTok(TokenType.DOUBLE_EQUALS, '=='); pos += 2; col += 2; continue;
        }
        if (c === '=' && pos + 1 < code.length && code[pos + 1] === '>') {
            addTok(TokenType.ARROW, '=>'); pos += 2; col += 2; continue;
        }
        if (c === '+' && pos + 1 < code.length && code[pos + 1] === '=') {
            addTok(TokenType.PLUS_EQUALS, '+='); pos += 2; col += 2; continue;
        }
        if (c === '-' && pos + 1 < code.length && code[pos + 1] === '=') {
            addTok(TokenType.MINUS_EQUALS, '-='); pos += 2; col += 2; continue;
        }
        if (c === '*' && pos + 1 < code.length && code[pos + 1] === '=') {
            addTok(TokenType.STAR_EQUALS, '*='); pos += 2; col += 2; continue;
        }
        if (c === '/' && pos + 1 < code.length && code[pos + 1] === '=') {
            addTok(TokenType.SLASH_EQUALS, '/='); pos += 2; col += 2; continue;
        }
        if (c === '>' && pos + 1 < code.length && code[pos + 1] === '=') {
            addTok(TokenType.GREATER_EQUALS, '>='); pos += 2; col += 2; continue;
        }
        if (c === '<' && pos + 1 < code.length && code[pos + 1] === '=') {
            addTok(TokenType.LESS_EQUALS, '<='); pos += 2; col += 2; continue;
        }
        if (c === '!' && pos + 1 < code.length && code[pos + 1] === '=') {
            addTok(TokenType.NOT_EQUALS, '!='); pos += 2; col += 2; continue;
        }
        if (c === '!') {
            addTok(TokenType.BANG, '!'); pos++; col++; continue;
        }

        const t = OP_MAP[c];
        if (t !== undefined) {
            addTok(t, c); pos++; col++; continue;
        }

        errors.push({ line, column: col + 1, message: `Unknown character / رمز غير معروف: ${c}` });
        pos++; col++;
    }

    addTok(TokenType.EOF, '');
    return tokens;
}

class Parser {
    constructor(tokens, errors) {
        this.tokens = tokens;
        this.errors = errors;
        this.pos = 0;
    }

    current() { return this.pos < this.tokens.length ? this.tokens[this.pos] : null; }
    peek(n) { return this.pos + n < this.tokens.length ? this.tokens[this.pos + n] : null; }

    eat(type, expectedMsg) {
        const t = this.current();
        if (!t || t.type !== type) {
            const got = t ? (TYPE_NAMES[t.type] || t.value) : 'end/نهاية';
            this.errors.push({
                line: t ? t.line : (this.tokens.length > 0 ? this.tokens[this.tokens.length - 1].line : 1),
                column: t ? t.col : 1,
                message: expectedMsg || `Expected ${TYPE_NAMES[type]} but got ${got} / متوقع ${TYPE_NAMES[type]} لكن وجد ${got}`
            });
            return null;
        }
        this.pos++;
        return t;
    }

    syncToStatement() {
        const stmtTokens = new Set([
            TokenType.LET, TokenType.PRINT, TokenType.IF, TokenType.WHILE, TokenType.FOR,
            TokenType.FUNCTION, TokenType.RETURN, TokenType.BREAK, TokenType.CONTINUE,
            TokenType.CLASS, TokenType.ABSTRACT,
            TokenType.INTERFACE, TokenType.SHOW, TokenType.A_PLUS, TokenType.EXTERN, TokenType.RBRACE, TokenType.EOF,
            TokenType.EXPORT, TokenType.IMPORT, TokenType.ASYNC, TokenType.GO, TokenType.SPAWN, TokenType.DEBUGGER,
            TokenType.VIEWPORT3D, TokenType.MEDIAELEMENT
        ]);
        while (this.current() && !stmtTokens.has(this.current().type)) this.pos++;
    }

    parse() {
        const stmts = [];
        while (this.current() && this.current().type !== TokenType.EOF) {
            const c = this.current();
            if (!c) break;

            try {
                if (c.type === TokenType.LET) { this.parseAssignment(); stmts.push(1); }
                else if (c.type === TokenType.PRINT) { this.parsePrint(); stmts.push(1); }
                else if (c.type === TokenType.IF) { this.parseIf(); stmts.push(1); }
                else if (c.type === TokenType.WHILE) { this.parseWhile(); stmts.push(1); }
                else if (c.type === TokenType.FOR) { this.parseFor(); stmts.push(1); }
                else if (c.type === TokenType.FUNCTION) { this.parseFuncDef(); stmts.push(1); }
                else if (c.type === TokenType.RETURN) { this.parseReturn(); stmts.push(1); }
                else if (c.type === TokenType.ABSTRACT && this.peek(1) && this.peek(1).type === TokenType.CLASS) {
                    this.eat(TokenType.ABSTRACT);
                    this.eat(TokenType.CLASS);
                    this.parseClassDef(false);
                    stmts.push(1);
                } else if (c.type === TokenType.INTERFACE) {
                    this.eat(TokenType.INTERFACE);
                    this.parseClassDef(true);
                    stmts.push(1);
                } else if (c.type === TokenType.BREAK) {
                    this.eat(TokenType.BREAK);
                    stmts.push(1);
                } else if (c.type === TokenType.CONTINUE) {
                    this.eat(TokenType.CONTINUE);
                    stmts.push(1);
                } else if (c.type === TokenType.THROW) {
                    this.parseThrow();
                    stmts.push(1);
                } else if (c.type === TokenType.TRY) {
                    this.parseTry();
                    stmts.push(1);
                } else if (c.type === TokenType.SWITCH) {
                    this.parseSwitch();
                    stmts.push(1);
                } else if (c.type === TokenType.INCLUDE) {
                    this.parseInclude();
                    stmts.push(1);
                } else if (c.type === TokenType.A_PLUS) {
                    this.parseAPlus();
                    stmts.push(1);
                } else if (c.type === TokenType.VIEWPORT3D) {
                    this.eat(TokenType.VIEWPORT3D);
                    this.eat(TokenType.NEW, 'متوقع new بعد viewport3d');
                    this.eatName('متوقع اسم المتغير');
                    if (this.current() && this.current().type === TokenType.SEMICOLON) this.eat(TokenType.SEMICOLON);
                    stmts.push(1);
                } else if (c.type === TokenType.MEDIAELEMENT) {
                    this.eat(TokenType.MEDIAELEMENT);
                    this.eat(TokenType.NEW, 'متوقع new بعد mediaelement');
                    this.eatName('متوقع اسم المتغير');
                    if (this.current() && this.current().type === TokenType.SEMICOLON) this.eat(TokenType.SEMICOLON);
                    stmts.push(1);
                } else if (c.type === TokenType.SHOW) {
                    this.eat(TokenType.SHOW);
                    if (this.current() && this.current().type === TokenType.LPAREN) this.parseArgs();
                    this.parseShowTail();
                    if (this.current() && this.current().type === TokenType.SEMICOLON) this.eat(TokenType.SEMICOLON);
                    stmts.push(1);
                } else if ([TokenType.IDENTIFIER, TokenType.LET].includes(c.type) && this.peek(1) && this.peek(1).type === TokenType.DOT &&
                           this.peek(2) && [TokenType.IDENTIFIER, TokenType.LET].includes(this.peek(2).type) &&
                           this.peek(3) && this.peek(3).type === TokenType.ARROW) {
                    this.parseArrowBind();
                    stmts.push(1);
                } else if (c.type === TokenType.CLASS) {
                    this.eat(TokenType.CLASS);
                    this.parseClassDef(false);
                    stmts.push(1);
                } else if (c.type === TokenType.EXTERN) {
                    this.eat(TokenType.EXTERN);
                    this.eat(TokenType.FUNCTION, 'متوقع func بعد extern');
                    this.eatName('متوقع اسم الدالة');
                    this.eat(TokenType.LPAREN, 'متوقع (');
                    if (this.current() && this.current().type !== TokenType.RPAREN) {
                        this.eatName('متوقع اسم المعامل');
                        while (this.current() && this.current().type === TokenType.COMMA) {
                            this.eat(TokenType.COMMA);
                            this.eatName('متوقع اسم المعامل بعد ,');
                        }
                    }
                    this.eat(TokenType.RPAREN, 'متوقع )');
                    this.eat(TokenType.FROM, 'متوقع from');
                    this.eat(TokenType.STRING, 'متوقع اسم DLL');
                    stmts.push(1);
                } else if (c.type === TokenType.EXPORT) {
                    this.eat(TokenType.EXPORT);
                    if (this.current() && this.current().type === TokenType.FUNCTION) this.parseFuncDef();
                    else if (this.current() && this.current().type === TokenType.CLASS) { this.eat(TokenType.CLASS); this.parseClassDef(false); }
                    else if (this.current() && this.current().type === TokenType.LET) this.parseAssignment();
                    stmts.push(1);
                } else if (c.type === TokenType.IMPORT) {
                    this.eat(TokenType.IMPORT);
                    if (this.current() && this.current().type === TokenType.STRING) {
                        this.eat(TokenType.STRING);
                        if (this.current() && this.current().type === TokenType.AS) { this.eat(TokenType.AS); this.eatName(); }
                    } else if (this.current() && this.current().type === TokenType.LBRACE) {
                        this.eat(TokenType.LBRACE);
                        if (this.current() && this.current().type !== TokenType.RBRACE) {
                            this.eatName();
                            while (this.current() && this.current().type === TokenType.COMMA) { this.eat(TokenType.COMMA); this.eatName(); }
                        }
                        this.eat(TokenType.RBRACE);
                        if (this.current() && this.current().type === TokenType.FROM) { this.eat(TokenType.FROM); this.eat(TokenType.STRING); }
                    } else {
                        this.eatName();
                        while (this.current() && this.current().type === TokenType.COMMA) { this.eat(TokenType.COMMA); this.eatName(); }
                        if (this.current() && this.current().type === TokenType.FROM) { this.eat(TokenType.FROM); this.eat(TokenType.STRING); }
                    }
                    stmts.push(1);
                } else if (c.type === TokenType.ASYNC) {
                    this.eat(TokenType.ASYNC);
                    this.parseFuncDef();
                    stmts.push(1);
                } else if (c.type === TokenType.GO) {
                    this.eat(TokenType.GO);
                    this.parseExpression();
                    stmts.push(1);
                } else if (c.type === TokenType.SPAWN) {
                    this.eat(TokenType.SPAWN);
                    this.eat(TokenType.FUNCTION);
                    this.eatName('متوقع اسم الدالة');
                    this.eat(TokenType.LPAREN);
                    if (this.current() && this.current().type !== TokenType.RPAREN) {
                        this.eatName();
                        while (this.current() && this.current().type === TokenType.COMMA) { this.eat(TokenType.COMMA); this.eatName(); }
                    }
                    this.eat(TokenType.RPAREN);
                    if (this.current() && this.current().type === TokenType.LBRACE) this.parseBlock();
                    else this.parseExpression();
                    stmts.push(1);
                } else if (c.type === TokenType.DEBUGGER) {
                    this.eat(TokenType.DEBUGGER);
                    stmts.push(1);
                } else if ([TokenType.IDENTIFIER, TokenType.LET].includes(c.type) && this.peek(1) && [
                    TokenType.EQUALS,
                    TokenType.PLUS_EQUALS,
                    TokenType.MINUS_EQUALS,
                    TokenType.STAR_EQUALS,
                    TokenType.SLASH_EQUALS
                ].includes(this.peek(1).type)) {
                    this.parseReAssignment();
                    stmts.push(1);
                } else if ([TokenType.IDENTIFIER, TokenType.LET].includes(c.type) && this.peek(1) && this.peek(1).type === TokenType.DOT &&
                           this.peek(2) && [TokenType.IDENTIFIER, TokenType.LET].includes(this.peek(2).type) &&
                           this.peek(3) && this.peek(3).type === TokenType.EQUALS) {
                    this.parseMemberSet();
                    stmts.push(1);
                } else if (c.type === TokenType.SELF && this.peek(1) && this.peek(1).type === TokenType.DOT &&
                           this.peek(2) && [TokenType.IDENTIFIER, TokenType.LET].includes(this.peek(2).type) &&
                           this.peek(3) && this.peek(3).type === TokenType.EQUALS) {
                    this.parseMemberSet();
                    stmts.push(1);
                } else if ([TokenType.IDENTIFIER, TokenType.LET].includes(c.type) && this.peek(1) && this.peek(1).type === TokenType.LBRACKET) {
                    this.parseArrayIndex();
                    stmts.push(1);
                } else if (c.type === TokenType.SELF && this.peek(1) && this.peek(1).type === TokenType.LBRACKET) {
                    this.parseArrayIndexSelf();
                    stmts.push(1);
                } else if (c.type === TokenType.IDENTIFIER || c.type === TokenType.LET ||
                           c.type === TokenType.NUMBER || c.type === TokenType.STRING ||
                           c.type === TokenType.TRUE || c.type === TokenType.FALSE || c.type === TokenType.LPAREN ||
                           c.type === TokenType.NEW || c.type === TokenType.SELF ||
                           c.type === TokenType.AWAIT || c.type === TokenType.NIL) {
                    this.parseExpression();
                    stmts.push(1);
                } else {
                    const got = TYPE_NAMES[c.type] || c.value;
                    const errLine = c.line;
                    const errCol = c.col;
                    this.errors.push({ line: errLine, column: errCol, message: `Unknown statement / عبارة غير معروفة: ${got}` });
                    this.pos++;
                    this.syncToStatement();
                }
            } catch (e) {
                this.syncToStatement();
            }
        }
        if (stmts.length === 0 && this.errors.length === 0) {
            const lastT = this.tokens.length > 1 ? this.tokens[0] : null;
            this.errors.push({ line: lastT ? lastT.line : 1, column: lastT ? lastT.col : 1, message: 'برنامج فارغ' });
        }
    }

    parseAssignment() {
        this.eat(TokenType.LET);
        const nameTok = this.eatName('متوقع اسم المتغير بعد let');
        if (this.current() && this.current().type === TokenType.COLON) {
            this.eat(TokenType.COLON);
            if (this.current()) this.eatName('متوقع نوع المتغير بعد :');
        }
        if (this.current() && this.current().type === TokenType.EQUALS) {
            this.eat(TokenType.EQUALS);
            this.parseExpression();
        }
    }

    parseReAssignment() {
        const nameTok = this.eatName();
        if (this.current() && [
            TokenType.EQUALS,
            TokenType.PLUS_EQUALS,
            TokenType.MINUS_EQUALS,
            TokenType.STAR_EQUALS,
            TokenType.SLASH_EQUALS
        ].includes(this.current().type)) {
            this.pos++;
            this.parseExpression();
        }
    }

    parsePrint() {
        this.eat(TokenType.PRINT);
        this.eat(TokenType.LPAREN, 'متوقع ( بعد print');
        if (this.current() && this.current().type !== TokenType.RPAREN) this.parseOr();
        this.eat(TokenType.RPAREN, 'متوقع ) بعد تعبير print');
        if (this.current() && this.current().type === TokenType.SEMICOLON) this.eat(TokenType.SEMICOLON);
    }

    parseIf() {
        this.eat(TokenType.IF);
        this.eat(TokenType.LPAREN, 'متوقع ( بعد if');
        this.parseOr();
        this.eat(TokenType.RPAREN, 'متوقع ) بعد شرط if');
        this.parseBody();
        if (this.current() && this.current().type === TokenType.ELSE) {
            this.eat(TokenType.ELSE);
            this.parseBody();
        }
    }

    parseWhile() {
        this.eat(TokenType.WHILE);
        this.eat(TokenType.LPAREN, 'متوقع ( بعد while');
        this.parseOr();
        this.eat(TokenType.RPAREN, 'متوقع ) بعد شرط while');
        this.parseBody();
    }

    parseFor() {
        this.eat(TokenType.FOR);
        this.eat(TokenType.LPAREN, 'متوقع ( بعد for');

        if (this.current() && [TokenType.IDENTIFIER, TokenType.LET].includes(this.current().type) &&
            this.peek(1) && this.peek(1).type === TokenType.IN) {
            this.eatName('متوقع اسم متغير for-in');
            this.eat(TokenType.IN, 'متوقع in في for-in');
            this.parseOr();
            this.eat(TokenType.RPAREN, 'متوقع ) بعد for-in');
            this.parseBody();
            return;
        }
        if (this.current() && this.current().type === TokenType.LET &&
            this.peek(2) && this.peek(2).type === TokenType.IN) {
            this.eat(TokenType.LET);
            this.eatName('متوقع اسم متغير for-in');
            this.eat(TokenType.IN, 'متوقع in في for-in');
            this.parseOr();
            this.eat(TokenType.RPAREN, 'متوقع ) بعد for-in');
            this.parseBody();
            return;
        }

        if (this.current() && this.current().type === TokenType.LET && this.peek(1) &&
            [TokenType.EQUALS, TokenType.PLUS_EQUALS, TokenType.MINUS_EQUALS, TokenType.STAR_EQUALS, TokenType.SLASH_EQUALS].includes(this.peek(1).type)) {
            this.parseReAssignment();
        } else if (this.current() && this.current().type === TokenType.LET) {
            this.parseAssignment();
        } else if (this.current() && [TokenType.IDENTIFIER, TokenType.LET].includes(this.current().type) &&
                 this.peek(1) && [
                     TokenType.EQUALS,
                     TokenType.PLUS_EQUALS,
                     TokenType.MINUS_EQUALS,
                     TokenType.STAR_EQUALS,
                     TokenType.SLASH_EQUALS
                 ].includes(this.peek(1).type)) this.parseReAssignment();

        this.eat(TokenType.SEMICOLON, 'متوقع ; بعد init في for');

        if (this.current() && this.current().type !== TokenType.SEMICOLON) this.parseOr();
        this.eat(TokenType.SEMICOLON, 'متوقع ; بعد condition في for');

        if (this.current() && this.current().type !== TokenType.RPAREN) {
            if (this.current() && [TokenType.IDENTIFIER, TokenType.LET].includes(this.current().type) &&
                this.peek(1) && [
                    TokenType.EQUALS,
                    TokenType.PLUS_EQUALS,
                    TokenType.MINUS_EQUALS,
                    TokenType.STAR_EQUALS,
                    TokenType.SLASH_EQUALS
                ].includes(this.peek(1).type)) this.parseReAssignment();
            else this.parseOr();
        }
        this.eat(TokenType.RPAREN, 'متوقع ) بعد increment في for');
        this.parseBody();
    }

    eatName(msg) {
        const t = this.current();
        if (!t) { this.errors.push({ line: 0, column: 0, message: msg || 'Expected a name / متوقع اسم' }); return null; }
        if (t.type === TokenType.IDENTIFIER) { this.pos++; return t; }
        const op = [TokenType.EQUALS,TokenType.PLUS,TokenType.MINUS,TokenType.STAR,TokenType.SLASH,TokenType.PERCENT,TokenType.PLUS_EQUALS,TokenType.MINUS_EQUALS,TokenType.STAR_EQUALS,TokenType.SLASH_EQUALS,TokenType.LPAREN,TokenType.RPAREN,TokenType.LBRACE,TokenType.RBRACE,TokenType.DOT,TokenType.GREATER,TokenType.LESS,TokenType.DOUBLE_EQUALS,TokenType.NOT_EQUALS,TokenType.LESS_EQUALS,TokenType.GREATER_EQUALS,TokenType.COMMA,TokenType.SEMICOLON,TokenType.COLON,TokenType.QUESTION,TokenType.LBRACKET,TokenType.RBRACKET,TokenType.ARROW,TokenType.BANG,TokenType.EOF];
        if (!op.includes(t.type)) { this.pos++; return t; }
        this.errors.push({ line: t.line, column: t.col, message: msg || 'Expected a name / متوقع اسم' }); return null;
    }

    parseFuncDef() {
        this.eat(TokenType.FUNCTION);
        const nameTok = this.eatName('متوقع اسم الدالة بعد func');
        this.eat(TokenType.LPAREN, 'متوقع ( بعد اسم الدالة');

        if (this.current() && this.current().type !== TokenType.RPAREN) {
            this.eatName('متوقع اسم المعامل');
            if (this.current() && this.current().type === TokenType.EQUALS) {
                this.eat(TokenType.EQUALS);
                this.parseOr();
            }
            while (this.current() && this.current().type === TokenType.COMMA) {
                this.eat(TokenType.COMMA);
                this.eatName('متوقع اسم المعامل بعد ,');
                if (this.current() && this.current().type === TokenType.EQUALS) {
                    this.eat(TokenType.EQUALS);
                    this.parseOr();
                }
            }
        }
        this.eat(TokenType.RPAREN, 'متوقع ) بعد معاملات الدالة');

        if (this.current() && this.current().type === TokenType.LBRACE) this.parseBlock();
        else this.parseStatement();
    }

    parseReturn() {
        this.eat(TokenType.RETURN);
        if (this.current() && this.current().type !== TokenType.SEMICOLON &&
            this.current().type !== TokenType.RBRACE &&
            this.current().type !== TokenType.EOF) {
            this.parseOr();
        }
    }

    parseInclude() {
        this.eat(TokenType.INCLUDE);
        this.eat(TokenType.STRING, 'متوقع مسار الملف بعد include');
        if (this.current() && this.current().type === TokenType.FROM) {
            this.eat(TokenType.FROM);
            if (this.current() && this.current().type === TokenType.STRING)
                this.eat(TokenType.STRING, 'متوقع مسار بعد from');
            else
                this.eatName('متوقع اسم المسار بعد from');
        }
    }

    parseThrow() {
        this.eat(TokenType.THROW);
        this.parseOr();
    }

    parseTry() {
        this.eat(TokenType.TRY);
        this.parseBody();
        if (this.current() && this.current().type === TokenType.CATCH) {
            this.eat(TokenType.CATCH);
            if (this.current() && this.current().type === TokenType.LPAREN) {
                this.eat(TokenType.LPAREN);
                this.eatName('متوقع اسم المتغير بعد catch');
                this.eat(TokenType.RPAREN, 'متوقع )');
            }
            this.parseBody();
        }
        if (this.current() && this.current().type === TokenType.FINALLY) {
            this.eat(TokenType.FINALLY);
            this.parseBody();
        }
    }

    parseSwitch() {
        this.eat(TokenType.SWITCH);
        this.eat(TokenType.LPAREN, 'متوقع ( بعد switch');
        this.parseOr();
        this.eat(TokenType.RPAREN, 'متوقع ) بعد switch');
        this.eat(TokenType.LBRACE, 'متوقع { بعد switch');
        while (this.current() && this.current().type !== TokenType.RBRACE && this.current().type !== TokenType.EOF) {
            if (this.current().type === TokenType.CASE) {
                this.eat(TokenType.CASE);
                this.parseOr();
                this.eat(TokenType.COLON, 'متوقع : بعد case');
                // Parse multiple statements until next case, default, or }
                while (this.current() && this.current().type !== TokenType.CASE && this.current().type !== TokenType.DEFAULT && this.current().type !== TokenType.RBRACE && this.current().type !== TokenType.EOF) {
                    this.parseStatement();
                }
            } else if (this.current().type === TokenType.DEFAULT) {
                this.eat(TokenType.DEFAULT);
                this.eat(TokenType.COLON, 'متوقع : بعد default');
                while (this.current() && this.current().type !== TokenType.CASE && this.current().type !== TokenType.DEFAULT && this.current().type !== TokenType.RBRACE && this.current().type !== TokenType.EOF) {
                    this.parseStatement();
                }
            } else {
                break;
            }
        }
        this.eat(TokenType.RBRACE, 'متوقع } بعد switch');
    }

    parseAPlus() {
        this.eat(TokenType.A_PLUS);
        this.eat(TokenType.NEW, 'متوقع new بعد a+');
        this.eatName('متوقع اسم class بعد a+ new');
        this.eatName('متوقع اسم المتغير بعد a+ new className');
        if (this.current() && this.current().type === TokenType.SEMICOLON)
            this.eat(TokenType.SEMICOLON);
    }

    parseArrowBind() {
        this.eatName();
        this.eat(TokenType.DOT, 'متوقع .');
        this.eatName('متوقع اسم الحدث');
        this.eat(TokenType.ARROW, 'متوقع =>');
        this.eatName('متوقع اسم الدالة');
        if (this.current() && this.current().type === TokenType.LPAREN) {
            this.eat(TokenType.LPAREN);
            this.eat(TokenType.RPAREN);
        }
        if (this.current() && this.current().type === TokenType.SEMICOLON)
            this.eat(TokenType.SEMICOLON);
    }

    parseBody() {
        if (this.current() && this.current().type === TokenType.LBRACE) this.parseBlock();
        else this.parseStatement();
    }

    parseBlock() {
        this.eat(TokenType.LBRACE, 'متوقع {');
        while (this.current() && this.current().type !== TokenType.RBRACE && this.current().type !== TokenType.EOF) {
            this.parseStatement();
        }
        this.eat(TokenType.RBRACE, 'متوقع }');
    }

    parseStatement() {
        const c = this.current();
        if (!c) return;

        if (c.type === TokenType.SEMICOLON) { this.eat(TokenType.SEMICOLON); return; }
        if (c.type === TokenType.LET) { this.parseAssignment(); return; }
        if (c.type === TokenType.PRINT) { this.parsePrint(); return; }
        if (c.type === TokenType.IF) { this.parseIf(); return; }
        if (c.type === TokenType.WHILE) { this.parseWhile(); return; }
        if (c.type === TokenType.FOR) { this.parseFor(); return; }
        if (c.type === TokenType.FUNCTION) { this.parseFuncDef(); return; }
        if (c.type === TokenType.RETURN) { this.parseReturn(); return; }
        if (c.type === TokenType.BREAK) { this.eat(TokenType.BREAK); if (this.current() && this.current().type === TokenType.SEMICOLON) this.eat(TokenType.SEMICOLON); return; }
        if (c.type === TokenType.CONTINUE) { this.eat(TokenType.CONTINUE); if (this.current() && this.current().type === TokenType.SEMICOLON) this.eat(TokenType.SEMICOLON); return; }
        if (c.type === TokenType.INCLUDE) { this.parseInclude(); return; }
        if (c.type === TokenType.THROW) { this.parseThrow(); return; }
        if (c.type === TokenType.TRY) { this.parseTry(); return; }
        if (c.type === TokenType.SWITCH) { this.parseSwitch(); return; }
        if (c.type === TokenType.A_PLUS) { this.parseAPlus(); return; }
        if (c.type === TokenType.VIEWPORT3D) { this.eat(TokenType.VIEWPORT3D); this.eat(TokenType.NEW, 'متوقع new'); this.eatName(); if (this.current() && this.current().type === TokenType.SEMICOLON) this.eat(TokenType.SEMICOLON); return; }
        if (c.type === TokenType.MEDIAELEMENT) { this.eat(TokenType.MEDIAELEMENT); this.eat(TokenType.NEW, 'متوقع new'); this.eatName(); if (this.current() && this.current().type === TokenType.SEMICOLON) this.eat(TokenType.SEMICOLON); return; }
        if (c.type === TokenType.SHOW) { this.eat(TokenType.SHOW); if (this.current() && this.current().type === TokenType.LPAREN) this.parseArgs(); this.parseShowTail(); if (this.current() && this.current().type === TokenType.SEMICOLON) this.eat(TokenType.SEMICOLON); return; }
        if (c.type === TokenType.CLASS) { this.eat(TokenType.CLASS); this.parseClassDef(false); return; }
                if (c.type === TokenType.ABSTRACT && this.peek(1) && this.peek(1).type === TokenType.CLASS) {
            this.eat(TokenType.ABSTRACT); this.eat(TokenType.CLASS); this.parseClassDef(false); return;
        }
        if (c.type === TokenType.INTERFACE) { this.eat(TokenType.INTERFACE); this.parseClassDef(true); return; }
        if (c.type === TokenType.EXTERN) {
            this.eat(TokenType.EXTERN);
            this.eat(TokenType.FUNCTION, 'متوقع func بعد extern');
            this.eat(TokenType.IDENTIFIER, 'متوقع اسم الدالة');
            this.eat(TokenType.LPAREN, 'متوقع (');
            if (this.current() && this.current().type !== TokenType.RPAREN) {
                this.eat(TokenType.IDENTIFIER, 'متوقع اسم المعامل');
                while (this.current() && this.current().type === TokenType.COMMA) {
                    this.eat(TokenType.COMMA);
                    this.eat(TokenType.IDENTIFIER, 'متوقع اسم المعامل بعد ,');
                }
            }
            this.eat(TokenType.RPAREN, 'متوقع )');
            this.eat(TokenType.FROM, 'متوقع from');
            this.eat(TokenType.STRING, 'متوقع اسم DLL');
            return;
        }

        if (c.type === TokenType.IDENTIFIER && this.peek(1) && [
            TokenType.EQUALS,
            TokenType.PLUS_EQUALS,
            TokenType.MINUS_EQUALS,
            TokenType.STAR_EQUALS,
            TokenType.SLASH_EQUALS
        ].includes(this.peek(1).type)) {
            this.parseReAssignment(); return;
        }
        if (c.type === TokenType.IDENTIFIER && this.peek(1) && this.peek(1).type === TokenType.DOT &&
            this.peek(2) && this.peek(2).type === TokenType.IDENTIFIER &&
            this.peek(3) && this.peek(3).type === TokenType.EQUALS) {
            this.parseMemberSet(); return;
        }
        if (c.type === TokenType.IDENTIFIER && this.peek(1) && this.peek(1).type === TokenType.DOT &&
            this.peek(2) && this.peek(2).type === TokenType.IDENTIFIER &&
            this.peek(3) && this.peek(3).type === TokenType.ARROW) {
            this.parseArrowBind(); return;
        }

        if (c.type === TokenType.IDENTIFIER && this.peek(1) && this.peek(1).type === TokenType.LBRACKET) {
            this.parseArrayIndex(); return;
        }

        if (c.type === TokenType.SELF && this.peek(1) && this.peek(1).type === TokenType.DOT &&
            this.peek(2) && this.peek(2).type === TokenType.IDENTIFIER &&
            this.peek(3) && this.peek(3).type === TokenType.EQUALS) {
            this.parseMemberSet(); return;
        }

        if (c.type === TokenType.SELF && this.peek(1) && this.peek(1).type === TokenType.LBRACKET) {
            this.parseArrayIndexSelf(); return;
        }

        if (c.type === TokenType.IDENTIFIER || c.type === TokenType.NUMBER || c.type === TokenType.STRING ||
            c.type === TokenType.TRUE || c.type === TokenType.FALSE || c.type === TokenType.LPAREN ||
            c.type === TokenType.NEW || c.type === TokenType.SELF) {
            this.parseExpression(); return;
        }

        const got = TYPE_NAMES[c.type] || c.value;
        this.errors.push({ line: c.line, column: c.col, message: `عبارة غير معروفة: ${got}` });
        this.pos++;
    }

    parseExpression() {
        this.parsePipe();
        if (this.current() && this.current().type === TokenType.QUESTION) {
            this.eat(TokenType.QUESTION);
            this.parsePipe();
            this.eat(TokenType.COLON, 'متوقع : في ternary');
            this.parsePipe();
        }
    }

    parsePipe() {
        this.parseOr();
        while (this.current() && this.current().type === TokenType.PIPE) {
            this.eat(TokenType.PIPE);
            this.parseOr();
        }
    }

    parseOr() {
        this.parseAnd();
        while (this.current() && this.current().type === TokenType.OR) {
            this.eat(TokenType.OR);
            this.parseAnd();
        }
    }

    parseAnd() {
        this.parseComparison();
        while (this.current() && this.current().type === TokenType.AND) {
            this.eat(TokenType.AND);
            this.parseComparison();
        }
    }

    parseComparison() {
        this.parseAddSubtract();
        if (this.current() && (
            this.current().type === TokenType.GREATER || this.current().type === TokenType.LESS ||
            this.current().type === TokenType.DOUBLE_EQUALS || this.current().type === TokenType.NOT_EQUALS ||
            this.current().type === TokenType.LESS_EQUALS || this.current().type === TokenType.GREATER_EQUALS
        )) {
            this.pos++;
            this.parseAddSubtract();
        }
    }

    parseAddSubtract() {
        this.parseTerm();
        while (this.current() && (this.current().type === TokenType.PLUS || this.current().type === TokenType.MINUS)) {
            this.pos++;
            this.parseTerm();
        }
    }

    parseTerm() {
        this.parsePrimary();
        while (this.current() && (this.current().type === TokenType.STAR || this.current().type === TokenType.SLASH || this.current().type === TokenType.PERCENT)) {
            this.pos++;
            this.parsePrimary();
        }
    }

    parsePrimary() {
        this.parseFactor();
        while (this.current() && (this.current().type === TokenType.DOT || this.current().type === TokenType.LBRACKET)) {
            if (this.current().type === TokenType.DOT) {
                this.eat(TokenType.DOT);
                const memberTok = this.eatName('متوقع اسم العضو بعد .');
                if (this.current() && this.current().type === TokenType.LPAREN) {
                    this.parseArgs();
                }
            } else {
                this.eat(TokenType.LBRACKET);
                this.parseOr();
                this.eat(TokenType.RBRACKET, 'متوقع ]');
            }
        }
    }

    parseFactor() {
        const t = this.current();
        if (!t) return;

        if (t.type === TokenType.NUMBER) { this.pos++; return; }
        if (t.type === TokenType.STRING) { this.pos++; return; }
        if (t.type === TokenType.TRUE || t.type === TokenType.FALSE) { this.pos++; return; }
        if (t.type === TokenType.NEW) { this.parseNew(); return; }
        if (t.type === TokenType.SELF) { this.pos++; return; }
        if (t.type === TokenType.MINUS || t.type === TokenType.PLUS || t.type === TokenType.NOT) { this.pos++; this.parseFactor(); return; }

        if (t.type === TokenType.IDENTIFIER || t.type === TokenType.LET) {
            this.pos++;
            if (this.current() && this.current().type === TokenType.LPAREN) this.parseArgs();
            return;
        }

        if (t.type === TokenType.LPAREN) {
            this.eat(TokenType.LPAREN);
            this.parseOr();
            this.eat(TokenType.RPAREN, 'متوقع )');
            return;
        }

        if (t.type === TokenType.LBRACKET) {
            this.eat(TokenType.LBRACKET);
            if (this.current() && this.current().type !== TokenType.RBRACKET) {
                this.parseOr();
                while (this.current() && this.current().type === TokenType.COMMA) {
                    this.eat(TokenType.COMMA);
                    this.parseOr();
                }
            }
            this.eat(TokenType.RBRACKET, 'متوقع ]');
            return;
        }

        this.errors.push({ line: t.line, column: t.col, message: `Unexpected expression / تعبير غير متوقع: ${TYPE_NAMES[t.type] || t.value}` });
        this.pos++;
    }

    parseArgs() {
        this.eat(TokenType.LPAREN, 'متوقع (');
        if (this.current() && this.current().type !== TokenType.RPAREN) {
            this.parseArg();
            while (this.current() && this.current().type === TokenType.COMMA) {
                this.eat(TokenType.COMMA);
                this.parseArg();
            }
        }
        this.eat(TokenType.RPAREN, 'متوقع )');
    }

    parseArg() {
        if (this.current() && [TokenType.IDENTIFIER, TokenType.LET].includes(this.current().type) &&
            this.peek(1) && this.peek(1).type === TokenType.EQUALS) {
            this.eatName();
            this.eat(TokenType.EQUALS);
            this.parseOr();
        } else {
            this.parseOr();
        }
    }

    parseNew() {
        this.eat(TokenType.NEW);
        this.eatName('متوقع اسم class بعد new');
        this.parseArgs();
    }

    parseMemberSet() {
        if (this.current() && this.current().type === TokenType.SELF) {
            this.eat(TokenType.SELF);
        } else {
            this.eatName();
        }
        this.eat(TokenType.DOT, 'متوقع .');
        this.eatName('متوقع اسم العضو بعد .');
        this.eat(TokenType.EQUALS, 'متوقع = في تعيين العضو');
        this.parseOr();
    }

    parseArrayIndex() {
        this.eatName();
        while (this.current() && this.current().type === TokenType.LBRACKET) {
            this.eat(TokenType.LBRACKET);
            this.parseOr();
            this.eat(TokenType.RBRACKET, 'متوقع ]');
        }
        if (this.current() && this.current().type === TokenType.EQUALS) {
            this.eat(TokenType.EQUALS);
            this.parseOr();
        }
    }

    parseArrayIndexSelf() {
        this.eat(TokenType.SELF);
        while (this.current() && this.current().type === TokenType.LBRACKET) {
            this.eat(TokenType.LBRACKET);
            this.parseOr();
            this.eat(TokenType.RBRACKET, 'متوقع ]');
        }
        if (this.current() && this.current().type === TokenType.EQUALS) {
            this.eat(TokenType.EQUALS);
            this.parseOr();
        }
    }

    parseShowTail() {
        while (this.current() && this.current().type === TokenType.COMMA) {
            this.eat(TokenType.COMMA);
            while (this.current() &&
                   this.current().type !== TokenType.COMMA &&
                   this.current().type !== TokenType.SEMICOLON &&
                   this.current().type !== TokenType.RBRACE &&
                   this.current().type !== TokenType.EOF) {
                this.pos++;
            }
        }
    }

    parseClassDef(isInterface) {
        const nameTok = this.eatName('متوقع اسم class');

        if (this.current() && this.current().type === TokenType.SLASH) {
            this.eat(TokenType.SLASH);
            this.eatName('متوقع اسم class الأب بعد /');
        }

        this.eat(TokenType.LBRACE, 'متوقع { في class');

        while (this.current() && this.current().type !== TokenType.RBRACE && this.current().type !== TokenType.EOF) {
            this.parseAccessModifiers();
            const c = this.current();
            if (!c) break;

            if (c.type === TokenType.LET) {
                if (isInterface) {
                    this.errors.push({ line: c.line, column: c.col, message: 'لا يمكن وجود حقول في interface' });
                }
                this.eat(TokenType.LET);
                this.eatName('متوقع اسم الحقل بعد let');
                if (this.current() && this.current().type === TokenType.EQUALS) {
                    this.eat(TokenType.EQUALS);
                    this.parseOr();
                }
            } else if (c.type === TokenType.FUNCTION) {
                this.eat(TokenType.FUNCTION);
                const mTok = this.eatName('متوقع اسم الدالة');
                this.eat(TokenType.LPAREN, 'متوقع (');
                if (this.current() && this.current().type !== TokenType.RPAREN) {
                    this.eatName('متوقع اسم المعامل');
                    if (this.current() && this.current().type === TokenType.EQUALS) {
                        this.eat(TokenType.EQUALS);
                        this.parseOr();
                    }
                    while (this.current() && this.current().type === TokenType.COMMA) {
                        this.eat(TokenType.COMMA);
                        this.eatName('متوقع اسم المعامل بعد ,');
                        if (this.current() && this.current().type === TokenType.EQUALS) {
                            this.eat(TokenType.EQUALS);
                            this.parseOr();
                        }
                    }
                }
                this.eat(TokenType.RPAREN, 'متوقع )');
                if (this.current() && this.current().type === TokenType.LBRACE) {
                    this.parseBlock();
                }
            } else {
                if (isInterface) {
                    this.errors.push({ line: c.line, column: c.col, message: 'في interface: let فقط أو func' });
                    this.pos++;
                    this.syncToStatement();
                } else {
                    this.parseStatement();
                }
            }
        }
        this.eat(TokenType.RBRACE, 'متوقع } بعد class');
    }

    parseAccessModifiers() {
        while (this.current() && (
            this.current().type === TokenType.PUBLIC ||
            this.current().type === TokenType.PRIVATE ||
            this.current().type === TokenType.STATIC ||
            this.current().type === TokenType.ABSTRACT
        )) this.pos++;
    }
}

const uiAttributes = new Set([
    'Content', 'Text', 'Width', 'Height', 'Margin', 'Padding',
    'Click', 'Loaded', 'Source', 'Value', 'Minimum', 'Maximum',
    'Background', 'Foreground', 'IsChecked', 'IsEnabled', 'Visibility',
    'Name', 'MouseEnter', 'MouseLeave',
    'Command', 'CommandParameter',
    'Orientation', 'TextDecoration', 'HorizontalAlignment', 'VerticalAlignment',
    'FontSize', 'FontFamily', 'FontWeight', 'FontStyle',
    'BorderThickness', 'BorderBrush', 'Fill', 'Stroke', 'Opacity',
    'Grid.Column', 'Grid.Row', 'Grid.ColumnSpan', 'Grid.RowSpan',
    'Canvas.Left', 'Canvas.Top', 'Canvas.Right', 'Canvas.Bottom',
    'HorizontalScrollBarVisibility', 'VerticalScrollBarVisibility',
    'SelectedIndex', 'SelectedItem', 'ItemsSource',
    'ImageSource', 'Stretch', 'StretchDirection',
    'AutoReverse', 'RepeatBehavior', 'Duration',
    'WidthRequest', 'HeightRequest',
]);

module.exports = { checkCode, arabicTags, arabicAttrs, knownUiTypes, uiAttributes };
