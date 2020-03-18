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

            public Command(int realLine)
            {
                RealLine = realLine;
            }

            public virtual void  Do()
            {
                
            }
        }


        public class Print : Command
        {
            public VariableInfo Variable;

            public Print(string variableName, int realLine) : base(realLine)
            {
                Variable = GetFromStorage(variableName);
            }

            public override void Do()
            {
                _writer.WriteLine(Variable.Value);
            }
        }


        public class Set : Command
        {
            public VariableInfo Variable;

            public Set(int realLine, VariableInfo variable) : base(realLine)
            {
                Variable = variable;
            }

            public override void Do()
            {
                storage.Add(Variable.Name, Variable);
    
            }
        }

        public class Sub : Command
        {
            public VariableInfo Variable; //тут будет стейт до вызова этой комманды
            public int SubValue;

            public Sub(int realLine, string variableName, int subValue) : base(realLine)
            {
                var storageVar = GetFromStorage(variableName);
                Variable = storageVar;
                
                SubValue = subValue;
            }

            public override void Do()
            {
                Variable.Value -= SubValue;
            }
        }

        public class Remove : Command
        {
            public VariableInfo Variable;

            public Remove(int realLine, string variableName) : base(realLine)
            {
                GetFromStorage(variableName)
            }

            public override void Do()
            {
                if (!storage.Remove(variableName.Command))
                {
                    
                }
            }
        }
        
        public class AddBreak
        {
        }

        public class StepOver
        {
        }

        public class Step
        {
        }
        
        public class PrintMem
        {
       
        }

        public class PrintTrace
        {
        }

        public Dictionary<string, VariableInfo> storage = new Dictionary<string, VariableInfo>();

        public Dictionary<string, List<Command>>
            functionList = new Dictionary<string, List<Command>>(); //может быть сетом // тут будет название функции/команды внутри нее

        public LinkedList<Command> interpretComands = new LinkedList<Command>(); // просто команды которые будем запускать

        private bool isRunning = false;
//ll



        public void AddCommandToMemory(string code)
        {
            var commands = code.Split('\n').ToList();
            int line = 0;

            foreach (var command in commands)
            {
                var parsedCommand = command.Split(' ');
            
                var currentCommand = Switch(parsedCommand, line);
                interpretComands.Add(currentCommand);

                line++;
            }
        }

        public void StartRunCommands(Command blob)
        {
            blob.Do();
        }

        public Command Switch(string[] parsedCommand, int line)
        {
            Command currentCommand;
            var variableName = parsedCommand[1];
            
            switch (parsedCommand[0])
            {
                case "print":
                    currentCommand = new Print(variableName, line);
                    break;
                case "set":
                    currentCommand = new Set(line, new VariableInfo(variableName, Int32.Parse(parsedCommand[2]), line));
                    break;
                case "sub":
                    currentCommand = new Sub(line, ,  Int32.Parse(parsedCommand[2]));
                    break;
                case  "rem":
                    currentCommand = new Remove(line, new VariableInfo(variableName));
                    break;
                default:
                    throw new Exception("Не найдена комманда");
            }

            return currentCommand;
        }

        public class NotFoundException : Exception
        {
            public NotFoundException()
            {
                WriteToStream("Переменная отсутствует в памяти");

            }

            private void WriteToStream(string str)
            {
                _writer.WriteLine(str);
                Console.WriteLine(str);
            }

        }

        public VariableInfo GetFromStorage(string variableName)
        {
            if (storage.TryGetValue(variableName, out var ourVariable))
            {
                return ourVariable;
            }

            throw new NotFoundException();
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