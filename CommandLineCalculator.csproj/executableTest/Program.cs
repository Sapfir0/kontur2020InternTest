using System;
using System.IO;
using CommandLineCalculator.Tests;
using NUnit.Framework.Internal;


namespace CommandLineCalculator {
    class Program {
        static void Main(string[] args) {
            Console.WriteLine("Hello World!");
            var storage = new MemoryStorage();
            TestConsole console = new TestConsole();
            var interpreter = new StatefulInterpreter();
            interpreter.Run(console, storage);
            
        }
    }
}
