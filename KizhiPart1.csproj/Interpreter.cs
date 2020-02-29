using System;
using System.Collections.Generic;
using System.IO;

namespace KizhiPart1
{
    public class Interpreter
    {
        private TextWriter _writer;
        Dictionary<string, int> storage = new Dictionary<string, int>();

        private void WriteToStream(string str) 
        {
            _writer.WriteLine(str);
        }

        private void WriteNotFoundMessage()
        {
            WriteToStream("Переменная отсутствует в памяти");
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
        
        public void ExecuteLine(string command) 
        {
            var parsedCommand = command.Split(' ');
            switch (parsedCommand[0]) {
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
            }
        }
    }

}