using System;
using System.Linq;
using Xunit;

namespace Parser.Tests
{
    public class LexerTests
    {

        [Fact]
        public void OptionsLexing1()
        {
            var spans = Lexer.Lex("$filter=name eq 'Boris'").ToArray();

            Assert.Equal(spans,
                        new[] {
                            Span.Of(Token.Start, 0, 0),
                            Span.Of(Token.Option, 0, 7),
                            Span.Of(Token.Equals, 7, 8),
                            Span.Of(Token.Word, 8, 12),
                            Span.Of(Token.Space, 12, 13),
                            Span.Of(Token.Word, 13, 15),
                            Span.Of(Token.Space, 15, 16),
                            Span.Of(Token.String, 16, 23),
                            Span.Of(Token.End, 23, 23)
                        });            
        }


        [Fact]
        public void OptionsLexing2() 
        {
            var spans = Lexer.Lex("$top=2&$orderby=Name&$filter=(Score gt 1000)").ToArray();

            Assert.Equal(spans,
                        new[] {
                            Span.Of(Token.Start, 0, 0),
                            Span.Of(Token.Option, 0, 4),
                            Span.Of(Token.Equals, 4, 5),
                            Span.Of(Token.Number, 5, 6),
                            Span.Of(Token.Ampersand, 6, 7),
                            Span.Of(Token.Option, 7, 15),
                            Span.Of(Token.Equals, 15, 16),
                            Span.Of(Token.Word, 16, 20),

                            Span.Of(Token.Ampersand, 20, 21),
                            Span.Of(Token.Option, 21, 28),
                            Span.Of(Token.Equals, 28, 29),
                            Span.Of(Token.Open, 29, 30),
                            Span.Of(Token.Word, 30, 35),
                            Span.Of(Token.Space, 35, 36),
                            Span.Of(Token.Word, 36, 38),
                            Span.Of(Token.Space, 38, 39),
                            Span.Of(Token.Number, 39, 43),
                            Span.Of(Token.Close, 43, 44),

                            Span.Of(Token.End, 44, 44)
                        });
        }


        [Fact]
        public void OptionsLexing_Handles_QuoteMarksInStrings() 
        {
            var spans = Lexer.Lex("$filter=Surname eq 'O''Brien'").ToArray();

            Assert.Equal(spans,
                        new[] {
                            Span.Of(Token.Start, 0, 0),
                            Span.Of(Token.Option, 0, 7),
                            Span.Of(Token.Equals, 7, 8),
                            Span.Of(Token.Word, 8, 15),
                            Span.Of(Token.Space, 15, 16),
                            Span.Of(Token.Word, 16, 18),
                            Span.Of(Token.Space, 18, 19),
                            Span.Of(Token.String, 19, 29),
                            Span.Of(Token.End, 29, 29)                        
                        });
        }



        [Fact]
        public void OptionsLexing_Handles_PercentEncodings1() {
            var spans = Lexer.Lex("(%27Hello%27)").ToArray();

            Assert.Equal(spans,
                        new[] {
                            Span.Of(Token.Start, 0, 0),
                            Span.Of(Token.Open, 0, 1),
                            Span.Of(Token.String, 1, 12),
                            Span.Of(Token.Close, 12, 13),
                            Span.Of(Token.End, 13, 13)
                        });
        }




        [Fact]
        public void OptionsLexing_Handles_PercentEncodings2() 
        {
            var spans = Lexer.Lex("$filter=%28Surname eq %27O%27%27Brien%27%29").ToArray();

            Assert.Equal(spans,
                        new[] {
                            Span.Of(Token.Start, 0, 0),
                            Span.Of(Token.Option, 0, 7),
                            Span.Of(Token.Equals, 7, 8),
                            Span.Of(Token.Open, 8, 11),
                            Span.Of(Token.Word, 11, 18),
                            Span.Of(Token.Space, 18, 19),
                            Span.Of(Token.Word, 19, 21),
                            Span.Of(Token.Space, 21, 22),
                            Span.Of(Token.String, 22, 40),
                            Span.Of(Token.Close, 40, 43),
                            Span.Of(Token.End, 43, 43)
                        });
        }


    }
}
