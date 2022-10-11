using System;
using System.Globalization;

namespace Offmodel.FFXIV.Log.Model
{
    public class Actor
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

        public int Id { get; private set; }
        public string Name { get; private set; }

        public ActorJob Job { get; private set; }

        public int Level { get; private set; }

        public Position Position { get; private set; }

        public Actor Owner { get; private set; }

        public string World { get; private set; }

        public int NPCNameId { get; private set; }
        public int NPCBaseId { get; private set; }

        public int HP { get; private set; }
        public int HPMax { get; private set; }
        public int MP { get; private set; }
        public int MPMax { get; private set; }

        public Actor(LogLine line)
        {
        }

        /** message type 2 **/
        private void UpdateFromChangePlayer(State state, LogLine line)
        {
            Id = Int32.Parse(line.Text(2), NumberStyles.HexNumber);
            Name = line.Text(3);
            state.PlayerId = Id;
        }

        /** message type 3 **/
        private void UpdateFromUpdateCombatant(State state, LogLine line)
        {
            Id = Int32.Parse(line.Text(2), NumberStyles.HexNumber);
            Name = line.Text(3);
            Job = (ActorJob)Int32.Parse(line.Text(4), NumberStyles.HexNumber);
            Level = Int32.Parse(line.Text(5), NumberStyles.HexNumber);

            int ownerId = Int32.Parse(line.Text(6), NumberStyles.HexNumber);
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

            NPCNameId = Int32.Parse(line.Text(9));
            NPCBaseId = Int32.Parse(line.Text(10));
            HP = Int32.Parse(line.Text(11));
            HPMax = Int32.Parse(line.Text(12));
            MP = Int32.Parse(line.Text(13));
            MPMax = Int32.Parse(line.Text(14));

            // TP/TPMax?

            Position = new Position(line, 17);
        }
    }
}