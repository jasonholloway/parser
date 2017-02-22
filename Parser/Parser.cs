using System;
using System.Collections.Generic;
using System.Text;

namespace ODataParsing
{

    public enum Token
    {
        Scheme,
        Part,
        Query,
        Fragment        
    }


    public struct Span
    {
        public readonly Token Token;
        public readonly int Left;
        public readonly int Right;

        public Span(Token token, int left, int right) {
            Token = token;
            Left = left;
            Right = right;
        }
    }


    public static class Parser
    {
        
        class Context
        {
            



        }

        


    }


}
