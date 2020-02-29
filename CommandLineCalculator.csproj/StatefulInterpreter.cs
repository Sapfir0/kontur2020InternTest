using System;
using System.Collections.Generic;
using System.Globalization;
using CommandLineCalculator.Tests;

namespace CommandLineCalculator
{

    public sealed class StatefulInterpreter : Interpreter
    {
        // должны запомнианать текущее состояние интерперетотра и всех переменных
        private static CultureInfo Culture => CultureInfo.InvariantCulture;

        public override void Run(UserConsole console, Storage storage)
        {
            var x = 420L;
            while (true)
            {
                var input = Console.ReadLine();
                switch (input.Trim())
                {
                    case "exit":
                        return;
                    case "add":
                        Add();
                        break;
                    case "median":
                        Median();
                        break;
                    case "help":
                        Help();
                        break;
                    case "rand":
                        x = Random(x);
                        break;
                    default:
                        Console.WriteLine("Такой команды нет, используйте help для списка команд");
                        break;
                }
            }
        }
        

        private long Random(long x)
        {
            const int a = 16807;
            const int m = 2147483647;

            var count = ReadNumber();
            for (var i = 0; i < count; i++)
            {
                Console.WriteLine(x.ToString(Culture));
                x = a * x % m;
            }

            return x;
        }

        private void Add()
        {
            var a = ReadNumber();
            var b = ReadNumber();
            var str = (a + b).ToString();
            Console.WriteLine(str);
        }

        private void Median()
        {
            var count = ReadNumber();
            var numbers = new List<int>();
            for (var i = 0; i < count; i++)
            {
                numbers.Add(ReadNumber());
            }

            var result = CalculateMedian(numbers);
            Console.WriteLine(result.ToString(Culture));
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

        private static void Help()
        {
            const string exitMessage = "Чтобы выйти из режима помощи введите end";
            const string commands = "Доступные команды: add, median, rand";

            Console.WriteLine("Укажите команду, для которой хотите посмотреть помощь");
            Console.WriteLine(commands);
            Console.WriteLine(exitMessage);
            while (true)
            {
                var command = Console.ReadLine();
                switch (command.Trim())
                {
                    case "end":
                        return;
                    case "add":
                        Console.WriteLine("Вычисляет сумму двух чисел");
                        Console.WriteLine(exitMessage);
                        break;
                    case "median":
                        Console.WriteLine("Вычисляет медиану списка чисел");
                        Console.WriteLine(exitMessage);
                        break;
                    case "rand":
                        Console.WriteLine("Генерирует список случайных чисел");
                        Console.WriteLine(exitMessage);
                        break;
                    default:
                        Console.WriteLine("Такой команды нет");
                        Console.WriteLine(commands);
                        Console.WriteLine(exitMessage);
                        break;
                }
            }
        }

        private int ReadNumber()
        {
            return int.Parse(Console.ReadLine().Trim(), Culture);
        }
    
        
    }
}