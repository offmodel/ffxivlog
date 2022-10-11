using System;
using System.Collections.Generic;

namespace Offmodel.FFXIV.Log.Model
{
    public class Actors
    {
        private readonly Dictionary<Int32, Actor> actors;

        public Actors()
        {
            actors = new Dictionary<Int32, Actor>();
        }

        public void AddActor(Actor actor)
        {
            actors.Add(actor.Id, actor);
        }

        public void RemoveActor(Actor actor)
        {
            RemoveActor(actor.Id);
        }

        public void RemoveActor(int id)
        {
            actors.Remove(id);
        }

        public Actor GetActor(int id)
        {
            return actors[id];
        }
    }
}
