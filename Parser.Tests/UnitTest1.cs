using System;
using Xunit;

namespace Parser.Tests
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            var query = "dogs/chihuahuas?$filter=name eq frank";

            var result = Parser.Parse(query);
            
        }
    }
}
