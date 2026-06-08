using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A_
{
enum TokenType
{
    LET, PRINT, WHILE, IF, FOR, ELSE, FUNCTION, RETURN, CLASS, NEW, SELF,
    PUBLIC, PRIVATE, STATIC, ABSTRACT, INTERFACE,
    IDENTIFIER, NUMBER, STRING,
    EQUALS, PLUS, MINUS, STAR, SLASH, PERCENT,
    PLUS_EQUALS, MINUS_EQUALS, STAR_EQUALS, SLASH_EQUALS, PLUS_PLUS, MINUS_MINUS,
    LPAREN, RPAREN, LBRACE, RBRACE, DOT,
    GREATER, LESS, DOUBLE_EQUALS, NOT_EQUALS, LESS_EQUALS, GREATER_EQUALS,
    AND, OR, NOT, BANG,
    COMMA, SEMICOLON, COLON, QUESTION,
    LBRACKET, RBRACKET,
    INCLUDE, FROM, IN,
    TRUE, FALSE,
    THROW, TRY, CATCH, FINALLY, EXTENDS,
    BREAK, CONTINUE, SWITCH, CASE, DEFAULT,
    A_PLUS, ARROW, SHOW, EXTERN, DOC_COMMENT,
    // Module System
    EXPORT, IMPORT, AS,
    // Async/Await
    ASYNC, AWAIT, GO, SPAWN,
    // Type System
    NIL, PIPE, REGEX,
    // Debugger
    DEBUGGER,
    // 3D and Media Elements
    VIEWPORT3D, MODELVISUAL3D, MODEL3DGROUP, GEOMETRYMODEL3D, MESHGEOMETRY3D,
    DIRECTIONALLIGHT, AMBIENTLIGHT, POINTLIGHT, SPOTLIGHT,
    PERSPECTIVECAMERA, ORTHOGRAPHICCAMERA,
    MEDIAELEMENT
}

    class Token
    {
        public TokenType Type;
        public string Value;
        public int Line = 1;
        public int Column = 1;

        public Token(TokenType type, string value, int line = 1, int col = 1)
        {
            Type = type;
            Value = value;
            Line = line;
            Column = col;
        }
    }
}
