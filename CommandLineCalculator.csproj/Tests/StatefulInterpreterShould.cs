using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using FluentAssertions;
using NUnit.Framework;
using static CommandLineCalculator.Tests.TestConsole.Action;

namespace CommandLineCalculator.Tests
{
    public class StatefulInterpreterShould
    {
        public static TestCaseData[] RegularCases => new[]
        {
            new TestCaseData(
                new TestConsole(
                    (Read, "exit")
                )
            ).SetName("exit"),
            new TestCaseData(
                new TestConsole(
                    (Read, "add"),
                    (Read, "15"),
                    (Read, "60"),
                    (Write, "75"),
                    (Read, "exit")
                )
            ).SetName("add"),
            new TestCaseData(
                new TestConsole(
                    (Read, "median"),
                    (Read, "5"),
                    (Read, "17"),
                    (Read, "30"),
                    (Read, "29"),
                    (Read, "23"),
                    (Read, "20"),
                    (Write, "23"),
                    (Read, "exit")
                )
            ).SetName("odd median"),
            new TestCaseData(
                new TestConsole(
                    (Read, "median"),
                    (Read, "6"),
                    (Read, "17"),
                    (Read, "30"),
                    (Read, "29"),
                    (Read, "23"),
                    (Read, "20"),
                    (Read, "24"),
                    (Write, "23.5"),
                    (Read, "exit")
                )
            ).SetName("even median"),
            new TestCaseData(
                new TestConsole(
                    (Read, "rand"),
                    (Read, "1"),
                    (Write, "420"),
                    (Read, "rand"),
                    (Read, "2"),
                    (Write, "7058940"),
                    (Write, "528003995"),
                    (Read, "rand"),
                    (Read, "3"),
                    (Write, "760714561"),
                    (Write, "1359476136"),
                    (Write, "1636897319"),
                    (Read, "exit")
                )
            ).SetName("rand 3x"),
            new TestCaseData(
                new TestConsole(
                    (Read, "ramd"),
                    (Write, "Такой команды нет, используйте help для списка команд"),
                    (Read, "exit")
                )
            ).SetName("unknown command"),
            new TestCaseData(
                new TestConsole(
                    (Read, "help"),
                    (Write, "Укажите команду, для которой хотите посмотреть помощь"),
                    (Write, "Доступные команды: add, median, rand"),
                    (Write, "Чтобы выйти из режима помощи введите end"),
                    (Read, "end"),
                    (Read, "exit")
                )
            ).SetName("empty help"),
            new TestCaseData(
                new TestConsole(
                    (Read, "help"),
                    (Write, "Укажите команду, для которой хотите посмотреть помощь"),
                    (Write, "Доступные команды: add, median, rand"),
                    (Write, "Чтобы выйти из режима помощи введите end"),
                    (Read, "add"),
                    (Write, "Вычисляет сумму двух чисел"),
                    (Write, "Чтобы выйти из режима помощи введите end"),
                    (Read, "end"),
                    (Read, "exit")
                )
            ).SetName("add help"),
            new TestCaseData(
                new TestConsole(
                    (Read, "help"),
                    (Write, "Укажите команду, для которой хотите посмотреть помощь"),
                    (Write, "Доступные команды: add, median, rand"),
                    (Write, "Чтобы выйти из режима помощи введите end"),
                    (Read, "median"),
                    (Write, "Вычисляет медиану списка чисел"),
                    (Write, "Чтобы выйти из режима помощи введите end"),
                    (Read, "end"),
                    (Read, "exit")
                )
            ).SetName("median help"),
            new TestCaseData(
                new TestConsole(
                    (Read, "help"),
                    (Write, "Укажите команду, для которой хотите посмотреть помощь"),
                    (Write, "Доступные команды: add, median, rand"),
                    (Write, "Чтобы выйти из режима помощи введите end"),
                    (Read, "rand"),
                    (Write, "Генерирует список случайных чисел"),
                    (Write, "Чтобы выйти из режима помощи введите end"),
                    (Read, "end"),
                    (Read, "exit")
                )
            ).SetName("rand help"),
            new TestCaseData(
                new TestConsole(
                    (Read, "help"),
                    (Write, "Укажите команду, для которой хотите посмотреть помощь"),
                    (Write, "Доступные команды: add, median, rand"),
                    (Write, "Чтобы выйти из режима помощи введите end"),
                    (Read, "media"),
                    (Write, "Такой команды нет"),
                    (Write, "Доступные команды: add, median, rand"),
                    (Write, "Чтобы выйти из режима помощи введите end"),
                    (Read, "end"),
                    (Read, "exit")
                )
            ).SetName("unknown help"),
            new TestCaseData(
                new TestConsole(
                    (Read, "help"),
                    (Write, "Укажите команду, для которой хотите посмотреть помощь"),
                    (Write, "Доступные команды: add, median, rand"),
                    (Write, "Чтобы выйти из режима помощи введите end"),
                    (Read, "media"),
                    (Write, "Такой команды нет"),
                    (Write, "Доступные команды: add, median, rand"),
                    (Write, "Чтобы выйти из режима помощи введите end"),
                    (Read, "add"),
                    (Write, "Вычисляет сумму двух чисел"),
                    (Write, "Чтобы выйти из режима помощи введите end"),
                    (Read, "rand"),
                    (Write, "Генерирует список случайных чисел"),
                    (Write, "Чтобы выйти из режима помощи введите end"),
                    (Read, "median"),
                    (Write, "Вычисляет медиану списка чисел"),
                    (Write, "Чтобы выйти из режима помощи введите end"),
                    (Read, "end"),
                    (Read, "exit")
                )
            ).SetName("several commands help"),
        };

