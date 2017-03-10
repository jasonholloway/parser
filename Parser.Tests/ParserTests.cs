using System;
using System.Linq;
using System.Collections.Generic;
using Xunit;
using Shouldly;
using System.Text.RegularExpressions;

namespace Parser.Tests
{
    
    public class ParserTests
    {

        [Fact(DisplayName = "Parses subset segments")]
        public void Parses_Subsets() {
            var parsed = Parser.Parse("Dogs/Chihuahuas");

            var node1 = parsed.Resource.ShouldBeOfType<AccessorNode>();
            node1.Name.ShouldBe("Chihuahuas");

            var node2 = node1.Parent.ShouldBeOfType<AccessorNode>();
            node2.Name.ShouldBe("Dogs");            
        }




        [Fact(DisplayName = "Parses function segments & args")]
        public void Parses_Functions() {
            var parsed = Parser.Parse("Animals/Choose('Dogs','Chihuahuas')/Biggest()");

            var call1 = parsed.Resource.ShouldBeOfType<CallNode>();
            call1.Args.ShouldBeEmpty();

            var func1 = call1.Function.ShouldBeOfType<AccessorNode>();
            func1.Name.ShouldBe("Biggest");

            var call2 = func1.Parent.ShouldBeOfType<CallNode>();
            call2.Args[0].ShouldBeOfType<ValueNode<string>>().Value.ShouldBe("Dogs");
            call2.Args[1].ShouldBeOfType<ValueNode<string>>().Value.ShouldBe("Chihuahuas");

            var func2 = call2.Function.ShouldBeOfType<AccessorNode>();
            func2.Name.ShouldBe("Choose");

            func2.Parent.ShouldBeOfType<AccessorNode>()
                        .Name.ShouldBe("Animals");            
        }




        [Fact(DisplayName = "Parses segment & very simple filter")]
        public void Parses_SimpleFilter() 
        {
            var parsed = Parser.Parse("BigDogs?$filter=true");

            var stage1 = parsed.Resource.ShouldBeOfType<AccessorNode>();
            stage1.Name.ShouldBe("BigDogs");

            var assignNode = parsed.Options.ShouldHaveSingleItem().ShouldBeOfType<AssignmentNode>();

            assignNode.Left.ShouldBeOfType<SymbolNode>()
                            .Symbol.ShouldBe(Symbol.Filter);

            assignNode.Right.ShouldBeOfType<ValueNode<bool>>()
                            .Value.ShouldBeTrue();
        }





        [Fact(DisplayName = "Parses more complicated filter")]
        public void Parses_MoreComplicatedFilter() {
            var parsed = Parser.Parse("?$filter=(2 eq 4) or false");

            parsed.Resource.ShouldBeNull();

            var assignNode = parsed.Options.ShouldHaveSingleItem().ShouldBeOfType<AssignmentNode>();

            assignNode.Left.ShouldBeOfType<SymbolNode>()
                            .Symbol.ShouldBe(Symbol.Filter);

            var orNode = assignNode.Right.ShouldBeOfType<BinaryOperatorNode>();
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

            var resNode = parsed.Resource.ShouldBeOfType<AccessorNode>();
            resNode.Name.ShouldBe("Animals");
            resNode.Parent.ShouldBeNull();

            var assignNode = parsed.Options.ShouldHaveSingleItem().ShouldBeOfType<AssignmentNode>();
            assignNode.Left.ShouldBeOfType<SymbolNode>().Symbol.ShouldBe(Symbol.Filter);

            var eqNode = assignNode.Right.ShouldBeOfType<BinaryOperatorNode>();
            eqNode.Operator.ShouldBe(Operator.Equals);

            var rightNode = eqNode.Right.ShouldBeOfType<ValueNode<int>>();
            rightNode.Value.ShouldBe(10);

            var callNode = eqNode.Left.ShouldBeOfType<CallNode>();
            callNode.Args.ShouldBeEmpty();

            var funcNode = callNode.Function.ShouldBeOfType<AccessorNode>();
            funcNode.Name.ShouldBe("Length");
            funcNode.Parent.ShouldBeOfType<AccessorNode>().Name.ShouldBe("Name");
        }



        [Fact(DisplayName = "Accessor names include contiguous numbers")]
        public void NumbersInNames() {
            var parsed = Parser.Parse("AB123CD/E3");

            var node1 = parsed.Resource.ShouldBeOfType<AccessorNode>();
            node1.Name.ShouldBe("E3");

            var node2 = node1.Parent.ShouldBeOfType<AccessorNode>();
            node2.Name.ShouldBe("AB123CD");
        }




