using System;
using System.Globalization;

namespace Offmodel.FFXIV.Log.Model
{
    public class Actor: LogEvent
    {
        public enum ActorJob
        {
            None = 0,
            Gladiator = 1,
            Pugilist = 2,
            Marauder = 3,
            Lancer = 4,
            Archer = 5,
            Conjurer = 6,
            Thaumaturge = 7,
            Carpenter = 8,
            Blacksmith = 9,
            Armorer = 10,
            Goldsmith = 11,
            Leatherworker = 12,
            Weaver = 13,
            Alchemist = 14,
            Cluinarian = 15,
            Miner = 16,
            Botanist = 17,
            Fisher = 18,
            Paladin = 19,
            Monk = 20,
            Warrior = 21,
            Dragoon = 22,
            Bard = 23,
            WhiteMage = 24,
            BlackMage = 25,
            Arcanist = 26,
            Summoner = 27,
            Scholar = 28,
            Rouge = 29,
            Ninja = 30,
            Machinist = 31,
            DarkKnight = 32,
            Astologian = 33,
            Samurai = 34,
            RedMage = 35,
            BlueMage = 36,
            Gunbreaker = 37,
            Dancer = 38,
            Reaper = 39,
            Sage = 40
        };

        public uint Id { get; }

        public string Name { get; }

        public ActorJob Job { get; }

        public uint Level { get; }

        public Position Position { get; }

        public Actor Owner { get; }

        public string World { get; }

        public uint NPCNameId { get; }

        public uint NPCBaseId { get; }

        public uint HP { get; }

        public uint HPMax { get; }

        public uint MP { get; }

        public uint MPMax { get; }

        public bool IsRemove { get; }

        public Actor(LogLine line, State state): base(line)
        {
            Id = uint.Parse(line.Text(2), NumberStyles.HexNumber);
            Name = line.Text(3);
            IsRemove = false;
            uint ownerId;

            switch (EventId)
            {
                case 2:
                    state.PlayerId = Id;
                    state.Actors.AddActor(this);
                    break;

                case 3:
                    Job = (ActorJob)uint.Parse(line.Text(4), NumberStyles.HexNumber);
                    Level = uint.Parse(line.Text(5), NumberStyles.HexNumber);

                    ownerId = uint.Parse(line.Text(6), NumberStyles.HexNumber);
                    if (ownerId != 0)
                    {
                        Owner = state.Actors.GetActor(ownerId);
                    }
                    else
                    {
                        Owner = null;
                    }

                    // ignoring world id
                    World = line.Text(8);

                    NPCNameId = uint.Parse(line.Text(9));
                    NPCBaseId = uint.Parse(line.Text(10));
                    HP = uint.Parse(line.Text(11));
                    HPMax = uint.Parse(line.Text(12));
                    MP = uint.Parse(line.Text(13));
                    MPMax = uint.Parse(line.Text(14));

                    // TP/TPMax?

                    Position = new Position(line, 17);
                    state.Actors.AddActor(this);
                    break;

                case 4:
                    IsRemove = true;
                    Job = (ActorJob)uint.Parse(line.Text(4), NumberStyles.HexNumber);
                    Level = uint.Parse(line.Text(5), NumberStyles.HexNumber);

                    ownerId = uint.Parse(line.Text(6), NumberStyles.HexNumber);
                    if (ownerId != 0)
                    {
                        Owner = state.Actors.GetActor(ownerId);
                    }
                    else
                    {
                        Owner = null;
                    }

                    // ignoring world id
                    World = line.Text(8);

                    NPCNameId = uint.Parse(line.Text(9));
                    NPCBaseId = uint.Parse(line.Text(10));
                    state.Actors.RemoveActor(this);
                    break;
            }
        }
    }
}