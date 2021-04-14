using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using GUIx;

namespace CampLog {
    public partial class EntryWindow : Window {
        private static readonly TimeSpan SESSION_DOWNTIME_THRESHOLD = new TimeSpan(12, 0, 0);

        public bool valid;
        private CampaignSave save_state;
        private int entry;
        private List<Entry> entries;
        private int previous_entry_idx = -1;
        private CampaignState state;
        public List<EntryAction> actions;
        public ICalendarControl timestamp_box;
        private ICalendarControl timestamp_diff_box;
        private CharacterListControl character_list;
        private InventoryListControl inventory_list;

        public EntryWindow(CampaignSave save_state, int entry = -1, List<EntryAction> actions = null) {
            this.valid = false;
            this.save_state = save_state;
            this.entry = entry;
            this.entries = new List<Entry>();
            for (int i = 0; i < save_state.domain.entries.Count; i++) {
                if (i != entry) { this.entries.Add(save_state.domain.entries[i]); }
            }
            this.state = save_state.domain.state.copy();
            Entry previous_entry = null, current_entry;
            if ((entry >= 0) && (entry < save_state.domain.entries.Count)) {
                current_entry = save_state.domain.entries[entry];
                if (entry > 0) {
                    this.previous_entry_idx = entry - 1;
                    previous_entry = this.entries[entry - 1];
                }
            }
            else {
                decimal timestamp;
                int session;
                if (save_state.domain.valid_entries > 0) {
                    this.previous_entry_idx = save_state.domain.valid_entries - 1;
                    previous_entry = this.entries[this.previous_entry_idx];
                    timestamp = previous_entry.timestamp + 1;
                }
                else {
                    timestamp = save_state.calendar.default_timestamp;
                }
                session = previous_entry?.session ?? 1;
                if ((previous_entry is not null) && ((DateTime.Now - previous_entry.created) >= SESSION_DOWNTIME_THRESHOLD)) { session += 1; }
                current_entry = new Entry(timestamp, DateTime.Now, "", session, actions);
                if (actions is not null) {
                    foreach (EntryAction action in actions) {
                        action.apply(this.state, current_entry);
                    }
                }
            }
            this.actions = new List<EntryAction>();
            this.add_actions(current_entry.actions, false);
            InitializeComponent();
            this.session_box.textBox.VerticalContentAlignment = VerticalAlignment.Center;
            this.session_box.Value = current_entry.session ?? 0;
            this.created_time_box.textBox.VerticalContentAlignment = VerticalAlignment.Center;
            this.created_date_box.SelectedDate = current_entry.created.Date;
            this.created_time_box.Value = current_entry.created.TimeOfDay.TotalSeconds;
            FrameworkElement timestamp_box = save_state.calendar.timestamp_control();
            Grid.SetRow(timestamp_box, 0);
            Grid.SetColumn(timestamp_box, 6);
            this.header_grid.Children.Add(timestamp_box);
            this.timestamp_box = timestamp_box as ICalendarControl;
            if (this.timestamp_box is null) { throw new InvalidOperationException(); }
            this.timestamp_box.calendar_value = current_entry.timestamp;
            FrameworkElement timestamp_diff_box = save_state.calendar.interval_control();
            Grid.SetRow(timestamp_diff_box, 0);
            Grid.SetColumn(timestamp_diff_box, 8);
            this.header_grid.Children.Add(timestamp_diff_box);
            this.timestamp_diff_box = timestamp_diff_box as ICalendarControl;
            if (this.timestamp_diff_box is null) { throw new InvalidOperationException(); }
            this.timestamp_diff_box.calendar_value = (previous_entry is null ? 0 : current_entry.timestamp - previous_entry.timestamp);
            this.timestamp_box.value_changed = this.timestamp_changed;
            this.timestamp_diff_box.value_changed = this.timestamp_diff_changed;
            //TODO: previous entry
            this.description_box.Text = current_entry.description;
            this.action_list.ItemsSource = this.actions;
            //TODO: events
            this.character_list = new CharacterListControl(this.entry_action_callback);
            this.character_list.set_char_sheet(this.save_state.character_sheet);
            this.character_list.set_state(this.state);
            this.character_group.Content = this.character_list;
            this.inventory_list = new InventoryListControl(this.entry_action_callback);
            this.inventory_list.set_state(this.state);
            this.inventory_group.Content = this.inventory_list;
            //TODO: topics, tasks
        }

