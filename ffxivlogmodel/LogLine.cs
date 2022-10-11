using System;
using System.Text.RegularExpressions;

namespace Offmodel.FFXIV.Log.Model
{
    public class LogLine
    {
        private String[] line;

        public LogLine(String text)
        {
            line = new Regex("\\|").Split(text);
        }

        public int Length
        {
            get { return line.Length; }
        }

        public string Text(int position)
        {
            return line[position];
        }
    }
}
