using System.IO;
using System.Linq;
using NUnit.Framework;
using FluentAssertions;

namespace KizhiPart3
{
    public class SimpleTest
    {
        
        Debugger pult = new Debugger(new StringWriter());
        
        [SetUp]
        public void buildNewPult() {
            pult = new Debugger(new StringWriter());
        }
        
        
        [Test]
        public void Example() {
            pult.ExecuteLine("set code\n" +
                             "set a 5\n" +
                             "sub a 3\n" +
                             "set b 4\n" +
                             "print b\n" +
                             "end set code\n" +
                             "run");
            
        } 
        
    }
}