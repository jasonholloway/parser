using System;
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





    public static class Parser
    {        

        public static IEnumerable<IStage> Parse(string source)
            => Parse(Lexer.Lex(source), source);
        

        public static IEnumerable<IStage> Parse(IEnumerable<Span<Token>> tokenSpans, string source) 
        {
            var x = new Context(tokenSpans.GetEnumerator(), source);
            x.Shift();

            if(x.Current.Token != Token.Start) throw new InvalidOperationException();
            x.Shift();

            while(x.Current.Token != Token.End) {                
                var stage = _ParseWord(x);

                if(stage == null) throw new InvalidOperationException();

                yield return stage;
            }
        }
        

        static IStage _ParseWord(Context x) 
        {
            if(x.Current.Token != Token.Word) return null;

            var nameSpan = x.Current;

            x.Shift();

            switch(x.Current.Token) {
                case Token.Slash:
                case Token.End: {
                        var stage = new SubsetStage(nameSpan);
                        x.Shift();
                        return stage;
                    }

                case Token.Open: {
                        //now we know its a function, ja...
                        //parse args

                        var args = new INode[0];
                                                
                        while(x.Current.Token != Token.Close) x.Shift();
                                                
                        if(x.Shift().Token == Token.Slash) x.Shift();
                        
                        //x.Shift();

                        return new FunctionStage(nameSpan, args);
                    }
            }

            throw new NotImplementedException();
        }









        class Context
        {
            readonly IEnumerator<Span<Token>> _tokens;

            public readonly string Source;

            public Context(IEnumerator<Span<Token>> tokens, string source) {
                _tokens = tokens;
                Source = source;
            }
            
            public Span<Token> Current => _tokens.Current;

            public Span<Token> Shift() {
                _tokens.MoveNext();
                return _tokens.Current;
            }
            
        }


        
        
        














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
