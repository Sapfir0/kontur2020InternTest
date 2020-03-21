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
            storage = new Dictionary<string, VariableInfo>();
            functionList = new Dictionary<string, LinkedList<Command>>();
            interpretComands = new LinkedList<Command>();
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
            public string commandName;
            public Command(ref TextWriter writer, ref Dictionary<string, VariableInfo> storage, int realLine, string commandName)
            {
                this.writer = writer;
                this.storage = storage;
                RealLine = realLine;
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
            public Print(ref TextWriter writer, ref Dictionary<string, VariableInfo> storage, string variableName, int realLine) 
                : base(ref writer, ref storage, realLine, "print")
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
                : base(ref writer, ref storage, realLine, "set")
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
                : base(ref writer, ref storage, realLine, "sub")
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
                : base(ref writer, ref storage, realLine, "rem")
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
            public string functionName;

            public Call(ref TextWriter writer,  ref Dictionary<string, VariableInfo> storage, int realLine, string functionName) 
                : base(ref writer, ref storage, realLine, "call")
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
            
            public Def(ref TextWriter writer, ref Dictionary<string, LinkedList<Command>> functionList, ref Dictionary<string, VariableInfo> storage, int realLine, string functionName) 
                : base(ref writer, ref storage, realLine, "def")
            {
                this.functionName = functionName;
                // TODO неявно добавим ключик с именем функции в functionList
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

        private bool isRunning = false;

        public bool isFunction = false;
        public Def currentFunction;

        
        
        public void AddCommandToMemory(string command, int line)
        {
            Command currentCommand;

            var commandTrimmed = command.Trim().Split(' ');
            currentCommand = Switch(commandTrimmed, line);
            
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
        

        public void RunCommand(Command blob)
        {
            try
            {
                blob.Do();
            }
            catch (NotFoundException e)
            {
                /*kjh.Add(blob);
                if (kjh.Count > 2)
                {
                    throw new Exception(debugLine + "\n" + debugFunction + howMatchExecuteThis.ToString());
                }*/
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
        
        //private List<Command> kjh = new List<Command>();
        //private string debugLine;
        //private string debugFunction;
        //private int howMatchExecuteThis = 0;
        /*public string GetInterpreCommandsAsString()
        {
            string foo = "";
            foreach (var VARIABLE in interpretComands)
            {
                foo += VARIABLE.commandName + " ";
            }

            return foo;
        }*/
        
        public void ExecuteLine(string command)
        {
            var commands = command.Split('\n');
            var line = 0;

            foreach (var cmd in commands) //первоначальная разметка кода
            {
                if (cmd != "end set code" && cmd != "set code" && cmd != "run")
                {
                    //debugLine = command;
                    AddCommandToMemory(cmd, line);
                }
                line++;
            }

            var fixedInterpreterCommands = new LinkedList<Command>();

            //debugFunction += GetInterpreCommandsAsString();

            foreach (var interpretComand in interpretComands)
            {
                if (interpretComand is Call call)
                {
                    var functionCommands = functionList[call.functionName];
                    //howMatchExecuteThis++;
                    foreach (var func in functionCommands)
                    {
                        fixedInterpreterCommands.AddLast(func);
                    }
                }
                else
                {
                    fixedInterpreterCommands.AddLast(interpretComand);
                }
                
            }

            foreach (var mycommand in fixedInterpreterCommands) // c рекурсией не прокатит, но пох
            {
                //debugFunction += mycommand.ToString();
                RunCommand(mycommand);
            }
            
        }
    }
}