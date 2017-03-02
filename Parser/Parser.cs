using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Parser
{
    
    

    public interface IStage : INode
    {
    }

    
    public class SubsetStage : IStage
    {
        public SubsetStage(Span<Token> name) {
            Name = name;
        }
        
        public Span<Token> Name { get; private set; }        
    }


    

    public class FunctionStage : IStage
    {
        public FunctionStage(Span<Token> name, IEnumerable<INode> args) {
            Name = name;
            Args = args;
        }

        public Span<Token> Name { get; private set; }        
        public IEnumerable<INode> Args { get; private set; }
        
    }




    public class StringNode : INode
    {
        public readonly Span<Token> Value;

        public StringNode(Span<Token> value) {
            Value = value;
        }
    }






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







    public enum StageType
    {
        Root,
        Subset,
        Filter,
        Select,
        Bind
    }



    public static class Stagifier
    {
        enum Mode
        {
            Path,
            Options,
            Bindings
        }


        public static IEnumerable<Span<StageType>> Stagify(IEnumerable<Span<Token>> tokens) 
        {
            var en = tokens.GetEnumerator();
            int left = en.Current.Left;
            
            while(true) {
                var curr = en.Current;

                switch(curr.Token) {
                    case Token.Start:
                        en.MoveNext();
                        break;

                    case Token.Word:
                        yield return Span.Of(StageType.Subset, left, curr.Right);
                        left = curr.Right;

                        en.MoveNext();
                        break;
                                            
                    case Token.Slash:
                        en.MoveNext();          
                        break;

                    case Token.End:
                    case Token.QuestionMark:
                        yield break;

                    default:
                        throw new NotImplementedException();
                }
            }

            throw new NotImplementedException();
        }
        




    }





    public class Query
    {
        public readonly IStage[] Path;
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
        public IStage[] Path;
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
        readonly IEnumerator<Span<Token>> _tokens;

        
        public Parser(string source) {
            _source = source;
            _tokens = Lexer.Lex(_source).GetEnumerator();
        }



        Span<Token> Next() {
            _tokens.MoveNext();
            return _tokens.Current;
        }

        public Span<Token> Current => _tokens.Current;

        public Token Token => Current.Token;

        #endregion








        public class RootStage : IStage
        {

        }


        QuerySpec _spec;

        
        Query Parse() 
        {
            Next().Token.MustBe(Token.Start);
            
            Next();

            _spec.Path = ParsePath();
            ParseOptions();
            ParseFragment();

            Token.MustBe(Token.End);
            
            return new Query(_spec);
        }



        IStage[] ParsePath() 
        {
            var stages = new List<IStage>(8);

            while(true) {                
                if(Token == Token.Slash) Next();

                var stage = ParseStage();

                if(stage == null) break;
                else stages.Add(stage);
            }

            return stages.ToArray();
        }


        void ParseOptions() {
            //...
        }

        void ParseFragment() {
            //...
        }



        

        

        

        IStage ParseStage()
            => ParseStage_FromWord()
                ?? Error<IStage>();
        


        IStage ParseSegment_Subset() {
            if(Token != Token.Word) return null;

            var name = Current;           

            Next();

            return new SubsetStage(name);
        }

        IStage ParseSegment_Function() {
            if(Token != Token.Word) return null;   //need lookahead here

            var name = Current;

        }




        IStage ParseStage_FromWord() 
        {
            if(Token != Token.Word) return null;
            
            var name = Current;

            Next();

            switch(Current.Token) {
                case Token.Open: {                  //now we know its a function
                        var args = ParseArgs();
                        
                        while(Token != Token.Close) ParseNode();

                        Next();

                        if(Token == Token.Slash) Next();

                        return new FunctionStage(name, args);
                    }

                default: {                          //if terminated by something else, treat as subset
                        var stage = new SubsetStage(name);
                        Next();
                        return stage;
                    }                                        
            }

            throw new NotImplementedException();
        }


        

        IEnumerable<INode> ParseArgs() 
        {
            Next();

            while(true) {
                switch(Current.Token) {                    
                    case Token.Comma:
                        break;

                    case Token.Close:
                        yield break;

                    default:
                        yield return ParseNode();
                        break;
                }
            }
        }





        INode ParseNode()
            => ParseNode_FromWord()
                ?? ParseNode_FromString()
                ?? Error<INode>();


        INode ParseNode_FromWord() {
            if(Current.Token != Token.Word) return null;
            throw new NotImplementedException();
        }

        INode ParseNode_FromString() {
            if(Current.Token != Token.String) return null;
            
            var node = new StringNode(Current);

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
