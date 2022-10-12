namespace Offmodel.FFXIV.Log.Model
{
    public class State
    {
        public Actors Actors { get; } = new Actors();

        public uint PlayerId { get; set; }

        public State()
        {
        }
    }
}