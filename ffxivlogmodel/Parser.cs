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
        protected State state = new();

        public IEnumerable<LogEvent> Events { get; }

        protected LogEvent LineToEvent(LogLine logLine)
        {
            switch (uint.Parse(logLine.Text(0)))
            {
                case 2:
                case 3:
                    return new Actor(logLine, state);

                case 21:
                case 22:
                    return new Action(logLine, state);

                case 37:
                    return new ActionSync(logLine, state);

                default:
                    return new LogEvent(logLine);
            }
        }

        public Parser(TextReader reader)
        {
            this.reader = reader;

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
