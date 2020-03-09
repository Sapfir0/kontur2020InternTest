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
        private Datas data;
        
        public MyConsole(Storage storage, UserConsole userConsole, Datas data) : base()
        {
            this.userConsole = userConsole;
            this.storage = storage;
            this.data = data;
            this.data.FromString(Byte2Str(storage.Read())); // заполняем дату из нашего стореджа
        }
        
        public override void WriteLine(string content)
        {
            data.outputCommands.Add(content);
            storage.Write(Str2Bytes(data.ToString())); // перезапишем все что было
            

            
            
            var commands = Byte2Str(storage.Read());
            Console.WriteLine(commands);

        }

        public override string ReadLine()
        {
            string line;
            if (storage.Read().Length != 0)
            {
                line = Byte2Str(storage.Read());
            }
            else
            {
                line = Console.ReadLine();
            }
            data.inputCommands.Add(line);
            storage.Write(Str2Bytes(data.ToString()));
        
            return line;

        }
        
        private byte[] Str2Bytes(string str)
        {
            return Encoding.UTF8.GetBytes(str);
        }

        private string Byte2Str(byte[] bytes)
        {
            return Encoding.UTF8.GetString(bytes, 0, bytes.Length);
        }
    }

    [Serializable]
    public class Datas
    {
        public List<string> inputCommands = new List<string>();
        public List<string> outputCommands = new List<string>();
        public int lastInputCommand;
        public int lastOutputCommand;

        public void Serialize()
        {
            BinaryFormatter formatter = new BinaryFormatter();
            try 
            {
                using (FileStream fs = new FileStream("people.dat", FileMode.OpenOrCreate))
                {
                    formatter.Serialize(stream, inputCommands);
                    formatter.Serialize(stream, outputCommands);
                }
            }
            catch (SerializationException e) 
            {
                Console.WriteLine("Failed to serialize. Reason: " + e.Message);
                throw;
            }
            
        }

        public void Deserialize(string ser)
        {
  
        }
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
                
                data.inputCommands.Clear();
                data.outputCommands.Clear();
                storage.Write(new byte[0]);
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