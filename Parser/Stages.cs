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
        public readonly string Name;

        public SubsetSegment(string name) {
            Name = name;
        }        
    }




    public class FunctionSegment : ISegment
    {
        public readonly string Name;
        public readonly IEnumerable<INode> Args;

        public FunctionSegment(string name, IEnumerable<INode> args) {
            Name = name;
            Args = args;
        }
    }




}
