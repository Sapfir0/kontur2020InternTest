using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace CommandLineCalculator
{

    class MyConsole : UserConsoleDecorator
    {
        protected UserConsole userConsole;
        protected Storage storage;
        private Datas data;
        
        public MyConsole(Storage storage, UserConsole userConsole) : base(storage, userConsole)
        {
            this.userConsole = userConsole;
            this.storage = storage;
            data = new Datas();
        }
        
        public override void WriteLine(string content)
        {
            Console.WriteLine(content);
        }

        public override string ReadLine()
        {
            var line = Console.ReadLine();
            
            if (Int32.TryParse(line, out int number))
            {
                
            }
            else
            {
                data.command = line;
            }
            storage.Write(Str2Bytes(line));

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
    abstract class UserConsoleDecorator : UserConsole
    {
        private UserConsole userConsole;
        private Storage storage;

        public UserConsoleDecorator(Storage storage, UserConsole userConsole) : base()
        {
            this.userConsole = userConsole;
            this.storage = storage;
        }
    }

    public class Datas
    {
        public string command;
        public int a;
        public int b;
        public List<int> list;

        public string ToString()
        {
            return $"{command} {a} {b} {list}";
        }

        public void FromString()
        {
            
        }
    }
    
    public sealed class StatefulInterpreter : Interpreter
    {
        private static CultureInfo Culture => CultureInfo.InvariantCulture;
        private Storage _storage;
        
        public override void Run(UserConsole userConsole, Storage storage)
        {
            var x = 420L;
            _storage = storage;
            
            var myConsole = new MyConsole(storage, userConsole);
            
            while (true)
            {
                var input = userConsole.ReadLine();

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
                        x = Random(myConsole, x);
                        break;
                    default:
                        userConsole.WriteLine("Такой команды нет, используйте help для списка команд");
                        break;
                }
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