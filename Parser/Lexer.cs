using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Parser
{
    
    public enum Token
    {
        None,
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
        Minus,
        QuestionMark,
        Hash,
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
            public CharReader Reader { get; private set; }

            public string Source { get; private set; }

            public int Left { get; private set; }
            public int Right { get; private set; }
            
            public char Char { get; private set; }
            public char NextChar { get; private set; }


            public Context(string source) 
            {
                Source = source;

                Reader = new CharReader(source);

                Left = Right = Reader.ReadStart;
            }
            

            public char Shift() 
            {
                Char = NextChar;
                
                Reader.MoveNext();
                
                Right = Reader.ReadStart;

                NextChar = Reader.Current;

                return Char;
            }


            public bool AtEnd => Char == 0;


            public TokenSpan Emit(Token type) 
            {
                var token = TokenSpan.Of(type, Left, Right);

                Left = Right;

                Shift();

                return token;
            }
                                   
        }



        

        public static IEnumerable<TokenSpan> Lex(string source)
            => Lex(new Context(source));
            
                

        static IEnumerable<TokenSpan> Lex(Context x) 
        {
            yield return x.Emit(Token.Start);

            x.Shift();
            
            while(!x.AtEnd) {
                yield return (TokenSpan)(LexSpace(x)
                                            ?? LexChars(x)
                                            ?? LexNumeric(x)
                                            ?? LexString(x)
                                            ?? LexWord(x)
                                            ?? LexReservedWord(x));  
            }
            
            yield return x.Emit(Token.End);
        }

        
        static TokenSpan? LexNumeric(Context x) 
        {
            if(!x.Char.IsNumber()) return null;

            while(x.NextChar.IsNumber()) x.Shift(); //but also beware dates, and of course decimals
            
            return x.Emit(Token.Number);            
        }


        static TokenSpan? LexString(Context x) 
        {
            if(!x.Char.IsQuoteMark()) return null;
            
            while(true) {
                if(x.Shift().IsQuoteMark()) {
                    if(x.NextChar.IsQuoteMark()) {
                        x.Shift();
                    }
                    else {
                        break;
                    }
                }
            }
            
            return x.Emit(Token.String);
        }

        static TokenSpan? LexReservedWord(Context x) 
        {
            if(x.Char != '$') return null;

            x.Shift(); //skip '$'

            while(x.NextChar.IsWordChar()) x.Shift();
            
            return x.Emit(Token.ReservedWord);
        }
                        
        static TokenSpan? LexWord(Context x) 
        {
            if(!x.Char.IsWordChar()) return null;

            while(x.NextChar.IsWordChar()) x.Shift();
            
            return x.Emit(Token.Word);
        }

        static TokenSpan? LexSpace(Context x) 
        {
            if(!x.Char.IsWhitespace()) return null;

            while(x.NextChar.IsWhitespace()) x.Shift();
            
            return x.Emit(Token.Space);
        }


        static TokenSpan? LexChars(Context x) 
        {
            switch(x.Char) {
                case '/': return x.Emit(Token.Slash);
                case ',': return x.Emit(Token.Comma);
                case '.': return x.Emit(Token.Dot);
                case '=': return x.Emit(Token.Equals);
                case '(': return x.Emit(Token.Open);
                case ')': return x.Emit(Token.Close);
                case '&': return x.Emit(Token.Ampersand);
                case '?': return x.Emit(Token.QuestionMark);
                case '#': return x.Emit(Token.Hash);
                case '-': return x.Emit(Token.Minus);
                default:  return null;
            }
        }
                

    }
        
}
