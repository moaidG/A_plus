using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A_
{
    abstract class Node
    {
        public int Line = 0;
        public int Column = 0;
    }

    [Flags]
    enum AccessFlags
    {
        None = 0,
        Public = 1,
        Private = 2,
        Static = 4,
        Abstract = 8,
    }

    class NumberNode : Node
    {
        public double Value;
        public NumberNode(double value) => Value = value;
    }

    class StringNode : Node
    {
        public string Value;
        public StringNode(string value) => Value = value;
    }

    class BoolNode : Node
    {
        public bool Value;
        public BoolNode(bool value) => Value = value;
    }

    class VariableNode : Node
    {
        public string Name;
        public VariableNode(string name) => Name = name;
    }

    class AddNode : Node
    {
        public Node Left, Right;
        public AddNode(Node left, Node right) { Left = left; Right = right; }
    }

    class SubtractNode : Node
    {
        public Node Left, Right;
        public SubtractNode(Node left, Node right) { Left = left; Right = right; }
    }

    class MultiplyNode : Node
    {
        public Node Left, Right;
        public MultiplyNode(Node left, Node right) { Left = left; Right = right; }
    }

    class DivideNode : Node
    {
        public Node Left, Right;
        public DivideNode(Node left, Node right) { Left = left; Right = right; }
    }

    class FieldDefNode : Node
    {
        public string Name;
        public AccessFlags Flags;
        public FieldDefNode(string name, AccessFlags flags = AccessFlags.Public)
        {
            Name = name;
            Flags = flags;
        }
    }

    class AssignNode : Node
    {
        public string Name;
        public Node Value;
        public bool Typed;
        public string TypeName;
        public bool IsExported;
        public bool IsLet;
        public AssignNode(string name, Node value, bool typed = false, string typeName = null)
        {
            Name = name;
            Value = value;
            Typed = typed;
            TypeName = typeName;
        }
    }

    class PrintNode : Node
    {
        public Node Value;
        public PrintNode(Node value) => Value = value;
    }

    class IfNode : Node
    {
        public Node Condition;
        public Node Body;
        public Node ElseBody;
        public IfNode(Node condition, Node body, Node elseBody = null)
        {
            Condition = condition;
            Body = body;
            ElseBody = elseBody;
        }
    }

    class GreaterNode : Node
    {
        public Node Left, Right;
        public GreaterNode(Node left, Node right) { Left = left; Right = right; }
    }

    class LessNode : Node
    {
        public Node Left, Right;
        public LessNode(Node left, Node right) { Left = left; Right = right; }
    }

    class EqualNode : Node
    {
        public Node Left, Right;
        public EqualNode(Node left, Node right) { Left = left; Right = right; }
    }

    class WhileNode : Node
    {
        public Node Condition;
        public Node Body;
        public WhileNode(Node condition, Node body) { Condition = condition; Body = body; }
    }

    class BlockNode : Node
    {
        public List<Node> Statements;
        public BlockNode(List<Node> statements) { Statements = statements; }
    }

    class AndNode : Node
    {
        public Node Left, Right;
        public AndNode(Node left, Node right) { Left = left; Right = right; }
    }

    class OrNode : Node
    {
        public Node Left, Right;
        public OrNode(Node left, Node right) { Left = left; Right = right; }
    }

    class NotEqualNode : Node
    {
        public Node Left, Right;
        public NotEqualNode(Node left, Node right) { Left = left; Right = right; }
    }

    class LeNode : Node
    {
        public Node Left, Right;
        public LeNode(Node left, Node right) { Left = left; Right = right; }
    }

    class GeNode : Node
    {
        public Node Left, Right;
        public GeNode(Node left, Node right) { Left = left; Right = right; }
    }

    class ForNode : Node
    {
        public Node Init;
        public Node Condition;
        public Node Increment;
        public Node Body;
        public ForNode(Node init, Node condition, Node increment, Node body)
        {
            Init = init;
            Condition = condition;
            Increment = increment;
            Body = body;
        }
    }

    class FuncDefNode : Node
    {
        public string Name;
        public List<string> Parameters;
        public Dictionary<string, Node> Defaults;
        public Node Body;
        public AccessFlags Flags;
        public string Doc;
        public bool IsExported;
        public bool IsAsync;
        public List<string> TypeParams;
        public Dictionary<string, string> ParamTypes;
        public string ReturnType;
        public FuncDefNode(string name, List<string> parameters, Node body, AccessFlags flags = AccessFlags.Public, Dictionary<string, Node> defaults = null, Dictionary<string, string> paramTypes = null, string returnType = null)
        {
            Name = name;
            Parameters = parameters;
            Defaults = defaults ?? new Dictionary<string, Node>();
            Body = body;
            Flags = flags;
            ParamTypes = paramTypes ?? new Dictionary<string, string>();
            ReturnType = returnType;
        }
    }

    class FuncCallNode : Node
    {
        public string Name;
        public List<Node> Arguments;
        public FuncCallNode(string name, List<Node> arguments)
        {
            Name = name;
            Arguments = arguments;
        }
    }

    class NamedArgNode : Node
    {
        public string Name;
        public Node Value;
        public NamedArgNode(string name, Node value) { Name = name; Value = value; }
    }

    class ReturnNode : Node
    {
        public Node Value;
        public ReturnNode(Node value) { Value = value; }
    }

    class ClassDefNode : Node
    {
        public string Name;
        public string ParentName;
        public List<FieldDefNode> Fields;
        public List<FuncDefNode> Methods;
        public bool IsAbstract;
        public bool IsInterface;
        public Node InitBody;
        public string Doc;
        public bool IsExported;
        public List<string> TypeParams;
        public ClassDefNode(string name, List<FieldDefNode> fields, List<FuncDefNode> methods, string parentName = null, bool isAbstract = false, bool isInterface = false, Node initBody = null)
        {
            Name = name;
            ParentName = parentName;
            Fields = fields;
            Methods = methods;
            IsAbstract = isAbstract;
            IsInterface = isInterface;
            InitBody = initBody;
        }
    }

    class NewNode : Node
    {
        public string ClassName;
        public List<Node> Arguments;
        public NewNode(string className, List<Node> args)
        {
            ClassName = className;
            Arguments = args;
        }
    }

    class SelfNode : Node
    {
    }

    class IncludeNode : Node
    {
        public string Path;
        public string From;
        public IncludeNode(string path, string from = null) { Path = path; From = from; }
    }

    class ArrayLiteralNode : Node
    {
        public List<Node> Elements;
        public ArrayLiteralNode(List<Node> elements) { Elements = elements; }
    }

    class ObjectLiteralNode : Node
    {
        public List<(string Key, Node Value)> Fields;
        public ObjectLiteralNode(List<(string Key, Node Value)> fields) { Fields = fields; }
    }

    class ArrayGetNode : Node
    {
        public Node Object;
        public Node Index;
        public ArrayGetNode(Node obj, Node index) { Object = obj; Index = index; }
    }

    class ArraySetNode : Node
    {
        public Node Object;
        public Node Index;
        public Node Value;
        public ArraySetNode(Node obj, Node index, Node value) { Object = obj; Index = index; Value = value; }
    }

    class MemberGetNode : Node
    {
        public Node Object;
        public string Member;
        public MemberGetNode(Node obj, string member) { Object = obj; Member = member; }
    }

    class MemberSetNode : Node
    {
        public Node Object;
        public string Member;
        public Node Value;
        public MemberSetNode(Node obj, string member, Node value) { Object = obj; Member = member; Value = value; }
    }

    class MethodCallNode : Node
    {
        public Node Object;
        public string Method;
        public List<Node> Arguments;
        public MethodCallNode(Node obj, string method, List<Node> args) { Object = obj; Method = method; Arguments = args; }
    }

    class ThrowNode : Node
    {
        public Node Value;
        public ThrowNode(Node value) { Value = value; }
    }

    class TryNode : Node
    {
        public Node TryBody;
        public string CatchVar;
        public Node CatchBody;
        public Node FinallyBody;
        public TryNode(Node tryBody, string catchVar, Node catchBody, Node finallyBody = null)
        {
            TryBody = tryBody;
            CatchVar = catchVar;
            CatchBody = catchBody;
            FinallyBody = finallyBody;
        }
    }

    class APlusNewNode : Node
    {
        public string ClassName;
        public string VarName;
        public APlusNewNode(string className, string varName)
        {
            ClassName = className;
            VarName = varName;
        }
    }

    class ShowNode : Node
    {
        public List<Node> Args;
        public ShowNode(List<Node> args = null) { Args = args ?? new List<Node>(); }
    }

    class ArrowBindNode : Node
    {
        public string VarName;
        public string EventName;
        public string FuncName;
        public ArrowBindNode(string varName, string eventName, string funcName)
        {
            VarName = varName;
            EventName = eventName;
            FuncName = funcName;
        }
    }

    class ExternDefNode : Node
    {
        public string Name;
        public string DllName;
        public List<string> Parameters;
        public ExternDefNode(string name, string dllName, List<string> parameters)
        {
            Name = name;
            DllName = dllName;
            Parameters = parameters;
        }
    }

    class ModuloNode : Node
    {
        public Node Left, Right;
        public ModuloNode(Node left, Node right) { Left = left; Right = right; }
    }

    class UnaryNode : Node
    {
        public string Op; // "NOT", "NEGATE"
        public Node Operand;
        public UnaryNode(string op, Node operand) { Op = op; Operand = operand; }
    }

    class PostfixNode : Node
    {
        public Node Operand;
        public string Op; // "++" or "--"
        public PostfixNode(Node operand, string op) { Operand = operand; Op = op; }
    }

    class BreakNode : Node { }

    class ContinueNode : Node { }

    class TernaryNode : Node
    {
        public Node Condition;
        public Node TrueExpr;
        public Node FalseExpr;
        public TernaryNode(Node condition, Node trueExpr, Node falseExpr)
        {
            Condition = condition;
            TrueExpr = trueExpr;
            FalseExpr = falseExpr;
        }
    }

    class SwitchNode : Node
    {
        public Node Value;
        public List<CaseNode> Cases;
        public Node DefaultBody;
        public SwitchNode(Node value, List<CaseNode> cases, Node defaultBody = null)
        {
            Value = value;
            Cases = cases;
            DefaultBody = defaultBody;
        }
    }

    class CaseNode : Node
    {
        public Node Value;
        public Node Body;
        public CaseNode(Node value, Node body) { Value = value; Body = body; }
    }

    class ForInNode : Node
    {
        public string VarName;
        public Node Iterable;
        public Node Body;
        public ForInNode(string varName, Node iterable, Node body)
        {
            VarName = varName;
            Iterable = iterable;
            Body = body;
        }
    }

    // ─── Module System ───
    class ExportNode : Node
    {
        public Node Declaration;
        public ExportNode(Node declaration) { Declaration = declaration; }
    }

    class ImportNode : Node
    {
        public string From;
        public string AsAlias;
        public List<string> Names;
        public ImportNode(List<string> names, string from = null, string asAlias = null)
        {
            Names = names ?? new List<string>();
            From = from;
            AsAlias = asAlias;
        }
    }

    // ─── Async/Await ───
    class AwaitNode : Node
    {
        public Node Expr;
        public AwaitNode(Node expr) { Expr = expr; }
    }

    class GoNode : Node
    {
        public Node Expr;
        public GoNode(Node expr) { Expr = expr; }
    }

    class SpawnNode : Node
    {
        public string Name;
        public List<string> Parameters;
        public Node Body;
        public SpawnNode(string name, List<string> parameters, Node body)
        {
            Name = name;
            Parameters = parameters;
            Body = body;
        }
    }

    // ─── Type System ───
    class NilNode : Node { }

    class DebuggerNode : Node { }

    // ─── Pipe Operator ───
    class PipeNode : Node
    {
        public Node Left, Right;
        public PipeNode(Node left, Node right) { Left = left; Right = right; }
    }

    // ─── Destructuring ───
    class ArrayDestructureNode : Node
    {
        public List<string> Names;
        public Node Value;
        public ArrayDestructureNode(List<string> names, Node value) { Names = names; Value = value; }
    }

    class ObjectDestructureNode : Node
    {
        public List<string> Names;
        public Node Value;
        public ObjectDestructureNode(List<string> names, Node value) { Names = names; Value = value; }
    }

    // ─── Native Regex ───
    class RegexNode : Node
    {
        public string Pattern;
        public string Flags;
        public System.Text.RegularExpressions.Regex RegexObj;
        public RegexNode(string pattern, string flags)
        {
            Pattern = pattern;
            Flags = flags;
            try
            {
                var opts = System.Text.RegularExpressions.RegexOptions.ECMAScript;
                if (flags.Contains('i')) opts |= System.Text.RegularExpressions.RegexOptions.IgnoreCase;
                if (flags.Contains('m')) opts |= System.Text.RegularExpressions.RegexOptions.Multiline;
                if (flags.Contains('s')) opts |= System.Text.RegularExpressions.RegexOptions.Singleline;
                RegexObj = new System.Text.RegularExpressions.Regex(pattern, opts);
            }
            catch { RegexObj = null; }
        }
    }

    // ─── A+ Xml Declaration ───
    class APlusXmlNode : Node
    {
        public string VarName;
        public XmlElementNode Element;
        public APlusXmlNode(string varName, XmlElementNode element)
        {
            VarName = varName;
            Element = element;
        }
    }

    // ─── Xml Element (Component Functions) ───
    class XmlElementNode : Node
    {
        public string TagName;
        public List<(string Name, Node Value)> Attributes;
        public List<Node> Children;
        public bool SelfClosing;
        public XmlElementNode(string tagName, List<(string, Node)> attrs, List<Node> children, bool selfClosing)
        {
            TagName = tagName;
            Attributes = attrs;
            Children = children;
            SelfClosing = selfClosing;
        }
    }
}