        public static TestCaseData[] InterruptionCases => new[]
        {
            new TestCaseData(
                new TestConsole(
                    (Read, "add"),
                    (Read, "15"),
                    (Read, "60"),
                    (Write, "75"),
                    (Read, "exit")
                ),
                new[] {0, 1, 2, 3}
            ).SetName("add"),
            new TestCaseData(
                new TestConsole(
                    (Read, "median"),
                    (Read, "3"),
                    (Read, "60"),
                    (Read, "50"),
                    (Read, "41"),
                    (Write, "50"),
                    (Read, "exit")
                ),
                new[] {0, 1, 2, 3, 4}
            ).SetName("median"),
            new TestCaseData(
                new TestConsole(
                    (Read, "rand"),
                    (Read, "10"),
                    (Write, "420"),
                    (Write, "7058940"),
                    (Write, "528003995"),
                    (Write, "760714561"),
                    (Write, "1359476136"),
                    (Write, "1636897319"),
                    (Write, "2067722363"),
                    (Write, "1629379187"),
                    (Write, "264529365"),
                    (Write, "653888265"),
                    (Read, "exit")
                ),
                new[] {0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10}
            ).SetName("rand"),
            new TestCaseData(
                new TestConsole(
                    (Read, "help"),
                    (Write, "Укажите команду, для которой хотите посмотреть помощь"),
                    (Write, "Доступные команды: add, median, rand"),
                    (Write, "Чтобы выйти из режима помощи введите end"),
                    (Read, "media"),
                    (Write, "Такой команды нет"),
                    (Write, "Доступные команды: add, median, rand"),
                    (Write, "Чтобы выйти из режима помощи введите end"),
                    (Read, "add"),
                    (Write, "Вычисляет сумму двух чисел"),
                    (Write, "Чтобы выйти из режима помощи введите end"),
                    (Read, "rand"),
                    (Write, "Генерирует список случайных чисел"),
                    (Write, "Чтобы выйти из режима помощи введите end"),
                    (Read, "median"),
                    (Write, "Вычисляет медиану списка чисел"),
                    (Write, "Чтобы выйти из режима помощи введите end"),
                    (Read, "end"),
                    (Read, "exit")
                ),
                new[] {0, 2, 3, 4, 5, 6, 7, 8, 9, 10}
            ).SetName("several commands help"),
            new TestCaseData(
                new TestConsole(
                    (Read, "median"),
                    (Read, "5"),
                    (Read, "17"),
                    (Read, "30"),
                    (Read, "29"),
                    (Read, "23"),
                    (Read, "20"),
                    (Write, "23"),
                    (Read, "exit")
                ),
                new[] {0, 1, 2, 3, 4, 5, 6, 7, 8}
            ).SetName("odd median"),
            new TestCaseData(
                new TestConsole(
                    (Read, "exit")
                ),
                new[] {0}
            ).SetName("exit"),
            new TestCaseData(
                new TestConsole(
                    (Read, "rand"),
                    (Read, "1"),
                    (Write, "420"),
                    (Read, "rand"),
                    (Read, "2"),
                    (Write, "7058940"),
                    (Write, "528003995"),
                    (Read, "rand"),
                    (Read, "3"),
                    (Write, "760714561"),
                    (Write, "1359476136"),
                    (Write, "1636897319"),
                    (Read, "exit")
                ),
                new[] {0, 1, 2, 3, 4, 5, 6}
            ).SetName("rand 3x"),
        };