        [Fact(DisplayName = "Parses decimals")]
        public void ParsesDecimals() {
            var parsed = Parser.Parse("43.123456 add -1.123");

            var add = parsed.Resource.ShouldBeOfType<BinaryOperatorNode>();
            add.Operator.ShouldBe(Operator.Add);

            var left = add.Left.ShouldBeOfType<ValueNode<decimal>>();
            left.Value.ShouldBe(43.123456M);

            var neg = add.Right.ShouldBeOfType<UnaryOperatorNode>();
            neg.Operator.ShouldBe(Operator.Negate);

            var right = neg.Operand.ShouldBeOfType<ValueNode<decimal>>();
            right.Value.ShouldBe(1.123M);
        }




        [Fact(DisplayName = "Parses multiplicatives ahead of additives 1")]
        public void ParsesMultiplicativesAheadOfAdditives1() 
        {
            var parsed = Parser.Parse("43 add 3 mul 7");

            var add = parsed.Resource.ShouldBeOfType<BinaryOperatorNode>();
            add.Operator.ShouldBe(Operator.Add);

            var addLeft = add.Left.ShouldBeOfType<ValueNode<int>>();
            addLeft.Value.ShouldBe(43);

            var addRight = add.Right.ShouldBeOfType<BinaryOperatorNode>();
            addRight.Operator.ShouldBe(Operator.Multiply);

            var mulLeft = addRight.Left.ShouldBeOfType<ValueNode<int>>();
            mulLeft.Value.ShouldBe(3);

            var mulRight = addRight.Right.ShouldBeOfType<ValueNode<int>>();
            mulRight.Value.ShouldBe(7);
        }


        [Fact(DisplayName = "Parses multiplicatives ahead of additives 2")]
        public void ParsesMultiplicativesAheadOfAdditives2() 
        {
            var parsed = Parser.Parse("43 mul 3 add 7");

            var add = parsed.Resource.ShouldBeOfType<BinaryOperatorNode>();
            add.Operator.ShouldBe(Operator.Add);
            add.Right.ShouldBeOfType<ValueNode<int>>().Value.ShouldBe(7);

            var mul = add.Left.ShouldBeOfType<BinaryOperatorNode>();
            mul.Operator.ShouldBe(Operator.Multiply);
            mul.Left.ShouldBeOfType<ValueNode<int>>().Value.ShouldBe(43);
            mul.Right.ShouldBeOfType<ValueNode<int>>().Value.ShouldBe(3);
        }





        [Fact(DisplayName = "Parses V4 dates")]
        public void Parses_V4_Dates() {
            var parsed = Parser.Parse("?$filter=Date gt 2012-05-29");

            var assignNode = parsed.Options.ShouldHaveSingleItem().ShouldBeOfType<AssignmentNode>();

            var gtNode = assignNode.Right.ShouldBeOfType<BinaryOperatorNode>();
            gtNode.Operator.ShouldBe(Operator.GreaterThan);

            var leftNode = gtNode.Left.ShouldBeOfType<AccessorNode>();
            leftNode.Name.ShouldBe("Date");

            var rightNode = gtNode.Right.ShouldBeOfType<ValueNode<DateTimeOffset>>();
            rightNode.Value.Year.ShouldBe(2012);
            rightNode.Value.Month.ShouldBe(5);
            rightNode.Value.Day.ShouldBe(29);
        }




        [Fact(DisplayName = "Parses V4 datetimes")]
        public void Parses_V4_DateTimes() {
            var parsed = Parser.Parse("?$filter=Date gt 2012-05-29T23:11:11.123Z");

            var assignNode = parsed.Options.ShouldHaveSingleItem().ShouldBeOfType<AssignmentNode>();

            var gtNode = assignNode.Right.ShouldBeOfType<BinaryOperatorNode>();
            gtNode.Operator.ShouldBe(Operator.GreaterThan);

            var leftNode = gtNode.Left.ShouldBeOfType<AccessorNode>();
            leftNode.Name.ShouldBe("Date");

            var rightNode = gtNode.Right.ShouldBeOfType<ValueNode<DateTimeOffset>>();
            rightNode.Value.Year.ShouldBe(2012);
            rightNode.Value.Month.ShouldBe(5);
            rightNode.Value.Day.ShouldBe(29);
            rightNode.Value.Hour.ShouldBe(23);
            rightNode.Value.Minute.ShouldBe(11);
            rightNode.Value.Second.ShouldBe(11);
            rightNode.Value.Millisecond.ShouldBe(123);
        }





        //[Fact(DisplayName = "V3 Dates parsed")]
        //public void Parses_V3_Dates() {
        //    var parsed = Parser.Parse("?$filter=Date ge datetime'2012-05-29T09:13:28.123");

        //    var assignNode = parsed.Options.ShouldHaveSingleItem().ShouldBeOfType<AssignmentNode>();

