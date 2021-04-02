using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace CampLog {
    [Serializable]
    public class CampaignSave {
        public CampaignDomain domain;
        public Calendar calendar;
        //TODO: public CharacterSheet character_sheet;
        public bool show_past_events;
        public bool show_inactive_tasks;

        public CampaignSave(Calendar calendar) {
            this.domain = new CampaignDomain();
            this.calendar = calendar;
            //TODO: character_sheet
            this.show_past_events = false;
            this.show_inactive_tasks = false;
        }
    }
}