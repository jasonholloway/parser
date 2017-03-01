using System;
using System.Linq;
using System.Collections.Generic;
using Xunit;

namespace Parser.Tests
{
    
    public class ParserTests
    {

        [Fact]
        public void Stagifying1() 
        {
            var tokens = Lexer.Lex("Dogs/Chihuahuas?$filter=Name eq 'Boris'");
            var stages = Stagifier.Stagify(tokens).ToArray();

            Assert.Equal(stages,
                new[] {
                    Span.Of(StageType.Subset, 0, 4),
                    Span.Of(StageType.Subset, 5, 14),
                    Span.Of(StageType.Filter, 23, 38)
                });            
        }




        [Fact]
        public void Stagifies_Subsets() {
            var source = "Dogs/Chihuahuas";
            var stages = Parser.Parse(source).ToArray();

            var stage1 = (SubsetStage)stages[0];
            Assert.Equal("Dogs", stage1.Name.From(source));

            var stage2 = (SubsetStage)stages[1];
            Assert.Equal("Chihuahuas", stage2.Name.From(source));
        }


        [Fact(DisplayName = "Parser stagifies functions & args")]
        public void Stagifies_Functions() 
        {
            var source = "Animals/Choose('Dogs','Chihuahuas')/Biggest()";
            var stages = Parser.Parse(source).ToArray();

            var stage1 = (SubsetStage)stages[0];
            Assert.Equal("Animals", stage1.Name.From(source));
            
            var stage2 = (FunctionStage)stages[1];
            Assert.Equal("Choose", stage2.Name.From(source));

            var args = stage2.Args.ToArray();
            var arg1 = args[0];

            //Assert.Equal(Span.Of(Token.String, 15, 21), argSpans[0]);
            //Assert.Equal(Span.Of(Token.String, 22, 34), argSpans[1]);
            
            //TEST ARGS HERE!!!!!

            var stage3 = (FunctionStage)stages[2];
            Assert.Equal("Biggest", stage2.Name.From(source));
            Assert.Empty(stage3.Args);
        }









        //instead of being dead types, emitted stages should include resolvers
        //though these can be included by enum value type and then interpreted






    }

    
}
