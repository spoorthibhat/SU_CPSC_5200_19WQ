using System;
using Newtonsoft.Json;

namespace restapi.Models
{
    public class Transition
    {
        public Transition(Event ev, TimecardStatus status) 
        {
            this.OccurredAt = DateTime.UtcNow;
            this.TransitionedTo = status;
            this.Event = ev;
        }

        public Transition(Event ev) : this(ev, TimecardStatus.Draft) {}

        public DateTime OccurredAt { get; private set; }

        public TimecardStatus TransitionedTo { get; private set; }

        [JsonProperty("detail")]
        public Event Event { get; private set; }
    }
}