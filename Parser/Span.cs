using System;
using System.Collections.Generic;
using System.Text;

namespace Parser
{

    public struct Span
    {
        public readonly int Left;
        public readonly int Right;

        public Span(int left, int right) {
            Left = left;
            Right = right;
        }
    }




    public struct TokenSpan
    {
        public readonly int Left;
        public readonly int Right;
        public readonly Token Token;

        public TokenSpan(Token token, int left, int right) {
            Left = left;
            Right = right;
            Token = token;
        }


        public int Size => Right - Left;
        
        
        static public implicit operator Span(TokenSpan inp)
            => new Span(inp.Left, inp.Right);


        public static TokenSpan Of(Token token, int left, int right)
            => new TokenSpan(token, left, right);


        public static TokenSpan None
            = new TokenSpan(Token.None, 0, 0);
        

        public override string ToString()
            => $"({Left}, {Right}) {Token}";

    }



    public static class SpanExtensions
    {
        //public static string From(this Span span, string source)
        //    => source.Substring(span.Left, span.Right - span.Left);




        public static string From(this TokenSpan span, string source) 
        {
            var sb = new StringBuilder(span.Size);

            switch(span.Token) {
                case Token.String:
                    break;

                default:
                    return source.Substring(span.Left, span.Right - span.Left);         //instead of substring here, need to walk through chars
            }

            return sb.ToString();
        }


        





        public static bool Match(this TokenSpan span, string from, string comp)
            => string.Compare(from, span.Left, comp, 0, span.Right - span.Left) == 0;

    }



}
