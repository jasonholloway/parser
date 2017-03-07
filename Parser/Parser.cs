using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Parser
{
    
    public class Query
    {
        public readonly IReadOnlyList<ISegment> Path;
        public readonly INode Filter;
        public readonly INode Select;
        public readonly int? Top;
        public readonly int? Skip;

        public Query(QuerySpec spec) {
            Path = spec.Path.AsReadOnly();
            Filter = spec.Filter;
            Select = spec.Select;
            Top = spec.Top;
            Skip = spec.Skip;
        }
    }



    public class QuerySpec
    {
        public List<ISegment> Path = new List<ISegment>(4);
        public INode Filter;
        public INode Select;
        public int? Top;
        public int? Skip;
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

        string CurrAsString()
            => CurrSpan.From(_source);

        bool MatchCurr(string comp)
            => CurrSpan.Match(_source, comp);
        
        QuerySpec Spec = new QuerySpec();

        #endregion








        public class RootStage : IStage
        {

        }


        
        Query Parse() 
        {
            CurrToken.MustBe(Token.Start);            
            Next();

            ParsePath();

            if(CurrToken == Token.QuestionMark) {
                Next();
                ParseOptions();
            }

            if(CurrToken == Token.Hash) {
                Next();
                ParseFragment();
            }

            CurrToken.MustBe(Token.End);
            
            return new Query(Spec);
        }



        void ParsePath() 
        {
            while(true) {
                switch(CurrToken) {                    
                    case Token.Slash:
                        Next();
                        break;

                    case Token.QuestionMark:
                    case Token.End:
                        return;

                    default:
                        var segment = ParseSegment();
                        Spec.Path.Add(segment);
                        break;
                }
            }            
        }


        void ParseOptions() 
        {
            while(true) {
                switch(CurrToken) {
                    case Token.Ampersand:
                        Next();
                        break;

                    case Token.Hash:
                    case Token.End:
                        return;

                    default:
                        ParseOption();                        
                        break;
                }
            }
        }

                

        bool ParseOption()
            => ParseFilter()
                || ParseSelect()
                || Error<bool>();



        bool ParseFilter() 
        {
            if(CurrToken == Token.ReservedWord 
                && NextToken == Token.Equals 
                && CurrSpan.Match(_source, "$filter")) 
            {
                Next();
                Next();

                Spec.Filter = ParseNode();
                
                return true;
            }

            return false;
        }


        bool ParseSelect() {
            return false;
        }






        void ParseFragment() {
            //...
        }







        ISegment ParseSegment()
            => Parse_FunctionSegment()
                ?? Parse_SubsetSegment()
                ?? Error<ISegment>();










        ISegment Parse_FunctionSegment() {
            if(CurrToken == Token.Word && NextToken == Token.Open) {
                var name = CurrSpan.From(_source);

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
                var name = CurrSpan.From(_source);

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
                        ?? Error<INode>();

            while(true) {
                var next = ParseNavigation(node)
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

            if(CurrToken == Token.Minus) {
                Next();
                while(CurrToken == Token.Space) Next();

                return new UnaryOperatorNode(Operator.Negate, ParseNode(consumeBinaries: false));
            }

            //casting...

            return null;
        }

        


        INode ParseCall(INode leftNode) {
            if(CurrToken == Token.Open) {
                var args = ParseArgs();
                return new FunctionCallNode(leftNode, args);
            }

            return null;
        }


        INode ParseNavigation(INode leftNode) {
            if(CurrToken == Token.Slash) {
                Next();
                return ParseAccessor(leftNode);
            }

            return null;
        }



        INode ParseAccessor(INode parentNode) {
            if(CurrToken == Token.Word) {
                var node = new AccessorNode(parentNode, CurrAsString());

                Next();

                return node;
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
                var op = GetOperator(CurrAsString());

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
                ?? ParseInteger()
                ?? ParseBoolean()
                ?? Null<INode>();


        

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

                return new ValueNode<string>(@string.From(_source));
            }

            return null;
        }



        INode ParseInteger() 
        {
            if(CurrToken == Token.Number) 
            {
                var @int = int.Parse(CurrSpan.From(_source));

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





    public enum QueryOptionType
    {
        Filter,
        Select,
        OrderBy,
        Skip,
        Top,
        Custom
    }


    public struct QueryOption
    {
        public readonly QueryOptionType Type;
        public readonly string Name;

        public QueryOption(QueryOptionType type, string name) {
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
