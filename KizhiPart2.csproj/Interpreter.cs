using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace KizhiPart2 {

    public class Interpreter {
        public class MainClass
        {
            static int Main()
            {
                Interpreter pult = new Interpreter(new StringWriter());
                while (true)
                {
                    pult.ExecuteLine(Console.ReadLine());
                }

            }
        }
        
        private TextWriter _writer;
        Dictionary<string, int> storage = new Dictionary<string, int>(); // словарь для переменных
        Dictionary<string, List<string>> functionList = new Dictionary<string, List<string>>(); //может быть сетом // тут будет название функции/команды внутри нее
        public List<string> interpretComands = new List<string>(); // просто команды которые будем запускать
        
        bool thisIsCodeBlock = false;

        private bool isRunning = false;

        
        public Interpreter(TextWriter writer) 
        {
            _writer = writer;
        }

        public class NotFoundException : Exception
        {
            private TextWriter _writer;
            public NotFoundException(TextWriter writer)
            {
                _writer = writer;
            } 
        }
        
        
        public abstract class Command
        {
            public Dictionary<string, int> storage;
            public TextWriter writer;
            public string commandName;
            public Command(ref TextWriter writer, ref Dictionary<string, int> storage, string commandName)
            {
                this.writer = writer;
                this.storage = storage;
                this.commandName = commandName;
            }

            public override string ToString()
            {
                return commandName;
            }

            public virtual void Do() {}
            public int GetFromStorage(string variableName)
            {
                if (storage.TryGetValue(variableName, out var ourVariable))
                {
                    return ourVariable;
                }
                throw new NotFoundException(writer);
            }
        }


        public class Print : Command
        {
            public string Variable;
            private TextWriter _writer;
            public Print(ref TextWriter writer, ref Dictionary<string, int> storage, string variableName) 
                : base(ref writer, ref storage, "print")
            {
                _writer = writer;
                Variable = variableName;
            }

            public override void Do()
            {
                var variableValue = GetFromStorage(Variable); 
                 _writer.WriteLine(variableValue);
                Console.WriteLine(variableValue);
            }
        }


        public class Set : Command
        {
            public string variableName;
            public int variableValue;

            public Set(ref TextWriter writer, ref Dictionary<string, int> storage, string variableName, int variableValue) 
                : base(ref writer, ref storage, "set")
            {
                this.variableName = variableName;
                this.variableValue = variableValue;
            }

            public override void Do()
            {
                if (!storage.ContainsKey(variableName))
                    storage.Add(variableName, variableValue);
                else
                    storage[variableName] = variableValue;
            }
        }

        public class Sub : Command
        {
            public string Variable; //тут будет стейт до вызова этой комманды
            public int SubValue;

            public Sub(ref TextWriter writer, ref Dictionary<string, int> storage, string variableName, int subValue) 
                : base(ref writer, ref storage, "sub")
            {
                Variable = variableName;
                SubValue = subValue;
            }

            public override void Do()
            {
                storage[Variable] -= SubValue;
            }
        }

        public class Remove : Command
        {
            public string Variable;

            public Remove(ref TextWriter writer, ref Dictionary<string, int> storage, string variableName) 
                : base(ref writer, ref storage, "rem")
            {
                Variable = variableName;

            }

            public override void Do()
            {
                storage.Remove(Variable);
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
                //WriteNotFoundMessage();
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
                            if (parsedCmd.Length == 3)
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
                if (parsedCommand.Length == 3)
                {
                    Switch(parsedCommand[0], parsedCommand[1], parsedCommand[2]);
                }
                else
                {
                    Switch(parsedCommand[0], parsedCommand[1]);

                }
            }
            
        }

        public void Switch(string command, string variable, string value="0")
        {
            Command currentCommand;
            switch (command)
            {
                case "set":
                {
                    currentCommand = new Set(ref _writer, ref storage, variable, Int32.Parse(value));
                    break;
                }
                case "sub":
                {
                    currentCommand = new Sub(ref _writer, ref storage, variable, Int32.Parse(value));
                    break;
                }
                case "print":
                {
                    currentCommand = new Print(ref _writer, ref storage, variable);
                    break;
                }
                case "rem":
                {
                    currentCommand = new Remove(ref _writer, ref storage, variable);
                    break;
                }
                default:
                    throw new SuccessException("Успешный провал");
                    break;
            }

            try
            {
                currentCommand.Do();

            }
            catch (NotFoundException e)
            {
                _writer.WriteLine("Переменная отсутствует в памяти");
                Console.WriteLine("Переменная отсутствует в памяти");
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