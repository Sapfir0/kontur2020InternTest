using System.IO;
using System.Linq;
using NUnit.Framework;


using KizhiPart3;

namespace KizhiPart3 {
    [TestFixture]
    public class MediumTest {
        Debugger pult = new Debugger(new StringWriter());
        
        [SetUp]
        public void buildNewPult() {
            pult = new Debugger(new StringWriter());
        }

        [Test]
        public void Example3()
        {
            pult.ExecuteLine("set code");
            pult.ExecuteLine( "def test\n" +
                              "    set a 5\n" +
                              "    sub a 3\n" +
                              "    print b\n" +
                              "call test"
            );
            pult.ExecuteLine("end set code");
            pult.ExecuteLine( "add break 2\n" +
                              "run\n" +
                              "print mem");

        }

        [Test]
        public void ExampleFunction() {
            pult.ExecuteLine("set code");
            pult.ExecuteLine("def test\n" +
                             "    set a 5\n" +
                             "    sub a 3\n" +
                             "    print b\n" +
                             "set b 7\n" +
                             "call test\n" +
                             "print a");
            pult.ExecuteLine("end set code");
            pult.ExecuteLine("run");

            
        }
        
        /*[Test]
        public void NotExistedFunction() {
            pult.ExecuteLine("set code\n" +
                             "call test\n" +
                             "end set code\n" +
                             "run");
  
        }*/

        [Test]
        public void ShouldCallFunctionAndOutputNotFound()
        {
            pult.ExecuteLine("set code\n" +
                             "def test\n" +
                             "     rem a\n" +
                             "set a 12\n" +
                             "call test\n" +
                             "print a\n" +
                             "end set code\n" +
                             "run");
            
        }
        
        [Test]
        public void ShouldClearMemoryAfterInterpretation()
        {
            pult.ExecuteLine("set code");
            pult.ExecuteLine("print a\nset a 5");
            pult.ExecuteLine("print a\nset a 5");
            pult.ExecuteLine("end set code");
            pult.ExecuteLine("run");
            
        }


        [Test]
        public void CallABeforeDefine()
        {
            
            pult.ExecuteLine(
                "set code\n" +
                        "print a\n" +
                        "call A\n" +
                        "def A\n" +
                        "    set a 12\n" +
                        "    sub a 1\n" +
                        "    rem a\n" +
                        "print a\n" +
                        "call A\n" +
                        "print a\n" +
                        "end set code\n" +
                        "add break 3"
                );

        }
        
        [Test]
        public void CallA()
        {
            pult.ExecuteLine("set code\n" +
                             "print a\n" +
                             "def A\n" +
                             "    set a 12\n" +
                             "    sub a 1\n" +
                             "    rem a\n" +
                             "call A\n" +                             
                             "print a\n" +
                             "end set code\n" +
                             "run"
            );

        }



        [Test]
        public void CrossFunctions()
        {            
            pult.ExecuteLine("set code");
            pult.ExecuteLine(
                             "def A\n" +
                             "    print a\n" +
                             "    set a 2\n" +
                             "    call B\n" +
                             "set a 13\n" +
                             "call A\n" +
                             "def B\n" +
                             "    print a"
            );
            // set | print | set | print
            pult.ExecuteLine("end set code");
            pult.ExecuteLine("run");
            // 13 2
        }
        

        
        [Test]
        public void NotEnteredRecursion()
        {
            pult.ExecuteLine("set code\n" +
                             "def test \n" +
                             "    print a \n" +
                             "    call test \n" +
                             "set a 321 \n" +
                             "print a \n" +
                             "end set code\n" +
                             "run");

        }
        
        /*[Test]
        public void Recursive()
        {
            pult.ExecuteLine("set code");
            pult.ExecuteLine(
                "def test \n" +
                "    print a\n" +
                "    call test\n" +
                "set a 321\n" +
                "call test");
            pult.ExecuteLine("end set code");
            pult.ExecuteLine("run");
        }*/
        /*
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

        }*/
    }
}