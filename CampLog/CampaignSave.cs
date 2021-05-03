using System;
using System.Collections.Generic;

namespace CampLog {
    [Serializable]
    public class ElementReference<T> {
        public T element;
        public int ref_count;

        public ElementReference(T element) {
            this.element = element;
            this.ref_count = 0;
        }
    }


    [Serializable]
    public class CampaignSave {
        public CampaignDomain domain;
        public Dictionary<string, ElementReference<ItemCategory>> categories;
        public Dictionary<string, ElementReference<ItemSpec>> items;
        public Calendar calendar;
        public CharacterSheet character_sheet;
        public bool show_past_events;
        public bool show_inactive_tasks;

        public CampaignSave(Calendar calendar, CharacterSheet character_sheet) {
            this.domain = new CampaignDomain();
            this.categories = new Dictionary<string, ElementReference<ItemCategory>>();
            this.items = new Dictionary<string, ElementReference<ItemSpec>>();
            this.calendar = calendar;
            this.character_sheet = character_sheet;
            this.show_past_events = false;
            this.show_inactive_tasks = false;
        }

        public void add_reference(ItemSpec item) {
            if (!this.items.ContainsKey(item.name)) {
                this.items[item.name] = new ElementReference<ItemSpec>(item);
                // we weren't tracking this item before, so add a category reference
                if (!this.categories.ContainsKey(item.category.name)) {
                    this.categories[item.category.name] = new ElementReference<ItemCategory>(item.category);
                }
                this.categories[item.category.name].ref_count += 1;
            }
            this.items[item.name].ref_count += 1;
        }

        public void add_references(List<EntryAction> actions) {
            foreach (EntryAction action in actions) {
                if (action is ActionInventoryEntryAdd add_action) {
                    ItemSpec item = add_action.entry.item;
                    this.add_reference(item);
                }
            }
        }

        public void remove_references(List<EntryAction> actions) {
            foreach (EntryAction action in actions) {
                if (action is ActionInventoryEntryAdd add_action) {
                    ItemSpec item = add_action.entry.item;
                    if (this.items.ContainsKey(item.name)) { this.items[item.name].ref_count -= 1; }
                }
            }
        }
    }
}