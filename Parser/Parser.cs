using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Parser
{
    
    //public class StringNode : INode
    //{
    //    public readonly Span<Token> Value;

    //    public StringNode(Span<Token> value) {
    //        Value = value;
    //    }
    //}






    //public class FilterStage : IStage
    //{
    //    public StageType Type => StageType.Filter;

    //    public IStage Source { get; private set; }
    //    public INode Filter { get; private set; }

    //    public FilterStage(IStage source, INode filter) {
    //        Source = source;
    //        Filter = filter;
    //    }
    //}


    //public class SelectStage : IStage
    //{
    //    public StageType Type => StageType.Select;

    //    public IStage Source { get; private set; }
    //    public INode Projection { get; private set; } //symbols in the projection to be bound to resources in the source

    //    public SelectStage(IStage source) {
    //        Source = source;
    //    }
    //}







    //public enum StageType
    //{
    //    Root,
    //    Subset,
    //    Filter,
    //    Select,
    //    Bind
    //}



    //public static class Stagifier
    //{
    //    enum Mode
    //    {
    //        Path,
    //        Options,
    //        Bindings
    //    }


    //    public static IEnumerable<TokenSpan<StageType>> Stagify(IEnumerable<TokenSpan> tokens) 
    //    {
    //        var en = tokens.GetEnumerator();
    //        int left = en.Current.Left;
            
    //        while(true) {
    //            var curr = en.Current;

    //            switch(curr.Token) {
    //                case Token.Start:
    //                    en.MoveNext();
    //                    break;

    //                case Token.Word:
    //                    yield return Span.Of(StageType.Subset, left, curr.Right);
    //                    left = curr.Right;

    //                    en.MoveNext();
    //                    break;
                                            
    //                case Token.Slash:
    //                    en.MoveNext();          
    //                    break;

    //                case Token.End:
    //                case Token.QuestionMark:
    //                    yield break;

    //                default:
    //                    throw new NotImplementedException();
    //            }
    //        }

    //        throw new NotImplementedException();
    //    }
        




    //}
    


    public class Query
    {
        public readonly ISegment[] Path;
        public readonly INode Filter;
        public readonly INode Select;
        public readonly int? Top;
        public readonly int? Skip;

        public Query(QuerySpec spec) {
            Path = spec.Path;
            Filter = spec.Filter;
            Select = spec.Select;
            Top = spec.Top;
            Skip = spec.Skip;
        }
    }



    public class QuerySpec
    {
        public ISegment[] Path;
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


        #endregion








        public class RootStage : IStage
        {

        }


        QuerySpec _spec = new QuerySpec();

        
        Query Parse() 
        {
            CurrToken.MustBe(Token.Start);
            
            Next();

            _spec.Path = ParsePath();
            ParseOptions();
            ParseFragment();

            CurrToken.MustBe(Token.End);
            
            return new Query(_spec);
        }



        ISegment[] ParsePath() 
        {
            var segments = new List<ISegment>(8);

            while(true) {
                switch(CurrToken) {                    
                    case Token.Slash:
                        Next();
                        break;

                    case Token.QuestionMark:
                    case Token.End:
                        return segments.ToArray();

                    default:
                        var segment = ParseSegment();
                        segments.Add(segment);
                        break;
                }
            }            
        }


        void ParseOptions() {
            //...
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
                var name = CurrSpan;

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
                var name = CurrSpan;

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





        INode ParseNode()
            => ParseConstant()
                ?? Error<INode>();



        INode ParseConstant()
            => ParseString()
                ?? ParseInteger()
                ?? Null<INode>();


        

        INode ParseString() 
        {
            if(CurrToken == Token.String) 
            {
                var @string = CurrSpan;

                Next();

                return new StringNode(@string);
            }

            return null;
        }



        INode ParseInteger() 
        {
            if(CurrToken == Token.Number) 
            {
                var number = CurrSpan;
                Next();

                return new IntegerNode(number);
            }

            return null;
        }







        INode ParseNode_FromWord() {
            if(CurrSpan.Token != Token.Word) return null;
            throw new NotImplementedException();
        }

        INode ParseNode_FromString() {
            if(CurrSpan.Token != Token.String) return null;
            
            var node = new StringNode(CurrSpan);

            Next();

            return node;
        }






        //static IStage ParseStage_FromOption(Context x) {
        //    if(x.Current.Token != Token.)
        //}



        static T Null<T>()
            where T : class
            => null;
        

        static T Error<T>()
            => throw new InvalidOperationException($"No parsing strategy available!");

        
        








        //public static IEnumerable<Span<StageType>> Stagify(IEnumerable<Span<Token>> tokens)
        //    => throw new NotImplementedException();


        //public static IEnumerable<QueryOption> ParseOptions(string source)
        //    => ParseOptions(Lexer.Lex(source), source);
        


        //class Context
        //{
        //    readonly IEnumerator<Span<Token>> _en;
        //    readonly string _source;

        //    public Context(IEnumerable<Span<Token>> en, string source) {
        //        _en = en.GetEnumerator();
        //        _source = source;
        //    }

        //    public Span<Token> Current { get; private set; }

        //    public Span<Token> Shift() {
        //        throw new NotImplementedException();
        //    }
        //}



        //static IEnumerable<QueryOption> ParseOptions(IEnumerable<Span<Token>> spans, string source) 
        //{
        //    var cursor = spans.GetEnumerator();
        //    var stack = new Stack<Span<Token>>(64);

        //    while(true) {
        //        var curr = cursor.Current;

        //        switch(curr.Token) {
        //            case Token.Open:
        //                stack.Push(curr);
        //                cursor.MoveNext();
        //                break;

        //            case Token.Close:
        //                stack.Pop();
        //                //and now emit our nice node
        //                break;

        //            case Token.End:
        //                return null;

                                            
        //            //so we accumulate context
        //            //but don't we also accumulate payload? Nope - context isn't just on the top of the stack; its in the parser's current mode
        //            //or - its in the context of the function doing the parsing. Each function, when we're in it, represents a separate node-building-in-process,
        //            //with whatever local variables it needs.
                    
        //            //

                    
        //        }                
        //    }            
        //}

        
     
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
