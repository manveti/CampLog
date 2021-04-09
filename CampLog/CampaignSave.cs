using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace CampLog {
    [Serializable]
    public class CampaignSave {
        public CampaignDomain domain;
        public Calendar calendar;
        public CharacterSheet character_sheet;
        public bool show_past_events;
        public bool show_inactive_tasks;

        public CampaignSave(Calendar calendar, CharacterSheet character_sheet) {
            this.domain = new CampaignDomain();
            this.calendar = calendar;
            this.character_sheet = character_sheet;
            this.show_past_events = false;
            this.show_inactive_tasks = false;
        }
    }
}