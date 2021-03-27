﻿using System;
using System.Collections.Generic;

namespace CampLog {
    [Serializable]
    public class CampaignState {
        public CharacterDomain characters;
        public InventoryDomain inventories;
        public Dictionary<Guid, Guid> character_inventory;
        public NoteDomain notes;
        public TaskDomain tasks;
        public CalendarEventDomain events;

        public CampaignState() {
            this.characters = new CharacterDomain();
            this.inventories = new InventoryDomain();
            this.character_inventory = new Dictionary<Guid, Guid>();
            this.notes = new NoteDomain();
            this.tasks = new TaskDomain();
            this.events = new CalendarEventDomain();
        }
    }


    [Serializable]
    public class CampaignDomain {
        public CampaignState state;
        public List<Entry> entries;
        public Dictionary<Guid, Topic> topics;
        public List<ExternalNote> notes;

        public CampaignDomain() {
            this.state = new CampaignState();
            this.entries = new List<Entry>();
            this.topics = new Dictionary<Guid, Topic>();
            this.notes = new List<ExternalNote>();
        }
    }
}