using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;


namespace KizhiPart2
{
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

    public class Interpreter
    {
        public TextWriter _writer;

        public Interpreter(TextWriter writer)
        {
            _writer = writer;
        }


        public class VariableInfo
        {
            public string Name;
            public int Value;
            public int LastChangeInString;

            public VariableInfo(string name, int? value=null, int? lastChangeInString=null)
            {
                Name = name;
                if (value != null)
                    Value = (int)value;
                if (lastChangeInString != null)
                    LastChangeInString = (int)lastChangeInString;
            }
        }

        public abstract class Command
        {
            public int RealLine;
            public Dictionary<string, VariableInfo> storage;
            public TextWriter writer;
            public Command(ref TextWriter writer, ref Dictionary<string, VariableInfo> storage, int realLine)
            {
                this.writer = writer;
                this.storage = storage;
                RealLine = realLine;
            }

            public virtual void Do() {}
            public VariableInfo GetFromStorage(string variableName)
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
            public VariableInfo Variable;
            private TextWriter _writer;
            public Print(ref TextWriter writer, ref Dictionary<string, VariableInfo> storage, string variableName, int realLine) 
                : base(ref writer, ref storage, realLine)
            {
                _writer = writer;
                Variable = new VariableInfo(variableName);
            }

            public override void Do()
            {
                Variable = GetFromStorage(Variable.Name); // вау аутизм
                _writer.WriteLine(Variable.Value);
                Console.WriteLine(Variable.Value);
            }
        }


        public class Set : Command
        {
            public VariableInfo Variable;

            public Set(ref TextWriter writer, ref Dictionary<string, VariableInfo> storage, int realLine, VariableInfo variable) 
                : base(ref writer, ref storage, realLine)
            {
                Variable = variable;
                Variable.LastChangeInString = realLine;
            }

            public override void Do()
            {
                if (!storage.ContainsKey(Variable.Name))
                    storage.Add(Variable.Name, Variable);
    
            }
        }

        public class Sub : Command
        {
            public VariableInfo Variable; //тут будет стейт до вызова этой комманды
            public int SubValue;

            public Sub(ref TextWriter writer, ref Dictionary<string, VariableInfo> storage, int realLine, string variableName, int subValue) 
                : base(ref writer, ref storage, realLine)
            {
                Variable = new VariableInfo(variableName) {LastChangeInString = realLine};

                SubValue = subValue;
            }

            public override void Do()
            {
                var lastChanged = Variable.LastChangeInString;
                Variable = GetFromStorage(Variable.Name);
                Variable.LastChangeInString = lastChanged;
                Variable.Value -= SubValue;
            }
        }

        public class Remove : Command
        {
            public VariableInfo Variable;

            public Remove(ref TextWriter writer, ref Dictionary<string, VariableInfo> storage, int realLine, string variableName) 
                : base(ref writer, ref storage, realLine)
            {
                Variable = new VariableInfo(variableName) {LastChangeInString = realLine};

            }

            public override void Do()
            {
                var lastChanged = Variable.LastChangeInString;
                Variable = GetFromStorage(Variable.Name);
                Variable.LastChangeInString = lastChanged;

                storage.Remove(Variable.Name);
                

            }
        }

        public class Call : Command
        {
            public LinkedList<Command> Function;
            public string functionName;

            public Call(ref TextWriter writer,  ref Dictionary<string, VariableInfo> storage, int realLine, string functionName) 
                : base(ref writer, ref storage, realLine)
            {
                this.functionName = functionName;
            }

            public override void Do()
            {
            }
        }


        public class Def : Command
        {
            public LinkedList<Command> Function;
            public Dictionary<string, LinkedList<Command>> functionList;
            public string functionName;
            
            public Def(ref TextWriter writer, ref Dictionary<string, LinkedList<Command>> functionList, ref Dictionary<string, VariableInfo> storage, int realLine, string functionName) 
                : base(ref writer, ref storage, realLine)
            {
                Function = new LinkedList<Command>();
                this.functionList = functionList;
                this.functionName = functionName;
                // TODO неявно добавим ключик с именем функции в functionList
                functionList.Add(functionName, new LinkedList<Command>()); // для удобного контроля в других местах
            }

            public override void Do()
            {
                foreach (var command in Function)
                {
                    command.Do();
                }
            }
        }
        



        public Dictionary<string, VariableInfo> storage = new Dictionary<string, VariableInfo>();

        public Dictionary<string, LinkedList<Command>> functionList = new Dictionary<string, LinkedList<Command>>(); 
        //может быть сетом // тут будет название функции/команды внутри нее

        public LinkedList<Command> interpretComands = new LinkedList<Command>(); // просто команды которые будем запускать

        private bool isRunning = false;

        public bool isFunction = false;
        public Def previousFunction;

        public void AddCommandToMemory(string command, int line)
        {
            Command currentCommand;

            var commandTrimmed = command.Trim().Split(' ');
            currentCommand = Switch(commandTrimmed, line);

            if (currentCommand is Call)
            {
                var mycall = ((Call) currentCommand).functionName;
                var func = functionList[mycall];
                foreach (var f in func)
                {
                    interpretComands.AddLast(f);

                }
            }
            
            if (isFunction)
            {
                isFunction = command.Contains("    "); // важно, тут изначальная строка, а не порезанная
                if (isFunction)
                {
                    previousFunction.Function.AddLast(currentCommand);
                }
                else
                {
                    interpretComands.AddLast(currentCommand);
                }
            }
            else if (currentCommand is Def def)
            {
                previousFunction = def;
            }
            else
            {
                interpretComands.AddLast(currentCommand);
                
            }

            if (currentCommand is Def)
                isFunction = true;

        }

        public void StartRunCommands(Command blob)
        {
            try
            {
                blob.Do();
            }
            catch (NotFoundException e)
            {
                _writer.WriteLine("Переменная отсутствует в памяти");
                Console.WriteLine("Переменная отсутствует в памяти");
            }
        }

        public Command Switch(string[] parsedCommand, int line)
        {
            Command currentCommand;
            var variableName = parsedCommand[1];


            switch (parsedCommand[0])
            {
                case "print":
                    currentCommand = new Print(ref _writer, ref storage, variableName, line);
                    break;
                case "set":
                    currentCommand = new Set(ref _writer, ref storage, line, new VariableInfo(variableName, Int32.Parse(parsedCommand[2]), line));
                    break;
                case "sub":
                    currentCommand = new Sub(ref _writer, ref storage, line, variableName,Int32.Parse(parsedCommand[2]));
                    break;
                case  "rem":
                    currentCommand = new Remove(ref _writer, ref storage, line, variableName);
                    break;
                case "call":
                    currentCommand = new Call(ref _writer, ref storage, line, variableName);
                    break;
                case "def":
                    currentCommand = new Def(ref _writer, ref functionList, ref storage, line, parsedCommand[1]);                    
                    break;
                default:
                    throw new Exception("Не найдена комманда");
            }

            return currentCommand;
        }

        public class NotFoundException : Exception
        {
            private TextWriter _writer;
            public NotFoundException(TextWriter writer)
            {
                _writer = writer;
            } 
        }

        public void ExecuteLine(string command)
        {
            var commands = command.Split('\n');
            var line = 0;

            foreach (var cmd in commands)
            {
                if (cmd != "end set code" && cmd != "set code" && cmd != "run")
                {
                    AddCommandToMemory(cmd, line);
                }
                else if (cmd == "run")
                {
                    //начинаем интерпреатцию
                    foreach (var interpretComand in interpretComands)
                    {
                        StartRunCommands(interpretComand);
                    }
                }
                line++;

            }
            
        }
    }
}