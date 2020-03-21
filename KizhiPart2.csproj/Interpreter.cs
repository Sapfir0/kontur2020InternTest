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

            public VariableInfo(string name, int? value=null)
            {
                Name = name;
                if (value != null)
                    Value = (int)value;
            }
        }

        public abstract class Command
        {
            public Dictionary<string, VariableInfo> storage;
            public TextWriter writer;
            public string commandName;
            public Command(ref TextWriter writer, ref Dictionary<string, VariableInfo> storage, string commandName)
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
            public Print(ref TextWriter writer, ref Dictionary<string, VariableInfo> storage, string variableName) 
                : base(ref writer, ref storage,  "print")
            {
                _writer = writer;
                Variable = new VariableInfo(variableName);
            }

            public override void Do()
            {
                Variable = GetFromStorage(Variable.Name); // вау аутизм
                 _writer.WriteLine(Variable.Value);
            }
        }


        public class Set : Command
        {
            public VariableInfo Variable;

            public Set(ref TextWriter writer, ref Dictionary<string, VariableInfo> storage, VariableInfo variable) 
                : base(ref writer, ref storage, "set")
            {
                Variable = variable;
            }

            public override void Do()
            {
                if (!storage.ContainsKey(Variable.Name))
                    storage.Add(Variable.Name, Variable);
                else
                    storage[Variable.Name] = Variable;
            }
        }

        public class Sub : Command
        {
            public VariableInfo Variable; //тут будет стейт до вызова этой комманды
            public int SubValue;

            public Sub(ref TextWriter writer, ref Dictionary<string, VariableInfo> storage,  string variableName, int subValue) 
                : base(ref writer, ref storage,  "sub")
            {
                Variable = new VariableInfo(variableName);

                SubValue = subValue;
            }

            public override void Do()
            {
                Variable = GetFromStorage(Variable.Name);
                Variable.Value -= SubValue;
            }
        }

        public class Remove : Command
        {
            public VariableInfo Variable;

            public Remove(ref TextWriter writer, ref Dictionary<string, VariableInfo> storage,  string variableName) 
                : base(ref writer, ref storage,  "rem")
            {
                Variable = new VariableInfo(variableName);

            }

            public override void Do()
            {
                Variable = GetFromStorage(Variable.Name);
                storage.Remove(Variable.Name);
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
        

        public Dictionary<string, VariableInfo> storage = new Dictionary<string, VariableInfo>();

        public Dictionary<string, LinkedList<Command>> functionList = new Dictionary<string, LinkedList<Command>>(); 
        //может быть сетом // тут будет название функции/команды внутри нее

        public LinkedList<Command> interpretComands = new LinkedList<Command>(); // просто команды которые будем запускать
        
        public bool isFunction;
        public Def currentFunction;
        
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

        public void RunCommands(List<Command> commands)
        {
            foreach (var command in commands)
            {
                RunCommand(command);
            }
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
        
        public List<Command> GetFixedInterpretationList()
        {
            var fixedInterpreterCommands = new List<Command>();
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
                            foreach (var innerCommand in innerFunction)
                            {
                                fixedInterpreterCommands.Add(innerCommand);
                            }
                            if (callingInnerFuinction.functionName == call.functionName) // не спасет от кроссрекурсии 
                            {
                                while (true)
                                {
                                }
                            }
                        }
                        else
                        {
                            fixedInterpreterCommands.Add(functionCommand);
                        }
                        
                    }
                }
                else
                {
                    fixedInterpreterCommands.Add(interpretCommand);
                }
                
            }

            return fixedInterpreterCommands;
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

            var fixedInterpreterCommands = GetFixedInterpretationList();

            
            storage.Clear();
            functionList.Clear();

        }
    }
}