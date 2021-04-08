using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace CampLog {
    public class PropertyRow {
        public string _name;
        public List<string> _path;
        public string _value;
        public ObservableCollection<PropertyRow> _children;

        public string name { get => this._name; }
        public List<string> path { get => this._path; }
        public string value { get => this._value; }
        public ObservableCollection<PropertyRow> children { get => this._children; }

        public PropertyRow(string name, List<string> path, string value = "") {
            this._name = name;
            this._path = new List<string>(path);
            this._value = value;
            this._children = new ObservableCollection<PropertyRow>();
        }
    }


    public partial class SimpleCharacterWindow : Window {
        public bool valid;
        private CampaignSave save_state;
        private CampaignState state;
        private Character character;
        private Guid? guid;
        ObservableCollection<PropertyRow> property_rows;
        public List<EntryAction> actions;

        private void populate_property_rows(ObservableCollection<PropertyRow> prop_rows, List<string> path, CharDictProperty prop) {
            List<string> prop_names = new List<string>(prop.value.Keys);
            prop_names.Sort();
            foreach (string prop_name in prop_names) {
                List<string> row_path = new List<string>(path);
                row_path.Add(prop_name);
                PropertyRow row = null;
                if (prop.value[prop_name] is CharTextProperty text_prop) {
                    row = new PropertyRow(prop_name, row_path, text_prop.value);
                }
                else if (prop.value[prop_name] is CharNumProperty num_prop) {
                    row = new PropertyRow(prop_name, row_path, num_prop.value.ToString());
                }
                else if (prop.value[prop_name] is CharSetProperty set_prop) {
                    row = new PropertyRow(prop_name, row_path);
                    List<string> members = new List<string>(set_prop.value);
                    members.Sort();
                    foreach (string member in members) {
                        List<string> member_path = new List<string>(row_path);
                        member_path.Add(member);
                        row.children.Add(new PropertyRow(member, member_path));
                    }
                }
                else if (prop.value[prop_name] is CharDictProperty dict_prop) {
                    row = new PropertyRow(prop_name, row_path);
                    this.populate_property_rows(row.children, row_path, dict_prop);
                }
                if (row is not null) {
                    prop_rows.Add(row);
                }
            }
        }

        public SimpleCharacterWindow(CampaignSave save_state, List<EntryAction> actions, Guid? guid = null) {
            this.valid = false;
            this.save_state = save_state;
            this.state = save_state.domain.state.copy();
            this.actions = new List<EntryAction>(actions);
            if (guid is null) {
                ActionCharacterSet add_action = new ActionCharacterSet(Guid.NewGuid(), null, new Character(""));
                this.actions.Add(add_action);
                this.character = add_action.to;
            }
            else {
                this.character = save_state.domain.state.characters.characters[guid.Value];
            }
            this.guid = guid;
            this.property_rows = new ObservableCollection<PropertyRow>();
            this.populate_property_rows(this.property_rows, new List<string>(), this.character.properties);
            InitializeComponent();
            this.name_box.Text = this.character.name;
            this.properties_list.ItemsSource = this.property_rows;
        }

        private static int match_length(List<string> l1, List<string> l2) {
            int i;
            for (i = 0; (i < l1.Count) && (i < l2.Count); i++) {
                if (l1[i] != l2[i]) { break; }
            }
            return i;
        }

        private static bool paths_equal(List<string> p1, List<string> p2) {
            if (p1.Count != p2.Count) { return false; }
            return match_length(p1, p2) == p1.Count;
        }

        private void add_action(EntryAction action) {
            if (action is ActionCharacterSet character_set_action) {
                // see if there's an existing character set action we can replace; remove all property actions
                List<int> remove_indices = new List<int>();
                bool updated = false;
                for (int i = 0; i < this.actions.Count; i++) {
                    if (this.actions[i] is ActionCharacterPropertyAdjust adj_action) {
                        if ((adj_action.guids is not null) && (adj_action.guids.Count == 1) && (adj_action.guids.Contains(character_set_action.guid))){
                            remove_indices.Add(i);
                        }
                    }
                    else if (this.actions[i] is ActionCharacterPropertySet prop_set_action) {
                        if (prop_set_action.guid == character_set_action.guid) {
                            remove_indices.Add(i);
                        }
                    }
                    else if (this.actions[i] is ActionCharacterSet existing_set_action) {
                        if (existing_set_action.guid == character_set_action.guid) {
                            this.actions[i] = new ActionCharacterSet(existing_set_action.guid, existing_set_action.from, character_set_action.to);
                            updated = true;
                        }
                    }
                }
                remove_indices.Reverse();
                foreach (int rem_idx in remove_indices) {
                    this.actions.RemoveAt(rem_idx);
                }
                if (updated) { return; }
            }
            else if (action is ActionCharacterPropertySet prop_set_action) {
                // see if there's an existing action we can update
                for (int i = 0; i < this.actions.Count; i++) {
                    if (this.actions[i] is ActionCharacterSet char_set_action) {
                        if (char_set_action.guid == prop_set_action.guid) {
                            // there's an existing character set action; update it
                            if (prop_set_action.to is null) {
                                char_set_action.to.remove_property(prop_set_action.path);
                            }
                            else {
                                char_set_action.to.set_property(prop_set_action.path, prop_set_action.to.copy());
                            }
                            // if everything's been done right so far, there aren't any other relevant actions to update, and our action is already added
                            return;
                        }
                    }
                    else if (this.actions[i] is ActionCharacterPropertySet existing_set_action) {
                        if (existing_set_action.guid != prop_set_action.guid) { continue; }
                        int prefix_length = match_length(existing_set_action.path, prop_set_action.path);
                        if (prefix_length < existing_set_action.path.Count) { continue; }
                        if (prefix_length >= prop_set_action.path.Count) {
                            // there's an existing property set action for this or a child property; remove it
                            this.actions.RemoveAt(i);
                            // if everything's been done right so far, there aren't any other relevant actions to update, but our action hasn't been added yet
                            break;
                        }
                        // there's an existing property set action for a parent property; update it
                        CharProperty prop = existing_set_action.to;
                        CharDictProperty dict_prop = null;
                        string prop_name = null;
                        for (int j = prefix_length; j < prop_set_action.path.Count; j++) {
                            dict_prop = prop as CharDictProperty;
                            prop_name = prop_set_action.path[j];
                            if (dict_prop is null) { throw new ArgumentOutOfRangeException(nameof(action)); }
                            if (j < prop_set_action.path.Count - 1) {
                                if (!dict_prop.value.ContainsKey(prop_name)) { throw new ArgumentOutOfRangeException(nameof(action)); }
                                prop = dict_prop.value[prop_name];
                            }
                        }
                        // guaranteed at least one pass through loop, so dict_prop and prop_name are not null
                        if (existing_set_action.to is null) {
                            dict_prop.value.Remove(prop_name);
                        }
                        else {
                            dict_prop.value[prop_name] = existing_set_action.to;
                        }
                        // if everything's been done right so far, there aren't any other relevant actions to update, and our action is already added
                        return;
                    }
                    else if (this.actions[i] is ActionCharacterPropertyAdjust existing_adj_action) {
                        if ((existing_adj_action.guids is null) || (existing_adj_action.guids.Count != 1)) { continue; }
                        if (!existing_adj_action.guids.Contains(prop_set_action.guid)) { continue; }
                        if (!paths_equal(existing_adj_action.path, prop_set_action.path)) { continue; }
                        // there's an existing property adjust action; remove it
                        this.actions.RemoveAt(i);
                        // if everything's been done right so far, there aren't any other relevant actions to update, but our action hasn't been added yet
                        break;
                    }
                }
            }
            else if (action is ActionCharacterPropertyAdjust prop_adj_action) {
                // see if there's an existing action we can update
                for (int i = 0; i < this.actions.Count; i++) {
                    if (this.actions[i] is ActionCharacterSet char_set_action) {
                        if ((prop_adj_action.guids is null) || (prop_adj_action.guids.Count != 1)) { continue; }
                        if (!prop_adj_action.guids.Contains(char_set_action.guid)) { continue; }
                        // there's an existing character set action; update it
                        CharProperty prop = char_set_action.to.get_property(prop_adj_action.path);
                        if (prop_adj_action.subtract is not null) { prop.subtract(prop_adj_action.subtract); }
                        if (prop_adj_action.add is not null) { prop.add(prop_adj_action.add); }
                        // if everything's been done right so far, there aren't any other relevant actions to update, and our action is already added
                        return;
                    }
                    else if (this.actions[i] is ActionCharacterPropertySet existing_set_action) {
                        if ((prop_adj_action.guids is null) || (prop_adj_action.guids.Count != 1)) { continue; }
                        if (!prop_adj_action.guids.Contains(existing_set_action.guid)) { continue; }
                        if (existing_set_action.path.Count > prop_adj_action.path.Count) { continue; }
                        int prefix_length = match_length(existing_set_action.path, prop_adj_action.path);
                        if (prefix_length < existing_set_action.path.Count) { continue; }
                        // there's an existing property set action for this or a parent property; update it
                        CharProperty prop = existing_set_action.to;
                        for (int j = prefix_length; j < prop_adj_action.path.Count; j++) {
                            CharDictProperty dict_prop = prop as CharDictProperty;
                            string prop_name = prop_adj_action.path[j];
                            if (dict_prop is null) { throw new ArgumentOutOfRangeException(nameof(action)); }
                            if (j < prop_adj_action.path.Count - 1) {
                                if (!dict_prop.value.ContainsKey(prop_name)) { throw new ArgumentOutOfRangeException(nameof(action)); }
                                prop = dict_prop.value[prop_name];
                            }
                        }
                        if (prop_adj_action.subtract is not null) { prop.subtract(prop_adj_action.subtract); }
                        if (prop_adj_action.add is not null) { prop.add(prop_adj_action.add); }
                        // if everything's been done right so far, there aren't any other relevant actions to update, and our action is already added
                        return;
                    }
                    else if (this.actions[i] is ActionCharacterPropertyAdjust existing_adj_action) {
                        if ((existing_adj_action.guids is null) != (prop_adj_action.guids is null)) { continue; }
                        if (existing_adj_action.guids is not null) {
                            if (existing_adj_action.guids.Count != prop_adj_action.guids.Count) { continue; }
                            if (!existing_adj_action.guids.IsSubsetOf(prop_adj_action.guids)) { continue; }
                        }
                        if (!paths_equal(existing_adj_action.path, prop_adj_action.path)) { continue; }
                        // there's an existing property adjust action; update it
                        CharProperty new_sub = null, new_add = null;
                        if ((existing_adj_action.subtract is null) || (prop_adj_action.subtract is null)) {
                            new_sub = existing_adj_action.subtract ?? prop_adj_action.subtract;
                        }
                        else {
                            new_sub = existing_adj_action.subtract;
                            new_sub.add(prop_adj_action.subtract);
                        }
                        if ((existing_adj_action.add is null) || (prop_adj_action.add is null)) {
                            new_add = existing_adj_action.add ?? prop_adj_action.add;
                        }
                        else {
                            new_add = existing_adj_action.add;
                            new_add.add(prop_adj_action.add);
                        }
                        this.actions[i] = new ActionCharacterPropertyAdjust(existing_adj_action.guids, existing_adj_action.path, new_sub, new_add);
                        // if everything's been done right so far, there aren't any other relevant actions to update, and our action is already added
                        return;
                    }
                }
            }
            else if (action is ActionCharacterSetInventory inventory_set_action) {
                // see if there's an existing inventory set action we can replace
                for (int i = 0; i < this.actions.Count; i++) {
                    if ((this.actions[i] is ActionCharacterSetInventory existing_action) && (existing_action.guid == inventory_set_action.guid)) {
                        this.actions[i] = new ActionCharacterSetInventory(existing_action.guid, existing_action.from, inventory_set_action.to);
                        return;
                    }
                }
            }
            this.actions.Add(action);
        }

        private void properties_list_sel_changed(object sender, RoutedEventArgs e) {
            List<string> path = this.properties_list.SelectedValue as List<string>;
            string member = null;
            CharProperty prop = null;
            try {
                prop = this.character.get_property(path);
            }
            catch (ArgumentException) {
                if (path.Count > 1) {
                    member = path[^1];
                    path.RemoveAt(path.Count - 1);
                    try {
                        prop = this.character.get_property(path);
                    }
                    catch (ArgumentException) { }
                }
            }
            if ((prop is CharTextProperty) || (prop is CharNumProperty)) {
                this.edit_but.Content = "Adjust Value...";
                this.edit_but.IsEnabled = (prop is CharNumProperty);
                this.set_but.Content = "Set Value...";
                this.set_but.IsEnabled = true;
                this.rem_but.IsEnabled = true;
            }
            else if(prop is CharSetProperty) {
                this.edit_but.Content = "Add Element...";
                this.edit_but.IsEnabled = true;
                if (member is null) {
                    this.set_but.Content = "Clear Value";
                }
                else {
                    this.set_but.Content = "Set Element...";
                }
                this.set_but.IsEnabled = true;
                this.rem_but.IsEnabled = true;
            }
            else if (prop is CharDictProperty) {
                this.edit_but.Content = "Add Child...";
                this.edit_but.IsEnabled = true;
                this.set_but.Content = "Clear Value";
                this.set_but.IsEnabled = true;
                this.rem_but.IsEnabled = true;
            }
            else {
                this.edit_but.Content = "Adjust Value...";
                this.edit_but.IsEnabled = false;
                this.set_but.Content = "Set Value...";
                this.set_but.IsEnabled = false;
                this.rem_but.IsEnabled = false;
            }
        }

        //TODO: add_top_level, adjust_value/add_child/add_element, set/clear, remove

        private void do_ok(object sender, RoutedEventArgs e) {
            if (this.name_box.Text != this.character.name) {
                if (this.guid is null) {
                    this.character.name = this.name_box.Text;
                }
                else {
                    ActionCharacterSet action = new ActionCharacterSet(this.guid.Value, this.character, this.character);
                    action.to.name = this.name_box.Text;
                    this.add_action(action);
                }
            }
            this.valid = true;
            this.Close();
        }

        private void do_cancel(object sender, RoutedEventArgs e) {
            this.Close();
        }
    }
}
