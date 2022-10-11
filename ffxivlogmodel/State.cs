namespace Offmodel.FFXIV.Log.Model
{
    public class State
    {
        public Actors Actors { get; } = new Actors();

        public int PlayerId { get; set; }

        public State()
        {
        }
    }
}