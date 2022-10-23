using Offmodel.FFXIV.Log.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Offmodel.FFXIV.Log.Model
{
    public class Parser
    {
        protected TextReader reader;
        protected State state;

        public IEnumerable<LogEvent> Events { get; };

        protected LogEvent LineToEvent(LogLine logLine)
        {
            switch (uint.Parse(logLine.Text(0)))
            {
                default:
                    return new LogEvent(logLine);
            }
        }

        public Parser(TextReader reader)
        {
            this.reader = reader;
            this.state = new State();

            List<LogEvent> events = new List<LogEvent>();
            for (String line = reader.ReadLine(); line != null; line = reader.ReadLine()) {
                LogLine logline = new(line);
                LogEvent logEvent = LineToEvent(logline);
                events.Add(logEvent);
            }

            Events = events;
            reader.Close();
        }
    }
}
