using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
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
        enum AbilityFlags : uint
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
            ActionId = 0x1B,
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
         * F1730006: parried rearing rampage
         * mount Eden: 128
         * 
         * target id: E0000000 is an AOE miss
         */

        public Actor Initiator { get; }

        public uint AbilityId { get; }
        public string AbilityName { get { return NameEntry.Actions.ContainsKey(AbilityId & 0xFFFF) ? NameEntry.Actions[AbilityId & 0xFFFF].Name : AbilityId.ToString("X4"); } }

        public Actor Target { get; }

        public List<Effect> Effects { get; }  = new List<Effect>();

        public Effect FirstEffect { get { return (Effects.Count > 0) ? Effects[0] : new Effect(0, 0); } }

        public uint CorrelationId { get; } // correlate with other lines
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
                        (uint)AbilityFlags.TargetStatusGrant => BuffName,
                        (uint)AbilityFlags.SourceStatusGrant => BuffName,
                        (uint)AbilityFlags.TargetStatusRemove => BuffName,
                        (uint)AbilityFlags.SourceStatusRemove => BuffName,
                        (uint)AbilityFlags.GaugeCharge => GaugeName,
                        _ => StatusValue.ToString(),
                    };
                }
            }

            public String DescData
            {
                get { return (Desc >> 8).ToString("X6"); }
            }

            public String BuffName
            {
                get
                {
                    if (NameEntry.Statuses.ContainsKey(StatusValue))
                        return NameEntry.Statuses[StatusValue].Name;
                    else
                        return (StatusValue & 0x7FFF).ToString("X4");
                }
            }

            public String GaugeName
            {
                get
                {
                    if (Enum.IsDefined(typeof(Gauge), StatusValue))
                        return ((Gauge)StatusValue).ToString();
                    else
                        return (StatusValue & 0x7FFF).ToString("X4");
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

            public uint StatusValue
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

            CorrelationId = uint.Parse(source.Text(44), NumberStyles.HexNumber);
            HitIndex = uint.Parse(source.Text(45), NumberStyles.HexNumber);
            HitTotal = uint.Parse(source.Text(46), NumberStyles.HexNumber);
        }
    }
}
