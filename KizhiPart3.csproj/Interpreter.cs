using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;


namespace KizhiPart3 {
    public class MainClass {
        static int Main() {
            Debugger pult = new Debugger(new StringWriter());
            while (true)
            {
                pult.ExecuteLine(Console.ReadLine());
            }
            return 0;
            
        }
    }
    
            
    public class VariableInfo
    {
        public int Value;
        public int LastChangeInString;

        public VariableInfo(int value, int lastChangeInString)
        {
            Value = value;
            LastChangeInString = lastChangeInString;
        }
    }
    public abstract class Command
    {
        public string Name;
        public int RealLine;
        
    }
    

    public class Print : Command
    {
        public VariableInfo Variable;
    }
        
        
    public class Set : Command
    {
        public VariableInfo Variable;
    }

    public class Sub : Command
    {
        public VariableInfo Variable;
        public int SubValue;
        
        
    }
        
    public class Remove : Command
    {
        public VariableInfo Variable;

    }
    
    public class Debugger
    {
        public TextWriter _writer;

        public Debugger(TextWriter writer)
        {
            _writer = writer;
        }


        public Dictionary<string, VariableInfo> storage = new Dictionary<string, VariableInfo>(); // словарь для переменных
        public Dictionary<string, List<CommandInfo>> functionList = new Dictionary<string, List<CommandInfo>>(); //может быть сетом // тут будет название функции/команды внутри нее
        public List<CommandInfo> interpretComands = new List<CommandInfo>(); // просто команды которые будем запускать
        
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


        public void AddCommandToMemory(string code)
        {
            var commands = code.Split('\n').ToList();
            var parsedCommands = new List<CommandInfo>();
            for (int i=0; i<commands.Count; i++)
            {
                
                parsedCommands.Add(new CommandInfo(commands[i], i));
            }
            
            bool functionStart = false;
            List<CommandInfo> currentFunction = new List<CommandInfo>();
            string nameOfFunction = "none";

            foreach (var command in parsedCommands)
            {
                if (functionStart)
                {
                    if (command.Command.Contains("   "))
                    {
                        var item = new CommandInfo(command.Command.Trim(), command.RealLine);
                        currentFunction.Add(item);
                    }
                    else
                    {
                        functionStart = false;
                        functionList.Add(nameOfFunction, currentFunction);
                    }
                }
                
                if (command.Command.Contains("def"))
                {
                    nameOfFunction = command.Command.Split(' ')[1];
                    functionStart = true;
                }
                else if (command.Command.Contains("call")) //ели у нас вызов функции, то мы инлайним 
                {
                    if (!command.Command.Contains("   ")) // вызов вне функций
                    {              
                        var nameOfCalledFunction = command.Command.Split(' ')[1];
                        var calledFunc = GetFunctionCommandsByName(nameOfCalledFunction);
                        if (calledFunc != null)
                        {
                            foreach (var functionCommands in calledFunc)
                            {
                                interpretComands.Add(functionCommands);
                            }
                        }
                 
                    }
                }
                else if (!command.Command.Contains("def") && !functionStart && command.Command != "")
                {
                    interpretComands.Add(command);
                }
                
            }
        }


        private List<CommandInfo> GetFunctionCommandsByName(string name)
        {
            if (functionList.TryGetValue(name, out List<CommandInfo> commands))
            {
                return commands;
            }
            else
            {
                WriteNotFoundMessage();
                return null;
            }
        }

        public void StartRunCommands(CommandInfo blob) 
        {
            var parsedCommand = blob.Command.Split(' ');
            
            if (parsedCommand[0] == "call") // в коде будет обязательно бесконечная рекурсия, если мы встертили ее тут
            {
                while(true)
                {
                    var funcCommands = GetFunctionCommandsByName(parsedCommand[1]);
                    foreach (var cmd in funcCommands)
                    {
                        var parsedCmd = cmd.Command.Split(' ');

                        if (parsedCmd[0] != "call")
                        {
                            if (parsedCmd.Length == 2)
                            {
                                Switch(parsedCmd[0]);
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
                    var value = parsedCommand[2];
                    Switch(parsedCommand[0], parsedCommand[1], value);
                }
                else if (parsedCommand.Length == 2)
                {
                    Switch(parsedCommand[0], parsedCommand[1]);
                }
            }
            
        }

        public void Switch(CommandInfo command, string variable, string value="0")
        {
            switch (command.Command)
            {
                case "set": 
                {
                    try {
                        storage.Add(command.Command, new VariableInfo(variableValue, command.RealLine));
                    }
                    catch (ArgumentException e) {
                        storage[command.Command].Value = variableValue; 
                    }
                    break;
                }
                case "sub": 
                {
                    try 
                    {
                        storage[variableName.Command].Value -= subValue;
                    }
                    catch (KeyNotFoundException e) 
                    {
                        WriteNotFoundMessage();
                    }
                    break;
                }
                case "print": 
                {
                    if(storage.TryGetValue(variableName.Command, out var ourVariable))
                    {
                        WriteToStream(ourVariable.ToString());
                    }
                    else
                    {
                        WriteNotFoundMessage();
                    }
                    break;
                }
                case "rem": 
                {
                    if (!storage.Remove(variableName.Command)) 
                    {
                        WriteNotFoundMessage();
                    }
                    break;
                }
            }
        }

        public void AddBreak()
        {
            
        }

        public void StepOver()
        {
            
        }

        public void Step()
        {
            
        }

        public void PrintMem()
        {
            foreach (var variable in storage)
            {
                WriteToStream($"{variable.Key} {variable.Value.Value} {variable.Value.LastChangeInString}");
            }

            
        }

        public void PrintTrace()
        {
            
        }

        public void ExecuteLine(string command) 
        {
            if (command != "end set code" && command != "set code" && command != "run")
            {
                AddCommandToMemory(command);
            }
            else if (command == "run") 
            {
                //начинаем интерпреатцию
                foreach (var interpretComand in interpretComands)
                {
                    StartRunCommands(interpretComand);
                }
            }
        }
    }

}