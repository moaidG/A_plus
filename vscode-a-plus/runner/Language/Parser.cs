using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A_
{
    class Parser
    {
        private List<Token> _tokens;
        private int _pos = 0;
        public List<string> Errors = new List<string>();

        public Parser(List<Token> tokens) => _tokens = tokens;

        Token Current() => _pos < _tokens.Count ? _tokens[_pos] : null;

        string Pos(Token t) => t != null ? $" (سطر {t.Line}, عمود {t.Column})" : "";
        string CurrentPos() => Pos(Current());
        string CurrentErr(string msg) => $"Error/خطأ ({Current().Line}, {Current().Column}): {msg}";

        void SetPos(Node n) { var t = Current(); if (t != null) { n.Line = t.Line; n.Column = t.Column; } }

        Token Eat(TokenType type)
        {
            var token = Current();
            if (token?.Type == type) { _pos++; return token; }
            string got = token != null ? $"{token.Type} ('{token.Value}'){Pos(token)}" : "نهاية الملف";
            string expected = $"{type}";
            string msg = $"Syntax error/خطأ نحوي{Pos(token)}: expected '{expected}' but got '{got}' / توقعت '{expected}' لكن وجدت '{got}'";
            Errors.Add(msg);
            throw new Exception(msg);
        }

        public Node Parse()
        {
            var statements = new List<Node>();
            while (Current() != null)
            {
                var c = Current();
                if (c.Type == TokenType.LET) statements.Add(ParseAssignment());
                else if (c.Type == TokenType.PRINT) statements.Add(ParsePrint());
                else if (c.Type == TokenType.IF) statements.Add(ParseIf());
                else if (c.Type == TokenType.WHILE) statements.Add(ParseWhile());
                else if (c.Type == TokenType.FOR) statements.Add(ParseFor());
                else if (c.Type == TokenType.FUNCTION) statements.Add(ParseFuncDef());
                else if (c.Type == TokenType.RETURN) statements.Add(ParseReturn());
                else if (c.Type == TokenType.ABSTRACT && _pos + 1 < _tokens.Count && _tokens[_pos + 1].Type == TokenType.CLASS)
                {
                    Eat(TokenType.ABSTRACT);
                    Eat(TokenType.CLASS);
                    var cls = (ClassDefNode)ParseClassDef();
                    cls.IsAbstract = true;
                    statements.Add(cls);
                }
                else if (c.Type == TokenType.INTERFACE)
                {
                    Eat(TokenType.INTERFACE);
                    var cls = (ClassDefNode)ParseClassDef(true);
                    cls.IsInterface = true;
                    statements.Add(cls);
                }
                else if (c.Type == TokenType.INCLUDE) statements.Add(ParseInclude());
                else if (c.Type == TokenType.THROW) statements.Add(ParseThrow());
                else if (c.Type == TokenType.TRY) statements.Add(ParseTry());
                else if (c.Type == TokenType.BREAK) { Eat(TokenType.BREAK); statements.Add(new BreakNode()); }
                else if (c.Type == TokenType.CONTINUE) { Eat(TokenType.CONTINUE); statements.Add(new ContinueNode()); }
                else if (c.Type == TokenType.SWITCH) statements.Add(ParseSwitch());
                else if (c.Type == TokenType.A_PLUS) statements.Add(ParseAPlusNew());
                 else if (c.Type == TokenType.SHOW) { Eat(TokenType.SHOW); var showArgs = Current()?.Type == TokenType.LPAREN ? ParseArgs() : null; if (Current()?.Type == TokenType.SEMICOLON) Eat(TokenType.SEMICOLON); statements.Add(new ShowNode(showArgs)); }
               else if (c.Type == TokenType.VIEWPORT3D)
                {
                    Eat(c.Type);
                    Eat(TokenType.NEW);
                    string varName = Eat(TokenType.IDENTIFIER).Value;
                    if (Current()?.Type == TokenType.SEMICOLON) Eat(TokenType.SEMICOLON);
                    statements.Add(new APlusNewNode(GetElementName(c.Type), varName));
                }
                else if (c.Type == TokenType.MEDIAELEMENT)
                {
                    Eat(c.Type);
                    Eat(TokenType.NEW);
                    string varName = Eat(TokenType.IDENTIFIER).Value;
                    if (Current()?.Type == TokenType.SEMICOLON) Eat(TokenType.SEMICOLON);
                    statements.Add(new APlusNewNode(GetElementName(c.Type), varName));
                }
                 else if (c.Type == TokenType.CLASS) { Eat(TokenType.CLASS); statements.Add(ParseClassDef()); }
                else if (c.Type == TokenType.EXTERN) statements.Add(ParseExternDef());
                else if (c.Type == TokenType.EXPORT) statements.Add(ParseExport());
                else if (c.Type == TokenType.IMPORT) statements.Add(ParseImport());
                else if (c.Type == TokenType.ASYNC) { Eat(TokenType.ASYNC); var fn = (FuncDefNode)ParseFuncDef(); fn.IsAsync = true; statements.Add(fn); }
                else if (c.Type == TokenType.GO) statements.Add(ParseGo());
                else if (c.Type == TokenType.SPAWN) statements.Add(ParseSpawn());
                else if (c.Type == TokenType.DEBUGGER) { Eat(TokenType.DEBUGGER); var dn = new DebuggerNode(); SetPos(dn); statements.Add(dn); }
                else if (c.Type == TokenType.DOC_COMMENT)
                {
                    string doc = Eat(TokenType.DOC_COMMENT).Value;
                    var next = Current();
                    if (next?.Type == TokenType.FUNCTION || next?.Type == TokenType.PUBLIC || next?.Type == TokenType.PRIVATE || next?.Type == TokenType.STATIC)
                    {
                        // Peek ahead past modifiers to find the function keyword
                        Node stmt = ParseStatement();
                        if (stmt is FuncDefNode fn) fn.Doc = doc;
                        statements.Add(stmt);
                    }
                    else if (next?.Type == TokenType.CLASS || next?.Type == TokenType.ABSTRACT || next?.Type == TokenType.INTERFACE)
                        statements.Add(ParseClassDefWithDoc(doc));
                    else
                        statements.Add(new BlockNode(new List<Node>()));
                }
                else if (c.Type == TokenType.IDENTIFIER &&
                         _pos + 1 < _tokens.Count &&
                         (_tokens[_pos + 1].Type == TokenType.EQUALS ||
                          _tokens[_pos + 1].Type == TokenType.PLUS_EQUALS ||
                          _tokens[_pos + 1].Type == TokenType.MINUS_EQUALS ||
                          _tokens[_pos + 1].Type == TokenType.STAR_EQUALS ||
                          _tokens[_pos + 1].Type == TokenType.SLASH_EQUALS))
                    statements.Add(ParseReAssignment());
                else if (IsMemberSet())
                    statements.Add(ParseMemberSet());
                else if (c.Type == TokenType.IDENTIFIER &&
                         _pos + 1 < _tokens.Count && _tokens[_pos + 1].Type == TokenType.LBRACKET)
                    statements.Add(ParseArrayIndex());
                else if (c.Type == TokenType.SELF &&
                         _pos + 1 < _tokens.Count && _tokens[_pos + 1].Type == TokenType.LBRACKET)
                    statements.Add(ParseArrayIndexSelf());
                else if (c.Type == TokenType.IDENTIFIER &&
                         _pos + 3 < _tokens.Count &&
                         _tokens[_pos + 1].Type == TokenType.DOT &&
                         _tokens[_pos + 2].Type == TokenType.IDENTIFIER &&
                         _tokens[_pos + 3].Type == TokenType.ARROW)
                    statements.Add(ParseArrowBind());
                else
                    statements.Add(ParseExpression());
                if (Current()?.Type == TokenType.SEMICOLON)
                    Eat(TokenType.SEMICOLON);
            }
            if (statements.Count == 0) throw new Exception("Error/خطأ: Empty program / برنامج فارغ");
            if (statements.Count == 1) return statements[0];
            return new BlockNode(statements);
        }

        Node ParseStatement()
        {
            var c = Current();
            if (c?.Type == TokenType.LET) return ParseAssignment();
            if (c?.Type == TokenType.IDENTIFIER && _pos + 1 < _tokens.Count)
            {
                var next = _tokens[_pos + 1];
                if (next.Type == TokenType.EQUALS || next.Type == TokenType.PLUS_EQUALS ||
                    next.Type == TokenType.MINUS_EQUALS || next.Type == TokenType.STAR_EQUALS ||
                    next.Type == TokenType.SLASH_EQUALS)
                    return ParseReAssignment();
            }
            if (c?.Type == TokenType.PRINT) return ParsePrint();
            if (c?.Type == TokenType.IF) return ParseIf();
            if (c?.Type == TokenType.WHILE) return ParseWhile();
            if (c?.Type == TokenType.FOR) return ParseFor();
            if (c?.Type == TokenType.FUNCTION) return ParseFuncDef();
            if (c?.Type == TokenType.RETURN) return ParseReturn();
            if (c?.Type == TokenType.INCLUDE) return ParseInclude();
            if (c?.Type == TokenType.THROW) return ParseThrow();
            if (c?.Type == TokenType.TRY) return ParseTry();
            if (c?.Type == TokenType.BREAK) { Eat(TokenType.BREAK); return new BreakNode(); }
            if (c?.Type == TokenType.CONTINUE) { Eat(TokenType.CONTINUE); return new ContinueNode(); }
            if (c?.Type == TokenType.SWITCH) return ParseSwitch();
            if (c?.Type == TokenType.EXPORT) return ParseExport();
            if (c?.Type == TokenType.IMPORT) return ParseImport();
            if (c?.Type == TokenType.DEBUGGER) { Eat(TokenType.DEBUGGER); var dn = new DebuggerNode(); SetPos(dn); return dn; }
            if (c?.Type == TokenType.ASYNC) { Eat(TokenType.ASYNC); var fn = (FuncDefNode)ParseFuncDef(); fn.IsAsync = true; return fn; }
            if (c?.Type == TokenType.GO) return ParseGo();
            if (c?.Type == TokenType.SPAWN) return ParseSpawn();
            if (c?.Type == TokenType.A_PLUS) return ParseAPlusNew();
             if (c?.Type == TokenType.SHOW) { Eat(TokenType.SHOW); var showArgs = Current()?.Type == TokenType.LPAREN ? ParseArgs() : null; if (Current()?.Type == TokenType.SEMICOLON) Eat(TokenType.SEMICOLON); return new ShowNode(showArgs); }
              // Removed VIEWPORT3D handling from ParseStatement - it's handled in Parse()
            if (c?.Type == TokenType.CLASS) { Eat(TokenType.CLASS); return ParseClassDef(); }
            if (c?.Type == TokenType.ABSTRACT && _pos + 1 < _tokens.Count && _tokens[_pos + 1].Type == TokenType.CLASS)
            {
                Eat(TokenType.ABSTRACT);
                Eat(TokenType.CLASS);
                var cls = (ClassDefNode)ParseClassDef();
                cls.IsAbstract = true;
                return cls;
            }
            if (c?.Type == TokenType.INTERFACE)
            {
                Eat(TokenType.INTERFACE);
                var cls = (ClassDefNode)ParseClassDef(true);
                cls.IsInterface = true;
                return cls;
            }
            if (IsMemberSet())
                return ParseMemberSet();
            if (c?.Type == TokenType.SELF &&
                _pos + 1 < _tokens.Count && _tokens[_pos + 1].Type == TokenType.LBRACKET)
                return ParseArrayIndexSelf();
            if (c?.Type == TokenType.IDENTIFIER &&
                _pos + 3 < _tokens.Count &&
                _tokens[_pos + 1].Type == TokenType.DOT &&
                _tokens[_pos + 2].Type == TokenType.IDENTIFIER &&
                _tokens[_pos + 3].Type == TokenType.ARROW)
                return ParseArrowBind();
            if (c?.Type == TokenType.SELF) return ParsePrimary();
            if (c?.Type == TokenType.IDENTIFIER &&
                _pos + 1 < _tokens.Count && _tokens[_pos + 1].Type == TokenType.LBRACKET)
                return ParseArrayIndex();
            if (c?.Type == TokenType.LESS) return ParseXmlElement();
            if (c?.Type == TokenType.IDENTIFIER || c?.Type == TokenType.NUMBER || c?.Type == TokenType.STRING ||
                c?.Type == TokenType.TRUE || c?.Type == TokenType.FALSE || c?.Type == TokenType.LPAREN ||
                c?.Type == TokenType.NEW || c?.Type == TokenType.BANG || c?.Type == TokenType.NOT)
                return ParseExpression();
            throw new Exception(CurrentErr($"Unknown statement / عبارة غير معروفة: '{Current()?.Value}'"));
        }

        Node ParseAssignment()
        {
            Eat(TokenType.LET);
            bool isLet = true;
            // Destructuring: let [a, b] = expr  or  let {x, y} = expr
            if (Current()?.Type == TokenType.LBRACKET)
            {
                Eat(TokenType.LBRACKET);
                var names = new List<string>();
                if (Current()?.Type != TokenType.RBRACKET)
                {
                    names.Add(EatName());
                    while (Current()?.Type == TokenType.COMMA)
                    { Eat(TokenType.COMMA); names.Add(EatName()); }
                }
                Eat(TokenType.RBRACKET);
                Eat(TokenType.EQUALS);
                var dst = new ArrayDestructureNode(names, ParseExpression());
                SetPos(dst);
                return dst;
            }
            if (Current()?.Type == TokenType.LBRACE)
            {
                Eat(TokenType.LBRACE);
                var names = new List<string>();
                if (Current()?.Type != TokenType.RBRACE)
                {
                    names.Add(EatName());
                    while (Current()?.Type == TokenType.COMMA)
                    { Eat(TokenType.COMMA); names.Add(EatName()); }
                }
                Eat(TokenType.RBRACE);
                Eat(TokenType.EQUALS);
                var dst = new ObjectDestructureNode(names, ParseExpression());
                SetPos(dst);
                return dst;
            }
            string name = EatName();
            bool typed = Current()?.Type == TokenType.COLON;
            string typeName = null;
            if (typed) { Eat(TokenType.COLON); typeName = EatName(); }
            Node node;
            if (Current()?.Type == TokenType.EQUALS)
            {
                Eat(TokenType.EQUALS);
                node = new AssignNode(name, ParseExpression(), typed, typeName) { IsLet = isLet };
            }
            else
                node = new AssignNode(name, null, typed, typeName) { IsLet = isLet };
            SetPos(node);
            return node;
        }

        Node ParseReAssignment()
        {
            string name = Eat(TokenType.IDENTIFIER).Value;
            if (Current()?.Type == TokenType.EQUALS)
            {
                Eat(TokenType.EQUALS);
                return new AssignNode(name, ParseExpression());
            }
            if (Current()?.Type == TokenType.PLUS_EQUALS)
            {
                Eat(TokenType.PLUS_EQUALS);
                return new AssignNode(name, new AddNode(new VariableNode(name), ParseExpression()));
            }
            if (Current()?.Type == TokenType.MINUS_EQUALS)
            {
                Eat(TokenType.MINUS_EQUALS);
                return new AssignNode(name, new SubtractNode(new VariableNode(name), ParseExpression()));
            }
            if (Current()?.Type == TokenType.STAR_EQUALS)
            {
                Eat(TokenType.STAR_EQUALS);
                return new AssignNode(name, new MultiplyNode(new VariableNode(name), ParseExpression()));
            }
            if (Current()?.Type == TokenType.SLASH_EQUALS)
            {
                Eat(TokenType.SLASH_EQUALS);
                return new AssignNode(name, new DivideNode(new VariableNode(name), ParseExpression()));
            }
            return new AssignNode(name, null);
        }

        Node ParsePrint()
        {
            Eat(TokenType.PRINT);
            Node value;
            if (Current()?.Type == TokenType.LPAREN)
            {
                Eat(TokenType.LPAREN);
                value = ParseExpression();
                Eat(TokenType.RPAREN);
            }
            else
            {
                value = ParseExpression();
            }
            return new PrintNode(value);
        }

        Node ParseExpression() => ParseTernary();

        Node ParseTernary()
        {
            Node left = ParseOr();
            if (Current()?.Type == TokenType.QUESTION)
            {
                Eat(TokenType.QUESTION);
                Node trueExpr = ParseExpression();
                Eat(TokenType.COLON);
                Node falseExpr = ParseExpression();
                var node = new TernaryNode(left, trueExpr, falseExpr);
                SetPos(node);
                return node;
            }
            return left;
        }

        Node ParseOr()
        {
            Node left = ParsePipe();
            while (Current()?.Type == TokenType.OR)
            {
                Eat(TokenType.OR);
                left = new OrNode(left, ParsePipe());
            }
            return left;
        }

        Node ParsePipe()
        {
            Node left = ParseAnd();
            while (Current()?.Type == TokenType.PIPE)
            {
                Eat(TokenType.PIPE);
                left = new PipeNode(left, ParseAnd());
            }
            return left;
        }

        Node ParseAnd()
        {
            Node left = ParseUnary();
            while (Current()?.Type == TokenType.AND)
            {
                Eat(TokenType.AND);
                left = new AndNode(left, ParseUnary());
            }
            return left;
        }

        Node ParseUnary()
        {
            if (Current()?.Type == TokenType.NOT)
            {
                Eat(TokenType.NOT);
                var node = new UnaryNode("NOT", ParseComparison());
                SetPos(node);
                return node;
            }
            if (Current()?.Type == TokenType.BANG)
            {
                Eat(TokenType.BANG);
                var node = new UnaryNode("NOT", ParseComparison());
                SetPos(node);
                return node;
            }
            return ParseComparison();
        }

        Node ParseComparison()
        {
            Node left = ParseAddSubtract();
            if (Current()?.Type == TokenType.GREATER) { Eat(TokenType.GREATER); return new GreaterNode(left, ParseAddSubtract()); }
            if (Current()?.Type == TokenType.LESS) { Eat(TokenType.LESS); return new LessNode(left, ParseAddSubtract()); }
            if (Current()?.Type == TokenType.DOUBLE_EQUALS) { Eat(TokenType.DOUBLE_EQUALS); return new EqualNode(left, ParseAddSubtract()); }
            if (Current()?.Type == TokenType.NOT_EQUALS) { Eat(TokenType.NOT_EQUALS); return new NotEqualNode(left, ParseAddSubtract()); }
            if (Current()?.Type == TokenType.LESS_EQUALS) { Eat(TokenType.LESS_EQUALS); return new LeNode(left, ParseAddSubtract()); }
            if (Current()?.Type == TokenType.GREATER_EQUALS) { Eat(TokenType.GREATER_EQUALS); return new GeNode(left, ParseAddSubtract()); }
            return left;
        }

        Node ParseRightOperand() => ParseUnary();

        Node ParseAddSubtract()
        {
            Node left = ParseTerm();
            while (Current() != null && (Current().Type == TokenType.PLUS || Current().Type == TokenType.MINUS))
            {
                if (Current().Type == TokenType.PLUS)
                {
                    Eat(TokenType.PLUS);
                    left = new AddNode(left, ParseTerm());
                }
                else
                {
                    Eat(TokenType.MINUS);
                    left = new SubtractNode(left, ParseTerm());
                }
            }
            return left;
        }

        Node ParsePrimary()
        {
            Node left = ParseFactor();
            while (Current()?.Type == TokenType.DOT || Current()?.Type == TokenType.LBRACKET
                   || Current()?.Type == TokenType.PLUS_PLUS || Current()?.Type == TokenType.MINUS_MINUS)
            {
                if (Current().Type == TokenType.DOT)
                {
                    Eat(TokenType.DOT);
                    string member = EatName();
                    if (Current()?.Type == TokenType.LPAREN)
                        left = new MethodCallNode(left, member, ParseArgs());
                    else
                        left = new MemberGetNode(left, member);
                }
                else if (Current().Type == TokenType.LBRACKET)
                {
                    Eat(TokenType.LBRACKET);
                    Node index = ParseOr();
                    Eat(TokenType.RBRACKET);
                    left = new ArrayGetNode(left, index);
                }
                else if (Current().Type == TokenType.PLUS_PLUS)
                {
                    Eat(TokenType.PLUS_PLUS);
                    left = new PostfixNode(left, "++");
                }
                else
                {
                    Eat(TokenType.MINUS_MINUS);
                    left = new PostfixNode(left, "--");
                }
            }
            return left;
        }

        Node ParseTerm()
        {
            Node left = ParsePrimary();
            while (Current() != null && (Current().Type == TokenType.STAR || Current().Type == TokenType.SLASH || Current().Type == TokenType.PERCENT))
            {
                if (Current().Type == TokenType.STAR)
                {
                    Eat(TokenType.STAR);
                    left = new MultiplyNode(left, ParsePrimary());
                }
                else if (Current().Type == TokenType.PERCENT)
                {
                    Eat(TokenType.PERCENT);
                    left = new ModuloNode(left, ParsePrimary());
                }
                else
                {
                    Eat(TokenType.SLASH);
                    left = new DivideNode(left, ParsePrimary());
                }
            }
            return left;
        }

        Node ParseFactor()
        {
            var token = Current();
            Node node;
            if (token.Type == TokenType.NUMBER) { Eat(TokenType.NUMBER); node = new NumberNode(double.Parse(token.Value)); SetPos(node); return node; }
            if (token.Type == TokenType.STRING) { Eat(TokenType.STRING); node = new StringNode(token.Value); SetPos(node); return node; }
            if (token.Type == TokenType.TRUE) { Eat(TokenType.TRUE); node = new BoolNode(true); SetPos(node); return node; }
            if (token.Type == TokenType.FALSE) { Eat(TokenType.FALSE); node = new BoolNode(false); SetPos(node); return node; }
            if (token.Type == TokenType.NEW) return ParseNew();
            if (token.Type == TokenType.SELF) { Eat(TokenType.SELF); node = new SelfNode(); SetPos(node); return node; }
            if (token.Type == TokenType.NIL) { Eat(TokenType.NIL); node = new NilNode(); SetPos(node); return node; }
            if (token.Type == TokenType.REGEX) { Eat(TokenType.REGEX); return ParseRegex(token); }
            if (token.Type == TokenType.AWAIT) { Eat(TokenType.AWAIT); node = new AwaitNode(ParseFactor()); SetPos(node); return node; }
            if (token.Type == TokenType.IDENTIFIER)
            {
                string name = token.Value;
                Eat(TokenType.IDENTIFIER);
                if (Current()?.Type == TokenType.LPAREN)
                    return ParseFuncCall(name);
                node = new VariableNode(name);
                SetPos(node);
                return node;
            }
            if (token.Type == TokenType.LPAREN)
            {
                Eat(TokenType.LPAREN);
                // Check for arrow function: () => body or (x) => body or (x,y) => body
                bool isArrow = false;
                int scan = _pos;
                if (scan < _tokens.Count && _tokens[scan].Type == TokenType.RPAREN)
                    isArrow = scan + 1 < _tokens.Count && _tokens[scan + 1].Type == TokenType.ARROW;
                else if (scan < _tokens.Count && _tokens[scan].Type == TokenType.IDENTIFIER)
                {
                    scan++;
                    while (scan < _tokens.Count && _tokens[scan].Type == TokenType.COMMA)
                    { scan++; if (scan < _tokens.Count && _tokens[scan].Type == TokenType.IDENTIFIER) scan++; else break; }
                    isArrow = scan < _tokens.Count && _tokens[scan].Type == TokenType.RPAREN &&
                              scan + 1 < _tokens.Count && _tokens[scan + 1].Type == TokenType.ARROW;
                }
                if (isArrow)
                {
                    var parameters = new List<string>();
                    if (Current()?.Type == TokenType.IDENTIFIER)
                    {
                        parameters.Add(EatName());
                        while (Current()?.Type == TokenType.COMMA)
                        { Eat(TokenType.COMMA); parameters.Add(EatName()); }
                    }
                    Eat(TokenType.RPAREN);
                    Eat(TokenType.ARROW);
                    Node body = Current()?.Type == TokenType.LBRACE ? ParseBlock() : ParseExpression();
                    return new FuncDefNode("", parameters, body);
                }
                var expr = ParseExpression();
                Eat(TokenType.RPAREN);
                return expr;
            }
            if (token.Type == TokenType.LBRACKET)
            {
                Eat(TokenType.LBRACKET);
                var elements = new List<Node>();
                if (Current()?.Type != TokenType.RBRACKET)
                {
                    elements.Add(ParseOr());
                    while (Current()?.Type == TokenType.COMMA)
                    {
                        Eat(TokenType.COMMA);
                        elements.Add(ParseOr());
                    }
                }
                Eat(TokenType.RBRACKET);
                node = new ArrayLiteralNode(elements);
                SetPos(node);
                return node;
            }
            if (token.Type == TokenType.MINUS)
            {
                Eat(TokenType.MINUS);
                var inner = ParsePrimary();
                var unary = new UnaryNode("NEGATE", inner);
                SetPos(unary);
                return unary;
            }
            if (token.Type == TokenType.LBRACE)
            {
                Eat(TokenType.LBRACE);
                var fields = new List<(string Key, Node Value)>();
                if (Current()?.Type != TokenType.RBRACE)
                {
                    var key = EatName();
                    Eat(TokenType.COLON);
                    var val = ParseOr();
                    fields.Add((key, val));
                    while (Current()?.Type == TokenType.COMMA)
                    {
                        Eat(TokenType.COMMA);
                        key = EatName();
                        Eat(TokenType.COLON);
                        val = ParseOr();
                        fields.Add((key, val));
                    }
                }
                Eat(TokenType.RBRACE);
                var objLit = new ObjectLiteralNode(fields);
                SetPos(objLit);
                return objLit;
            }
                throw new Exception(CurrentErr($"Unexpected expression / تعبير غير متوقع: '{Current()?.Value}'"));
        }

        Node ParseRegex(Token token)
        {
            string raw = token.Value; // /pattern/flags
            int lastSlash = raw.LastIndexOf('/');
            string pattern = raw.Substring(1, lastSlash - 1);
            string flags = lastSlash < raw.Length - 1 ? raw.Substring(lastSlash + 1) : "";
            var node = new RegexNode(pattern, flags);
            SetPos(node);
            return node;
        }

        Node ParseFuncCall(string name)
        {
            var args = ParseArgs();
            var node = new FuncCallNode(name, args);
            SetPos(node);
            return node;
        }

        Node ParseIf()
        {
            Eat(TokenType.IF);
            Eat(TokenType.LPAREN);
            Node condition = ParseOr();
            Eat(TokenType.RPAREN);
            Node body = Current().Type == TokenType.LBRACE ? ParseBlock() : ParseStatement();
            Node elseBody = null;
            if (Current()?.Type == TokenType.ELSE)
            {
                Eat(TokenType.ELSE);
                elseBody = Current().Type == TokenType.LBRACE ? ParseBlock() : ParseStatement();
            }
            return new IfNode(condition, body, elseBody);
        }

        Node ParseCondition() => ParseOr();

        Node ParseWhile()
        {
            Eat(TokenType.WHILE);
            Eat(TokenType.LPAREN);
            Node condition = ParseOr();
            Eat(TokenType.RPAREN);
            Node body = Current().Type == TokenType.LBRACE ? ParseBlock() : ParseStatement();
            return new WhileNode(condition, body);
        }

        Node ParseSwitch()
        {
            Eat(TokenType.SWITCH);
            Eat(TokenType.LPAREN);
            Node value = ParseOr();
            Eat(TokenType.RPAREN);
            // Optional opening brace
            bool hasBrace = Current()?.Type == TokenType.LBRACE;
            if (hasBrace) Eat(TokenType.LBRACE);
            var cases = new List<CaseNode>();
            Node defaultBody = null;
            while (Current() != null && (hasBrace ? Current().Type != TokenType.RBRACE : (Current().Type == TokenType.CASE || Current().Type == TokenType.DEFAULT)))
            {
                if (Current()?.Type == TokenType.CASE)
                {
                    Eat(TokenType.CASE);
                    Node caseVal = ParseOr();
                    Eat(TokenType.COLON);
                    var bodyStmts = new List<Node>();
                    while (Current() != null && Current().Type != TokenType.CASE && Current().Type != TokenType.DEFAULT && Current().Type != TokenType.RBRACE)
                        bodyStmts.Add(ParseStatement());
                    Node body = bodyStmts.Count == 1 ? bodyStmts[0] : new BlockNode(bodyStmts);
                    cases.Add(new CaseNode(caseVal, body));
                }
                else if (Current()?.Type == TokenType.DEFAULT)
                {
                    Eat(TokenType.DEFAULT);
                    Eat(TokenType.COLON);
                    var bodyStmts = new List<Node>();
                    while (Current() != null && Current().Type != TokenType.CASE && Current().Type != TokenType.DEFAULT && Current().Type != TokenType.RBRACE)
                        bodyStmts.Add(ParseStatement());
                    defaultBody = bodyStmts.Count == 1 ? bodyStmts[0] : new BlockNode(bodyStmts);
                }
                else if (hasBrace)
                {
                    throw new Exception(CurrentErr("Expected case or default inside switch / متوقع case أو default داخل switch"));
                }
                else break;
            }
            if (hasBrace) Eat(TokenType.RBRACE);
            var node = new SwitchNode(value, cases, defaultBody);
            SetPos(node);
            return node;
        }

        Node ParseFor()
        {
            Eat(TokenType.FOR);
            Eat(TokenType.LPAREN);

            // Check for for...in syntax: for (x in arr)
            if (_pos + 1 < _tokens.Count)
            {
                // Look ahead: IDENTIFIER IN
                if (Current()?.Type == TokenType.IDENTIFIER &&
                    _tokens[_pos + 1]?.Type == TokenType.IN)
                {
                    string forVarName = Eat(TokenType.IDENTIFIER).Value;
                    Eat(TokenType.IN);
                    Node forIterable = ParseOr();
                    Eat(TokenType.RPAREN);
                    Node forBody = Current().Type == TokenType.LBRACE ? ParseBlock() : ParseStatement();
                    var forNode = new ForInNode(forVarName, forIterable, forBody);
                    SetPos(forNode);
                    return forNode;
                }
                // Look ahead: let IDENTIFIER IN
                if (Current()?.Type == TokenType.LET &&
                    _pos + 2 < _tokens.Count &&
                    _tokens[_pos + 2]?.Type == TokenType.IN)
                {
                    Eat(TokenType.LET);
                    string forVarName = Eat(TokenType.IDENTIFIER).Value;
                    Eat(TokenType.IN);
                    Node forIterable = ParseOr();
                    Eat(TokenType.RPAREN);
                    Node forBody = Current().Type == TokenType.LBRACE ? ParseBlock() : ParseStatement();
                    var forNode = new ForInNode(forVarName, forIterable, forBody);
                    SetPos(forNode);
                    return forNode;
                }
            }

            Node init = null;
            if (Current()?.Type == TokenType.LET)
                init = ParseAssignment();
            else if (Current()?.Type == TokenType.IDENTIFIER &&
                     _pos + 1 < _tokens.Count && _tokens[_pos + 1].Type == TokenType.EQUALS)
                init = ParseReAssignment();

            Eat(TokenType.SEMICOLON);

            Node condition = null;
            if (Current()?.Type != TokenType.SEMICOLON)
                condition = ParseOr();
            Eat(TokenType.SEMICOLON);

            Node increment = null;
            if (Current()?.Type != TokenType.RPAREN)
            {
                if (Current()?.Type == TokenType.IDENTIFIER &&
                    _pos + 1 < _tokens.Count &&
                    (_tokens[_pos + 1].Type == TokenType.EQUALS ||
                     _tokens[_pos + 1].Type == TokenType.PLUS_EQUALS ||
                     _tokens[_pos + 1].Type == TokenType.MINUS_EQUALS ||
                     _tokens[_pos + 1].Type == TokenType.STAR_EQUALS ||
                     _tokens[_pos + 1].Type == TokenType.SLASH_EQUALS))
                    increment = ParseReAssignment();
                else
                    increment = ParseOr();
            }
            Eat(TokenType.RPAREN);

            Node body = Current().Type == TokenType.LBRACE ? ParseBlock() : ParseStatement();
            return new ForNode(init, condition, increment, body);
        }

        List<string> ParseTypeParams()
        {
            var tps = new List<string>();
            Eat(TokenType.LBRACKET);
            tps.Add(EatName());
            while (Current()?.Type == TokenType.COMMA)
            {
                Eat(TokenType.COMMA);
                tps.Add(EatName());
            }
            Eat(TokenType.RBRACKET);
            return tps;
        }

        Node ParseFuncDef(string doc = null)
        {
            Eat(TokenType.FUNCTION);
            string name = Eat(TokenType.IDENTIFIER).Value;
            List<string> typeParams = null;
            if (Current()?.Type == TokenType.LBRACKET)
                typeParams = ParseTypeParams();
            Eat(TokenType.LPAREN);
            var parameters = new List<string>();
            var defaults = new Dictionary<string, Node>();
            var paramTypes = new Dictionary<string, string>();
            if (Current()?.Type != TokenType.RPAREN)
            {
                string pname = Eat(TokenType.IDENTIFIER).Value;
                if (Current()?.Type == TokenType.COLON) { Eat(TokenType.COLON); paramTypes[pname] = EatName(); }
                if (Current()?.Type == TokenType.EQUALS) { Eat(TokenType.EQUALS); defaults[pname] = ParseOr(); }
                parameters.Add(pname);
                while (Current()?.Type == TokenType.COMMA)
                {
                    Eat(TokenType.COMMA);
                    pname = Eat(TokenType.IDENTIFIER).Value;
                    if (Current()?.Type == TokenType.COLON) { Eat(TokenType.COLON); paramTypes[pname] = EatName(); }
                    if (Current()?.Type == TokenType.EQUALS) { Eat(TokenType.EQUALS); defaults[pname] = ParseOr(); }
                    parameters.Add(pname);
                }
            }
            Eat(TokenType.RPAREN);
            string returnType = null;
            if (Current()?.Type == TokenType.COLON) { Eat(TokenType.COLON); returnType = EatName(); }
            Node body = Current().Type == TokenType.LBRACE ? ParseBlock() : ParseStatement();
            var funcNode = new FuncDefNode(name, parameters, body, AccessFlags.Public, defaults, paramTypes, returnType);
            funcNode.TypeParams = typeParams;
            funcNode.Doc = doc;
            SetPos(funcNode);
            return funcNode;
        }

        Node ParseReturn()
        {
            Eat(TokenType.RETURN);
            Node value = null;
            if (Current() != null)
            {
                var ct = Current().Type;
                if (ct == TokenType.NUMBER || ct == TokenType.STRING || ct == TokenType.TRUE || ct == TokenType.FALSE ||
                    ct == TokenType.NEW || ct == TokenType.SELF || ct == TokenType.IDENTIFIER ||
                    ct == TokenType.LPAREN || ct == TokenType.LBRACKET || ct == TokenType.BANG || ct == TokenType.NOT || ct == TokenType.MINUS ||
                    ct == TokenType.NIL)
                    value = ParseExpression();
            }
            var node = new ReturnNode(value);
            SetPos(node);
            return node;
        }

        Node ParseInclude()
        {
            Eat(TokenType.INCLUDE);
            string path = Eat(TokenType.STRING).Value;
            string from = null;
            if (Current()?.Type == TokenType.FROM)
            {
                Eat(TokenType.FROM);
                if (Current()?.Type == TokenType.STRING)
                    from = Eat(TokenType.STRING).Value;
                else
                    from = Eat(TokenType.IDENTIFIER).Value;
            }
            return new IncludeNode(path, from);
        }

        List<Node> ParseArgs()
        {
            Eat(TokenType.LPAREN);
            var args = new List<Node>();
            if (Current()?.Type != TokenType.RPAREN)
            {
                args.Add(ParseArg());
                while (Current()?.Type == TokenType.COMMA)
                {
                    Eat(TokenType.COMMA);
                    args.Add(ParseArg());
                }
            }
            Eat(TokenType.RPAREN);
            return args;
        }

        Node ParseArg()
        {
            if (Current()?.Type == TokenType.IDENTIFIER &&
                _pos + 1 < _tokens.Count && _tokens[_pos + 1].Type == TokenType.EQUALS)
            {
                string name = Eat(TokenType.IDENTIFIER).Value;
                Eat(TokenType.EQUALS);
                return new NamedArgNode(name, ParseExpression());
            }
            return ParseExpression();
        }

        Node ParseThrow()
        {
            Eat(TokenType.THROW);
            return new ThrowNode(ParseExpression());
        }

        Node ParseTry()
        {
            Eat(TokenType.TRY);
            Node tryBody = Current().Type == TokenType.LBRACE ? ParseBlock() : ParseStatement();
            string catchVar = null;
            Node catchBody = null;
            Node finallyBody = null;
            if (Current()?.Type == TokenType.CATCH)
            {
                Eat(TokenType.CATCH);
                if (Current()?.Type == TokenType.LPAREN)
                {
                    Eat(TokenType.LPAREN);
                    catchVar = Eat(TokenType.IDENTIFIER).Value;
                    Eat(TokenType.RPAREN);
                }
                catchBody = Current().Type == TokenType.LBRACE ? ParseBlock() : ParseStatement();
            }
            if (Current()?.Type == TokenType.FINALLY)
            {
                Eat(TokenType.FINALLY);
                finallyBody = Current().Type == TokenType.LBRACE ? ParseBlock() : ParseStatement();
            }
            return new TryNode(tryBody, catchVar, catchBody, finallyBody);
        }

        Node ParseNew()
        {
            Eat(TokenType.NEW);
            string className = Eat(TokenType.IDENTIFIER).Value;
            var args = ParseArgs();
            var node = new NewNode(className, args);
            SetPos(node);
            return node;
        }

        bool IsMemberSet()
        {
            var c = Current();
            if (c?.Type != TokenType.IDENTIFIER && c?.Type != TokenType.SELF) return false;
            int scan = _pos + 1;
            while (scan + 2 < _tokens.Count && _tokens[scan].Type == TokenType.DOT &&
                   _tokens[scan + 1].Type == TokenType.IDENTIFIER)
            {
                scan += 2;
                if (scan < _tokens.Count && _tokens[scan].Type == TokenType.EQUALS)
                    return true;
            }
            return false;
        }

        Node ParseMemberSet()
        {
            Node obj;
            if (Current()?.Type == TokenType.SELF)
            {
                Eat(TokenType.SELF);
                obj = new SelfNode();
            }
            else
            {
                string name = Eat(TokenType.IDENTIFIER).Value;
                obj = new VariableNode(name);
            }
            while (Current()?.Type == TokenType.DOT)
            {
                Eat(TokenType.DOT);
                string member = Eat(TokenType.IDENTIFIER).Value;
                if (Current()?.Type == TokenType.EQUALS)
                {
                    Eat(TokenType.EQUALS);
                    Node value = ParseExpression();
                    var msNode = new MemberSetNode(obj, member, value);
                    SetPos(msNode);
                    return msNode;
                }
                obj = new MemberGetNode(obj, member);
            }
            throw new Exception("Expected = in member set");
        }

        Node ParseArrayIndex()
        {
            Node obj = new VariableNode(Eat(TokenType.IDENTIFIER).Value);
            while (Current()?.Type == TokenType.LBRACKET)
            {
                Eat(TokenType.LBRACKET);
                Node index = ParseOr();
                Eat(TokenType.RBRACKET);
                if (Current()?.Type == TokenType.EQUALS)
                {
                    Eat(TokenType.EQUALS);
                    return new ArraySetNode(obj, index, ParseOr());
                }
                obj = new ArrayGetNode(obj, index);
            }
            return obj;
        }

        Node ParseArrayIndexSelf()
        {
            Eat(TokenType.SELF);
            Node obj = new SelfNode();
            while (Current()?.Type == TokenType.LBRACKET)
            {
                Eat(TokenType.LBRACKET);
                Node index = ParseOr();
                Eat(TokenType.RBRACKET);
                if (Current()?.Type == TokenType.EQUALS)
                {
                    Eat(TokenType.EQUALS);
                    return new ArraySetNode(obj, index, ParseOr());
                }
                obj = new ArrayGetNode(obj, index);
            }
            return obj;
        }

        AccessFlags ParseAccessModifiers()
        {
            AccessFlags flags = AccessFlags.Public;
            while (true)
            {
                if (Current()?.Type == TokenType.PUBLIC) { Eat(TokenType.PUBLIC); flags |= AccessFlags.Public; }
                else if (Current()?.Type == TokenType.PRIVATE) { Eat(TokenType.PRIVATE); flags = (flags & ~AccessFlags.Public) | AccessFlags.Private; }
                else if (Current()?.Type == TokenType.STATIC) { Eat(TokenType.STATIC); flags |= AccessFlags.Static; }
                else if (Current()?.Type == TokenType.ABSTRACT) { Eat(TokenType.ABSTRACT); flags |= AccessFlags.Abstract; }
                else break;
            }
            return flags;
        }

        Node ParseClassDefWithDoc(string doc)
        {
            var c = Current();
            if (c?.Type == TokenType.ABSTRACT)
            {
                Eat(TokenType.ABSTRACT);
                Eat(TokenType.CLASS);
                var cls = (ClassDefNode)ParseClassDef(false, doc);
                cls.IsAbstract = true;
                return cls;
            }
            if (c?.Type == TokenType.INTERFACE)
            {
                Eat(TokenType.INTERFACE);
                var cls = (ClassDefNode)ParseClassDef(true, doc);
                cls.IsInterface = true;
                return cls;
            }
            Eat(TokenType.CLASS);
            return ParseClassDef(false, doc);
        }

        string EatName()
        {
            var t = Current();
            if (t != null && t.Type != TokenType.LPAREN && t.Type != TokenType.RPAREN && t.Type != TokenType.LBRACE && t.Type != TokenType.RBRACE && t.Type != TokenType.LBRACKET && t.Type != TokenType.RBRACKET)
            {
                _pos++;
                return t.Value;
            }
            Errors.Add($"Syntax error/خطأ نحوي{Pos(t)}: Expected a name but got '{t?.Type}' / توقعت اسماً لكن وجدت '{t?.Type}'");
            return "";
        }

        Node ParseClassDef(bool isInterface = false, string doc = null)
        {
            string name = EatName();
            List<string> typeParams = null;
            if (Current()?.Type == TokenType.LBRACKET)
                typeParams = ParseTypeParams();
            string parentName = null;
            if (Current()?.Type == TokenType.SLASH)
            {
                Eat(TokenType.SLASH);
                parentName = EatName();
            }
            else if (Current()?.Type == TokenType.EXTENDS)
            {
                Eat(TokenType.EXTENDS);
                parentName = EatName();
            }
            Eat(TokenType.LBRACE);
            var fields = new List<FieldDefNode>();
            var methods = new List<FuncDefNode>();
            var initStmts = new List<Node>();
            string pendingDoc = null;
            while (Current()?.Type != TokenType.RBRACE)
            {
                if (Current()?.Type == TokenType.DOC_COMMENT)
                {
                    pendingDoc = Eat(TokenType.DOC_COMMENT).Value;
                    continue;
                }
                AccessFlags mods = ParseAccessModifiers();
                if (Current()?.Type == TokenType.LET)
                {
                    Eat(TokenType.LET);
                    string fname = EatName();
                    if (Current()?.Type == TokenType.EQUALS)
                    {
                        if (isInterface) throw new Exception(CurrentErr("Cannot use let with = in interface / لا يمكن استخدام let مع = في interface"));
                        Eat(TokenType.EQUALS);
                        initStmts.Add(new AssignNode(fname, ParseOr()));
                        fields.Add(new FieldDefNode(fname, mods));
                    }
                    else
                    {
                        if (isInterface) throw new Exception(CurrentErr("No fields allowed in interface / لا يمكن وجود حقول في interface"));
                        fields.Add(new FieldDefNode(fname, mods));
                    }
                    pendingDoc = null;
                }
                else if (Current()?.Type == TokenType.FUNCTION)
                {
                    if (isInterface) mods |= AccessFlags.Abstract;
                    Eat(TokenType.FUNCTION);
                    string mname = EatName();
                    Eat(TokenType.LPAREN);
                    var parameters = new List<string>();
                    var defaults = new Dictionary<string, Node>();
                    if (Current()?.Type != TokenType.RPAREN)
                    {
                        string pname = Eat(TokenType.IDENTIFIER).Value;
                        if (Current()?.Type == TokenType.EQUALS) { Eat(TokenType.EQUALS); defaults[pname] = ParseOr(); }
                        parameters.Add(pname);
                        while (Current()?.Type == TokenType.COMMA)
                        {
                            Eat(TokenType.COMMA);
                            pname = Eat(TokenType.IDENTIFIER).Value;
                            if (Current()?.Type == TokenType.EQUALS) { Eat(TokenType.EQUALS); defaults[pname] = ParseOr(); }
                            parameters.Add(pname);
                        }
                    }
                    Eat(TokenType.RPAREN);
                    Node body = null;
                    if ((mods & AccessFlags.Abstract) == 0)
                        body = Current().Type == TokenType.LBRACE ? ParseBlock() : ParseStatement();
                    var m = new FuncDefNode(mname, parameters, body, mods, defaults);
                    m.Doc = pendingDoc;
                    methods.Add(m);
                    pendingDoc = null;
                }
                else
                {
                    if (isInterface) throw new Exception(CurrentErr("Only let or func allowed in interface / في interface مسموح فقط let أو func"));
                    initStmts.Add(ParseStatement());
                    pendingDoc = null;
                }
            }
            Eat(TokenType.RBRACE);
            Node initBody = initStmts.Count > 0 ? new BlockNode(initStmts) : null;
            var clsNode = new ClassDefNode(name, fields, methods, parentName, false, isInterface, initBody);
            clsNode.TypeParams = typeParams;
            clsNode.Doc = doc;
            SetPos(clsNode);
            return clsNode;
        }

        Node ParseBlock()
        {
            Eat(TokenType.LBRACE);
            var statements = new List<Node>();
            while (Current() != null && Current().Type != TokenType.RBRACE)
            {
                statements.Add(ParseStatement());
                if (Current()?.Type == TokenType.SEMICOLON)
                    Eat(TokenType.SEMICOLON);
            }
            Eat(TokenType.RBRACE);
            return new BlockNode(statements);
        }

         string GetElementName(TokenType type)
         {
             switch (type)
             {
                 case TokenType.VIEWPORT3D: return "Viewport3D";
                 case TokenType.MEDIAELEMENT: return "MediaElement";
                 default: return "Unknown";
             }
         }

        Node ParseAPlusNew()
        {
            Eat(TokenType.A_PLUS);
            if (Current()?.Type == TokenType.IDENTIFIER && Current().Value == "xml")
            {
                Eat(TokenType.IDENTIFIER);
                string xmlVar = EatName();
                Eat(TokenType.EQUALS);
                var node = (XmlElementNode)ParseXmlElement();
                var xn = new APlusXmlNode(xmlVar, node);
                SetPos(xn);
                return xn;
            }
            Eat(TokenType.NEW);
            string className = EatName();
            string varName = EatName();
            if (Current()?.Type == TokenType.EQUALS)
            {
                Eat(TokenType.EQUALS);
                Eat(TokenType.LBRACE);
                var body = new List<Node>();
                while (Current() != null && Current().Type != TokenType.RBRACE)
                {
                    body.Add(ParseStatement());
                    if (Current()?.Type == TokenType.SEMICOLON) Eat(TokenType.SEMICOLON);
                }
                Eat(TokenType.RBRACE);
                var apNode = new APlusNewNode(className, varName);
                SetPos(apNode);
                return apNode;
            }
            if (Current()?.Type == TokenType.SEMICOLON) Eat(TokenType.SEMICOLON);
            return new APlusNewNode(className, varName);
        }

        Node ParseXmlElement()
        {
            Eat(TokenType.LESS);
            string tagName = Eat(TokenType.IDENTIFIER).Value;
            var attrs = new List<(string, Node)>();
            var children = new List<Node>();
            bool selfClosing = false;
            while (Current() != null)
            {
                if (Current().Type == TokenType.SLASH)
                {
                    Eat(TokenType.SLASH);
                    Eat(TokenType.GREATER);
                    selfClosing = true;
                    break;
                }
                if (Current().Type == TokenType.GREATER)
                {
                    Eat(TokenType.GREATER);
                    break;
                }
                string attrName = Eat(TokenType.IDENTIFIER).Value;
                Node attrValue = null;
                if (Current()?.Type == TokenType.EQUALS)
                {
                    Eat(TokenType.EQUALS);
                    attrValue = ParseXmlAttrValue();
                }
                attrs.Add((attrName, attrValue));
            }
            if (!selfClosing)
            {
                while (Current() != null && !IsClosingTag(tagName))
                {
                    if (Current().Type == TokenType.LESS && (_pos + 1 >= _tokens.Count || _tokens[_pos + 1].Type != TokenType.SLASH))
                        children.Add(ParseXmlElement());
                    else
                        children.Add(ParseExpression());
                }
                Eat(TokenType.LESS);
                Eat(TokenType.SLASH);
                Eat(TokenType.IDENTIFIER);
                Eat(TokenType.GREATER);
            }
            var node = new XmlElementNode(tagName, attrs, children, selfClosing);
            SetPos(node);
            return node;
        }

        Node ParseXmlAttrValue()
        {
            if (Current()?.Type == TokenType.STRING)
                return new StringNode(Eat(TokenType.STRING).Value);
            if (Current()?.Type == TokenType.IDENTIFIER && (_pos + 1 >= _tokens.Count || _tokens[_pos + 1].Type != TokenType.ARROW))
                return new VariableNode(Eat(TokenType.IDENTIFIER).Value);
            return ParseExpression();
        }

        bool IsClosingTag(string tagName)
        {
            if (Current()?.Type != TokenType.LESS) return false;
            if (_pos + 2 >= _tokens.Count) return false;
            if (_tokens[_pos + 1].Type != TokenType.SLASH) return false;
            if (_tokens[_pos + 2].Type != TokenType.IDENTIFIER) return false;
            return _tokens[_pos + 2].Value == tagName;
        }

        Node ParseArrowBind()
        {
            string varName = Eat(TokenType.IDENTIFIER).Value;
            Eat(TokenType.DOT);
            string eventName = Eat(TokenType.IDENTIFIER).Value;
            Eat(TokenType.ARROW);
            string funcName = Eat(TokenType.IDENTIFIER).Value;
            if (Current()?.Type == TokenType.LPAREN)
            {
                Eat(TokenType.LPAREN);
                Eat(TokenType.RPAREN);
            }
            if (Current()?.Type == TokenType.SEMICOLON) Eat(TokenType.SEMICOLON);
            return new ArrowBindNode(varName, eventName, funcName);
        }

        Node ParseExternDef()
        {
            Eat(TokenType.EXTERN);
            Eat(TokenType.FUNCTION);
            string name = Eat(TokenType.IDENTIFIER).Value;
            Eat(TokenType.LPAREN);
            var parameters = new List<string>();
            if (Current()?.Type != TokenType.RPAREN)
            {
                parameters.Add(Eat(TokenType.IDENTIFIER).Value);
                while (Current()?.Type == TokenType.COMMA)
                {
                    Eat(TokenType.COMMA);
                    parameters.Add(Eat(TokenType.IDENTIFIER).Value);
                }
            }
            Eat(TokenType.RPAREN);
            Eat(TokenType.FROM);
            string dll = Eat(TokenType.STRING).Value;
            var extNode = new ExternDefNode(name, dll, parameters);
            SetPos(extNode);
            return extNode;
        }

        Node ParseExport()
        {
            Eat(TokenType.EXPORT);
            var c = Current();
            Node inner;
            if (c.Type == TokenType.FUNCTION)
            {
                inner = ParseFuncDef();
                if (inner is FuncDefNode fn) fn.IsExported = true;
            }
            else if (c.Type == TokenType.CLASS)
            {
                Eat(TokenType.CLASS);
                inner = ParseClassDef();
                if (inner is ClassDefNode cls) cls.IsExported = true;
            }
            else if (c.Type == TokenType.LET)
            {
                inner = ParseAssignment();
                if (inner is AssignNode an) an.IsExported = true;
            }
            else
                throw new Exception(CurrentErr($"export يحتاج let/func/class بعده"));
            return new ExportNode(inner);
        }

        Node ParseImport()
        {
            Eat(TokenType.IMPORT);
            var names = new List<string>();
            string asAlias = null;
            // import "path" [as alias]
            if (Current()?.Type == TokenType.STRING)
            {
                string path = Eat(TokenType.STRING).Value;
                if (Current()?.Type == TokenType.AS)
                {
                    Eat(TokenType.AS);
                    asAlias = Eat(TokenType.IDENTIFIER).Value;
                }
                var imp = new ImportNode(null, path, asAlias);
                SetPos(imp);
                return imp;
            }
            // import {a, b} from "path"
            if (Current()?.Type == TokenType.LBRACE)
            {
                Eat(TokenType.LBRACE);
                if (Current()?.Type != TokenType.RBRACE)
                {
                    names.Add(Eat(TokenType.IDENTIFIER).Value);
                    while (Current()?.Type == TokenType.COMMA)
                    {
                        Eat(TokenType.COMMA);
                        names.Add(Eat(TokenType.IDENTIFIER).Value);
                    }
                }
                Eat(TokenType.RBRACE);
            }
            else if (Current()?.Type == TokenType.IDENTIFIER)
            {
                names.Add(Eat(TokenType.IDENTIFIER).Value);
                while (Current()?.Type == TokenType.COMMA)
                {
                    Eat(TokenType.COMMA);
                    names.Add(Eat(TokenType.IDENTIFIER).Value);
                }
            }
            string fromPath = null;
            if (Current()?.Type == TokenType.FROM)
            {
                Eat(TokenType.FROM);
                fromPath = Eat(TokenType.STRING).Value;
            }
            var impNode = new ImportNode(names, fromPath);
            SetPos(impNode);
            return impNode;
        }

        Node ParseGo()
        {
            Eat(TokenType.GO);
            var expr = ParseExpression();
            var go = new GoNode(expr);
            SetPos(go);
            return go;
        }

        Node ParseSpawn()
        {
            Eat(TokenType.SPAWN);
            if (Current()?.Type == TokenType.FUNCTION) Eat(TokenType.FUNCTION);
            string name = Eat(TokenType.IDENTIFIER).Value;
            Eat(TokenType.LPAREN);
            var parameters = new List<string>();
            if (Current()?.Type != TokenType.RPAREN)
            {
                parameters.Add(Eat(TokenType.IDENTIFIER).Value);
                while (Current()?.Type == TokenType.COMMA)
                {
                    Eat(TokenType.COMMA);
                    parameters.Add(Eat(TokenType.IDENTIFIER).Value);
                }
            }
            Eat(TokenType.RPAREN);
            Node body;
            if (Current()?.Type == TokenType.LBRACE)
                body = ParseBlock();
            else
                body = ParseExpression();
            var spawn = new SpawnNode(name, parameters, body);
            SetPos(spawn);
            return spawn;
        }
    }
}
