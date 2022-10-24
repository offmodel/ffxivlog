using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Offmodel.FFXIV.Log.Model
{
    public class BuffUpdate : LogEvent
    {
        public uint Buff { get; }
        public float Duration { get; }
        public String BuffName { get; }
        public Actor Source { get; }
        public Actor Target { get; }
        public int Count { get; }
        public bool Expired { get { return Duration == 0.0; } }

        public BuffUpdate(LogLine line, State state) : base(line)
        {
            Buff = uint.Parse(line.Text(2), NumberStyles.HexNumber);
            BuffName = NameEntry.Statuses.ContainsKey(Buff) ? NameEntry.Statuses[Buff].Name : Buff.ToString("X4");
            Duration = float.Parse(line.Text(4), NumberStyles.Float);
            Source = state.Actors.GetActor(uint.Parse(line.Text(5), NumberStyles.HexNumber));
            Target = state.Actors.GetActor(uint.Parse(line.Text(7), NumberStyles.HexNumber));
            Count = int.Parse(line.Text(9), NumberStyles.HexNumber);
        }
    }
}
