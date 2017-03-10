using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Parser
{
    
    public class Query : INode
    {
        public readonly INode Resource;
        public readonly IReadOnlyList<INode> Options;
                
        public Query(INode path, IReadOnlyList<INode> options) {
            Resource = path;
            Options = options;
        }
    }


    


    public static class ParseExtensions
    {
        public static void MustBe(this Token token, Token comp) {
            if(token != comp) throw new InvalidOperationException("Unexpected token encountered!");
        }
    }





    public class Parser
    {

        public static Query Parse(string source)
            => new Parser(source).Parse();



        #region bits

        readonly string _source;
        readonly IEnumerator<TokenSpan> _tokens;

        
        public Parser(string source) {
            _source = source;

            _tokens = Lexer.Lex(_source).GetEnumerator();
            
            Next();
            Next();
        }
        

        void Next() {
            CurrSpan = NextSpan;
            NextSpan = _tokens.MoveNext()
                            ? _tokens.Current
                            : TokenSpan.None;
        }



        TokenSpan Take(Token token, int size = -1) {
            var span = CurrSpan;

            if(span.Token != token) throw new InvalidOperationException("Unexpected token encountered!");

            if(size >= 0 && span.Size != size) throw new InvalidOperationException("Unexpected token encountered!");

            Next();

            return span;
        }

        void Skip(Token token, int size = -1, string match = null) {
            if(CurrToken != token) throw new InvalidOperationException("Unexpected token encountered!");

            if(match != null && !CurrSpan.Match(_source, match)) throw new InvalidOperationException("Unexpected token encountered!");
            
            if(size >= 0 && CurrSpan.Size != size) throw new InvalidOperationException("Unexpected token encountered!");

            Next();
        }



        TokenSpan CurrSpan;
        TokenSpan NextSpan;

        Token CurrToken => CurrSpan.Token;
        Token NextToken => NextSpan.Token;


        bool MatchCurr(string comp)
            => CurrSpan.Match(_source, comp);
        


        //but we can't do the backtracking if our results are returned in place
        //they shouldn't be returned in place
        //this is the SyntacticalParser - it should return by result, not by in-place query, which would certainly 
        //be a further step


        int _stackCount = 0;
        Stack<TokenSpan> _stTokens = null;


        object CreateCheckpoint() {
            throw new NotImplementedException();
            //when in checkpoint mode, push all consumed tokens onto a stack
        }

        void Restore(object checkpoint) {            
            //emplace enumerator to replay stack and delegate to current enumerator
            //emplace new stack - but only if stack was there before

            throw new NotImplementedException();
        }


        #endregion


        

        
        Query Parse() 
        {
            Skip(Token.Start);            
            
            var path = ParseNode();
            var options = (IReadOnlyList<INode>)new INode[0];

            if(CurrToken == Token.QuestionMark) {
                Skip(Token.QuestionMark);                
                options = ParseOptions();
            }

            //if(CurrToken == Token.Hash) {
            //    Next();
            //    ParseFragment();
            //}

            Skip(Token.End);
            
            return new Query(path, options);
        }

                

        IReadOnlyList<INode> ParseOptions() 
        {
            var nodes = new List<INode>();

            while(true) {
                switch(CurrToken) {
                    case Token.Ampersand:
                        Next();
                        break;

                    case Token.Hash:
                    case Token.End:
                        return nodes;

                    default:
                        nodes.Add(ParseNode());                        
                        break;
                }
            }
        }

                



        ISegment ParseSegment()
            => Parse_FunctionSegment()
                ?? Parse_SubsetSegment()
                ?? Error<ISegment>();










        ISegment Parse_FunctionSegment() {
            if(CurrToken == Token.Word && NextToken == Token.Open) {
                var name = CurrSpan.AsString(_source);

                Next();

                var args = ParseArgs();

                return new FunctionSegment(name, args);
            }

            return null;   //need lookahead here
        }






        ISegment Parse_SubsetSegment() 
        {
            if(CurrToken == Token.Word) 
            {
                var name = CurrSpan.AsString(_source);

                Next();

                return new SubsetSegment(name);
            }

            return null;
        }


              




        

        INode[] ParseArgs() 
        {
            if(CurrToken == Token.Open) 
            {
                Next();

                var args = new List<INode>();

                while(true) {
                    switch(CurrToken) {
                        case Token.Comma:
                            Next();
                            break;

                        case Token.Close:
                            Next();
                            return args.ToArray();

                        default:
                            var node = ParseNode();
                            args.Add(node);
                            break;
                    }
                }
            }

            return null;
        }




        enum Greed
        {
            Full            = 999,
            Grouping        = 0,
            Primary         = 1,
            Unary           = 2,
            Multiplicative  = 3,
            Additive        = 4,
            Relational      = 5,
            Equality        = 6,
            ConditionalAnd  = 7,
            ConditionalOr   = 8
        }



        INode ParseNode() 
        {
            var node = ParseGroup()                             //start our parsing
                        ?? ParseUnary()                         //unaries shouldn't continue to consume binaries (or navigations, etc)
                        ?? ParseValue()
                        ?? ParseAccessor(parentNode: null)
                        ?? ParseSymbol()
                        ?? Null<INode>();

            if(node == null) return null;

            while(true) { 
                var next = ParseNavigation(node)
                            ?? ParseAssignment(node)
                            ?? ParseCall(node)
                            ?? ParseBinary(node);

                if(next != null) {
                    node = next;
                }
                else {
                    return node;
                }
            }
        }



        
        Stack<Greed> _stGreed = new Stack<Greed>();
        
        Greed CurrGreed => _stGreed.Count > 0 ? _stGreed.Peek() : Greed.Full;



        INode ParseBinary(INode leftNode) {
            if(CurrToken == Token.Space && NextToken == Token.Word) 
            {
                var op = GetOperator(NextSpan.AsString(_source));
                var opPrecedence = GetPrecedence(op);

                if(CurrGreed >= opPrecedence) {
                    Skip(Token.Space);
                    Skip(Token.Word);
                    Skip(Token.Space);

                    _stGreed.Push(opPrecedence);

                    var rightNode = ParseNode();

                    _stGreed.Pop();

                    return new BinaryOperatorNode(op, leftNode, rightNode);
                }
            }

            return null;
        }




        INode ParseUnary() {            
            if(CurrToken == Token.Word) {                
                if(MatchCurr("not")) {
                    Next();
                    while(CurrToken == Token.Space) Next();

                    _stGreed.Push(Greed.Unary);

                    var operand = ParseNode();

                    _stGreed.Pop();

                    return new UnaryOperatorNode(Operator.Not, operand);
                }
            }

            if(CurrToken == Token.Hyphen) {
                Next();
                while(CurrToken == Token.Space) Next();

                _stGreed.Push(Greed.Unary);

                var operand = ParseNode();

                _stGreed.Pop();

                return new UnaryOperatorNode(Operator.Negate, operand);
            }

            //casting is the last unary left to do...

            return null;
        }

        

        INode ParseCall(INode leftNode) {
            if(CurrToken == Token.Open) {
                var args = ParseArgs();
                return new CallNode(leftNode, args);
            }

            return null;
        }


        INode ParseNavigation(INode leftNode) {
            if(CurrToken == Token.Slash) {
                Next();
                return ParseAccessor(leftNode); //or indeed a symbol...
            }

            return null;
        }



        INode ParseAssignment(INode leftNode) {
            if(CurrToken == Token.Equals) {
                Next();
                var rightNode = ParseNode();
                return new AssignmentNode(leftNode, rightNode);
            }

            return null;
        }


        INode ParseSymbol() {
            if(CurrToken == Token.ReservedWord) {
                var name = CurrSpan.AsString(_source);
                Next();

                switch(name) {
                    case "$filter": return new SymbolNode(Symbol.Filter);
                    case "$select": return new SymbolNode(Symbol.Select);
                    case "$top": return new SymbolNode(Symbol.Top);
                    case "$skip": return new SymbolNode(Symbol.Skip);
                    case "$orderby": return new SymbolNode(Symbol.OrderBy);
                    case "$count": return new SymbolNode(Symbol.Count);
                    default: throw new InvalidOperationException("Unhandled symbol encountered");
                }
            }

            return null;
        }


        INode ParseAccessor(INode parentNode) {
            if(CurrToken == Token.Word) {
                var left = CurrSpan.Left;
                var right = 0;

                do {
                    right = CurrSpan.Right;
                    Next();                    
                } while(CurrToken == Token.Word || CurrToken == Token.Number);
                
                return new AccessorNode(parentNode, _source.Read(left, right));                
            }

            return null;
        }

        

        INode ParseGroup() {
            if(CurrToken == Token.Open) 
            {
                Skip(Token.Open);

                _stGreed.Push(Greed.Full);

                var inner = ParseNode();

                _stGreed.Pop();

                Skip(Token.Close);

                return inner;
            }

            return null;
        }





        INode ParseValue()
            => ParseString()
                ?? ParseV4Date()
                ?? ParseDecimal()
                ?? ParseInteger()
                ?? ParseBoolean()
                ?? Null<INode>();


        




        INode ParseV4Date() {
            if(CurrToken == Token.Number 
                && CurrSpan.Size == 4
                && NextToken == Token.Hyphen) 
            {
                //it looks like this might be a date; but we need to checkpoint here to make sure
                //if anything below goes awry, we need to restore and return

                int year, month, day, hour = 0, minute = 0, second = 0, millisecond = 0;

                year = Take(Token.Number, size: 4).AsInt(_source);

                Skip(Token.Hyphen);

                month = Take(Token.Number, size: 2).AsInt(_source);

                Skip(Token.Hyphen);

                day = Take(Token.Number, size: 2).AsInt(_source);
                
                if(CurrToken == Token.Word && CurrSpan.Match(from: _source, comp: "T")) 
                {
                    Skip(Token.Word);

                    hour = Take(Token.Number, size: 2).AsInt(_source);

                    Skip(Token.Colon);

                    minute = Take(Token.Number, size: 2).AsInt(_source);

                    Skip(Token.Colon);

                    second = Take(Token.Number, size: 2).AsInt(_source);

                    if(CurrToken == Token.Dot) {
                        Skip(Token.Dot);
                        millisecond = Take(Token.Number).AsInt(_source);
                    }
                    
                    Skip(Token.Word, match: "Z");
                }
                
                return new ValueNode<DateTimeOffset>(new DateTimeOffset(year, month, day, hour, minute, second, millisecond, TimeSpan.Zero));
            }

            return null;
        }


        INode ParseBoolean() {
            if(CurrToken == Token.Word) {                
                if(CurrSpan.Match(_source, "true")) {
                    Next();
                    return new ValueNode<bool>(true);
                }

                if(CurrSpan.Match(_source, "false")) {
                    Next();
                    return new ValueNode<bool>(false);
                }
            }

            return null;
        }


        INode ParseString() 
        {
            if(CurrToken == Token.String) 
            {
                return new ValueNode<string>(Take(Token.String).AsString(_source));
            }

            return null;
        }





        INode ParseDecimal() 
        {
            if(CurrToken == Token.Number && NextToken == Token.Dot) {
                var left = CurrSpan.Left;

                Skip(Token.Number);
                Skip(Token.Dot);
                Skip(Token.Number);

                var right = CurrSpan.Right;

                var val = decimal.Parse(_source.Read(left, right));

                return new ValueNode<decimal>(val);
            }

            return null;
        }


        INode ParseInteger() 
        {
            if(CurrToken == Token.Number) {
                var whole = Take(Token.Number).AsInt(_source);
                
                return new ValueNode<int>(whole);
            }

            return null;
        }




        #region Helpers

        static Operator GetOperator(string name) {
            switch(name) {
                case "eq": return Operator.Equals;
                case "ne": return Operator.NotEquals;
                case "gt": return Operator.GreaterThan;
                case "lt": return Operator.LessThan;
                case "and": return Operator.And;
                case "or": return Operator.Or;
                case "add": return Operator.Add;
                case "div": return Operator.Divide;
                case "mul": return Operator.Multiply;
                case "sub": return Operator.Subtract;
                case "mod": return Operator.Modulo;
                default: throw new NotImplementedException();
            }
        }



        static Greed GetPrecedence(Operator op) {
            switch(op) {
                //case Operator.Navigate: return Precedence.Primary;

                case Operator.Not: return Greed.Unary;
                case Operator.Negate: return Greed.Unary;

                case Operator.Multiply: return Greed.Multiplicative;
                case Operator.Divide: return Greed.Multiplicative;
                case Operator.Modulo: return Greed.Multiplicative;

                case Operator.Add: return Greed.Additive;
                case Operator.Subtract: return Greed.Additive;

                case Operator.GreaterThan: return Greed.Relational;
                case Operator.LessThan: return Greed.Relational;

                case Operator.Equals: return Greed.Equality;
                case Operator.NotEquals: return Greed.Equality;

                case Operator.And: return Greed.ConditionalAnd;
                case Operator.Or: return Greed.ConditionalOr;

                default: throw new NotImplementedException();
            }
        }





        static T Null<T>()
            where T : class
            => null;
        

        static T Error<T>()
            => throw new InvalidOperationException($"No parsing strategy available!");

        #endregion

    }

    


    public class ResourcePath
    {
        public object Segments;
    }





    public enum Symbol
    {
        Filter,
        Select,
        OrderBy,
        Skip,
        Top,
        Count
    }


    public struct QueryOption
    {
        public readonly Symbol Type;
        public readonly string Name;

        public QueryOption(Symbol type, string name) {
            Type = type;
            Name = name;
        }        
    }



    public enum NodeType
    {
        Filter,
        Relational,
        Binary
    }



    public interface INode
    {
        //NodeType Type { get; }
    }


    public class Node : INode
    {
        public NodeType Type => throw new NotImplementedException();
    }





}
