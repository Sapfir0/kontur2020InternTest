using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace KizhiPart2
{
    public class Interpreter
    {
        private TextWriter _writer;

        public Interpreter(TextWriter writer)
        {
            _writer = writer;
        }


        public class VariableInfo
        {
            public readonly string Name;
            public int Value;

            public VariableInfo(string name, int? value=null)
            {
                Name = name;
                if (value != null)
                    this.Value = (int)value;
            }
        }

        public abstract class Command
        {
            protected readonly Dictionary<string, VariableInfo> Storage;
            protected readonly TextWriter Writer;
            private readonly string _commandName;

            public Command(ref TextWriter writer, ref Dictionary<string, VariableInfo> storage, string commandName)
            {
                Writer = writer;
                Storage = storage;
                _commandName = commandName;
            }

            public override string ToString()
            {
                return _commandName;
            }

            public virtual void Do() {}

            protected VariableInfo GetFromStorage(string variableName)
            {
                if (Storage.TryGetValue(variableName, out var ourVariable))
                {
                    return ourVariable;
                }
                throw new NotFoundException(Writer);
            }
        }


        private class Print : Command
        {
            private VariableInfo _variable;

            public Print(ref TextWriter writer, ref Dictionary<string, VariableInfo> storage, string variableName) 
                : base(ref writer, ref storage,  "print")
            {
                _variable = new VariableInfo(variableName);
            }

            public override void Do()
            {
                _variable = GetFromStorage(_variable.Name); // вау аутизм
                 Writer.WriteLine(_variable.Value);
            }
        }


        private class Set : Command
        {
            private readonly VariableInfo _variable;

            public Set(ref TextWriter writer, ref Dictionary<string, VariableInfo> storage, VariableInfo variable) 
                : base(ref writer, ref storage, "set")
            {
                this._variable = variable;
            }

            public override void Do()
            {
                if (!Storage.ContainsKey(_variable.Name))
                    Storage.Add(_variable.Name, _variable);
                else
                    Storage[_variable.Name] = _variable;
            }
        }

        private class Sub : Command
        {
            private VariableInfo _variable; //тут будет стейт до вызова этой комманды
            private readonly int _subValue;

            public Sub(ref TextWriter writer, ref Dictionary<string, VariableInfo> storage,  string variableName, int subValue) 
                : base(ref writer, ref storage,  "sub")
            {
                _variable = new VariableInfo(variableName);
                _subValue = subValue;
            }

            public override void Do()
            {
                _variable = GetFromStorage(_variable.Name);
                _variable.Value -= _subValue;
            }
        }

        private class Remove : Command
        {
            private VariableInfo _variable;

            public Remove(ref TextWriter writer, ref Dictionary<string, VariableInfo> storage,  string variableName) 
                : base(ref writer, ref storage,  "rem")
            {
                _variable = new VariableInfo(variableName);
            }

            public override void Do()
            {
                _variable = GetFromStorage(_variable.Name);
                Storage.Remove(_variable.Name);
            }
        }

        public class Call : Command
        {
            public string functionName;

            public Call(ref TextWriter writer,  ref Dictionary<string, VariableInfo> storage, string functionName) 
                : base(ref writer, ref storage,  "call")
            {
                this.functionName = functionName;
            }

            public override void Do()
            {
            }
        }


        public class Def : Command
        {
            public string functionName;
            
            public Def(ref TextWriter writer, ref Dictionary<string, LinkedList<Command>> functionList, ref Dictionary<string, VariableInfo> storage,  string functionName) 
                : base(ref writer, ref storage,  "def")
            {
                this.functionName = functionName;
                // неявно добавим ключик с именем функции в functionList
                functionList.Add(functionName, new LinkedList<Command>()); // для удобного контроля в других местах
            }

            public override void Do()
            {
            }
        }


        private Dictionary<string, VariableInfo> storage = new Dictionary<string, VariableInfo>();

        private Dictionary<string, LinkedList<Command>> functionList = new Dictionary<string, LinkedList<Command>>(); 
        //может быть сетом // тут будет название функции/команды внутри нее

        private LinkedList<Command> interpretComands = new LinkedList<Command>(); // просто команды которые будем запускать

        private bool isFunction;
        private Def currentFunction;
        
        public void AddCommandToMemory(string command)
        {
            Command currentCommand;

            var commandTrimmed = command.Trim().Split(' ');
            currentCommand = Switch(commandTrimmed);
            
            if (isFunction)
            {
                isFunction = command.Contains("    "); // важно, тут изначальная строка, а не порезанная
                if (isFunction)
                {
                    functionList[currentFunction.functionName].AddLast(currentCommand);
                }
                else
                {
                    interpretComands.AddLast(currentCommand);
                }
            }
            else if (currentCommand is Def def)
            {
                currentFunction = def;
            }
            else
            {
                interpretComands.AddLast(currentCommand);
                
            }
            
            if (currentCommand is Def)
                isFunction = true;
        }
        

        public void RunCommand(Command command)
        {
            try
            {
                command.Do();
            }
            catch (NotFoundException e)
            {
                PrintNotFound();
            }
        }

        private void PrintNotFound()
        {
            _writer.WriteLine("Переменная отсутствует в памяти");
        }
        

        public Command Switch(string[] parsedCommand)
        {
            Command currentCommand;
            var variableName = parsedCommand[1];


            switch (parsedCommand[0])
            {
                case "print":
                    currentCommand = new Print(ref _writer, ref storage, variableName);
                    break;
                case "set":
                    currentCommand = new Set(ref _writer, ref storage,  new VariableInfo(variableName, Int32.Parse(parsedCommand[2])));
                    break;
                case "sub":
                    currentCommand = new Sub(ref _writer, ref storage,  variableName,Int32.Parse(parsedCommand[2]));
                    break;
                case  "rem":
                    currentCommand = new Remove(ref _writer, ref storage,  variableName);
                    break;
                case "call":
                    currentCommand = new Call(ref _writer, ref storage,  variableName);
                    break;
                case "def":
                    currentCommand = new Def(ref _writer, ref functionList, ref storage,  parsedCommand[1]);                    
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

        private void RunCommands(LinkedList<Command> commands)
        {
            foreach (var command in commands)
            {
                RunCommand(command);
            }
        }
        
        public void RunFromMemory()
        {
            foreach (var interpretCommand in interpretComands)
            {
                if (interpretCommand is Call call)
                {
                    var functionCommands = functionList[call.functionName];

                    foreach (var functionCommand in functionCommands)
                    {
                        if (functionCommand is Call callingInnerFuinction) // да, поддерживаем только один уровень вложенности 
                        {
                            var innerFunction = functionList[callingInnerFuinction.functionName];
                            RunCommands(innerFunction);

                            if (callingInnerFuinction.functionName == call.functionName) // рекурсия(неполноценная)
                            {
                                while (true)
                                {
                                    RunCommands(innerFunction);
                                }
                            }
                        }
                        RunCommand(functionCommand);
                    }
                }
                else
                {
                    RunCommand(interpretCommand);
                }
                
            }
        }
        
        
        public void ExecuteLine(string command)
        {
            var commands = command.Split('\n');

            foreach (var cmd in commands) //первоначальная разметка кода
            {
                if (cmd != "end set code" && cmd != "set code" && cmd != "run")
                {
                    AddCommandToMemory(cmd);
                }
            }
            
            if (command != "run")
            {
                return;
            }

            RunFromMemory();

            
            storage.Clear();
            functionList.Clear();

        }
    }
}