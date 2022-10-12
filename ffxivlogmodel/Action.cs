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
    class Action
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

        /**
         * Known class supplementary statuses for 0x0E:
         *  0: tech finish esprit (738), standard finish esprit (737), infuriate (769), swiftcast (A7), 
         *      sharpcast (363), standard step (71A), tech step (71B), superbolide (72C), recitation (768),
         *      emergency tactics (318)
          *  1: GCD shield, (129 = galvanize, 77E = catalyze)
         *  5: brotherhood (4A1), tech finish (71E), standard finish/tiliana (71D)
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
         */

        /**
         * Known statuses for 0x0F:
         *  A58000: F3 Proc
         * 3638000: Sharpcast
         * 1998000: Holmgang (on target)
         */

        /**
         * F1730006: parried rearing rampage
         * mount Eden: 128
         * 
         * target id: E0000000 is an AOE miss
         */

        public Actor Initiator { get; private set; }

        uint AbilityId;
        string AbilityName;

        Actor Target;

        List<Effect> Effects = new List<Effect>();

        uint ActionId; // correlate with other lines
        uint HitIndex;
        uint HitTotal;

        class Effect
        {
            public uint Desc { get; }
            public uint Data { get; }

            public Effect(uint desc, uint data)
            {
                this.Desc = desc;
                this.Data = data;
            }

            public uint Type { get { return Desc & 0xFF; } }

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
                    switch (Type)
                    {
                        case (uint)AbilityFlags.Damage:
                        case (uint)AbilityFlags.BlockedDamage:
                        case (uint)AbilityFlags.ParriedDamage:
                            return true;

                        default:
                            return false;
                    }
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

        public void ReadAction(LogLine source, State state)
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