        // 420
        // 7058940
        // 528003995
        // 760714561
        // 1359476136
        // 1636897319
        // 2067722363
        // 1629379187
        // 264529365
        // 653888265


        [Test]
        [TestCaseSource(nameof(RegularCases))]
        public void Run_As_Expected(TestConsole console)
        {
            var storage = new MemoryStorage();
            var interpreter = new StatefulInterpreter();
            interpreter.Run(console, storage);
            console.AtEnd.Should().BeTrue();
        }

        [Test]
        [TestCaseSource(nameof(InterruptionCases))]
        public void Run_With_Interruptions(
            TestConsole console,
            int[] failureSchedule)
        {
            var storage = new MemoryStorage();
            var brokenConsole = new BrokenConsole(console, failureSchedule);
            for (var i = 0; i < failureSchedule.Length; i++)
            {
                var exception = Assert.Throws<TestException>(() =>
                {
                    var interpreter = new StatefulInterpreter();
                    interpreter.Run(brokenConsole, storage);
                });
                exception.Type.Should().Be(TestException.ExceptionType.InducedFailure);
            }

            var finalInterpreter = new StatefulInterpreter();
            finalInterpreter.Run(brokenConsole, storage);

            console.AtEnd.Should().BeTrue();
        }
        
        [Test]
        public void Long_Random()
        {
            const int testsCount = 1000;
            var actionConsoleList = new List<(TestConsole.Action Action, string Value)>(testsCount * 5);
            var stateless = new StatelessInterpreter();
            var commands = new[] {"add", "median", "help", "rand"};
            var random = new Random();
            
            for (var i = 0; i < testsCount; ++i)
            {
                var command = commands[random.Next(commands.Length)];
                
                var currentInput = new StringBuilder();
                var currentOutput = new StringWriter();
                
                actionConsoleList.Add((Read, command));
                currentInput.AppendLine(command);
                switch (command)
                {
                    case "add":
                    {
                        var first = random.Next();
                        var second = random.Next();

                        actionConsoleList.Add((Read, first.ToString().Trim()));
                        actionConsoleList.Add((Read, second.ToString().Trim()));
                        
                        currentInput.AppendLine(first.ToString().Trim());
                        currentInput.AppendLine(second.ToString().Trim());
                        currentInput.AppendLine("exit");
                        stateless.Run(new TextUserConsole(new StringReader(currentInput.ToString()), currentOutput),
                            new MemoryStorage());

                        actionConsoleList.AddRange(currentOutput.ToString().Split('\n')
                            .Where(line => line.Length != 0)
                            .Select(line => (Write, line.Trim())));
                        break;
                    }
                    case "median":
                    {
                        var count = random.Next(100);
                        var list = new List<int>(count);
                        for (var _ = 0; _ < count; ++_)
                        {
                            list.Add(random.Next());
                        }

                        actionConsoleList.Add((Read, count.ToString()));
                        actionConsoleList.AddRange(list.Select(i1 => (Read, i1.ToString())));
                        currentInput.AppendLine(count.ToString());
                        
                        foreach (var i1 in list)
                        {
                            currentInput.AppendLine(i1.ToString());
                        }
                        currentInput.AppendLine("exit");
                        stateless.Run(new TextUserConsole(new StringReader(currentInput.ToString()), currentOutput),
                            new MemoryStorage());
                        actionConsoleList.AddRange(currentOutput.ToString().Split('\n')
                            .Where(line => line.Length != 0)
                            .Select(line => (Write, line.Trim())));
                        break;
                    }
                    case "help":
                    {
                        var helpCommandCount = random.Next(5);
                        var helpCommands = new[] {"add", "median", "rand", "wrong-command"};

                        actionConsoleList.Add((Write, "Укажите команду, для которой хотите посмотреть помощь"));
                        actionConsoleList.Add((Write, "Доступные команды: add, median, rand"));
                        actionConsoleList.Add((Write, "Чтобы выйти из режима помощи введите end"));

                        for (var j = 0; j < helpCommandCount; ++j)
                        {
                            var currentCommand = helpCommands[random.Next(helpCommands.Length)];
                            actionConsoleList.Add((Read, currentCommand));
                            switch (currentCommand.Trim())
                            {
                                case "add":
                                    actionConsoleList.Add((Write, "Вычисляет сумму двух чисел"));
                                    actionConsoleList.Add((Write, "Чтобы выйти из режима помощи введите end"));
                                    break;
                                case "median":
                                    actionConsoleList.Add((Write, "Вычисляет медиану списка чисел"));
                                    actionConsoleList.Add((Write, "Чтобы выйти из режима помощи введите end"));
                                    break;
                                case "rand":
                                    actionConsoleList.Add((Write, "Генерирует список случайных чисел"));
                                    actionConsoleList.Add((Write, "Чтобы выйти из режима помощи введите end"));
                                    break;
                                default:
                                    actionConsoleList.Add((Write, "Такой команды нет"));
                                    actionConsoleList.Add((Write, "Доступные команды: add, median, rand"));
                                    actionConsoleList.Add((Write, "Чтобы выйти из режима помощи введите end"));
                                    break;
                            }
                        }
                        actionConsoleList.Add((Read, "end"));
                        break;
                    }
                    case "rand":
                    {
                        var count = random.Next(100);
                        actionConsoleList.Add((Read, count.ToString()));
                    
                        currentInput.AppendLine(count.ToString());
                        currentInput.AppendLine("exit");
                        stateless.Run(new TextUserConsole(new StringReader(currentInput.ToString()), currentOutput),
                            new MemoryStorage());
                        actionConsoleList.AddRange(currentOutput.ToString().Split('\n')
                            .Where(line => line.Length != 0)
                            .Select(line => (Write, line.Trim())));
                        
                        break;
                    }
                }
            }
            actionConsoleList.Add((Read, "exit"));
            
            
            var brokeSchedule = Enumerable.Range(0, actionConsoleList.Count).Where(i => i % 3 == 0).ToArray();
            var stateful = new StatefulInterpreter();
            var console = new TestConsole(actionConsoleList.ToArray());
            var brokenConsole =
                new BrokenConsole(console, brokeSchedule.ToArray());
            var storage = new MemoryStorage();
            for (int i = 0; i < brokeSchedule.Length; ++i)
            {
                var exception = Assert.Throws<TestException>(() =>
                {
                    var interpreter = new StatefulInterpreter();
                    interpreter.Run(brokenConsole, storage);
                });
                if (exception.Type != TestException.ExceptionType.InducedFailure)
                {
                    var foo = 0;
                }
                exception.Type.Should().Be(TestException.ExceptionType.InducedFailure);
            }
            
            var finalInterpreter = new StatefulInterpreter();
            finalInterpreter.Run(brokenConsole, storage);

            console.AtEnd.Should().BeTrue();
        }
    }
}