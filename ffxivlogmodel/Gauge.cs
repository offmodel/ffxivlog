namespace Offmodel.FFXIV.Log.Model
{
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

    enum Gauge : uint
    {
        BlackMageIncrement = 0x2D,
        BlackMageSwitch = 0x35,
        BlackMageSet = 0x43,
        BlackMageXenoglossy = 0x10C,
        DancerFeather = 0xEB,
        DancerEsprit = 0xEC,
        ScholarDissolveUnion = 0xC2,
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
}
