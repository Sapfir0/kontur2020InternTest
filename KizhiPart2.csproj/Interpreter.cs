using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace KizhiPart2 {
    public class MainClass {
        static int Main() {
            return 0;
            
        }
    }
    
    
    public class Interpreter {
        private TextWriter _writer;
        Dictionary<string, int> storage = new Dictionary<string, int>(); // словарь для переменных
        List<Dictionary<string, List<string>>> functionList = new List<Dictionary<string, List<string>>>(); //может быть сетом // тут будет название функции/команды внутри нее
        List<string> interpretComands = new List<string>(); // просто команды которые будем запускать
        
        bool thisIsCodeBlock = false;

        private bool isRunning = false;


        private void WriteNotFoundMessage()
        {
            WriteToStream("Переменная отсутствует в памяти");
        }
        
        private void WriteToStream(string str) 
        {
            _writer.WriteLine(str);
            Console.WriteLine(str);
        }
        
        public Interpreter(TextWriter writer) 
        {
            _writer = writer;
        }

        public void Print(string variableName)
        {
            if(storage.TryGetValue(variableName, out var ourVariable))
            {
                WriteToStream(ourVariable.ToString());
            }
            else
            {
                WriteNotFoundMessage();
            }

        }

        public void Set(string variableName, int variableValue) 
        {
            try {
                storage.Add(variableName, variableValue);
            }
            catch (ArgumentException e) {
                storage[variableName] = variableValue; 
            }
        }
        

        public void Rem(string variableName) 
        {
            if (!storage.Remove(variableName)) 
            {
                WriteNotFoundMessage();
            }
        }
        
        public void Sub(string variableName, int subValue) 
        {
            try 
            {
                storage[variableName] -= subValue;
            }
            catch (KeyNotFoundException e) 
            {
                WriteNotFoundMessage();
            }
        }

        /*public void Def(string nameOfFunction, List<string> commands)
        {
            functionList.Add(nameOfFunction, commands);
        }*/

        /*public void Call(string nameOfFunction)
        {
            if(functionList.TryGetValue(nameOfFunction, out var ourVariable))
            {
                
            }
            else
            {
                WriteNotFoundMessage(); //такой функции мы не знаем 
            }
        }*/

        public void Parse(string code)
        {
            var commands = code.Split('\n').ToList();
            
            bool functionStart = false;
            Dictionary<string, List<string>> currentFunction = new Dictionary<string, List<string>>();
            string nameOfFunction = "None";
            
            foreach (var command in commands)
            {
                if (functionStart)
                {
                    if (command.Contains("  "))
                    {
                        currentFunction[nameOfFunction].Add(command);
                    }
                    else
                    {
                        functionStart = false;
                        functionList.Add(currentFunction);
                        currentFunction.Clear();
                    }
                }
                
                if (command.Contains("def"))
                {
                    nameOfFunction = command.Split(' ')[1];
                    functionStart = true;
                    currentFunction.Add(nameOfFunction, null);
                }
                

                if (command.Contains("call")) //ели у нас вызов функции, то мы инлайним  
                {
                    var nameOfCalledFunction = command.Split(' ')[1];
                    var calledFunc = GetFunctionCommandsByName(nameOfCalledFunction);
                    foreach (var functionCommands in calledFunc)
                    {
                        interpretComands.Add(functionCommands);
                    }
                }
                else if (!command.Contains("def"))
                {
                    interpretComands.Add(command);
                }
                
            }
        }

        public List<string> GetFunctionCommandsByName(string name)
        {
            foreach (var function in functionList.Where(function => function.First().Key == name))
            {
                return function[name];
            }
            return new List<string>(null);
        }

        public void ParseString(string blob) {
            var parsedCommand = blob.Split(' ');

            switch (parsedCommand[0]) 
            {
                case "set": 
                {
                    Set(parsedCommand[1], Int32.Parse(parsedCommand[2]));
                    break;
                }
                case "sub": 
                {
                    Sub(parsedCommand[1], Int32.Parse(parsedCommand[2]));
                    break;
                }
                case "print": 
                {
                    Print(parsedCommand[1]);
                    break;
                }
                case "rem": 
                {
                    Rem(parsedCommand[1]);
                    break;
                }
                case "def":
                {
                    
                    ///Def(parsedCommand[1], listok);
                    break;   
                }
                case "call": {
                    //Call(parsedCommand[1]);
                    break;
                }

                default: { 
                    break;
                }
            }
        }
        
        
        
        public void ExecuteLine(string command) {
            var parsedCommand = command.Split(' ');
            
            if (thisIsCodeBlock) {
                if (parsedCommand[0] == "end")
                {
                    thisIsCodeBlock = false;
                }
                else
                {
                    Parse(command);
                }
            } else {
                if (parsedCommand[0] =="set")
                {
                    thisIsCodeBlock = true;
                }
            }

            
            if (parsedCommand[0] == "run") {
                //начинаем интерпреатцию
                for (int i = 0; i < interpretComands.Count; i++)
                {
                    ParseString(interpretComands[i]);

                }
            }
        
        }
    }

}