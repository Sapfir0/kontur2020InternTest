using System;
using System.Collections.Generic;
using System.IO;

namespace KizhiPart1 {
    public class MainClass {
        static int Main() {
            new test();
            return 0;
            
        }
    }
    
    public class Interpreter {
        private TextWriter _writer;
        Dictionary<string, int> dict = new Dictionary<string, int>();

        private void WriteToStream(string str) {
            _writer.WriteLine(str);
            Console.WriteLine(str);
        }
        
        public Interpreter(TextWriter writer) {
            _writer = writer;
        }

        public void Print(string variableName) {
            try {
                WriteToStream(dict[variableName].ToString());
            }
            catch (KeyNotFoundException e) {
                WriteToStream("Переменная отсутствует в памяти");
            }
        }

        public void Set(string variableName, int variableValue) {
            try {
                dict.Add(variableName, variableValue);
            }
            catch (ArgumentException e) {
                dict[variableName] = variableValue; 
            }
        }
        

        public void Rem(string variableName) {

            if (!dict.Remove(variableName)) {
                WriteToStream("Переменная отсутствует в памяти");
            }
        }
        
        public void Sub(string variableName, int subValue) {
            try {
                dict[variableName] -= subValue;
            }
            catch (KeyNotFoundException e) {
                WriteToStream("Переменная отсутствует в памяти");
            }
        }
        
        public void ExecuteLine(string command) {
            var parsedCommand = command.Split(' ');
            switch (parsedCommand[0]) {
                case "set": {
                    Set(parsedCommand[1], Int32.Parse(parsedCommand[2]));
                    break;

                }
                case "sub": {
                    Sub(parsedCommand[1], Int32.Parse(parsedCommand[2]));

                    break;
                }
                case "print": {
                    Print(parsedCommand[1]);

                    break;
                }
                case "rem": {
                    Rem(parsedCommand[1]);
                    break;
                }
            }
        }
    }

}