using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Offmodel.FFXIV.Log.Model
{
    /***
     * An action (including consequences) taken by a visible actor. Note that the effects of an action are generally 
     * not applied in-game until the ActionSync message, so changes in the meantime can render the action moot.
     * This is how you die with superbolide on cooldown and so on. If there's nothing to apply, there's no sync 
     * message. The sync message can be idenfitied because of a shared action id.
     * 
     * For a spell, this event occurs at the completion of spell casting. Cast start and cancel track the casting 
     * process itself, if it doesn't result in a successful cast. Note that boss cast completions may involve
     * nothing more than an animation effect. There's no id shared between the cast events and each other or this
     * event; matching has to be done based on the actor and ability ids.
     */
    public class Action: LogEvent
    {
        enum AbilityFlags: uint
        {
            None = 0x0,
            Dodge = 0x1,
            Damage = 0x3,
            Heal = 0x4,
            BlockedDamage = 0x05,
            ParriedDamage = 0x06,
            StatusSource = 0x09, //? only seen for deployment tactics
            ManaRestore = 0x0B, // damage is MP restored
            TargetStatusGrant = 0x0E, // other 3 bytes mean something??, data has the effect granted
            SourceStatusGrant = 0x0F, // data has the effect granted
            TargetStatusRemove = 0x10, // speculative
            SourceStatusRemove = 0x11, // e.g. sharpcast, data has the effect consumed
            UA1 = 0x14, // used for tech finish on the recipient of esprit
            TargetAnimation = 0x1B, // speculative: data has the id of the animation
            InstantDeath = 0x33,
            UA3 = 0x37, // used for holmgang
            UA2 = 0x3C, // used for mug, no data?
            GaugeCharge = 0x3D, // 0xFF00 (signed) is the amount of the gauge moved, data is the gauge id
            UA4 = 0x4A, // used for superbolide, data is 1 (set HP?)
            ActionOutcomeMask = 0xFF,
            U1 = 0x400, // shows up when someone has heart of corundum skills on them?
            Shielded = 0x600,
            Critical = 0x2000,
            DirectHit = 0x4000,
            Slashing = 0x10000,
            Piercing = 0x20000,
            Striking = 0x30000,
            Magic = 0x50000,
            FireDamage = 0x100000,
            IceDamage = 0x200000,
#pragma warning disable CA1069 // Enums values should not be duplicated
            CriticalHeal = 0x200000,
#pragma warning restore CA1069 // Enums values should not be duplicated
            WindDamage = 0x300000,
            EarthDamage = 0x400000,
            ThunderDamage = 0x500000,
            WaterDamage = 0x600000,
            Unaspected = 0x700000
        }

        /**
         * Known gauges:
         * EB: Dancer feathers
         * EC: Dancer esprit
         * C3: Scholar fairy
         * ED: Gunbreaker carts
         * 2D: Black mage, modify astral/umbral by value, reset enochian
         * 35: Black mage, switch astral/umbral and set, reset enochian
         * 43: Black mage, set astral/umbral to value, reset enochian
         * EE: White Mage blood lily
         * 9F: Ninja Ninki
         * C9: Ninja Huton (sets to 60s, no amount)
         * D4: Ninja Huton (Phantom Kamaitachi passes 100 for +10s, Armor Crush passes 44 for +30s)
         * 34: Warrior Beast
         */

        enum Gauge: uint
        {
            BlackMageIncrement = 0x2D,
            BlackMageSwitch = 0x35,
            BlackMageSet = 0x43,
            BlackMageXenoglossy = 0x10C,
            DancerFeather = 0xEB,
            DancerEsprit = 0xEC,
            ScholarFairy = 0xC3,
            GunbreakerCart = 0xED,
            WhiteMageBloodLily = 0xEE,
            NinjaNinkiBunshin = 0x9D,
            NinjaNinki = 0x9F,
            NinjaHutonSet = 0xC9,
            NinjaHutonIncrement = 0xD4,
            WarriorBeast = 0x34,
            EnmityShirk = 0xBF,
        }

        /**
         * Known class supplementary statuses for 0x0E:
         *  0: tech finish esprit (738), standard finish esprit (737), infuriate (769), swiftcast (A7), 
         *      sharpcast (363), standard step (71A), tech step (71B), superbolide (72C), recitation (768),
         *      emergency tactics (318), dance partner (720)
          *  1: GCD shield, (129 = galvanize, 77E = catalyze)
         *  5: brotherhood (4A1), tech finish (71E), standard finish/tiliana (71D self, 839 other)
         *  A: trick attack (CB6)
         * 85: divine benison (4C2)
         * D5: seraphic veil (77D)
         * F6: chain (4C5), reprisal (4A9), heart of light (72F), shield samba (722), expedient (A97)
         * 505: mug (27E)
         * 534, 558, 55D, 57E, 59C, 5C1, 5D2, 5E3: Shake it off (5B1)
         * 6400/1400: meditative brotherhood (49E)
         * E72F: Plenary (4C3)
         * E7DA: Dia (74F)
         * F260: biolysis (767)
         * F2D1: whispering dawn (13B)
         * FB0A: seraphic/fey illumination (753/13D)
         * FBF6: feint (4AB) ... phys 10/mag 5
         * F6FB: addle (4B3) ... phys 5/mag 10
         * 300000: Perfect balance (6E)
         * 320000: Hide (266)
         * 550000: str pot (31)
         * 560000: dex pot (31)
         * 580000: int pot (31)
         * 590000: mind pot (31)
         * 1E0000: Expedient (A98) sprint effect
         * 4B0000-540000?: Food (30)
         *
         * Known statuses for 0x0F (always xxxx8000):
         *  A5: F3 Proc
         * 363: Sharpcast
         * 199: Holmgang (on target)
         * 71F: Closed Position (4900 supplementary)
         */

        enum BuffDebuff : uint
        {
            Food = 0x30,
            Potion = 0x31,
            Sprint = 0x32,
            Vengeance1 = 0x59,
            Defiance = 0x5B,
            PerfectBalance = 0x6E,
            LifeSurge = 0x74,
            Resurrection = 0x94,
            Regen = 0x9E,
            Fire3Proc = 0xA5,
            Swiftcast = 0xA7,
            Amplifier = 0x10C,
            Galvanize = 0x129,
            WhiserpingDawn = 0x13B,
            FeyIllumination = 0x13D,
            HolmgangTarget = 0x199,
            Mudra = 0x1F0,
            Kassatsu = 0x1F1,
            Hide = 0x266,
            Mug = 0x27E,
            LeyLines = 0x2E1,
            BattleLitany = 0x312,
            Dissipation = 0x317,
            EmergencyTactics = 0x318,
            Sharpcast = 0x363,
            Vengeance2 = 0x390,
            MeditativeBrotherhood = 0x49E,
            Brotherhood = 0x4A1,
            TenChiJin = 0x4A2,
            Rampart = 0x4A7,
            Reprisal = 0x4A9,
            Feint = 0x4AB,
            Peloton = 0x4AF,
            Addle = 0x4B3,
            LucidDremaing = 0x4B4,
            ArmsLength = 0x4B9,
            Triplecast = 0x4BB,
            ThinAir = 0x4C1,
            DivineBenison = 0x4C2,
            PlenaryIndulgence = 0x4C3,
            ChainStrategem = 0x4C5,
            TrueNorth = 0x4E2,
            LeftEye = 0x5AE,
            ShakeItOff = 0x5B1,
            StandardStep = 0x71A,
            TechnicalStep = 0x71B,
            ThreefoldFanDance = 0x71C,
            StandardFinishSelf = 0x71D,
            TechnicalFinish = 0x71E,
            ClosedPosition = 0x71F,
            DancePartner = 0x720,
            Devilment = 0x721,
            ShieldSamba = 0x722,
            Improvisation = 0x723,
            NoMercy = 0x727,
            Camouflage = 0x728,
            RoyalGuard = 0x729,
            Nebula = 0x72A,
            Aurora = 0x72B,
            Superbolide = 0x72C,
            HeartOfLight = 0x72F,
            StandardFinishEsprit = 0x737,
            TechFinishEsprit = 0x738,
            LanceCharge = 0x748,
            Dia = 0x74F,
            Temperance = 0x750,
            AngelWhisper = 0x752,
            SerpahicIllumination = 0x753,
            Biolysis = 0x767,
            Recitation = 0x768,
            NascentChaos = 0x769,
            Asylum = 0x777,
            SerpahicVeil = 0x77D,
            Catalyze = 0x77E,
            SacredSoil = 0x798,
            BunshinClone = 0x7A2,
            StandardFinishPartner = 0x839,
            SurgingTempest = 0xA75,
            Bloodwhetting = 0xA76,
            HeartOfCorundum = 0xA78,
            TechnicalFinishSelf = 0xA8A,
            FourfoldFanDance = 0xA8B,
            Aquaveil = 0xA94,
            Protraction = 0xA96,
            Expedient = 0xA97,
            ExpedientSprint = 0xA98,
            Stormbite = 0x4B1,
            SilkenSymmetry = 0xA85,
            SilkenFlow = 0xA86,
            ImprovisedFinish = 0xA89,
            FlourishingStarfall = 0xA8C,
            PhantomKamaitachiReady = 0xAA3,
            FlourishingSymmetry = 0xBC9,
            FlourishingFlow = 0xBCA,
            TrickAttack = 0xCB6
        }

        /**
         * F1730006: parried rearing rampage
         * mount Eden: 128
         * 
         * target id: E0000000 is an AOE miss
         */

        public Actor Initiator { get; }

        public uint AbilityId { get; }
        public string AbilityName { get; } 

        public Actor Target { get; }

        public List<Effect> Effects { get; }  = new List<Effect>();

        public Effect FirstEffect { get { return (Effects.Count > 0) ? Effects[0] : new Effect(0, 0); } }

        public uint ActionId { get; } // correlate with other lines
        public uint HitIndex { get; }
        public uint HitTotal { get; }

        public class Effect
        {
            public uint Desc { get; }
            public uint Data { get; }

            public Effect(uint desc, uint data)
            {
                this.Desc = desc;
                this.Data = data;
            }

            public uint Type { get { return Desc & 0xFF; } }

            public string TypeName 
            { 
                get 
                { 
                    if (Enum.IsDefined(typeof(AbilityFlags), Type)) 
                        return ((AbilityFlags)Type).ToString(); 
                    else 
                        return Type.ToString();
                } 
            }

            public string EffectValue
            {
                get
                {
                    return Type switch
                    {
                        (uint)AbilityFlags.Damage => Damage.ToString(),
                        (uint)AbilityFlags.BlockedDamage => Damage.ToString(),
                        (uint)AbilityFlags.ParriedDamage => Damage.ToString(),
                        (uint)AbilityFlags.Heal => Damage.ToString(),
                        (uint)AbilityFlags.ManaRestore => Damage.ToString(),
                        (uint)AbilityFlags.TargetStatusGrant => BuffName.ToString(),
                        (uint)AbilityFlags.SourceStatusGrant => BuffName.ToString(),
                        (uint)AbilityFlags.TargetStatusRemove => BuffName.ToString(),
                        (uint)AbilityFlags.SourceStatusRemove => BuffName.ToString(),
                        (uint)AbilityFlags.GaugeCharge => GaugeName.ToString(),
                        _ => Status.ToString(),
                    };
                }
            }

            public String BuffName
            {
                get
                {
                    if (Enum.IsDefined(typeof(BuffDebuff), Status))
                        return ((BuffDebuff)Status).ToString();
                    else
                        return (Status & 0x7FFF).ToString("X4");
                }
            }

            public String GaugeName
            {
                get
                {
                    if (Enum.IsDefined(typeof(Gauge), Status))
                        return ((Gauge)Status).ToString();
                    else
                        return (Status & 0x7FFF).ToString("X4");
                }
            }

            public int Damage 
            {
                get {
                    if ((Data & 0xFF00) == 0x4000) // use third damage byte?
                    {
                        // cactbot notes would have us subtract (TempDamage & 0xFF) from this, but that's not borne out 
                        // by the in-game damage logs
                        return (int)(((Data & 0xFF) << 16) + (Data >> 16) & 0xFFFF);
                    }
                    else if ((Data & 0xFF00) == 0x0000)
                    {
                        return (int)((Data >> 16) & 0xFFFF);
                    }
                    else
                    {
                        return -1;
                    }
                }
            }

            public uint Status
            {
                get
                {
                    return Data >> 16;
                }
            }

            public bool IsDamage
            {
                get
                {
                    return Type switch
                    {
                        (uint)AbilityFlags.Damage => true,
                        (uint)AbilityFlags.BlockedDamage => true,
                        (uint)AbilityFlags.ParriedDamage => true,
                        _ => false
                    };
                }
            }

            public bool IsHeal
            {
                get
                {
                    return Type == (uint) AbilityFlags.Heal;
                }
            }

            public bool IsMagic
            {
                get
                {
                    return IsDamage && ((Desc & 0xF0000) == (uint)AbilityFlags.Magic);
                }
            }

            public bool IsCritical
            {
                get
                {
                    if (IsHeal)
                    {
                        return (Desc & (uint)AbilityFlags.CriticalHeal) != 0;
                    }
                    else if (IsDamage)
                    {
                        return (Desc & (uint)AbilityFlags.Critical) != 0;
                    }
                    else return false;
                }
            }

            public bool IsDirectHit
            {
                get
                {
                    if (IsDamage)
                    {
                        return (Desc & (uint)AbilityFlags.DirectHit) != 0;
                    }
                    else return false;
                }
            }
        }

        private int ParseDamage(string damageString)
        {
            uint TempDamage = uint.Parse(damageString, NumberStyles.HexNumber);
            if ((TempDamage & 0xFF00) == 0x4000) // use third damage byte?
            {
                // cactbot notes would have us subtract (TempDamage & 0xFF) from this, but that's not borne out 
                // by the actual damage messages
                return (int)(((TempDamage & 0xFF) << 16) + (TempDamage >> 16) & 0xFFFF);
            }
            else if ((TempDamage & 0xFF00) == 0x0000)
            {
                return (int)((TempDamage >> 16) & 0xFFFF);
            }
            else
            {
                return -1;
            }
        }

        public Action(LogLine source, State state): base(source)
        {
            Initiator = state.Actors.GetActor(uint.Parse(source.Text(2), NumberStyles.HexNumber));
            // initiator name ignored
            AbilityId = uint.Parse(source.Text(4), NumberStyles.HexNumber);
            AbilityName = source.Text(5);
            Target = state.Actors.GetActor(uint.Parse(source.Text(6), NumberStyles.HexNumber));
            // actor name ignored

            for (int i = 0; i < 8; i++)
            {
                uint desc = uint.Parse(source.Text(8 + i * 2), NumberStyles.HexNumber);
                uint data = uint.Parse(source.Text(9 + i * 2), NumberStyles.HexNumber);
                if (desc != 0 || data != 0)
                {
                    Effects.Add(new Effect(desc, data));
                }
            }

            // target and source location/hp/mp/tp ignored (20 fields)

            ActionId = uint.Parse(source.Text(44), NumberStyles.HexNumber);
            HitIndex = uint.Parse(source.Text(45), NumberStyles.HexNumber);
            HitTotal = uint.Parse(source.Text(46), NumberStyles.HexNumber);
        }
    }
}