        private void add_action(EntryAction action) {
            static int path_common_prefix_length(List<string> p1, List<string> p2) {
                int i;
                for (i = 0; (i < p1.Count) && (i < p2.Count); i++) {
                    if (p1[i] != p2[i]) { break; }
                }
                return i;
            }

            if (action is ActionCharacterSet act_char_set) {
                for (int i = this.actions.Count - 1; i >= 0; i--) {
                    if (this.actions[i] is ActionCharacterSet ext_char_set) {
                        if (ext_char_set.guid != act_char_set.guid) { continue; }
                        // existing ActionCharacterSet with this guid; update its "to" field to match this one and we're done
                        this.actions[i] = new ActionCharacterSet(act_char_set.guid, ext_char_set.from, act_char_set.to, ext_char_set.restore);
                        return;
                    }
                    if (this.actions[i] is ActionCharacterPropertySet ext_prop_set) {
                        if (ext_prop_set.guid != act_char_set.guid) { continue; }
                        // existing ActionCharacterPropertySet with this guid; delete it and update new action's "from" field
                        if (ext_prop_set.from is null) {
                            act_char_set.from.remove_property(ext_prop_set.path);
                        }
                        else {
                            act_char_set.from.set_property(ext_prop_set.path, ext_prop_set.from);
                        }
                        this.actions.RemoveAt(i);
                    }
                    if (this.actions[i] is ActionCharacterPropertyAdjust ext_prop_adj) {
                        if ((ext_prop_adj.guids is null) || (ext_prop_adj.guids.Count != 1) || (!ext_prop_adj.guids.Contains(act_char_set.guid))) {
                            continue;
                        }
                        // existing ActionCharacterPropertyAdjust with this guid; delete it and update new action's "from" field
                        CharProperty prop = act_char_set.from.get_property(ext_prop_adj.path);
                        if (ext_prop_adj.add is not null) { prop.subtract(ext_prop_adj.add); }
                        if (ext_prop_adj.subtract is not null) { prop.add(ext_prop_adj.subtract); }
                        this.actions.RemoveAt(i);
                    }
                    if (this.actions[i] is ActionCharacterSetInventory ext_set_inv) {
                        if ((ext_set_inv.guid != act_char_set.guid) || (act_char_set.to is not null)) { continue; }
                        // existing ActionCharacterSetInventory with this guid and we're deleting the character; delete the inventory set action
                        this.actions.RemoveAt(i);
                    }
                }
            }
            else if (action is ActionCharacterPropertySet act_prop_set) {
                for (int i = this.actions.Count - 1; i >= 0; i--) {
                    if (this.actions[i] is ActionCharacterSet ext_char_set) {
                        if (ext_char_set.guid != act_prop_set.guid) { continue; }
                        // existing ActionCharacterSet with this guid; update its "to" field based on new action and we're done
                        if (act_prop_set.to is null) {
                            ext_char_set.to.remove_property(act_prop_set.path);
                        }
                        else {
                            ext_char_set.to.set_property(act_prop_set.path, act_prop_set.to);
                        }
                        return;
                    }
                    if (this.actions[i] is ActionCharacterPropertySet ext_prop_set) {
                        if (ext_prop_set.guid != act_prop_set.guid) { continue; }
                        //TODO: maybe handle ancestor path which contains this path or its immediate parent
                        if (path_common_prefix_length(ext_prop_set.path, act_prop_set.path) < act_prop_set.path.Count) { continue; }
                        // existing ActionCharacterPropertySet with this guid and this or a child path
                        if (ext_prop_set.path.Count == act_prop_set.path.Count) {
                            // action refers to this path; update its "to" field based on new action and we're done
                            this.actions[i] = new ActionCharacterPropertySet(ext_prop_set.guid, ext_prop_set.path, ext_prop_set.from, act_prop_set.to);
                            return;
                        }
                        // action refers to a child path; update new action's "from" field based on it then delete it
                        CharDictProperty dict_prop = act_prop_set.from as CharDictProperty;
                        for (int j = act_prop_set.path.Count; j < ext_prop_set.path.Count - 1; j++) {
                            if ((dict_prop is null) || (!dict_prop.value.ContainsKey(ext_prop_set.path[j]))) {
                                dict_prop = null;
                                break;
                            }
                            dict_prop = dict_prop.value[ext_prop_set.path[j]] as CharDictProperty;
                        }
                        if (dict_prop is not null) {
                            dict_prop.value[ext_prop_set.path[^1]] = ext_prop_set.from;
                        }
                        this.actions.RemoveAt(i);
                    }
                    if (this.actions[i] is ActionCharacterPropertyAdjust ext_prop_adj) {
                        if ((ext_prop_adj.guids is null) || (ext_prop_adj.guids.Count != 1) || (!ext_prop_adj.guids.Contains(act_prop_set.guid))) {
                            continue;
                        }
                        if (path_common_prefix_length(ext_prop_adj.path, act_prop_set.path) < act_prop_set.path.Count) { continue; }
                        // existing ActionCharacterPropertyAdjust with this guid and this or a child path;
                        // update new action's "from" field based on it then delete it
                        CharProperty prop = act_prop_set.from;
                        for (int j = act_prop_set.path.Count; j < ext_prop_adj.path.Count; j++) {
                            if ((prop is CharDictProperty dict_prop) && (dict_prop.value.ContainsKey(ext_prop_adj.path[j]))) {
                                prop = dict_prop.value[ext_prop_adj.path[j]];
                            }
                            else {
                                prop = null;
                                break;
                            }
                        }
                        if (prop is not null) {
                            if (ext_prop_adj.add is not null) { prop.subtract(ext_prop_adj.add); }
                            if (ext_prop_adj.subtract is not null) { prop.add(ext_prop_adj.subtract); }
                        }
                        this.actions.RemoveAt(i);
                    }
                }
            }
            else if (action is ActionCharacterPropertyAdjust act_prop_adj) {
                for (int i = this.actions.Count - 1; i >= 0; i--) {
                    if (this.actions[i] is ActionCharacterSet ext_char_set) {
                        if ((act_prop_adj.guids is null) || (act_prop_adj.guids.Count != 1) || (!act_prop_adj.guids.Contains(ext_char_set.guid))) {
                            continue;
                        }
                        // existing ActionCharacterSet with this (single) guid; update its "to" field based on new action and we're done
                        CharProperty prop = ext_char_set.to.get_property(act_prop_adj.path);
                        if (act_prop_adj.subtract is not null) { prop.subtract(act_prop_adj.subtract); }
                        if (act_prop_adj.add is not null) { prop.add(act_prop_adj.add); }
                        return;
                    }
                    if (this.actions[i] is ActionCharacterPropertySet ext_prop_set) {
                        if ((act_prop_adj.guids is null) || (act_prop_adj.guids.Count != 1) || (!act_prop_adj.guids.Contains(ext_prop_set.guid))) {
                            continue;
                        }
                        if (path_common_prefix_length(ext_prop_set.path, act_prop_adj.path) < ext_prop_set.path.Count) { continue; }
                        // existing ActionCharacterPropertySet with this (single) guid and this or a parent path
                        //TODO: handle ancestor path which contains this path
                        if (ext_prop_set.path.Count != act_prop_adj.path.Count) { continue; }
                        // action refers to this path; update its "to" field based on new action and we're done
                        if (act_prop_adj.subtract is not null) { ext_prop_set.to.subtract(act_prop_adj.subtract); }
                        if (act_prop_adj.add is not null) { ext_prop_set.to.add(act_prop_adj.add); }
                        return;
                    }
                    if (this.actions[i] is ActionCharacterPropertyAdjust ext_prop_adj) {
                        if ((ext_prop_adj.guids is null) != (act_prop_adj.guids is null)) { continue; }
                        if (ext_prop_adj.guids is not null) {
                            if ((ext_prop_adj.guids.Count != act_prop_adj.guids.Count) || (!ext_prop_adj.guids.IsSubsetOf(act_prop_adj.guids))) {
                                continue;
                            }
                        }
                        if (ext_prop_adj.path.Count != act_prop_adj.path.Count) { continue; }
                        if (path_common_prefix_length(ext_prop_adj.path, act_prop_adj.path) < act_prop_adj.path.Count) { continue; }
                        // existing ActionCharacterPropertyAdjust with this guid (or set of guids) and this path; update it based on new action and we're done
                        CharProperty new_subtract, new_add;
                        if (ext_prop_adj.subtract is null) { new_subtract = act_prop_adj.subtract; }
                        else {
                            new_subtract = ext_prop_adj.subtract.copy();
                            if (act_prop_adj.subtract is not null) { new_subtract.add(act_prop_adj.subtract); }
                        }
                        if (ext_prop_adj.add is null) { new_add = act_prop_adj.add; }
                        else {
                            new_add = ext_prop_adj.add.copy();
                            if (act_prop_adj.add is not null) { new_add.add(act_prop_adj.add); }
                        }
                        this.actions[i] = new ActionCharacterPropertyAdjust(ext_prop_adj.guids, ext_prop_adj.path, new_subtract, new_add);
                        return;
                    }
                }
            }
            else if (action is ActionCharacterSetInventory act_char_inv) {
                for (int i = this.actions.Count - 1; i >= 0; i--) {
                    if (this.actions[i] is ActionCharacterSetInventory ext_set_inv) {
                        if (ext_set_inv.guid != act_char_inv.guid) { continue; }
                        // existing ActionCharacterSetInventory with this guid; update its "to" field based on new action and we're done
                        this.actions[i] = new ActionCharacterSetInventory(ext_set_inv.guid, ext_set_inv.from, act_char_inv.to);
                        return;
                    }
                }
            }
            // ActionInventoryCreate: no merging possible
            else if (action is ActionInventoryRemove act_inv_remove) {
                bool omit_new_action = false;
                for (int i = this.actions.Count - 1; i >= 0; i--) {
                    if (this.actions[i] is ActionInventoryCreate ext_inv_create) {
                        if (ext_inv_create.guid == act_inv_remove.guid) {
                            // existing ActionInventoryCreate with this guid; remove it and new action
                            this.actions.RemoveAt(i);
                            omit_new_action = true;
                        }
                    }
                    if (this.actions[i] is ActionInventoryRename ext_inv_rename) {
                        if (ext_inv_rename.guid == act_inv_remove.guid) {
                            // existing ActionInventoryRename with this guid; remove it
                            this.actions.RemoveAt(i);
                        }
                    }
                    if (this.actions[i] is ActionInventoryEntryAdd ext_inv_entry_add) {
                        if (ext_inv_entry_add.inv_guid == act_inv_remove.guid) {
                            // existing ActionInventoryEntryAdd with this guid; remove it
                            this.actions.RemoveAt(i);
                        }
                    }
                    if (this.actions[i] is ActionInventoryEntryRemove ext_inv_entry_rem) {
                        if (ext_inv_entry_rem.inv_guid == act_inv_remove.guid) {
                            // existing ActionInventoryEntryRemove with this guid; remove it
                            this.actions.RemoveAt(i);
                        }
                    }
                    //TODO: ActionItemStackSet, ActionItemStackAdjust, ActionSingleItemSet, ActionSingleItemAdjust with guid in act_inv_remove inventory
                    if (this.actions[i] is ActionInventoryEntryMove ext_inv_entry_move) {
                        if ((ext_inv_entry_move.from_guid == act_inv_remove.guid) || ((ext_inv_entry_move.to_guid == act_inv_remove.guid))) {
                            // existing ActionInventoryEntryMove from or to this guid; remove it
                            this.actions.RemoveAt(i);
                        }
                    }
                    if (this.actions[i] is ActionInventoryEntryMerge ext_inv_entry_merge) {
                        if (ext_inv_entry_merge.inv_guid == act_inv_remove.guid) {
                            // existing ActionInventoryEntryMerge with this guid; remove it
                            this.actions.RemoveAt(i);
                        }
                    }
                    if (this.actions[i] is ActionInventoryEntryUnstack ext_inv_entry_unstack) {
                        if (ext_inv_entry_unstack.inv_guid == act_inv_remove.guid) {
                            // existing ActionInventoryEntryUnstack with this guid; remove it
                            this.actions.RemoveAt(i);
                        }
                    }
                    if (this.actions[i] is ActionInventoryEntrySplit ext_inv_entry_split) {
                        if (ext_inv_entry_split.inv_guid == act_inv_remove.guid) {
                            // existing ActionInventoryEntrySplit with this guid; remove it
                            this.actions.RemoveAt(i);
                        }
                    }
                }
                if (omit_new_action) {
                    return;
                }
            }
            else if (action is ActionInventoryRename act_inv_rename) {
                for (int i = this.actions.Count - 1; i >= 0; i--) {
                    if (this.actions[i] is ActionInventoryCreate ext_inv_create) {
                        if (ext_inv_create.guid == act_inv_rename.guid) {
                            // existing ActionInventoryCreate with this guid; update its "done" field based on new action and we're done
                            this.actions[i] = new ActionInventoryCreate(ext_inv_create.guid, act_inv_rename.to);
                            return;
                        }
                    }
                    if (this.actions[i] is ActionInventoryRemove ext_inv_remove) {
                        if (ext_inv_remove.guid == act_inv_rename.guid) {
                            // existing ActionInventoryRemove with this guid; remove new entry
                            return;
                        }
                    }
                    if (this.actions[i] is ActionInventoryRename ext_inv_rename) {
                        if (ext_inv_rename.guid == act_inv_rename.guid) {
                            // existing ActionInventoryRename with this guid; update its "to" field based on new action and we're done
                            this.actions[i] = new ActionInventoryRename(ext_inv_rename.guid, ext_inv_rename.from, act_inv_rename.to);
                        }
                    }
                }
            }
            //TODO:
            //  ActionInventoryEntryAdd
            //  ActionInventoryEntryRemove
            //  ActionItemStackSet
            //  ActionItemStackAdjust
            //  ActionSingleItemSet
            //  ActionSingleItemAdjust
            //  ActionInventoryEntryMove
            //  ActionInventoryEntryMerge
            //  ActionInventoryEntryUnstack
            //  ActionInventoryEntrySplit
            //  ActionNoteCreate
            //  ActionNoteRemove
            //  ActionNoteRestore
            //  ActionNoteUpdate
            //  ActionTaskCreate
            //  ActionTaskRemove
            //  ActionTaskRestore
            //  ActionTaskUpdate
            //  ActionCalendarEventCreate
            //  ActionCalendarEventRemove
            //  ActionCalendarEventRestore
            //  ActionCalendarEventUpdate
            ///TODO
            this.actions.Add(action);
        }

