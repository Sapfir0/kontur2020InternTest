using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace CommandLineCalculator
{

    class MyConsole : UserConsole
    {
        protected UserConsole userConsole;
        protected Storage storage;
        public Datas data;
        
        public MyConsole(Storage storage, UserConsole userConsole, Datas data) : base()
        {
            this.userConsole = userConsole;
            this.storage = storage;
            this.data = data;
        }

        public void Serialize(Datas localData)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            try
            {
                using var stream = new MemoryStream();
                formatter.Serialize(stream, localData);
                storage.Write(stream.GetBuffer());
            }
            catch (SerializationException e) 
            {
                Console.WriteLine("Failed to serialize. Reason: " + e.Message);
                throw;
            }
            
        }
        public Datas Deserialize()
        {
            BinaryFormatter formatter = new BinaryFormatter();
            try
            {
                using var stream = new MemoryStream(storage.Read());
                if (stream.Length == 0)
                {
                    return new Datas();
                }
                return (Datas)formatter.Deserialize(stream);
            }
            catch (SerializationException e) 
            {
                Console.WriteLine("Failed to deserialize. Reason: " + e.Message);
                throw;
            }
        }
        
        public override void WriteLine(string content)
        {

            var storageData = Deserialize();
            if (storageData.lastOutputCommand != data.lastOutputCommand)
            {
                data = storageData;
                //data.lastOutputCommand++;
                userConsole.WriteLine(storageData.outputCommands[storageData.lastOutputCommand-1]);
                Serialize(data);
            }
            else
            {
                data.outputCommands.Add(content);
                data.lastOutputCommand++;
                userConsole.WriteLine(content);
                Serialize(data);
            }
            
            
            data = Deserialize();

        }

        public override string ReadLine()
        {
            string line = "its a nullable";
            var storageData = Deserialize();
            if (storageData.lastInputCommand != data.lastInputCommand) //нужно эмулировать считивание
            {
                data.lastInputCommand++;
                if (data.lastInputCommand <= storageData.lastInputCommand)
                {
                    return storageData.inputCommands[data.lastInputCommand-1];
                }
            }
            else
            {
                line = userConsole.ReadLine();
                data.inputCommands.Add(line);

                //допустим, что уже ничего плохого не произойдет
                data.lastInputCommand++;
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
        
    }
    
    public sealed class StatefulInterpreter : Interpreter
    {
        private static CultureInfo Culture => CultureInfo.InvariantCulture;
        private Datas data = new Datas(); // не уверен насчет этого момента
        
        public override void Run(UserConsole userConsole, Storage storage)
        {
            var x = 420L;

            var myConsole = new MyConsole(storage, userConsole, data);
            
            while (true)
            {
                var input = myConsole.ReadLine();

                switch (input.Trim())
                {
                    case "exit":
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
                        data.inputCommands.Add(x.ToString());
                        x = Random(myConsole, x);
                        break;
                    default:
                        userConsole.WriteLine("Такой команды нет, используйте help для списка команд");
                        break;
                }
                
                myConsole.data.inputCommands.Clear();
                myConsole.data.outputCommands.Clear();
                myConsole.data.lastInputCommand = 0;
                myConsole.data.lastOutputCommand = 0;
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