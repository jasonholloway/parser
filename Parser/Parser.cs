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
            CurrToken.MustBe(Token.Start);            
            Next();
            
            var path = ParseNode();
            var options = (IReadOnlyList<INode>)new INode[0];

            if(CurrToken == Token.QuestionMark) {
                Next();
                options = ParseOptions();
            }

            //if(CurrToken == Token.Hash) {
            //    Next();
            //    ParseFragment();
            //}

            CurrToken.MustBe(Token.End);
            
            return new Query(path, options);
        }



        //INode ParsePath() 
        //{
        //    while(true) {
        //        switch(CurrToken) {                    
        //            case Token.Slash:
        //                Next();
        //                break;

        //            case Token.QuestionMark:
        //            case Token.End:
        //                return;

        //            default:
        //                var segment = ParseSegment();
        //                Spec.Path.Add(segment);
        //                break;
        //        }
        //    }            
        //}


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

                

        //INode ParseOption()
        //    => ParseFilter()
        //        || ParseSelect()
        //        || Error<bool>();



        //bool ParseFilter() 
        //{
        //    if(CurrToken == Token.ReservedWord 
        //        && NextToken == Token.Equals 
        //        && CurrSpan.Match(_source, "$filter")) 
        //    {
        //        Next();
        //        Next();

        //        Spec.Filter = ParseNode();
                
        //        return true;
        //    }

        //    return false;
        //}


        //bool ParseSelect() {
        //    return false;
        //}






        //void ParseFragment() {
        //    //...
        //}







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





        INode ParseNode(bool consumeBinaries = true) 
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
                            ?? (consumeBinaries ? ParseBinary(node) : null);

                if(next != null) {
                    node = next;
                }
                else {
                    return node;
                }
            }
        }
        
        

        INode ParseUnary() {            
            if(CurrToken == Token.Word) {                
                if(MatchCurr("not")) {
                    Next();
                    while(CurrToken == Token.Space) Next();

                    return new UnaryOperatorNode(Operator.Not, ParseNode(consumeBinaries: false));
                }
            }

            if(CurrToken == Token.Hyphen) {
                Next();
                while(CurrToken == Token.Space) Next();

                return new UnaryOperatorNode(Operator.Negate, ParseNode(consumeBinaries: false));
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




        Operator GetOperator(string name) {
            switch(name) {
                case "eq": return Operator.Equals;
                case "ne": return Operator.NotEquals;
                case "gt": return Operator.GreaterThan;
                case "lt": return Operator.LessThan;
                case "and": return Operator.And;
                case "or": return Operator.Or;
                default: throw new NotImplementedException();
            }
        }
        

        INode ParseBinary(INode leftNode) {
            if(CurrToken == Token.Space && NextToken == Token.Word) 
            {
                Next();
                var op = GetOperator(CurrSpan.AsString(_source));

                Next();
                CurrToken.MustBe(Token.Space);

                Next();
                var rightNode = ParseNode();

                return new BinaryOperatorNode(op, leftNode, rightNode);
            }

            return null;
        }
        

        INode ParseGroup() {
            if(CurrToken == Token.Open) {
                Next();
                var inner = ParseNode();

                CurrToken.MustBe(Token.Close);
                Next();

                return inner;
            }

            return null;
        }





        INode ParseValue()
            => ParseString()
                ?? ParseDate()
                ?? ParseInteger()
                ?? ParseBoolean()
                ?? Null<INode>();


        

        INode ParseDate() {
            if(CurrToken == Token.Number 
                && CurrSpan.Size == 4
                && NextToken == Token.Hyphen) {
                //it looks like this might be a date; but we need to checkpoint here to make sure
                //if anything below goes awry, we need to restore and return

                var year = CurrSpan.AsInt(_source);
                Next();

                Next();

                var month = CurrSpan.AsInt(_source);
                Next();

                Next();

                var day = CurrSpan.AsInt(_source);
                Next();

                //now, a 'T' signifies a forthcoming time portion
                //...

                throw new NotImplementedException();
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
                var @string = CurrSpan;

                Next();

                return new ValueNode<string>(@string.AsString(_source));
            }

            return null;
        }



        INode ParseInteger() 
        {
            if(CurrToken == Token.Number) 
            {
                var @int = int.Parse(CurrSpan.AsString(_source));

                Next();

                return new ValueNode<int>(@int);
            }

            return null;
        }





        


        static T Null<T>()
            where T : class
            => null;
        

        static T Error<T>()
            => throw new InvalidOperationException($"No parsing strategy available!");

             
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
