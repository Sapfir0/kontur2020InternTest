using System;
using System.Configuration;
using System.IO;
using CommandLineCalculator.Tests;
using NUnit.Framework.Internal;


namespace CommandLineCalculator {
    class Program {
        static void Main(string[] args) {
            Console.WriteLine("Hello World!");
            var storage = new MemoryStorage();
            var console = new TextUserConsole(TextReader.Null, TextWriter.Null);
            var interpreter = new StatefulInterpreter();
            interpreter.Run(console, storage);
            
        }
    }
}
