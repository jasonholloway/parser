using System;
using System.Collections.Generic;
using System.Text;

namespace Parser
{
    
    public class StringNode : INode
    {
        public Span String;

        public StringNode(Span @string) {
            String = @string;
        }
    }


    public class IntegerNode : INode
    {
        public Span Integer;

        public IntegerNode(Span integer) {
            Integer = integer;
        }
    }



}
