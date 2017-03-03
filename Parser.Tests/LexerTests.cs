﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace Parser.Tests
{
    
    public class LexerTests
    {
        
        [Fact]
        public void ResourcePath_Lexing1() 
        {
            var spans = Lexer.Lex("Dogs/Chihuahuas('Boris')").ToArray();

            Assert.Equal(spans,
                new[] {
                    TokenSpan.Of(Token.Start, 0, 0),
                    TokenSpan.Of(Token.Word, 0, 4),
                    TokenSpan.Of(Token.Slash, 4, 5),
                    TokenSpan.Of(Token.Word, 5, 15),
                    TokenSpan.Of(Token.Open, 15, 16),
                    TokenSpan.Of(Token.String, 17, 22),
                    TokenSpan.Of(Token.Close, 23, 24),
                    TokenSpan.Of(Token.End, 24, 24)
                });
        }





        [Fact]
        public void OptionsLexing1() 
        {
            var spans = Lexer.Lex("$filter=name eq 'Boris'").ToArray();

            Assert.Equal(spans,
                        new[] {
                            TokenSpan.Of(Token.Start, 0, 0),
                            TokenSpan.Of(Token.ReservedWord, 0, 7),
                            TokenSpan.Of(Token.Equals, 7, 8),
                            TokenSpan.Of(Token.Word, 8, 12),
                            TokenSpan.Of(Token.Space, 12, 13),
                            TokenSpan.Of(Token.Word, 13, 15),
                            TokenSpan.Of(Token.Space, 15, 16),
                            TokenSpan.Of(Token.String, 17, 22),
                            TokenSpan.Of(Token.End, 23, 23)
                        });
        }


        [Fact]
        public void OptionsLexing2() 
        {
            var spans = Lexer.Lex("$top=2&$orderby=Name&$filter=(Score gt 1000)").ToArray();

            Assert.Equal(spans,
                        new[] {
                            TokenSpan.Of(Token.Start, 0, 0),
                            TokenSpan.Of(Token.ReservedWord, 0, 4),
                            TokenSpan.Of(Token.Equals, 4, 5),
                            TokenSpan.Of(Token.Number, 5, 6),
                            TokenSpan.Of(Token.Ampersand, 6, 7),
                            TokenSpan.Of(Token.ReservedWord, 7, 15),
                            TokenSpan.Of(Token.Equals, 15, 16),
                            TokenSpan.Of(Token.Word, 16, 20),

                            TokenSpan.Of(Token.Ampersand, 20, 21),
                            TokenSpan.Of(Token.ReservedWord, 21, 28),
                            TokenSpan.Of(Token.Equals, 28, 29),
                            TokenSpan.Of(Token.Open, 29, 30),
                            TokenSpan.Of(Token.Word, 30, 35),
                            TokenSpan.Of(Token.Space, 35, 36),
                            TokenSpan.Of(Token.Word, 36, 38),
                            TokenSpan.Of(Token.Space, 38, 39),
                            TokenSpan.Of(Token.Number, 39, 43),
                            TokenSpan.Of(Token.Close, 43, 44),

                            TokenSpan.Of(Token.End, 44, 44)
                        });
        }


        [Fact]
        public void OptionsLexing_Handles_QuoteMarksInStrings() 
        {
            var spans = Lexer.Lex("$filter=Surname eq 'O''Brien'").ToArray();

            Assert.Equal(spans,
                        new[] {
                            TokenSpan.Of(Token.Start, 0, 0),
                            TokenSpan.Of(Token.ReservedWord, 0, 7),
                            TokenSpan.Of(Token.Equals, 7, 8),
                            TokenSpan.Of(Token.Word, 8, 15),
                            TokenSpan.Of(Token.Space, 15, 16),
                            TokenSpan.Of(Token.Word, 16, 18),
                            TokenSpan.Of(Token.Space, 18, 19),
                            TokenSpan.Of(Token.String, 20, 28),
                            TokenSpan.Of(Token.End, 29, 29)
                        });
        }



        [Fact]
        public void OptionsLexing_Handles_PercentEncodings1() 
        {
            var spans = Lexer.Lex("(%27Hello%27)").ToArray();

            Assert.Equal(spans,
                        new[] {
                            TokenSpan.Of(Token.Start, 0, 0),
                            TokenSpan.Of(Token.Open, 0, 1),
                            TokenSpan.Of(Token.String, 4, 9),
                            TokenSpan.Of(Token.Close, 12, 13),
                            TokenSpan.Of(Token.End, 13, 13)
                        });
        }




        [Fact]
        public void OptionsLexing_Handles_PercentEncodings2() 
        {
            var spans = Lexer.Lex("$filter=%28Surname eq %27O%27%27Brien%27%29").ToArray();

            Assert.Equal(spans,
                        new[] {
                            TokenSpan.Of(Token.Start, 0, 0),
                            TokenSpan.Of(Token.ReservedWord, 0, 7),
                            TokenSpan.Of(Token.Equals, 7, 8),
                            TokenSpan.Of(Token.Open, 8, 11),
                            TokenSpan.Of(Token.Word, 11, 18),
                            TokenSpan.Of(Token.Space, 18, 19),
                            TokenSpan.Of(Token.Word, 19, 21),
                            TokenSpan.Of(Token.Space, 21, 22),
                            TokenSpan.Of(Token.String, 25, 37),
                            TokenSpan.Of(Token.Close, 40, 43),
                            TokenSpan.Of(Token.End, 43, 43)
                        });
        }


    }



}
