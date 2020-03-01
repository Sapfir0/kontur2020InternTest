using System.IO;

namespace KizhiPart3
{
    public class Debugger
    {
        private TextWriter _writer;

        public Debugger(TextWriter writer)
        {
            _writer = writer;
        }

        public void ExecuteLine(string command)
        {
        }
    }
}