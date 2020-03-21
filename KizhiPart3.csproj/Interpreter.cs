using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace KizhiPart3
{
    public class Debugger
    {
        private TextWriter _writer;
        private Dictionary<string, VariableInfo> _storage = new Dictionary<string, VariableInfo>();

        private Dictionary<string, LinkedList<Command>> _functionList = new Dictionary<string, LinkedList<Command>>(); 
        //может быть сетом // тут будет название функции/команды внутри нее

        private readonly LinkedList<Command> _interpretCommands = new LinkedList<Command>(); // просто команды которые будем запускать

        private bool _isFunction;
        private Def _currentFunction;


        public Debugger(TextWriter writer)
        {
            _writer = writer;
        }


        public class VariableInfo
        {
            public readonly string Name;
            public int Value;
            public int LastChangedLine;

            public VariableInfo(string name, int? value=null, int? lastChangedLine=null)
            {
                Name = name;
                if (value != null)
                    Value = (int)value;
                if (lastChangedLine != null)
                    LastChangedLine = (int)lastChangedLine;
            }
        }

        public abstract class Command
        {
            protected readonly Dictionary<string, VariableInfo> Storage;
            protected readonly TextWriter Writer;
            private readonly string _commandName;
            protected int RealLine;

            public Command(ref TextWriter writer, ref Dictionary<string, VariableInfo> storage, int realLine, string commandName)
            {
                Writer = writer;
                Storage = storage;
                _commandName = commandName;
                RealLine = realLine;
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

            public Print(ref TextWriter writer, ref Dictionary<string, VariableInfo> storage, string variableName, int realLine) 
                : base(ref writer, ref storage, realLine, "print")
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

            public Set(ref TextWriter writer, ref Dictionary<string, VariableInfo> storage, VariableInfo variable, int realLine) 
                : base(ref writer, ref storage, realLine, "set")
            {
                _variable = variable;
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

            public Sub(ref TextWriter writer, ref Dictionary<string, VariableInfo> storage,  string variableName, int subValue, int realLine) 
                : base(ref writer, ref storage,  realLine,"sub")
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

            public Remove(ref TextWriter writer, ref Dictionary<string, VariableInfo> storage,  string variableName, int realLine) 
                : base(ref writer, ref storage, realLine, "rem")
            {
                _variable = new VariableInfo(variableName);
            }

            public override void Do()
            {
                _variable = GetFromStorage(_variable.Name);
                Storage.Remove(_variable.Name);
            }
        }

        private class Call : Command
        {
            public readonly string FunctionName;

            public Call(ref TextWriter writer,  ref Dictionary<string, VariableInfo> storage, string functionName, int realLine) 
                : base(ref writer, ref storage,  realLine,"call")
            {
                this.FunctionName = functionName;
            }

            public override void Do()
            {
            }
        }


        private class Def : Command
        {
            public readonly string FunctionName;
            
            public Def(ref TextWriter writer, ref Dictionary<string, LinkedList<Command>> functionList, ref Dictionary<string, VariableInfo> storage,  string functionName, int realLine) 
                : base(ref writer, ref storage, realLine, "def")
            {
                FunctionName = functionName;
                // неявно добавим ключик с именем функции в functionList
                functionList.Add(functionName, new LinkedList<Command>()); // для удобного контроля в других местах
            }

            public override void Do()
            {
            }
        }

        private class PrintMem : Command
        {
            private Dictionary<string, VariableInfo> _storage;

            private TextWriter _writer;

            public PrintMem(ref Dictionary<string, VariableInfo> storage, ref TextWriter writer, int realLine) :
                base(ref writer, ref storage, realLine,"print mem")
            {
                _storage = storage;
                _writer = writer;
            }

            public override void Do()
            {
                foreach (var variable in _storage.Values)
                {
                    _writer.WriteLine($"{variable.Name} {variable.Value} {variable.LastChangedLine}");
                }
            }
            /*{var_name} {value} {line}
            {var_name} {value} {line}
            {var_name} {value} {line}

            {var_name} - имя переменной
            {value} - значение переменной
            {line} - строка последнего изменения переменной
            Порядок вывода переменных из памяти неважен и в тестах не учитывается.*/
        }
        
        
        private class
        
        private class PrintTrace : Command
        {
            private Dictionary<string, VariableInfo> _storage;
            private TextWriter _writer;

            public PrintTrace(ref TextWriter writer, ref Dictionary<string, VariableInfo> storage, int realLine) 
                : base(ref writer, ref storage, realLine, "print trace")
            {
                _storage = storage;
                _writer = writer;
            }
            /*Формат вывода стектрейса:
            {line} {function_name}
            {line} {function_name}
            {line} - номер строки с именем функции, номер именно строки с call
            {function_name} - имя вызываемой функции

                Стектрейс расположен в порядке от последней вызванной функции до первой.*/
        }


        public void AddCommandToMemory(string command, int line)
        {
            var commandTrimmed = command.Trim().Split(' ');
            var currentCommand = Switch(commandTrimmed, line);
            
            if (_isFunction)
            {
                _isFunction = command.Contains("    "); // важно, тут изначальная строка, а не порезанная
                if (_isFunction)
                {
                    _functionList[_currentFunction.FunctionName].AddLast(currentCommand);
                }
                else
                {
                    _interpretCommands.AddLast(currentCommand);
                }
            }
            else if (currentCommand is Def def)
            {
                _currentFunction = def;
            }
            else
            {
                _interpretCommands.AddLast(currentCommand);
                
            }
            
            if (currentCommand is Def)
                _isFunction = true;
        }


        private void RunCommand(Command command)
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


        private Command Switch(string[] parsedCommand, int line)
        {
            Command currentCommand;
            var variableName = parsedCommand[1];


            switch (parsedCommand[0])
            {
                case "print":
                    currentCommand = new Print(ref _writer, ref _storage, variableName, line);
                    break;
                case "set":
                    currentCommand = new Set(ref _writer, ref _storage,  new VariableInfo(variableName, Int32.Parse(parsedCommand[2]), line), line);
                    break;
                case "sub":
                    currentCommand = new Sub(ref _writer, ref _storage,  variableName,Int32.Parse(parsedCommand[2]), line);
                    break;
                case  "rem":
                    currentCommand = new Remove(ref _writer, ref _storage,  variableName, line);
                    break;
                case "call":
                    currentCommand = new Call(ref _writer, ref _storage,  variableName, line);
                    break;
                case "def":
                    currentCommand = new Def(ref _writer, ref _functionList, ref _storage,  parsedCommand[1], line);                    
                    break;
                default:
                    throw new Exception("Не найдена комманда");
            }

            return currentCommand;
        }


        private class NotFoundException : Exception
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

        private void RunFromMemory()
        {
            foreach (var interpretCommand in _interpretCommands)
            {
                if (interpretCommand is Call call)
                {
                    var functionCommands = _functionList[call.FunctionName];

                    foreach (var functionCommand in functionCommands)
                    {
                        if (functionCommand is Call callingInnerFunction) // да, поддерживаем только один уровень вложенности 
                        {
                            var innerFunction = _functionList[callingInnerFunction.FunctionName];
                            RunCommands(innerFunction);

                            if (callingInnerFunction.FunctionName == call.FunctionName) // рекурсия(неполноценная)
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

        private bool isDebugMode;
        private bool isRunning;

        private Command DebuggerSwitch(string[] command)
        {
            Command currentCommand;
            switch (command[0])
            {
                case "add":
                    break;
                case "step":
                    if (command.Length == 1) // step
                        
                    else //step over
                    
                    break;
                case "print":
                    switch (command[1])
                    {
                        case "mem":
                            currentCommand = new PrintMem(ref _storage, ref _writer, 0); //написать debuggercommand
                            break;
                        case "trace":
                            currentCommand = new PrintTrace(ref _writer, ref _storage, 0);
                            break;
                    }
                    break;
                case "run":
                    break;
                default:
                    throw new Exception("Не найдена команда");
            }

            return currentCommand;
        }
        
        
        public void ExecuteLine(string command)
        {
            var commands = command.Split('\n');
            var line = 0;
            foreach (var cmd in commands) //первоначальная разметка кода
            {
                if (cmd != "end set code" && cmd != "set code" && cmd != "run")
                {
                    AddCommandToMemory(cmd, line);
                }
                line++;
            }

            if (command == "end set code")
            {
                isDebugMode = true;
            }

            if (isRunning)
            {
                RunFromMemory();

            }

            
            _storage.Clear();
            _functionList.Clear();

        }
    }
}