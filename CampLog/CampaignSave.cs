﻿using System;
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
        public Dictionary<Guid, int> topic_refs;
        public Calendar calendar;
        public CharacterSheet character_sheet;
        public bool show_past_events;
        public bool show_inactive_tasks;

        public CampaignSave(Calendar calendar, CharacterSheet character_sheet) {
            this.domain = new CampaignDomain();
            this.categories = new Dictionary<string, ElementReference<ItemCategory>>();
            this.items = new Dictionary<string, ElementReference<ItemSpec>>();
            this.topic_refs = new Dictionary<Guid, int>();
            this.calendar = calendar;
            this.character_sheet = character_sheet;
            this.show_past_events = false;
            this.show_inactive_tasks = false;
        }

        public void add_category_reference(ItemSpec item) {
            if (!this.categories.ContainsKey(item.category.name)) {
                this.categories[item.category.name] = new ElementReference<ItemCategory>(item.category);
            }
            this.categories[item.category.name].ref_count += 1;
        }

        public void remove_category_reference(ItemSpec item) {
            if (!this.categories.ContainsKey(item.category.name)) { return; }
            this.categories[item.category.name].ref_count -= 1;
        }

        public void add_item_reference(ItemSpec item) {
            if (!this.items.ContainsKey(item.name)) {
                this.items[item.name] = new ElementReference<ItemSpec>(item);
                // we weren't tracking this item before, so add a category reference
                this.add_category_reference(item);
            }
            this.items[item.name].ref_count += 1;
        }

        public void add_topic_reference(Guid topic) {
            if (!this.topic_refs.ContainsKey(topic)) {
                this.topic_refs[topic] = 0;
            }
            this.topic_refs[topic] += 1;
        }

        public void remove_topic_reference(Guid topic) {
            if (!this.topic_refs.ContainsKey(topic)) { return; }
            this.topic_refs[topic] -= 1;
        }

        public void add_references(List<EntryAction> actions) {
            foreach (EntryAction action in actions) {
                if (action is ActionInventoryEntryAdd item_add_action) {
                    ItemSpec item = item_add_action.entry.item;
                    this.add_item_reference(item);
                    continue;
                }
                if (action is ActionNoteCreate note_add_action) {
                    foreach (Guid topic in note_add_action.note.topics) {
                        this.add_topic_reference(topic);
                    }
                    continue;
                }
                if (action is ActionNoteUpdate note_update_action) {
                    foreach (Guid topic in note_update_action.adjust_topics.Keys) {
                        this.add_topic_reference(topic);
                    }
                    continue;
                }
            }
        }

        public void remove_references(List<EntryAction> actions) {
            foreach (EntryAction action in actions) {
                if (action is ActionInventoryEntryAdd item_add_action) {
                    ItemSpec item = item_add_action.entry.item;
                    if (this.items.ContainsKey(item.name)) { this.items[item.name].ref_count -= 1; }
                    continue;
                }
                if (action is ActionNoteCreate note_add_action) {
                    foreach (Guid topic in note_add_action.note.topics) {
                        this.remove_topic_reference(topic);
                    }
                    continue;
                }
                if (action is ActionNoteUpdate note_update_action) {
                    foreach (Guid topic in note_update_action.adjust_topics.Keys) {
                        this.remove_topic_reference(topic);
                    }
                    continue;
                }
            }
        }
    }
}