using System;
using System.Collections.Generic;
using System.Text;

namespace Parser
{
 
    public class ValueNode<TVal> : INode
    {
        public readonly TVal Value;

        public ValueNode(TVal val) {
            Value = val;
        }
    }
    

    public class AssignmentNode : INode
    {
        public readonly INode Left;
        public readonly INode Right;

        public AssignmentNode(INode left, INode right) {
            Left = left;
            Right = right;
        }
    }


    public class SymbolNode : INode
    {
        public readonly Symbol Symbol;

        public SymbolNode(Symbol optionType) {
            Symbol = optionType;
        }
    }


    public class UnaryOperatorNode : INode
    {
        public readonly Operator Operator;
        public readonly INode Operand;

        public UnaryOperatorNode(Operator op, INode node) {
            Operator = op;
            Operand = node;
        }
    }


    public class BinaryOperatorNode : INode
    {
        public readonly Operator Operator;
        public readonly INode Left;
        public readonly INode Right;

        public BinaryOperatorNode(Operator op, INode left, INode right) {
            Operator = op;
            Left = left;
            Right = right;
        }
    }

    

    public class AccessorNode : INode
    {
        public readonly INode Parent;
        public readonly string Name;

        public AccessorNode(INode parent, string name) {
            Parent = parent;
            Name = name;
        }
    }



    public class CallNode : INode
    {
        public readonly INode Function;
        public readonly IReadOnlyList<INode> Args;
        
        public CallNode(INode function, IReadOnlyList<INode> args) {
            Function = function;
            Args = args;
        }
    }



    public enum Operator
    {
        //Unary
        Not,
        Negate,

        //Binary
        Equals,
        NotEquals,
        GreaterThan,
        LessThan,
        And,
        Or,

        Add,
        Subtract,
        Multiply,
        Divide,
        Modulo
    }


}
