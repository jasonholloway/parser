using System;
using System.Linq;
using System.Collections.Generic;
using Xunit;

namespace Parser.Tests
{
    
    public class ParserTests
    {        

        [Fact(DisplayName = "Parses subset segments")]
        public void Parses_Subsets() 
        {
            var source = "Dogs/Chihuahuas";
            var query = Parser.Parse(source);

            var stage1 = (SubsetSegment)query.Path[0];
            Assert.Equal("Dogs", stage1.Name.From(source));

            var stage2 = (SubsetSegment)query.Path[1];
            Assert.Equal("Chihuahuas", stage2.Name.From(source));
        }

        


        [Fact(DisplayName = "Parses function segments & args")]
        public void Parses_Functions() 
        {
            var source = "Animals/Choose('Dogs','Chihuahuas')/Biggest()";
            var query = Parser.Parse(source);

            var stage1 = (SubsetSegment)query.Path[0];
            Assert.Equal("Animals", stage1.Name.From(source));
            
            var stage2 = (FunctionSegment)query.Path[1];
            Assert.Equal("Choose", stage2.Name.From(source));

            var args = stage2.Args.ToArray();            
            Assert.Equal("Dogs", (args[0] as StringNode).String.From(source));
            Assert.Equal("Chihuahuas", (args[1] as StringNode).String.From(source));

            var stage3 = (FunctionSegment)query.Path[2];
            Assert.Equal("Biggest", stage3.Name.From(source));
            Assert.Empty(stage3.Args);            
        }







        [Fact(DisplayName = "Parses segment & simple filter")]
        public void Parses_SimpleFilter() {
            var source = "Animals?$filter=true";
            var query = Parser.Parse(source);

            var stage1 = (SubsetSegment)query.Path[0];
            Assert.Equal("Animals", stage1.Name.From(source));

            throw new NotImplementedException();
        }







        //[Fact(DisplayName = "Miscellaneous parsing")]
        //public void Stagifying1() {
        //    var tokens = Lexer.Lex("Dogs/Chihuahuas?$filter=Name eq 'Boris'");
        //    var stages = Stagifier.Stagify(tokens).ToArray();

        //    Assert.Equal(stages,
        //        new[] {
        //            Span.Of(StageType.Subset, 0, 4),
        //            Span.Of(StageType.Subset, 5, 14),
        //            Span.Of(StageType.Filter, 23, 38)
        //        });
        //}






        //instead of being dead types, emitted stages should include resolvers
        //though these can be included by enum value type and then interpreted






    }


}
