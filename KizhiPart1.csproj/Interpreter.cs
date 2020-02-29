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
        Dictionary<string, int> dict = new Dictionary<string, int>(); // словарь для переменных
        List<string> codeBlocks = new List<string>();
        bool thisIsCodeBlock = false;

        private bool isRunning = false;


        public void PrintError() {
            WriteToStream("Переменная отсутствует в памяти");

        }
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
                PrintError();
            }
        }

        public void Set(string variableName, int variableValue) 
        {
            try {
                dict.Add(variableName, variableValue);
            }
            catch (ArgumentException e) {
                dict[variableName] = variableValue; 
            }
        }
        

        public void Rem(string variableName) 
        {

            if (!dict.Remove(variableName)) {
                PrintError();
            }
        }
        
        public void Sub(string variableName, int subValue) 
        {
            try {
                dict[variableName] -= subValue;
            }
            catch (KeyNotFoundException e) {
                PrintError();
                
            }
        }

        public void Def()
        {
            
        }

        public void RecursiveSet(string blob) {
            var parsedCommand = blob.Split(' ');

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
                case "def": {
                    break;   
                }
                case "call": {
                    break;
                }

                default: { 
                    break;
                }
            }
        }
        
        
        
        public void ExecuteLine(string command) {
            var parsedCommand = command.Split(' ');
            
            if (!isRunning) {
                if (parsedCommand[0] == "run") {
                    isRunning = true;
                }
                if (thisIsCodeBlock) {
                    codeBlocks.Add(command);
                } else {
                    switch (parsedCommand[0]) {
                        case "set": {
                            thisIsCodeBlock = true;
                            break;
                        }
                        case "end": {
                            thisIsCodeBlock = false;
                            break;
                        }
                    }
                }
                
            }
            else {
                RecursiveSet(codeBlocks[0]);
            }
        
        }
    }

}