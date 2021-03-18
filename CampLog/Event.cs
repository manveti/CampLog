using System;
using System.Collections.Generic;

namespace CampLog {
    [Serializable]
    public abstract class Action {
        public abstract string description { get; }

        public abstract void apply(CampaignState state);
        public abstract void revert(CampaignState state);
    }


    //TODO: Event (collection of Actions, plus description, timestamps, etc.)
}