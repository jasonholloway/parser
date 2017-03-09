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



        [Fact(DisplayName = "Accessor names include contiguous numbers")]
        public void NumbersInNames() {
            throw new NotImplementedException();
        }




        [Fact(DisplayName = "V4 Dates parsed")]
        public void Parses_V4_Dates() {
            var parsed = Parser.Parse("?$filter=Date gt 2012-05-29T09:13:28.123Z");

            var gtNode = parsed.Filter.ShouldBeOfType<BinaryOperatorNode>();
            gtNode.Operator.ShouldBe(Operator.GreaterThan);

            var leftNode = gtNode.Left.ShouldBeOfType<AccessorNode>();
            leftNode.Name.ShouldBe("Date");
            
            var rightNode = gtNode.Right.ShouldBeOfType<ValueNode<DateTime>>();
            rightNode.Value.Year.ShouldBe(2012);
            rightNode.Value.Month.ShouldBe(5);
            rightNode.Value.Day.ShouldBe(29);
            rightNode.Value.Hour.ShouldBe(9);
            rightNode.Value.Minute.ShouldBe(13);
            rightNode.Value.Second.ShouldBe(28);
            rightNode.Value.Millisecond.ShouldBe(123);
        }



        [Fact(DisplayName = "V3 Dates parsed")]
        public void Parses_V3_Dates() {
            var parsed = Parser.Parse("?$filter=Date ge datetime'2012-05-29T09:13:28.123");

            var gtNode = parsed.Filter.ShouldBeOfType<BinaryOperatorNode>();
            gtNode.Operator.ShouldBe(Operator.GreaterThan);

            var leftNode = gtNode.Left.ShouldBeOfType<AccessorNode>();
            leftNode.Name.ShouldBe("Date");

            var rightNode = gtNode.Right.ShouldBeOfType<ValueNode<DateTime>>();
            rightNode.Value.Year.ShouldBe(2012);
            rightNode.Value.Month.ShouldBe(5);
            rightNode.Value.Day.ShouldBe(29);
            rightNode.Value.Hour.ShouldBe(9);
            rightNode.Value.Minute.ShouldBe(13);
            rightNode.Value.Second.ShouldBe(28);
            rightNode.Value.Millisecond.ShouldBe(123);
        }





        [Fact(DisplayName = "Parses unary operators")]
        public void Parses_Unary_Operators() {
            var parsed = Parser.Parse("?$filter=not (-Length eq -1) and true");

            parsed.Path.ShouldBeEmpty();

            var andNode = parsed.Filter.ShouldBeOfType<BinaryOperatorNode>();
            andNode.Operator.ShouldBe(Operator.And);

            var trueNode = andNode.Right.ShouldBeOfType<ValueNode<bool>>();
            trueNode.Value.ShouldBeTrue();

            var notNode = andNode.Left.ShouldBeOfType<UnaryOperatorNode>();
            notNode.Operator.ShouldBe(Operator.Not);

            var eqNode = notNode.Node.ShouldBeOfType<BinaryOperatorNode>();

            var negNode = eqNode.Left.ShouldBeOfType<UnaryOperatorNode>();
            negNode.Operator.ShouldBe(Operator.Negate);

            var negNode2 = eqNode.Right.ShouldBeOfType<UnaryOperatorNode>();
            negNode2.Operator.ShouldBe(Operator.Negate);
            negNode2.Node.ShouldBeOfType<ValueNode<int>>().Value.ShouldBe(1);

            var accessorNode = negNode.Node.ShouldBeOfType<AccessorNode>();
            accessorNode.Name.ShouldBe("Length");
            accessorNode.Parent.ShouldBeNull();
        }

        //the parser must have modes
        //it must know whether to consume binaries when paring a unary, and vice versa
        //and this mode must live on a poppable stack, as the modes will be nested

        //the mode is generally provided by the looping function:
        //so we need a separate loop for parsing unaries and binaries
        //the unary will try to take binary continuations, 







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
