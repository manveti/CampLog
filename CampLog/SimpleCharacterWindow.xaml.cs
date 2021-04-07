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

        public SimpleCharacterWindow(CampaignSave save_state, Guid? guid = null) {
            this.valid = false;
            this.save_state = save_state;
            this.state = save_state.domain.state.copy();
            this.actions = new List<EntryAction>();
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

        private void add_action(EntryAction action) {
            if (action is ActionCharacterSetInventory inventory_set_action) {
                // see if there's an existing inventory set action we can replace
                for (int i = 0; i < this.actions.Count; i++) {
                    if (this.actions[i] is ActionCharacterSetInventory existing_action) {
                        this.actions[i] = new ActionCharacterSetInventory(existing_action.guid, existing_action.from, inventory_set_action.to);
                        return;
                    }
                }
            }
            else if (action is ActionCharacterSet character_set_action) {
                // see if there's an existing character set action we can replace; remove all property actions
                bool updated = false;
                List<EntryAction> new_actions = new List<EntryAction>();
                foreach (EntryAction existing_action in this.actions) {
                    if ((existing_action is ActionCharacterPropertyAdjust) || (existing_action is ActionCharacterPropertySet)) { continue; }
                    if (existing_action is ActionCharacterSet existing_set_action) {
                        new_actions.Add(new ActionCharacterSet(existing_set_action.guid, existing_set_action.from, character_set_action.to));
                        updated = true;
                    }
                    else {
                        new_actions.Add(existing_action);
                    }
                }
                this.actions = new_actions;
                if (updated) { return; }
            }
            else if ((action is ActionCharacterPropertyAdjust) || (action is ActionCharacterPropertySet)) {
                // check for existing actions we can update
                ActionCharacterPropertyAdjust prop_adj_action = action as ActionCharacterPropertyAdjust;
                ActionCharacterPropertySet prop_set_action = action as ActionCharacterPropertySet;
                List<string> path = prop_adj_action?.path ?? prop_set_action?.path;
                int adj_idx = -1, set_idx = -1;
                for (int i = 0; i < this.actions.Count; i++) {
                    if (this.actions[i] is ActionCharacterSet existing_action) {
                        // there's an existing character set action, so update it
                        if (prop_set_action is null) { throw new ArgumentOutOfRangeException(nameof(action)); }
                        existing_action.to.set_property(path, prop_set_action.to.copy());
                        return;
                    }
                    if (this.actions[i] is ActionCharacterPropertySet existing_set_action) {
                        //TODO: if set_action operates on same path as action: set_idx = i
                    }
                    if (this.actions[i] is ActionCharacterPropertyAdjust existing_adj_action) {
                        //TODO: if adjust_action operates on same path as action: adjust_idx = i
                    }
                }
                //TODO: replace existing set or adjust action
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
