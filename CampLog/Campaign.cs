using System;
using System.Collections.Generic;

namespace CampLog {
    [Serializable]
    public class CampaignState {
        public CharacterDomain characters;
        public InventoryDomain inventories;
        public Dictionary<Guid, Guid> character_inventory;
        public NoteDomain notes;
        public TaskDomain tasks;

        public CampaignState() {
            this.characters = new CharacterDomain();
            this.inventories = new InventoryDomain();
            this.character_inventory = new Dictionary<Guid, Guid>();
            this.notes = new NoteDomain();
            this.tasks = new TaskDomain();
        }
    }
}