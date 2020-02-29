using System.IO;
using NUnit.Framework;


namespace KizhiPart2 {
    [TestFixture]
    public class test {
        Interpreter pult = new Interpreter(new StringWriter());

        [SetUp]
        public void buildNewPult() {
            pult = new Interpreter(new StringWriter());

        }
        
        [Test]
        public void Example() {
            pult.Set("a", 5);
            pult.Sub("a", 3);
            pult.Print("a");
            pult.Set("b", 4);
            pult.Print("b");
        } 
        
        [Test]
        public void RemError() {
            pult.Rem("a");
        } 
        
        [Test]
        public void PrintError() {
            pult.Print("a");
        } 
        
        [Test]
        public void SubError() {
            pult.Sub("a", 12);
        }

        [Test]
        public void ExampleFunction() {
            pult.ExecuteLine("set code");
            pult.ExecuteLine("def test " +
                             "set a 5 " +
                             "sub a 3 " +
                             "print b " +
                             "set b 7 " +
                             "call test");
            pult.ExecuteLine("end code");
            pult.ExecuteLine("run");
            
        }
    }
}