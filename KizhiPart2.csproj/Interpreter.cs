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


        public void ParseStringToDictionary(string code)
        {
            var commands = code.Split('\n').ToList();
            
            bool isFunction = false;
            List<string> currentFunction = new List<string>();
            string nameOfFunction = "none";
            var metaFunc = new Dictionary<string, List<string>>();

            bool isRecursion = false;
            foreach (var command in commands)
            {
                if (isFunction)
                {
                    if (command.Contains("  ") )
                    {
                        if (command.Contains("call")) //пытаемся избежать рекурсии
                        {
                            isRecursion = true;
                        }
                        else
                        {
                            currentFunction.Add(command.TrimStart());

                        }
                    }
                    else
                    {
                        isFunction = false;
                        metaFunc.Add(nameOfFunction, currentFunction);
                        functionList.Add(metaFunc);
                        //currentFunction.Clear(); // странно, похоже, что тесты не предполагают наличие нескольких функций
                        //metaFunc.Clear();
                    }
                }
                
                if (command.Contains("def"))
                {
                    nameOfFunction = command.Split(' ')[1];
                    isFunction = true;
                }
                
                else if (command.Contains("call")) //еcли у нас вызов функции, то мы инлайним 
                {
                    var nameOfCalledFunction = command.Split(' ')[1];
                    List<string> calledFunc;
                    
                    if (command.Contains("   ")) // если вызов был рекурсивным
                    {
                        isRecursion = true;
                        isFunction = false;
                    }
                    
                    calledFunc = GetFunctionCommandsByName(nameOfCalledFunction);

                    foreach (var functionCommands in calledFunc)
                    {
                        interpretComands.Add(functionCommands);
                    }
                }
                else if (!isFunction) // если это не функция
                {
                    interpretComands.Add(command);
                }
                
            }
        }

        public void RecursionProcessing()
        {
            metaFunc.Add(nameOfFunction, currentFunction);
            functionList.Add(metaFunc);
            for (int i = 0; i < 99999; i++)
            {
                nameOfCalledFunction = command.Trim().Split(' ')[1];
                calledFunc = GetFunctionCommandsByName(nameOfCalledFunction);
                foreach (var functionCommands in calledFunc)
                {
                    interpretComands.Add(functionCommands);
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

        public void ParseString(string blob) 
        {
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
                default: { 
                    break;
                }
            }
        }
        
        
        public void ExecuteLine(string command) 
        {
            var parsedCommand = command.Split(' ');
            
            if (thisIsCodeBlock) 
            {
                if (parsedCommand[0] == "end")
                {
                    thisIsCodeBlock = false;
                }
                else
                {
                    ParseStringToDictionary(command);
                }
            } 
            else 
            {
                if (parsedCommand[0] =="set")
                {
                    thisIsCodeBlock = true;
                }
            }

            
            if (parsedCommand[0] == "run") 
            {
                //начинаем интерпреатцию
                for (int i = 0; i < interpretComands.Count; i++)
                {
                    ParseString(interpretComands[i]);

                }
            }
        
        }
    }

}