using System;
using System.Collections.Generic;
using System.Text;

namespace Parser
{

    public static class Span
    {
        public static Span<T> Of<T>(T token, int left, int right)
            => new Span<T>(token, left, right);
    }

    public struct Span<T>
    {
        public readonly T Token;
        public readonly int Left;
        public readonly int Right;

        public Span(T token, int left, int right) {
            Token = token;
            Left = left;
            Right = right;
        }

        public override string ToString()
            => $"({Left}, {Right}) {Token}";

    }

}
