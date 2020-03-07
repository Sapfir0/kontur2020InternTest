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
        Dictionary<string, List<string>> functionList = new Dictionary<string, List<string>>(); //может быть сетом // тут будет название функции/команды внутри нее
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
            
            bool functionStart = false;
            List<string> currentFunction = new List<string>();
            string nameOfFunction = "none";

            foreach (var command in commands)
            {
                if (functionStart)
                {
                    if (command.Contains("  "))
                    {
                        currentFunction.Add(command.TrimStart());
                    }
                    else
                    {
                        functionStart = false;
                        functionList.Add(nameOfFunction, currentFunction);
                        //currentFunction.Clear();
                    }
                }
                
                if (command.Contains("def"))
                {
                    nameOfFunction = command.Split(' ')[1];
                    functionStart = true;
                }
                else if (command.Contains("call")) //ели у нас вызов функции, то мы инлайним 
                {
                    if (!command.Contains("   ")) // вызов вне функций
                    {              
                        var nameOfCalledFunction = command.Split(' ')[1];
                        var calledFunc = GetFunctionCommandsByName(nameOfCalledFunction);
                        foreach (var functionCommands in calledFunc)
                        {
                            interpretComands.Add(functionCommands);
                        }
                    }
                }
                else if (!command.Contains("def") && !functionStart && command != "")
                {
                    interpretComands.Add(command);
                }
                
            }
        }


        private List<string> GetFunctionCommandsByName(string name)
        {
            if (functionList.TryGetValue(name, out List<string> commands))
            {
                return commands;
            }
            else
            {
                WriteNotFoundMessage();
                return null;
            }
        }

        public void ParseString(string blob) 
        {
            var parsedCommand = blob.Split(' ');
            
            if (parsedCommand[0] == "call") // в коде будет обязательно бесконечная рекурсия, если мы встертили ее тут
            {
                while(true)
                {
                    var funcCommands = GetFunctionCommandsByName(parsedCommand[1]);
                    foreach (var cmd in funcCommands)
                    {
                        var parsedCmd = cmd.Split(' ');

                        if (parsedCmd[0] != "call")
                        {
                            if (parsedCmd.Length > 1)
                            {
                                Switch(parsedCmd[0], parsedCmd[1], parsedCmd[2]);
                            }
                            else
                            {
                                Switch(parsedCmd[0], parsedCmd[1]);
                            }
                            // не добавляем в очередь а сразу запускаем
                        }
                    }

                }
            }
            else
            {
                if (parsedCommand.Length > 1)
                {
                    var value = parsedCommand[2];
                    Switch(parsedCommand[0], parsedCommand[1], value);
                }
                else
                {
                    Switch(parsedCommand[0], parsedCommand[1]);

                }
            }
            
        }

        public void Switch(string command, string variable, string value="0")
        {
            switch (command)
            {
                case "set": 
                {
                    Set(variable, Int32.Parse(value));
                    break;
                }
                case "sub": 
                {
                    Sub(variable, Int32.Parse(value));
                    break;
                }
                case "print": 
                {
                    Print(variable);
                    break;
                }
                case "rem": 
                {
                    Rem(variable);
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