        //    var gtNode = assignNode.Right.ShouldBeOfType<BinaryOperatorNode>();
        //    gtNode.Operator.ShouldBe(Operator.GreaterThan);

        //    var leftNode = gtNode.Left.ShouldBeOfType<AccessorNode>();
        //    leftNode.Name.ShouldBe("Date");

        //    var rightNode = gtNode.Right.ShouldBeOfType<ValueNode<DateTime>>();
        //    rightNode.Value.Year.ShouldBe(2012);
        //    rightNode.Value.Month.ShouldBe(5);
        //    rightNode.Value.Day.ShouldBe(29);
        //    rightNode.Value.Hour.ShouldBe(9);
        //    rightNode.Value.Minute.ShouldBe(13);
        //    rightNode.Value.Second.ShouldBe(28);
        //    rightNode.Value.Millisecond.ShouldBe(123);
        //}



        [Fact(DisplayName = "Respects unary precedence 1")]
        public void Respects_Unary_Precedence1() {
            var parsed = Parser.Parse("not false and true");

            var andNode = parsed.Resource.ShouldBeOfType<BinaryOperatorNode>();
            andNode.Operator.ShouldBe(Operator.And);

            andNode.Right.ShouldBeOfType<ValueNode<bool>>()
                        .Value.ShouldBeTrue();

            var notNode = andNode.Left.ShouldBeOfType<UnaryOperatorNode>();

            notNode.Operand.ShouldBeOfType<ValueNode<bool>>()
                            .Value.ShouldBeFalse();            
        }



        [Fact(DisplayName = "Respects unary precedence 2")]
        public void Respects_Unary_Precedence2() {
            var parsed = Parser.Parse("true and not false");
            
            var andNode = parsed.Resource.ShouldBeOfType<BinaryOperatorNode>();
            andNode.Operator.ShouldBe(Operator.And);

            andNode.Left.ShouldBeOfType<ValueNode<bool>>()
                        .Value.ShouldBeTrue();

            var notNode = andNode.Right.ShouldBeOfType<UnaryOperatorNode>();

            notNode.Operand.ShouldBeOfType<ValueNode<bool>>()
                            .Value.ShouldBeFalse();

        }





        [Fact(DisplayName = "Groupings isolate internal precedences")]
        public void Respects_Groupings() {
            var parsed = Parser.Parse("not(true and false)");   //if they didn't isolate, the 'not' here would stop the 'and' being taken

            var notNode = parsed.Resource.ShouldBeOfType<UnaryOperatorNode>();
            notNode.Operator.ShouldBe(Operator.Not);

            var andNode = notNode.Operand.ShouldBeOfType<BinaryOperatorNode>();
            andNode.Operator.ShouldBe(Operator.And);
            andNode.Left.ShouldBeOfType<ValueNode<bool>>().Value.ShouldBeTrue();
            andNode.Right.ShouldBeOfType<ValueNode<bool>>().Value.ShouldBeFalse();
        }




        [Fact(DisplayName = "Parses GUIDs")]
        public void Parses_Guids() {
            throw new NotImplementedException();
        }





        [Fact(DisplayName = "Parses unary operators")]
        public void Parses_Unary_Operators() {
            var parsed = Parser.Parse("?$filter=not (-Length eq -1) and true");

            parsed.Resource.ShouldBeNull();

            var assignNode = parsed.Options.ShouldHaveSingleItem().ShouldBeOfType<AssignmentNode>();
            assignNode.Left.ShouldBeOfType<SymbolNode>().Symbol.ShouldBe(Symbol.Filter);
            
            var andNode = assignNode.Right.ShouldBeOfType<BinaryOperatorNode>();
            andNode.Operator.ShouldBe(Operator.And);

            var trueNode = andNode.Right.ShouldBeOfType<ValueNode<bool>>();
            trueNode.Value.ShouldBeTrue();

            var notNode = andNode.Left.ShouldBeOfType<UnaryOperatorNode>();
            notNode.Operator.ShouldBe(Operator.Not);

            var eqNode = notNode.Operand.ShouldBeOfType<BinaryOperatorNode>();

            var negNode = eqNode.Left.ShouldBeOfType<UnaryOperatorNode>();
            negNode.Operator.ShouldBe(Operator.Negate);

            var negNode2 = eqNode.Right.ShouldBeOfType<UnaryOperatorNode>();
            negNode2.Operator.ShouldBe(Operator.Negate);
            negNode2.Operand.ShouldBeOfType<ValueNode<int>>().Value.ShouldBe(1);

            var accessorNode = negNode.Operand.ShouldBeOfType<AccessorNode>();
            accessorNode.Name.ShouldBe("Length");
            accessorNode.Parent.ShouldBeNull();
        }
                
        
    }


}
