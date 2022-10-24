using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Offmodel.FFXIV.Log.Model
{
    public class LogEvent
    {
        public uint EventId { get; }
        public DateTime EventTime { get; }

        public LogEvent(LogLine line)
        {
            EventId = uint.Parse(line.Text(0));
            EventTime = DateTime.Parse(line.Text(1));
        }
    }
}
