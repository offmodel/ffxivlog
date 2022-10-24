using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace Offmodel.FFXIV.Log.Model
{

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

    public class NameEntry
    {
        public static Dictionary<uint, NameEntry> Statuses { get; } = LoadNames(FFXIVNames.statuses);
        public static Dictionary<uint, NameEntry> Actions { get; } = LoadNames(FFXIVNames.actions);

        internal static Dictionary<uint, NameEntry> LoadNames(byte[] source)
        {
            Dictionary<uint, NameEntry> result = new Dictionary<uint, NameEntry>();

            using (StringReader reader = new StringReader(Encoding.UTF8.GetString(source)))
            {
                string line = reader.ReadLine();
                line = reader.ReadLine(); // skip header
                while (line != null)
                {
                    NameEntry name = new(line);
                    if (name.Name.Length > 0)
                    {
                        result.Add(name.Id, name);
                    }
                    line = reader.ReadLine();
                }
            }

            return result;
        }

        public uint Id { get; }
        public string Name { get; }

        internal NameEntry(string line)
        {
            string[] values = line.Split('\t');
            Id = uint.Parse(values[0]);
            Name = values[1];
        }
    }
}
