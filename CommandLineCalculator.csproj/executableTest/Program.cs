using System;
using System.IO;


namespace CommandLineCalculator {
    class Program {
        static void Main(string[] args) {
            Console.WriteLine("Hello World!");
            var intepret = new StatefulInterpreter();
            intepret.Run(new TextUserConsole(TextReader.Null, TextWriter.Null), new FileStorage("./log.txt"));
            
            
        }
    }
}
