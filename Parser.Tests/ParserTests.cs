using System;
using System.Linq;
using System.Collections.Generic;
using Xunit;
using Shouldly;

namespace Parser.Tests
{
    
    public class ParserTests
    {        

        [Fact(DisplayName = "Parses subset segments")]
        public void Parses_Subsets() 
        {
            var parsed = Parser.Parse("Dogs/Chihuahuas");

            var stage1 = (SubsetSegment)parsed.Path[0];
            stage1.Name.ShouldBe("Dogs");
            
            var stage2 = (SubsetSegment)parsed.Path[1];
            stage2.Name.ShouldBe("Chihuahuas");
        }

        


        [Fact(DisplayName = "Parses function segments & args")]
        public void Parses_Functions() 
        {
            var parsed = Parser.Parse("Animals/Choose('Dogs','Chihuahuas')/Biggest()");

            var stage1 = (SubsetSegment)parsed.Path[0];
            stage1.Name.ShouldBe("Animals");
            
            var stage2 = (FunctionSegment)parsed.Path[1];
            stage2.Name.ShouldBe("Choose");

            var args = stage2.Args.ToArray();
            (args[0] as ValueNode<string>).Value.ShouldBe("Dogs");
            (args[1] as ValueNode<string>).Value.ShouldBe("Chihuahuas");
            
            var stage3 = (FunctionSegment)parsed.Path[2];
            stage3.Name.ShouldBe("Biggest");
            stage3.Args.ShouldBeEmpty();
        }







        [Fact(DisplayName = "Parses segment & very simple filter")]
        public void Parses_SimpleFilter() 
        {
            var parsed = Parser.Parse("BigDogs?$filter=true");

            var stage1 = (SubsetSegment)parsed.Path[0];
            stage1.Name.ShouldBe("BigDogs");

            parsed.Filter.ShouldNotBeNull();
            (parsed.Filter as ValueNode<bool>).Value.ShouldBeTrue();
        }





        [Fact(DisplayName = "Parses more complicated filter")]
        public void Parses_MoreComplicatedFilter() {
            var parsed = Parser.Parse("?$filter=(2 eq 4) or false");
            
            var orNode = parsed.Filter.ShouldBeOfType<BinaryOperatorNode>();
            orNode.Operator.ShouldBe(Operator.Or);

            var rightNode = orNode.Right.ShouldBeOfType<ValueNode<bool>>();
            rightNode.Value.ShouldBeFalse();

            var eqNode = orNode.Left.ShouldBeOfType<BinaryOperatorNode>();
            eqNode.Left.ShouldBeOfType<ValueNode<int>>().Value.ShouldBe(2);
            eqNode.Right.ShouldBeOfType<ValueNode<int>>().Value.ShouldBe(4);
        }




        [Fact(DisplayName = "Parses accessors")]
        public void Parses_Accessors() {
            var parsed = Parser.Parse("Animals?$filter=Name/Length() eq 10");

            parsed.Path.ShouldHaveSingleItem()
                        .ShouldBeOfType<SubsetSegment>()
                        .Name.ShouldBe("Animals");

            var eqNode = parsed.Filter.ShouldBeOfType<BinaryOperatorNode>();
            eqNode.Operator.ShouldBe(Operator.Equals);
            
            var rightNode = eqNode.Right.ShouldBeOfType<ValueNode<int>>();
            rightNode.Value.ShouldBe(10);

            var callNode = eqNode.Left.ShouldBeOfType<FunctionCallNode>();
            callNode.Args.ShouldBeEmpty();

            var funcNode = callNode.Function.ShouldBeOfType<AccessorNode>();            
            funcNode.Name.ShouldBe("Length");
            funcNode.Parent.ShouldBeOfType<AccessorNode>().Name.ShouldBe("Name");
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
