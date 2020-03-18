using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;


namespace KizhiPart3
{
    public class MainClass
    {
        static int Main()
        {
            Debugger pult = new Debugger(new StringWriter());
            while (true)
            {
                pult.ExecuteLine(Console.ReadLine());
            }

            return 0;
        }
    }


    public class Debugger
    {
        public TextWriter _writer;

        public Debugger(TextWriter writer)
        {
            _writer = writer;
        }


        public class VariableInfo
        {
            public string Name;
            public int Value;
            public int LastChangeInString;

            public VariableInfo(string name, int value, int? lastChangeInString=null)
            {
                Name = name;
                Value = value;
                if (lastChangeInString != null)
                    LastChangeInString = (int)lastChangeInString;
            }
        }

        public abstract class Command
        {
            public int RealLine;

            public Command(int realLine)
            {
                RealLine = realLine;
            }
        }


        public class Print : Command
        {
            public VariableInfo Variable;

            public Print(string variableName, int realLine) : base(realLine)
            {
                // get variable from storage
            }

            public void Do()
            {
                if (storage.TryGetValue(Variable.Name, out var ourVariable))
                {
                    WriteToStream(ourVariable.ToString());
                }
                else
                {
                    WriteNotFoundMessage();
                }
            }
        }


        public class Set : Command
        {
            public VariableInfo Variable;

            public Set(int realLine, VariableInfo variable) : base(realLine)
            {
                Variable = variable;
            }

            public void Do()
            {
                try
                {
                    storage.Add(Variable.Name, new VariableInfo(Variable.Value, Variable.LastChangeInString));
                }
                catch (ArgumentException e)
                {
                    storage[command.Command].Value = variableValue;
                }
            }
        }

        public class Sub : Command
        {
            public VariableInfo Variable;
            public int SubValue;

            public Sub(int realLine, VariableInfo variable, int subValue) : base(realLine)
            {
                Variable = variable;
                SubValue = subValue;
            }

            public void Do()
            {
                try
                {
                    storage[variableName.Command].Value -= subValue;
                }
                catch (KeyNotFoundException e)
                {
                    WriteNotFoundMessage();
                }
            }
        }

        public class Remove : Command
        {
            public VariableInfo Variable;

            public Remove(int realLine, VariableInfo variable) : base(realLine)
            {
                Variable = variable;
            }

            public void Do()
            {
                if (!storage.Remove(variableName.Command))
                {
                    WriteNotFoundMessage();
                }
            }
        }

        public List<VariableInfo> storage = new List<VariableInfo>();

        public Dictionary<string, List<Command>>
            functionList = new Dictionary<string, List<Command>>(); //может быть сетом // тут будет название функции/команды внутри нее

        public List<Command> interpretComands = new List<Command>(); // просто команды которые будем запускать

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
            bool functionStart = false;
            List<Command> currentFunction = new List<Command>();

            string nameOfFunction = "none";
            int line = 0;

            foreach (var command in commands)
            {
                var parsedCommand = command.Split(' ');
                if (functionStart)
                {
                    if (command.Contains("   "))
                    {
                        // тут должен быть свитч кейс
                        currentFunction.Add(new Command(parsedCommand[0]));
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
                        if (calledFunc != null)
                        {
                            foreach (var functionCommands in calledFunc)
                            {
                                interpretComands.Add(functionCommands);
                            }
                        }
                    }
                }
                else if (!command.Contains("def") && !functionStart && command!= "")
                {
                    Command currentCommand;
                    switch (parsedCommand[0])
                    {
                        case "print":
                            currentCommand = new Print(parsedCommand[1], line);
                            break;
                        case "set":
                            currentCommand = new Set(line, new VariableInfo(parsedCommand[1], Int32.Parse(parsedCommand[2]), line));
                            break;
                        case "sub":
                            currentCommand = new Sub(line, new VariableInfo(parsedCommand[1]), Int32.Parse(parsedCommand[2]));

                    }
                    interpretComands.Add();
                }

                line++;
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
                while (true)
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

        public void Switch(CommandInfo command, string variable, string value = "0")
        {
            switch (command.Command)
            {
                case "set":
                {
                    break;
                }
                case "sub":
                {
                    break;
                }
                case "print":
                {
                    break;
                }
                case "rem":
                {
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