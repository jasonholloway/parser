using System;
using System.Collections.Generic;
using System.Text;

namespace Parser
{

    public interface IStage : INode
    {
    }



    public interface ISegment : IStage
    {

    }




    public class SubsetSegment : ISegment
    {
        public SubsetSegment(Span name) {
            Name = name;
        }

        public Span Name { get; private set; }
    }




    public class FunctionSegment : ISegment
    {
        public FunctionSegment(Span name, IEnumerable<INode> args) {
            Name = name;
            Args = args;
        }

        public Span Name { get; private set; }
        public IEnumerable<INode> Args { get; private set; }

    }




}
