using System.IO;
using NUnit.Framework;


namespace KizhiPart2 {
    [TestFixture]
    public class test {
        Interpreter pult = new Interpreter(new StringWriter());

        [SetUp]
        public void buildNewPult() {
            pult = new Interpreter(new StringWriter());
            pult.ExecuteLine("set code");
        }
        
        [TearDown]
        public void Run()
        {
            pult.ExecuteLine("end code");
            pult.ExecuteLine("run");
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
            pult.ExecuteLine("def test \n" +
                             "   set a 5 \n" +
                             "   sub a 3 \n" +
                             "   print b \n" +
                             "   set b 7 \n" +
                             "call test \n");
        }
        
        [Test]
        public void NotExistedFunction() {
            pult.ExecuteLine("call test \n");
        }

        [Test]
        public void InlineFunction()
        {
            pult.ExecuteLine("def test \n" +
                             "    rem a \n" +
                             "set a 12 \n" +
                             "call test \n" +
                             "print a \n");

        }
        
        [Test]
        public void Recursive()
        {
            pult.ExecuteLine("def test \n" +
                             "    print a \n" +
                             "    call test \n" +
                             "set a 321 \n" +
                             "call test \n");

        }
        
        [Test]
        public void NotEnteredRecursion()
        {
            pult.ExecuteLine("def test \n" +
                             "    print a \n" +
                             "    call test \n" +
                             "set a 321 \n" +
                             "print a \n");

        }

        [Test]
        public void CrossRecursion()
        {
            pult.ExecuteLine("def A \n" +
                             "    print a \n" +
                             "    call B \n" +
                             "def B \n" +
                             "    print b \n" +
                             "    call A \n" +
                             "set a 321 \n" +
                             "set b 000 \n" +
                             "call A \n");

        }
    }
}