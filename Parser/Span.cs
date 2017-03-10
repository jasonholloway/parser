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


        public static string Read(this string source, int left, int right) 
        {
            var sb = new StringBuilder(right - left);

            var reader = new CharReader(source, left, right - left);

            while(reader.MoveNext()) sb.Append(reader.Current);

            return sb.ToString();
        }




        public static int AsInt(this TokenSpan span, string source) 
        {
            span.Token.MustBe(Token.Number);

            var reader = new CharReader(source, span.Left, span.Size);

            int acc = 0;

            while(reader.MoveNext()) {
                acc *= 10;
                acc += reader.Current.DecodeAsDecimal();
            }

            return acc;
        }


        public static string AsString(this TokenSpan span, string source) 
        {
            var sb = new StringBuilder(span.Size);

            var reader = new CharReader(source, span.Left, span.Size);

            switch(span.Token) {
                case Token.String:
                    reader.MoveNext();

                    while(reader.MoveNext() && reader.Current != '\'') {
                        sb.Append(reader.Current);
                    }
                    break;

                default:
                    //should use reader here as above!!!
                    //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                    return source.Substring(span.Left, span.Right - span.Left);
            }

            return sb.ToString();
        }


        





        public static bool Match(this TokenSpan span, string from, string comp)
            => string.Compare(from, span.Left, comp, 0, span.Right - span.Left) == 0;

    }



}
