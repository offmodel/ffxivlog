using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Offmodel.FFXIV.Log.Model
{
    public class ActorBuffs : LogEvent
    {
        public class BuffItem
        {
            public uint Buff { get; }

            public string BuffName 
            { 
                get
                {
                    if (NameEntry.Statuses.ContainsKey(Buff))
                        return NameEntry.Statuses[Buff].Name;
                    else
                        return (Buff & 0x7FFF).ToString("X4");
                }
            }

            public float Duration { get; }

            public uint BuffData { get; }
            public Actor Source { get; }

            internal BuffItem(uint buff, uint duration, Actor source)
            {
                Buff = buff & 0xFFFF;
                Duration = BitConverter.UInt32BitsToSingle(duration);
                BuffData = buff >> 16;
                Source = source;
            }
        }

        public Actor Actor { get; }
        public List<BuffItem> Buffs = new();

        public ActorBuffs(LogLine line, State state) : base(line)
        {
            Actor = state.Actors.GetActor(uint.Parse(line.Text(2), NumberStyles.HexNumber));
            int count = (line.Length - 19) / 3;
            for (int i = 0; i < count; i++)
            {
                uint desc = uint.Parse(line.Text(18 + i * 3), NumberStyles.HexNumber);
                uint durationUint = uint.Parse(line.Text(19 + i * 3), NumberStyles.HexNumber);
                Actor source = state.Actors.GetActor(uint.Parse(line.Text(20 + i * 3), NumberStyles.HexNumber));
                if (desc != 0)
                {
                    Buffs.Add(new BuffItem(desc, durationUint, source));
                }
            }
        }
    }
}
