using System;
using System.Collections.Generic;

namespace Offmodel.FFXIV.Log.Model
{
    public class Actors
    {
        private readonly Dictionary<uint, Actor> actors;

        public Actors()
        {
            actors = new Dictionary<uint, Actor>();
        }

        public void AddActor(Actor actor)
        {
            actors[actor.Id] = actor;
        }

        public void RemoveActor(Actor actor)
        {
            RemoveActor(actor.Id);
        }

        public void RemoveActor(uint id)
        {
            actors.Remove(id);
        }

        public Actor GetActor(uint id)
        {
            return actors.ContainsKey(id) ? actors[id] : null;
        }
    }
}
