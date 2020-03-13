using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using CommandLineCalculator.Tests;

namespace CommandLineCalculator
{

    class MyConsole : UserConsole
    {
        protected UserConsole userConsole;
        protected Storage storage;
        public Datas data;
        
        public int lastInputCommand = 0;
        public int lastOutputCommand = 0;
        
        public MyConsole(Storage storage, UserConsole userConsole) : base()
        {
            this.userConsole = userConsole;
            this.storage = storage;
            data = Deserialize();
        }

        public void Serialize(Datas localData)
        {
            BinaryFormatter formatter = new BinaryFormatter();
  
            using var stream = new MemoryStream();
            formatter.Serialize(stream, localData);
            storage.Write(stream.GetBuffer());
        }
        public Datas Deserialize()
        {
            BinaryFormatter formatter = new BinaryFormatter();

            using var stream = new MemoryStream(storage.Read());
            if (stream.Length == 0)
            {
                return new Datas();
            }
            return (Datas)formatter.Deserialize(stream);
      
        }
        
        public override void WriteLine(string content)
        {
            if (lastOutputCommand < data.lastOutputCommand)
            {
                lastOutputCommand++;
            }
            else
            {
                userConsole.WriteLine(content);
                data.outputCommands.Add(content);
                data.lastOutputCommand++;
                lastOutputCommand++;
                Serialize(data);
            }
        }

        public override string ReadLine()
        {
            string line;

            if (lastInputCommand < data.lastInputCommand) //нужно эмулировать считивание
            {
                line = data.inputCommands[lastInputCommand];
                lastInputCommand++;
            }
            else
            {
                line = userConsole.ReadLine();
                
                data.inputCommands.Add(line);

                // уже ничего плохого не произойдет
                data.lastInputCommand++;
                lastInputCommand++;
                Serialize(data);
            }
            return line;

        }
        
    }

    [Serializable]
    public class Datas
    {
        public List<string> inputCommands = new List<string>();
        public List<string> outputCommands = new List<string>();
        public int lastInputCommand = 0;
        public int lastOutputCommand = 0;
        public long x = 420;


    }
    
    public sealed class StatefulInterpreter : Interpreter
    {
        private static CultureInfo Culture => CultureInfo.InvariantCulture;
        
        public override void Run(UserConsole userConsole, Storage storage)
        {

            var myConsole = new MyConsole(storage, userConsole); 
            
            while (true)
            {
                var input = myConsole.ReadLine();

                switch (input.Trim())
                {
                    case "exit":
                        myConsole.data.x = 420;
                        return;
                    case "add":
                        Add(myConsole);
                        break;
                    case "median":
                        Median(myConsole);
                        break;
                    case "help":
                        Help(myConsole);
                        break;
                    case "rand":
                        myConsole.data.x = Random(myConsole, myConsole.data.x);
                        break;
                    default:
                        userConsole.WriteLine("Такой команды нет, используйте help для списка команд");
                        break;
                }

                long tempx = myConsole.data.x;
                myConsole.data = new Datas();
                myConsole.lastInputCommand = 0;
                myConsole.lastOutputCommand = 0;
                myConsole.data.x = tempx;
                
                myConsole.Serialize(myConsole.data);
            }
        }

        private long Random(UserConsole console, long x)
        {
            const int a = 16807;
            const int m = 2147483647;

            var count = ReadNumber(console);
            for (var i = 0; i < count; i++)
            {
                console.WriteLine(x.ToString(Culture));
                x = a * x % m;
            }

            return x;
        }

        private void Add(UserConsole console)
        {           
            var a = ReadNumber(console);
            var b = ReadNumber(console);
            console.WriteLine((a + b).ToString(Culture));
        }

        private void Median(UserConsole console)
        {
            var count = ReadNumber(console);
            var numbers = new List<int>();
            for (var i = 0; i < count; i++)
            {
                numbers.Add(ReadNumber(console));
            }

            var result = CalculateMedian(numbers);
            console.WriteLine(result.ToString(Culture));
        }

        private double CalculateMedian(List<int> numbers)
        {
            numbers.Sort();
            var count = numbers.Count;
            if (count == 0)
                return 0;

            if (count % 2 == 1)
                return numbers[count / 2];

            return (numbers[count / 2 - 1] + numbers[count / 2]) / 2.0;
        }

        private static void Help(UserConsole console)
        {
            const string exitMessage = "Чтобы выйти из режима помощи введите end";
            const string commands = "Доступные команды: add, median, rand";

            console.WriteLine("Укажите команду, для которой хотите посмотреть помощь");
            console.WriteLine(commands);
            console.WriteLine(exitMessage);
            while (true)
            {
                var command = console.ReadLine();
                switch (command.Trim())
                {
                    case "end":
                        return;
                    case "add":
                        console.WriteLine("Вычисляет сумму двух чисел");
                        console.WriteLine(exitMessage);
                        break;
                    case "median":
                        console.WriteLine("Вычисляет медиану списка чисел");
                        console.WriteLine(exitMessage);
                        break;
                    case "rand":
                        console.WriteLine("Генерирует список случайных чисел");
                        console.WriteLine(exitMessage);
                        break;
                    default:
                        console.WriteLine("Такой команды нет");
                        console.WriteLine(commands);
                        console.WriteLine(exitMessage);
                        break;
                }
                
            }
        }

        private int ReadNumber(UserConsole console)
        {
            return int.Parse(console.ReadLine().Trim(), Culture);
        }
    }
}