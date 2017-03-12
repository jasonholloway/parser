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
        Hyphen,
        Colon,
        Comma,
        Add,
        QuestionMark,
        Hash,
        Dot,
        Equals,
        Ampersand,

        Guid,
        Date,
        DateTime,
        Decimal
    }



    internal static class CharExtensions
    {        

        public static bool IsNumber(this char c)
            => c >= '0' && c <= '9';

        public static bool IsQuoteMark(this char c)
            => c == '\'' || c == '"';

        public static bool IsAlpha(this char c)
            => (c >= 'A' && c <= 'z');

        public static bool IsWhitespace(this char c)
            => c == ' ';


        public static bool IsHex(this char c)
            => c.IsNumber()
                || (c >= 'a' && c <= 'f')
                || (c >= 'A' && c <= 'F');
        

        public static int DecodeAsHex(this char c) {
            if(c <= '9') return c - '0';
            else if(c <= 'F') return c - 'A';
            else return c - 'a';
        }


        public static int DecodeAsDecimal(this char c) {
            if(c >= '0' && c <= '9') return c - '0';
            else throw new InvalidOperationException($"Encounterd non-decimal character '{c}'!");
        }


    }
    



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
            

            public void Take() 
            {
                Char = NextChar;

                Right = Reader.ReadStart;

                Reader.MoveNext();

                NextChar = Reader.Current;                
            }


            public bool Take(char @char) {
                if(Char != @char) return false;
                Take();
                return true;
            }
            

            public bool TakeHex(int count = 1) {
                for(int i = 0; i < count; i++) {
                    if(!Char.IsHex()) return false;
                    Take();
                }

                return true;
            }



            public bool TakeNumeric(int count = 1) {
                for(int i = 0; i < count; i++) {
                    if(!Char.IsNumber()) return false;
                    Take();
                }

                return true;
            }


            public bool TakeAlpha(int count = 1) {
                for(int i = 0; i < count; i++) {
                    if(!Char.IsAlpha()) return false;
                    Take();
                }

                return true;
            }


            public bool AtEnd => Char == 0;


            public TokenSpan Emit(Token type) 
            {
                var token = TokenSpan.Of(type, Left, Right);

                Left = Right;
                
                return token;
            }

            
            public Action CreateRestorer() {
                var oldReader = Reader.Clone();

                var oldLeft = Left;
                var oldRight = Right;
                var oldChar = Char;
                var oldNextChar = NextChar;

                return () => {
                    Left = oldLeft;    //bizarrely this should work, as these are captured as values
                    Right = oldRight;
                    Char = oldChar;
                    NextChar = oldNextChar;
                    Reader = oldReader;
                };
            }

        }





        public static IEnumerable<TokenSpan> Lex(string source)
            => Lex(new Context(source));
            
                

        static IEnumerable<TokenSpan> Lex(Context x) 
        {
            yield return x.Emit(Token.Start);

            x.Take();

            x.Take();
            
            while(!x.AtEnd) {
                yield return (TokenSpan)(LexSpace(x)
                                            ?? LexChars(x)
                                            ?? LexString(x)
                                            ?? LexGuid(x)
                                            ?? LexDate(x)
                                            ?? LexNumeric(x)
                                            ?? LexWord(x)
                                            ?? LexReservedWord(x));  
            }
            
            yield return x.Emit(Token.End);
        }



            

        static TokenSpan? LexGuid(Context x) {
            if(x.Char.IsHex() && x.NextChar.IsHex()) 
            {
                var restore = x.CreateRestorer();

                bool isGuid = x.TakeHex(8)
                                && x.Take('-')
                                && x.TakeHex(4)
                                && x.Take('-')
                                && x.TakeHex(4)
                                && x.Take('-')
                                && x.TakeHex(4)
                                && x.Take('-')
                                && x.TakeHex(12);

                if(isGuid) {
                    return x.Emit(Token.Guid);
                }
                else {
                    restore();
                }
            }

            return null;
        }





        static TokenSpan? LexDate(Context x) {
            if(x.Char.IsNumber() && x.NextChar.IsNumber()) 
            {
                var restore = x.CreateRestorer();

                bool isDate = x.TakeNumeric(4)
                                && x.Take('-')
                                && x.TakeNumeric(2)
                                && x.Take('-')
                                && x.TakeNumeric(2);

                if(isDate) {
                    return x.Emit(Token.Date);
                }
                else {
                    restore();
                }
            }

            return null;
        }





        static TokenSpan? LexNumeric(Context x) 
        {
            if(x.Char.IsNumber()) {
                while(x.TakeNumeric()) { }

                if(x.Take('.')) {
                    while(x.TakeNumeric()) { }

                    return x.Emit(Token.Decimal);
                }
                
                return x.Emit(Token.Number);
            }

            return null;  
        }


        static TokenSpan? LexString(Context x) {
            if(x.Char.IsQuoteMark()) 
            {
                while(true) 
                {
                    x.Take();

                    if(x.Char.IsQuoteMark()) {
                        x.Take();

                        if(x.Char.IsQuoteMark()) {
                            x.Take();
                        }
                        else {
                            break;
                        }
                    }
                }

                return x.Emit(Token.String);
            }

            return null;
        }


        static TokenSpan? LexReservedWord(Context x) 
        {
            if(x.Take('$')) {
                while(x.TakeAlpha()) { }

                return x.Emit(Token.ReservedWord);
            }

            return null;
        }
                        
        static TokenSpan? LexWord(Context x) 
        {
            if(x.TakeAlpha()) {
                while(x.TakeAlpha()) { }

                return x.Emit(Token.Word);
            }

            return null;
        }

        static TokenSpan? LexSpace(Context x) 
        {
            if(x.Take(' ')) {
                while(x.Take(' ')) { }

                return x.Emit(Token.Space);
            }

            return null;
        }


        static TokenSpan? LexChars(Context x) 
        {
            Token token;

            switch(x.Char) {
                case '/': token = Token.Slash; break;
                case ',': token = Token.Comma; break;
                case '.': token = Token.Dot; break;
                case '=': token = Token.Equals; break;
                case '(': token = Token.Open; break;
                case ')': token = Token.Close; break;
                case ':': token = Token.Colon; break;
                case '-': token = Token.Hyphen; break;
                case '&': token = Token.Ampersand; break;
                case '?': token = Token.QuestionMark; break;
                case '#': token = Token.Hash; break;
                default:  return null;
            }

            x.Take();

            return x.Emit(token);
        }
                

    }
        
}