        private void add_actions(List<EntryAction> actions, bool refresh = true) {
            foreach (EntryAction action in actions) {
                this.add_action(action);
            }
            if (refresh) {
                this.action_list.Items.Refresh();
            }
        }

        public DateTime get_created() {
            DateTime created = this.created_date_box.SelectedDate ?? DateTime.Today;
            created += new TimeSpan(((long)(this.created_time_box.Value)) * TimeSpan.TicksPerSecond);
            return created;
        }

        private void timestamp_changed() {
            decimal timestamp = this.timestamp_box.calendar_value, timestamp_diff;
            Entry ent = new Entry(timestamp, this.get_created(), "");
            int entry_idx = this.entries.BinarySearch(ent);
            if (entry_idx >= 0) {
                this.previous_entry_idx = entry_idx;
            }
            else {
                this.previous_entry_idx = ~entry_idx - 1;
            }
            if (this.previous_entry_idx >= 0) {
                timestamp_diff = timestamp - this.entries[this.previous_entry_idx].timestamp;
            }
            else {
                timestamp_diff = 0;
            }
            this.timestamp_diff_box.calendar_value = timestamp_diff;
            //TODO: update diff entry
        }

        private void timestamp_diff_changed() {
            decimal timestamp, timestamp_diff = this.timestamp_diff_box.calendar_value;
            if (this.previous_entry_idx < 0) {
                this.timestamp_diff_box.calendar_value = 0;
                return;
            }
            timestamp = this.entries[this.previous_entry_idx].timestamp + timestamp_diff;
            this.timestamp_box.calendar_value = timestamp;
            //TODO: update diff entry
        }

        private void action_list_sel_changed(object sender, RoutedEventArgs e) {
            int idx = this.action_list.SelectedIndex;
            if ((idx < 0) || (idx >= this.actions.Count)) {
                this.act_rem_but.IsEnabled = false;
                this.act_edit_but.IsEnabled = false;
                return;
            }
            this.act_rem_but.IsEnabled = true;
            this.act_edit_but.IsEnabled = true;
        }

        private void entry_action_callback(List<EntryAction> actions) {
            if ((actions is null) || (actions.Count <= 0)) { return; }
            decimal timestamp = this.timestamp_box.calendar_value;
            DateTime created = this.get_created();
            string description = this.description_box.Text;
            int session = (int)(this.session_box.Value);
            this.add_actions(actions);
            Entry ent = new Entry(timestamp, created, description, session, new List<EntryAction>(this.actions));
            foreach (EntryAction action in actions) {
                action.apply(this.state, ent);
            }
            //TODO: update events list
            this.character_list.set_state(this.state);
            //TODO: update inventories, topics, tasks lists
        }

        private void do_ok(object sender, RoutedEventArgs e) {
            this.valid = true;
            this.Close();
        }

        private void do_cancel(object sender, RoutedEventArgs e) {
            this.Close();
        }
    }
}
