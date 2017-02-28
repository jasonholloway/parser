using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Parser
{
    
    public enum Token
    {
        Start,
        End,
        Space,
        Number,
        Word,
        ReservedWord,
        Alias,
        String,
        Open,
        Close,
        Slash,
        Comma,
        QuestionMark,
        Dot,
        Equals,
        Ampersand
    }



    internal static class CharExtensions
    {
        public static bool IsNumber(this char c)
            => c >= '0' && c <= '9';

        public static bool IsQuoteMark(this char c)
            => c == '\'' || c == '"';

        public static bool IsWordChar(this char c)
            => (c >= 'A' && c <= 'z') || c.IsNumber();

        public static bool IsWhitespace(this char c)
            => c == ' ';
        


        public static int DecodeAsHex(this char c) {
            if(c <= '9') return c - '0';
            else if(c <= 'F') return c - 'A';
            else return c - 'a';
        }

    }


    //Split undecoded URL into components scheme, hier-part, query, and fragment at first ":", then first "?", and then first "#" 

    //so we should do prelimary splitting,
    //returning nested enumerations of nested tokens

    //At the top level: TopToken.Scheme, TopToken.Part, TopToken.Query, TopToken.Fragment
    //The QueryToken will then split into its internal tokens when inspected
    //

        



    public static class Lexer
    {
        
        class Context
        {
            public string Source { get; private set; }
            public int SourceLength { get; private set; }
            public int RemainingCount { get; private set; }

            public int Start { get; private set; }
            public int Last { get; private set; }
            public int Next { get; private set; }

            public char Char { get; private set; }
            

            public Context(string source) 
            {
                Source = source;
                SourceLength = Source.Length;
                RemainingCount = SourceLength;

                Start = Next = Last = 0;
                Char = SourceLength == 0 ? (char)0 : Source[0];
            }
            

            public char Shift() 
            {
                Last = Next;

                if(RemainingCount > 0) {
                    RemainingCount--;
                    if(RemainingCount < 0) throw new InvalidOperationException("Parsed past end of string!");

                    Char = Source[Next++];

                    if(Char == '%') {
                        RemainingCount -= 2;
                        if(RemainingCount < 0) throw new InvalidOperationException("Parsed past end of string!");

                        var nibble1 = Source[Next++];
                        var nibble2 = Source[Next++];
                        Char = (char)((nibble1.DecodeAsHex() << 4) + (nibble2.DecodeAsHex()));
                    }
                    
                    return Char;
                }
                else {
                    Next++;
                    return Char = (char)0; //null terminals not exposed normally
                }
            }
            

            public bool AtEnd 
                => RemainingCount == 0 && Start >= SourceLength;


            public Span<Token> Emit(Token type) {                                
                var token = Span.Of(type, Start, Last);
                Start = Last;
                return token;
            }
             
            public Span<Token> ShiftAndEmit(Token type) {
                Shift();
                return Emit(type);
            }
                      
        }



        

        public static IEnumerable<Span<Token>> Lex(string source)
            => Lex(new Context(source));
            
                

        static IEnumerable<Span<Token>> Lex(Context x) 
        {
            x.Shift();

            yield return x.Emit(Token.Start);
            
            while(!x.AtEnd) {
                yield return (Span<Token>)(LexSpace(x)
                                            ?? LexChars(x)
                                            ?? LexNumeric(x)
                                            ?? LexString(x)
                                            ?? LexWord(x)
                                            ?? LexHeading(x));  
            }
            
            yield return x.Emit(Token.End);
        }

        
        static Span<Token>? LexNumeric(Context x) 
        {
            if(!x.Char.IsNumber()) return null;

            while(x.Shift().IsNumber()) { } //but also beware dates
            
            return x.Emit(Token.Number);            
        }


        static Span<Token>? LexString(Context x) 
        {
            if(!x.Char.IsQuoteMark()) return null;

            while(true) {
                if(x.Shift().IsQuoteMark() && !x.Shift().IsQuoteMark()) break;
            }
            
            return x.Emit(Token.String);
        }

        static Span<Token>? LexHeading(Context x) 
        {
            if(x.Char != '$') return null;

            x.Shift(); //skip '$'

            while(x.Shift().IsWordChar()) { }
            
            return x.Emit(Token.ReservedWord);
        }
                        
        static Span<Token>? LexWord(Context x) 
        {
            if(!x.Char.IsWordChar()) return null;

            while(x.Shift().IsWordChar()) { }
            
            return x.Emit(Token.Word);
        }

        static Span<Token>? LexSpace(Context x) 
        {
            if(!x.Char.IsWhitespace()) return null;

            while(x.Shift().IsWhitespace()) { }
            
            return x.Emit(Token.Space);
        }


        static Span<Token>? LexChars(Context x) 
        {
            switch(x.Char) {
                case '/': return x.ShiftAndEmit(Token.Slash);
                case ',': return x.ShiftAndEmit(Token.Comma);
                case '.': return x.ShiftAndEmit(Token.Dot);
                case '=': return x.ShiftAndEmit(Token.Equals);
                case '(': return x.ShiftAndEmit(Token.Open);
                case ')': return x.ShiftAndEmit(Token.Close);
                case '&': return x.ShiftAndEmit(Token.Ampersand);
                case '?': return x.ShiftAndEmit(Token.QuestionMark);
                default:  return null;
            }
        }
                

    }
        
}
