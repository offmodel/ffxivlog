using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Offmodel.FFXIV.Log.Model.Action;

namespace Offmodel.FFXIV.Log.Model
{
    /** 
     * Event when an action actually "happens" if the action affects health/mp/buffs.
     * Note that if multiple targets are affected, there will be a sync for each.
     * This can mean two syncs for an action with a single target, if the action also
     * affects the source (e.g. dance partner).
     **/

    class ActionSync: LogEvent
    {
        public uint ActionId { get; }

        public Actor Target { get; }

        public ActionSync(LogLine source, State state): base(source)
        {
            Target = state.Actors.GetActor(uint.Parse(source.Text(2), NumberStyles.HexNumber));
            ActionId = uint.Parse(source.Text(4), NumberStyles.HexNumber);
        }
    }
}
