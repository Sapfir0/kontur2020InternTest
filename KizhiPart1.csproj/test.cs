using System.IO;
using NUnit.Framework;


namespace KizhiPart1 {
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
    }
}