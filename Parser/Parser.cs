using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Parser
{
    
    public enum TokenType
    {
        Start,
        Number,
        Symbol,
        String,
        Open,
        Close,
        Slash,
        End
    }


    public struct Token
    {
        public readonly TokenType Type;
        public readonly int Left;
        public readonly int Right;

        public Token(TokenType type, int left, int right) {
            Type = type;
            Left = left;
            Right = right;
        }
    }


    internal static class CharExtensions
    {
        public static bool IsNumber(this char c)
            => c >= '0' && c <= '9';

        public static bool IsQuoteMark(this char c)
            => c == '\'' || c == '"';

        public static bool IsWordChar(this char c)
            => (c >= 'a' && c <= 'Z') || c.IsNumber();

        public static bool IsWhitespace(this char c)
            => c == ' '; 
    }


    public static class Lexer
    {
        
        class Context
        {
            public int LastIndex;
            public int Index;
            
            string _source;
            int _length;
            
            public Context(string source) {
                LastIndex = Index = 0;
                _source = source;
                _length = _source.Length;
            }

            public char Shift()
                => _source[Index++];

            public char Peek()
                => _source[Index];

            public void Retreat()
                => Index--;

            public bool AtEnd 
                => Index == _length;

            public Token Token(TokenType type)
                => new Token(type, LastIndex, Index);
                        
        }



        public static IEnumerable<Token> Tokenize(string source)
            => LexOuter(new Context(source));
            
                

        static IEnumerable<Token> LexOuter(Context x) 
        {
            yield return x.Token(TokenType.Start);
            
            while(!x.AtEnd) {
                var c = x.Peek();

                if(c.IsNumber()) {
                    yield return LexNumeric(x);
                }
                else if(c.IsQuoteMark()) {
                    yield return LexString(x);
                }
                else if(c.IsWordChar()) {
                    yield return LexSymbol(x);
                }
                else if(c.IsWhitespace()) {
                    x.Shift();
                }
            }

            yield return x.Token(TokenType.End);
        }


        //TODO
        //brackets
        //dollar symbols = sections
        //


        static Token LexNumeric(Context x) 
        {
            while(x.Shift().IsNumber()) { } //but also beware dates

            return x.Token(TokenType.Number);            
        }


        static Token LexString(Context x) 
        {
            while(!x.Shift().IsQuoteMark()) { }

            return x.Token(TokenType.String);
        }


        static Token LexSymbol(Context x) 
        {
            while(x.Shift().IsWordChar()) { }

            return x.Token(TokenType.Symbol);
        }



    }


    public static class Parser
    {


        struct Context
        {
            public readonly int Position;

            public Context(int pos = 0) {
                Position = pos;
            }
        }


        public static bool Parse(string inp) 
        {
            var x = new Context();        

            return true;
        }
    }
}
