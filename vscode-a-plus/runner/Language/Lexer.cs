using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A_
{
    
    class Lexer
    {
        private string _code;
        private int _pos = 0;
        public List<string> Errors = new List<string>();

        static readonly Dictionary<string, TokenType> Keywords = new Dictionary<string, TokenType>
        {
            { "let", TokenType.LET }, { "م", TokenType.LET }, { "متغير", TokenType.LET },
            { "print", TokenType.PRINT }, { "ط", TokenType.PRINT }, { "اطبع", TokenType.PRINT },
            { "if", TokenType.IF }, { "لو", TokenType.IF }, { "إذا", TokenType.IF },
            { "while", TokenType.WHILE }, { "ت", TokenType.WHILE }, { "طالما", TokenType.WHILE },
            { "بينما", TokenType.WHILE },
            { "for", TokenType.FOR }, { "لكل", TokenType.FOR },
            { "else", TokenType.ELSE }, { "وإلا", TokenType.ELSE },
            { "func", TokenType.FUNCTION }, { "function", TokenType.FUNCTION }, { "دالة", TokenType.FUNCTION },
            { "return", TokenType.RETURN }, { "ارجع", TokenType.RETURN }, { "عود", TokenType.RETURN }, { "رجع", TokenType.RETURN },
            { "and", TokenType.AND }, { "و", TokenType.AND },
            { "or", TokenType.OR }, { "أو", TokenType.OR },
            { "true", TokenType.TRUE }, { "false", TokenType.FALSE },
            { "صحيح", TokenType.TRUE }, { "خاطئ", TokenType.FALSE },
            { "class", TokenType.CLASS }, { "صنف", TokenType.CLASS }, { "فئة", TokenType.CLASS },
            { "new", TokenType.NEW }, { "جديد", TokenType.NEW },
            { "self", TokenType.SELF }, { "this", TokenType.SELF }, { "نفس", TokenType.SELF }, { "هذا", TokenType.SELF },
            { "public", TokenType.PUBLIC }, { "عام", TokenType.PUBLIC },
            { "private", TokenType.PRIVATE }, { "خاص", TokenType.PRIVATE },
            { "static", TokenType.STATIC }, { "ثابت", TokenType.STATIC },
            { "abstract", TokenType.ABSTRACT }, { "مجرد", TokenType.ABSTRACT },
            { "extends", TokenType.EXTENDS }, { "يرث", TokenType.EXTENDS },
            { "interface", TokenType.INTERFACE }, { "واجهة", TokenType.INTERFACE },
            { "include", TokenType.INCLUDE }, { "استدعاء", TokenType.INCLUDE }, { "ضم", TokenType.INCLUDE },
            { "from", TokenType.FROM }, { "من", TokenType.FROM },
            { "throw", TokenType.THROW }, { "ارم", TokenType.THROW },
            { "try", TokenType.TRY }, { "حاول", TokenType.TRY }, { "محاولة", TokenType.TRY },
            { "catch", TokenType.CATCH }, { "التقط", TokenType.CATCH }, { "القط", TokenType.CATCH },
            { "finally", TokenType.FINALLY }, { "أخيرا", TokenType.FINALLY }, { "وأخيراً", TokenType.FINALLY },
            { "show", TokenType.SHOW }, { "اعرض", TokenType.SHOW },
            { "extern", TokenType.EXTERN }, { "خارجي", TokenType.EXTERN },
            { "not", TokenType.NOT }, { "ليس", TokenType.NOT },
            { "break", TokenType.BREAK }, { "continue", TokenType.CONTINUE },
            { "توقف", TokenType.BREAK }, { "كسر", TokenType.BREAK }, { "استمر", TokenType.CONTINUE },
            { "switch", TokenType.SWITCH }, { "اختيار", TokenType.SWITCH },
            { "case", TokenType.CASE }, { "حالة", TokenType.CASE },
            { "default", TokenType.DEFAULT }, { "افتراضي", TokenType.DEFAULT },
            { "in", TokenType.IN }, { "في", TokenType.IN },
            { "viewport3d", TokenType.VIEWPORT3D }, { "منظور3d", TokenType.VIEWPORT3D },
            { "mediaelement", TokenType.MEDIAELEMENT }, { "وسائط", TokenType.MEDIAELEMENT }, { "عنصر_وسائط", TokenType.MEDIAELEMENT },
            // Module System
            { "export", TokenType.EXPORT }, { "صدر", TokenType.EXPORT },
            { "import", TokenType.IMPORT }, { "استورد", TokenType.IMPORT },
            { "as", TokenType.AS }, { "كـ", TokenType.AS },
            // Async/Await
            { "async", TokenType.ASYNC }, { "غيرمتزامن", TokenType.ASYNC },
            { "await", TokenType.AWAIT }, { "انتظر", TokenType.AWAIT },
            { "go", TokenType.GO }, { "انطلق", TokenType.GO },
            { "spawn", TokenType.SPAWN }, { "أنشئ", TokenType.SPAWN },
            // Type System
            { "nil", TokenType.NIL }, { "عدم", TokenType.NIL },
            // Debugger
            { "debugger", TokenType.DEBUGGER }, { "مصحح", TokenType.DEBUGGER },
        };

        public Lexer(string code)
        {
            _code = PreprocessStringInterpolation(code);
        }

        string PreprocessStringInterpolation(string code)
        {
            int i = 0;
            var sb = new StringBuilder();
            while (i < code.Length)
            {
                if (i + 1 < code.Length && code[i] == '$' && code[i + 1] == '"')
                {
                    i += 2;
                    var parts = new List<string>();
                    var exprs = new List<string>();
                    var cur = new StringBuilder();
                    while (i < code.Length && code[i] != '"')
                    {
                        if (code[i] == '\\' && i + 1 < code.Length && (code[i + 1] == '{' || code[i + 1] == '}'))
                        {
                            cur.Append(code[i + 1]);
                            i += 2;
                        }
                        else if (code[i] == '{')
                        {
                            parts.Add(cur.ToString());
                            cur.Clear();
                            i++;
                            int depth = 1;
                            var expr = new StringBuilder();
                            while (i < code.Length && depth > 0)
                            {
                                if (code[i] == '{') depth++;
                                else if (code[i] == '}') depth--;
                                if (depth > 0) expr.Append(code[i]);
                                i++;
                            }
                            exprs.Add(expr.ToString());
                        }
                        else
                        {
                            cur.Append(code[i]);
                            i++;
                        }
                    }
                    parts.Add(cur.ToString());
                    if (i < code.Length) i++;

                    sb.Append("(");
                    bool first = true;
                    for (int p = 0; p < parts.Count; p++)
                    {
                        if (parts[p].Length > 0)
                        {
                            if (!first) sb.Append(" + ");
                            sb.Append('"');
                            sb.Append(EscapeString(parts[p]));
                            sb.Append('"');
                            first = false;
                        }
                        if (p < exprs.Count)
                        {
                            if (!first) sb.Append(" + ");
                            sb.Append(exprs[p]);
                            first = false;
                        }
                    }
                    sb.Append(")");
                }
                else
                {
                    sb.Append(code[i]);
                    i++;
                }
            }
            return sb.ToString();
        }

        string EscapeString(string s)
        {
            return s.Replace("\\", "\\\\").Replace("\"", "\\\"");
        }

        (int line, int col) GetLineCol(int pos)
        {
            int line = 1, col = 1;
            for (int i = 0; i < pos && i < _code.Length; i++)
            {
                if (_code[i] == '\n') { line++; col = 1; }
                else col++;
            }
            return (line, col);
        }
        
        public List<Token> Tokenize()
        { 
        
            var tokens = new List<Token>();

            while (_pos < _code.Length)
            {
                char c = _code[_pos];

                if (char.IsWhiteSpace(c))
                {
                    _pos++;
                    continue;
                }

                int tokStart = _pos;
                var (line, col) = GetLineCol(tokStart);

                if (char.IsLetter(c) || c == '_' || (c >= 0x0600 && c <= 0x06FF))
                {
                    string word = "";

                    while (_pos < _code.Length &&
                           (char.IsLetterOrDigit(_code[_pos]) || _code[_pos] == '_'))
                    {
                        word += _code[_pos];
                        _pos++;
                    }

                    if (Keywords.ContainsKey(word))
                    {
                        tokens.Add(new Token(Keywords[word], word, line, col));
                    }
                    else if (word == "a" && _pos < _code.Length && _code[_pos] == '+')
                    {
                        _pos++;
                        tokens.Add(new Token(TokenType.A_PLUS, "a+", line, col));
                    }
                    else
                    {
                        tokens.Add(new Token(TokenType.IDENTIFIER, word, line, col));
                    }
                    continue;
                }

                if (char.IsDigit(c))
                {
                    string number = "";
                    bool hasDot = false;

                    while (_pos < _code.Length &&
                          (char.IsDigit(_code[_pos]) || _code[_pos] == '.'))
                    {
                        if (_code[_pos] == '.')
                        {
                            if (hasDot)
                            {
                                Errors.Add($"خطأ (سطر {line}, عمود {col}): رقم عشري غير صالح");
                                break;
                            }
                            hasDot = true;
                        }
                        number += _code[_pos];
                        _pos++;
                    }
                    tokens.Add(new Token(TokenType.NUMBER, number, line, col));
                    continue;
                }

                if (c == '"')
                {
                    _pos++;
                    string text = "";
                    while (_pos < _code.Length && _code[_pos] != '"')
                    {
                        if (_code[_pos] == '\n') { _pos++; line++; col = 1; continue; }
                        if (_code[_pos] == '\\' && _pos + 1 < _code.Length)
                        {
                            char next = _code[_pos + 1];
                            if (next == '"') { text += '"'; _pos += 2; continue; }
                            if (next == '\\') { text += '\\'; _pos += 2; continue; }
                            if (next == 'n') { text += '\n'; _pos += 2; continue; }
                            if (next == 't') { text += '\t'; _pos += 2; continue; }
                            if (next == 'r') { text += '\r'; _pos += 2; continue; }
                        }
                        text += _code[_pos];
                        _pos++;
                    }
                    if (_pos >= _code.Length)
                    {
                        Errors.Add($"خطأ (سطر {line}, عمود {col}): النص غير مغلق");
                        tokens.Add(new Token(TokenType.STRING, text, line, col));
                        continue;
                    }
                    _pos++;
                    tokens.Add(new Token(TokenType.STRING, text, line, col));
                    continue;
                }

                if (c == '/' && _pos + 1 < _code.Length && _code[_pos + 1] == '/')
                {
                    // Check for doc comment ///
                    if (_pos + 2 < _code.Length && _code[_pos + 2] == '/')
                    {
                        _pos += 3;
                        string doc = "";
                        while (_pos < _code.Length && _code[_pos] != '\n') { doc += _code[_pos]; _pos++; }
                        tokens.Add(new Token(TokenType.DOC_COMMENT, doc.Trim(), line, col));
                        continue;
                    }
                    while (_pos < _code.Length && _code[_pos] != '\n') _pos++;
                    continue;
                }

                if (c == '/' && _pos + 1 < _code.Length && _code[_pos + 1] == '*')
                {
                    _pos += 2;
                    while (_pos + 1 < _code.Length && !(_code[_pos] == '*' && _code[_pos + 1] == '/')) _pos++;
                    if (_pos + 1 < _code.Length) _pos += 2;
                    continue;
                }

                if (c == '#')
                {
                    while (_pos < _code.Length && _code[_pos] != '\n') _pos++;
                    continue;
                }

                switch (c)
                {
                    case '=':
                        if (_pos + 1 < _code.Length && _code[_pos + 1] == '=')
                        {
                            tokens.Add(new Token(TokenType.DOUBLE_EQUALS, "==", line, col));
                            _pos++;
                        }
                        else if (_pos + 1 < _code.Length && _code[_pos + 1] == '>')
                        {
                            tokens.Add(new Token(TokenType.ARROW, "=>", line, col));
                            _pos++;
                        }
                        else
                            tokens.Add(new Token(TokenType.EQUALS, "=", line, col));
                        break;
                    case '+':
                        if (_pos + 1 < _code.Length && _code[_pos + 1] == '+')
                        {
                            tokens.Add(new Token(TokenType.PLUS_PLUS, "++", line, col));
                            _pos++;
                        }
                        else if (_pos + 1 < _code.Length && _code[_pos + 1] == '=')
                        {
                            tokens.Add(new Token(TokenType.PLUS_EQUALS, "+=", line, col));
                            _pos++;
                        }
                        else
                            tokens.Add(new Token(TokenType.PLUS, "+", line, col));
                        break;
                    case '-':
                        if (_pos + 1 < _code.Length && _code[_pos + 1] == '-')
                        {
                            tokens.Add(new Token(TokenType.MINUS_MINUS, "--", line, col));
                            _pos++;
                        }
                        else if (_pos + 1 < _code.Length && _code[_pos + 1] == '=')
                        {
                            tokens.Add(new Token(TokenType.MINUS_EQUALS, "-=", line, col));
                            _pos++;
                        }
                        else
                            tokens.Add(new Token(TokenType.MINUS, "-", line, col));
                        break;
                    case '*':
                        if (_pos + 1 < _code.Length && _code[_pos + 1] == '=')
                        {
                            tokens.Add(new Token(TokenType.STAR_EQUALS, "*=", line, col));
                            _pos++;
                        }
                        else
                            tokens.Add(new Token(TokenType.STAR, "*", line, col));
                        break;
                    case '/':
                        if (_pos + 1 < _code.Length && _code[_pos + 1] == '=')
                        {
                            tokens.Add(new Token(TokenType.SLASH_EQUALS, "/=", line, col));
                            _pos++;
                        }
                        else if (IsRegexStart(tokens))
                        {
                            _pos++;
                            int rline = line, rcol = col;
                            string pattern = "";
                            while (_pos < _code.Length && _code[_pos] != '/')
                            {
                                if (_code[_pos] == '\\' && _pos + 1 < _code.Length)
                                { pattern += _code[_pos] + _code[_pos + 1].ToString(); _pos += 2; }
                                else
                                { pattern += _code[_pos]; _pos++; }
                            }
                            if (_pos >= _code.Length) { _pos--; break; }
                            _pos++; // skip closing /
                            string flags = "";
                            while (_pos < _code.Length && char.IsLetter(_code[_pos]))
                            { flags += _code[_pos]; _pos++; }
                            _pos--;
                            tokens.Add(new Token(TokenType.REGEX, "/" + pattern + "/" + flags, rline, rcol));
                        }
                        else
                            tokens.Add(new Token(TokenType.SLASH, "/", line, col));
                        break;
                    case '&':
                        if (_pos + 1 < _code.Length && _code[_pos + 1] == '&')
                        { tokens.Add(new Token(TokenType.AND, "&&", line, col)); _pos++; }
                        else tokens.Add(new Token(TokenType.AND, "&", line, col));
                        break;
                    case '%': tokens.Add(new Token(TokenType.PERCENT, "%", line, col)); break;
                    case '(': tokens.Add(new Token(TokenType.LPAREN, "(", line, col)); break;
                    case ')': tokens.Add(new Token(TokenType.RPAREN, ")", line, col)); break;
                    case '{': tokens.Add(new Token(TokenType.LBRACE, "{", line, col)); break;
                    case '}': tokens.Add(new Token(TokenType.RBRACE, "}", line, col)); break;
                    case ',': tokens.Add(new Token(TokenType.COMMA, ",", line, col)); break;
                    case ';': tokens.Add(new Token(TokenType.SEMICOLON, ";", line, col)); break;
                    case ':':
                        if (_pos + 1 < _code.Length && _code[_pos + 1] == '=')
                        {
                            // := is treated as EQUALS for compatibility
                            tokens.Add(new Token(TokenType.EQUALS, ":=", line, col));
                            _pos++;
                        }
                        else
                            tokens.Add(new Token(TokenType.COLON, ":", line, col));
                        break;
                    case '|':
                        if (_pos + 1 < _code.Length && _code[_pos + 1] == '|')
                        { tokens.Add(new Token(TokenType.OR, "||", line, col)); _pos++; }
                        else tokens.Add(new Token(TokenType.PIPE, "|", line, col));
                        break;
                    case '[': tokens.Add(new Token(TokenType.LBRACKET, "[", line, col)); break;
                    case ']': tokens.Add(new Token(TokenType.RBRACKET, "]", line, col)); break;
                    case '.': tokens.Add(new Token(TokenType.DOT, ".", line, col)); break;
                    case '?': tokens.Add(new Token(TokenType.QUESTION, "?", line, col)); break;
                    case '>':
                        if (_pos + 1 < _code.Length && _code[_pos + 1] == '=')
                        {
                            tokens.Add(new Token(TokenType.GREATER_EQUALS, ">=", line, col));
                            _pos++;
                        }
                        else
                            tokens.Add(new Token(TokenType.GREATER, ">", line, col));
                        break;
                    case '<':
                        if (_pos + 1 < _code.Length && _code[_pos + 1] == '=')
                        {
                            tokens.Add(new Token(TokenType.LESS_EQUALS, "<=", line, col));
                            _pos++;
                        }
                        else
                            tokens.Add(new Token(TokenType.LESS, "<", line, col));
                        break;
                    case '!':
                        if (_pos + 1 < _code.Length && _code[_pos + 1] == '=')
                        {
                            tokens.Add(new Token(TokenType.NOT_EQUALS, "!=", line, col));
                            _pos++;
                        }
                        else
                            tokens.Add(new Token(TokenType.BANG, "!", line, col));
                        break;
                    default:
                        Errors.Add($"خطأ (سطر {line}, عمود {col}): رمز غير معروف: '{c}'");
                        break;
                }
                _pos++;
            }
            return tokens;
        }

        bool IsRegexStart(List<Token> tokens)
        {
            if (tokens.Count == 0) return true;
            var prev = tokens[tokens.Count - 1];
            switch (prev.Type)
            {
                case TokenType.EQUALS: case TokenType.PLUS_EQUALS: case TokenType.MINUS_EQUALS:
                case TokenType.STAR_EQUALS: case TokenType.SLASH_EQUALS:
                case TokenType.LPAREN: case TokenType.LBRACKET: case TokenType.LBRACE:
                case TokenType.COMMA: case TokenType.COLON: case TokenType.SEMICOLON:
                case TokenType.ARROW:
                case TokenType.PLUS: case TokenType.MINUS: case TokenType.STAR:
                case TokenType.SLASH: case TokenType.PERCENT:
                case TokenType.AND: case TokenType.OR: case TokenType.NOT: case TokenType.BANG:
                case TokenType.GREATER: case TokenType.LESS: case TokenType.DOUBLE_EQUALS:
                case TokenType.NOT_EQUALS: case TokenType.LESS_EQUALS: case TokenType.GREATER_EQUALS:
                case TokenType.QUESTION: case TokenType.PIPE:
                case TokenType.RETURN: case TokenType.THROW:
                    return true;
                default:
                    return false;
            }
        }
    }
}
   