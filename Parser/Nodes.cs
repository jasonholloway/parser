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
    
    public class UnaryOperatorNode : INode
    {
        public readonly Operator Operator;
        public readonly INode Node;

        public UnaryOperatorNode(Operator op, INode node) {
            Operator = op;
            Node = node;
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



    public class FunctionCallNode : INode
    {
        public readonly INode Function;
        public readonly IReadOnlyList<INode> Args;
        
        public FunctionCallNode(INode function, IReadOnlyList<INode> args) {
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
        Or
    }


}
