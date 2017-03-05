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
        public readonly Token Token;
        public readonly int Left;
        public readonly int Right;

        public TokenSpan(Token token, int left, int right) {
            Token = token;
            Left = left;
            Right = right;
        }

        
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
        public static string From(this Span span, string source)
            => source.Substring(span.Left, span.Right - span.Left);

        public static string From(this TokenSpan span, string source)
            => source.Substring(span.Left, span.Right - span.Left);         //but what about handling percent-encodings?
                                                                            //we'll need to manually populate a new string(n)

        public static bool Match(this TokenSpan span, string from, string comp)
            => string.Compare(from, span.Left, comp, 0, span.Right - span.Left) == 0;

    }



}
