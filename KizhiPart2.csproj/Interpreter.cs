using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace KizhiPart2 {
    public class MainClass {
        static int Main() {
            return 0;
            
        }
    }
    
    
    public class Interpreter {
        private TextWriter _writer;
        Dictionary<string, int> storage = new Dictionary<string, int>(); // словарь для переменных
        Dictionary<string, List<string>> functionList = new Dictionary<string, List<string>>(); // тут будет название функции/команды внутри нее
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

        public void Def(string nameOfFunction, List<string> commands)
        {
            functionList.Add(nameOfFunction, commands);
        }

        public void Call(string nameOfFunction)
        {
            if(functionList.TryGetValue(nameOfFunction, out var ourVariable))
            {
                
            }
            else
            {
                WriteNotFoundMessage(); //такой функции мы не знаем 
            }
        }

        public void RecursiveSet(string blob) {
            var parsedCommand = blob.Split(' ');

            switch (parsedCommand[0]) 
            {
                case "set": 
                {
                    Set(parsedCommand[1], Int32.Parse(parsedCommand[2]));
                    break;
                }
                case "sub": 
                {
                    Sub(parsedCommand[1], Int32.Parse(parsedCommand[2]));
                    break;
                }
                case "print": 
                {
                    Print(parsedCommand[1]);
                    break;
                }
                case "rem": 
                {
                    Rem(parsedCommand[1]);
                    break;
                }
                case "def":
                {
                    
                    string[] sep = new string[] {"    "};
                    var parsedFunction = blob.Split(sep, 9999, StringSplitOptions.None);
                    // последнее значение будет неверно, он оставит всю остальную строчку

                    var test = parsedFunction[parsedFunction.Length - 1].Split(' ');
                    parsedFunction[parsedFunction.Length-1] = parsedFunction[parsedFunction.Length-1].Split(' ')[1] + " " + parsedFunction[parsedFunction.Length-1].Split(' ')[2];
                    var listok = parsedFunction.ToList();
                    listok.RemoveAt(0);
                    //throw new Exception(blob);

                    // берем по разделителю 4 пробела пока не встретится код без пробелов, тогда перестаем брать
                    Def(parsedCommand[1], listok);
                    break;   
                }
                case "call": {
                    Call(parsedCommand[1]);
                    break;
                }

                default: { 
                    break;
                }
            }
        }
        
        
        
        public void ExecuteLine(string command) {
            var parsedCommand = command.Split(' ');
            
            if (thisIsCodeBlock) {
                if (parsedCommand[0] == "end")
                {
                    thisIsCodeBlock = false;
                }
                else
                {              
                }
            } else {
                if (parsedCommand[0] =="set")
                {
                    thisIsCodeBlock = true;
                }
            }

            
            if (parsedCommand[0] == "run") {
                //начинаем интерпреатцию
                for (int i = 0; i < interpretComands.Count; i++)
                {
                    RecursiveSet(interpretComands[i]);

                }
            }
        
        }
    }

}