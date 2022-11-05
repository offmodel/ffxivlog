using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Offmodel.FFXIV.Log.Model
{
    public class Director : LogEvent
    {
        public enum ContentTypes: uint
        {
            BattleLeve = 0x8001,
            GatheringLeve = 0x8002,
            Instance = 0x8003,
            Field = 0x8004, //?
            QuestBattle = 0x8006, //?
            CompanyLeve = 0x8007,
            TreaureHunt = 0x8009,
            GoldSaucer = 0x800A,
            CompanyCraft = 0x800B, // airship?
            DpsChallenge = 0x800D, // stone sky sea
            Fate = 0x801A
        }

        public enum Commands: uint
        {
            Commence = 0x40000001, // or continue after a boss fight... param 1 is a timer
            Complete = 0x40000002, // used for variant dungeon
            Clear = 0x40000003, // used for dungeons/raids
            Fadeout = 0x40000005,
            BarrierDown = 0x40000006, // also resumes timer, param 1 is duty timer
            VoteOptions = 0x40000007, // param 1: vote abandon; param 2: vote dismiss
            InitVote = 0x40000008,
            FinishVote = 0x40000009,
            PartyInvite = 0x4000000A,
            DutyGate = 0x4000000C, // ? fires at very start of raid
            NewToDuty = 0x4000000D,
            LevelUp = 0x4000000E,
            Fadein = 0x4000000F, // param 1 is duty timer
            BarrierUp = 0x40000011, // also pauses timer
            PartyChange = 0x40000012, // param 1 is number of party members elgible for rewards
            unk4013 = 0x40000013, // happens with fadein/barrier up in raids/ults
            MusicChange = 0x80000001,
            TimerSync = 0x80000004, // sync client timer with server, param 1 is duty timer, happens every five minutes elapsed
            unk8008 = 0x80000008,
            unk8009 = 0x80000009,
            BossLBCharge = 0x8000000C, // idk what happens when this is charged?
            unk800D = 0x8000000D, // Aglaia first boss arena changes?
            unk800E = 0x8000000E, // voice lines in Aglaia???
            ZoneOpen = 0x80000015, 
            ZoneComplete = 0x80000016,
            alli801A = 0x8000001A, // aglaia scales cutscene start and end
            alli801B = 0x8000001B,
            var8020 = 0x80000020, // variant dungeon events of some sort
            var8021 = 0x80000021,
        }

        public uint ContentType { get; }
        public uint Command { get; }
        public uint Content { get; }
        public uint Param1 { get; }
        public uint Param2 { get; }
        public uint Param3 { get; }
        public uint Param4 { get; }

        public string ContentTypeName
        {
            get
            {
                if (Enum.IsDefined(typeof(ContentTypes), ContentType))
                    return ((ContentTypes) ContentType).ToString();
                else
                    return ContentType.ToString("X4");
            }
        }

        public string CommandName
        {
            get
            {
                if (Enum.IsDefined(typeof(Commands), Command))
                    return ((Commands) Command).ToString();
                else
                    return Command.ToString("X4");
            }
        }

        public Director(LogLine line) : base(line)
        {
            uint contentDetails = uint.Parse(line.Text(2), NumberStyles.HexNumber);
            ContentType = contentDetails >> 16;
            Content = contentDetails & 0xFFFF;
            Command = uint.Parse(line.Text(3), NumberStyles.HexNumber);
            Param1 = uint.Parse(line.Text(4), NumberStyles.HexNumber);
            Param2 = uint.Parse(line.Text(5), NumberStyles.HexNumber);
            Param3 = uint.Parse(line.Text(6), NumberStyles.HexNumber);
            Param4 = uint.Parse(line.Text(7), NumberStyles.HexNumber);
        }
    }
}
