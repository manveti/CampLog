using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;

using GUIx;

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
        private List<string> selected_path;
        private CharProperty selected_prop;
        private string selected_member;

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

        public void repopulate_property(List<string> path) {
            List<string> parent_path = path.GetRange(0, path.Count - 1);
            CharDictProperty parent_prop = this.character.properties;
            ObservableCollection<PropertyRow> parent_rows = this.property_rows;
            foreach (string token in parent_path) {
                if (!parent_prop.value.ContainsKey(token)) { throw new ArgumentOutOfRangeException(nameof(path)); }
                parent_prop = parent_prop.value[token] as CharDictProperty;
                bool need_row = true;
                foreach (PropertyRow row in parent_rows) {
                    if (row.name == token) {
                        parent_rows = row.children;
                        need_row = false;
                        break;
                    }
                }
                if (need_row) { throw new ArgumentOutOfRangeException(nameof(path)); }
                if ((parent_prop is null) || (parent_rows is null)) { throw new ArgumentOutOfRangeException(nameof(path)); }
            }
            parent_rows.Clear();
            this.populate_property_rows(parent_rows, parent_path, parent_prop);
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
            this.selected_path = null;
            this.selected_prop = null;
            this.selected_member = null;
            InitializeComponent();
            this.name_box.Text = this.character.name;
            this.properties_list.ItemsSource = this.property_rows;
        }

        private void properties_list_sel_changed(object sender, RoutedEventArgs e) {
            List<string> path = this.properties_list.SelectedValue as List<string>;
            this.selected_path = null;
            this.selected_prop = null;
            this.selected_member = null;
            if (path is not null) {
                try {
                    this.selected_prop = this.character.get_property(path);
                }
                catch (ArgumentException) {
                    if (path.Count > 1) {
                        this.selected_member = path[^1];
                        path.RemoveAt(path.Count - 1);
                        try {
                            this.selected_prop = this.character.get_property(path);
                        }
                        catch (ArgumentException) { }
                    }
                }
                this.selected_path = new List<string>(path);
            }
            if ((this.selected_prop is CharTextProperty) || (this.selected_prop is CharNumProperty)) {
                this.edit_but.Content = "Adjust Value...";
                this.edit_but.IsEnabled = (this.selected_prop is CharNumProperty);
                this.set_but.Content = "Set Value...";
                this.set_but.IsEnabled = true;
                this.rem_but.IsEnabled = true;
            }
            else if(this.selected_prop is CharSetProperty) {
                this.edit_but.Content = "Add Element...";
                this.edit_but.IsEnabled = true;
                if (this.selected_member is null) {
                    this.set_but.Content = "Clear Elements";
                }
                else {
                    this.set_but.Content = "Set Element...";
                }
                this.set_but.IsEnabled = true;
                this.rem_but.IsEnabled = true;
            }
            else if (this.selected_prop is CharDictProperty) {
                this.edit_but.Content = "Add Child...";
                this.edit_but.IsEnabled = true;
                this.set_but.Content = "Clear Children";
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

        private void do_add(object sender, RoutedEventArgs e) {
            SimpleCharacterPropertyWindow prop_win = new SimpleCharacterPropertyWindow();
            prop_win.ShowDialog();
            if (!prop_win.valid) { return; }
            if (this.character.properties.value.ContainsKey(prop_win.name)) {
                MessageBox.Show("Property " + prop_win.name + " already exists.", "Error", MessageBoxButton.OK);
                return;
            }
            CharProperty new_prop = prop_win.get_property();
            if (new_prop is null) { return; }
            List<string> new_path = new List<string>() { prop_win.name };
            if (this.guid is not null) {
                this.actions.Add(new ActionCharacterPropertySet(this.guid.Value, new_path, null, new_prop));
            }
            this.character.set_property(new_path, new_prop);
            this.property_rows.Clear();
            this.populate_property_rows(this.property_rows, new List<string>(), this.character.properties);
        }

        private void do_edit(object sender, RoutedEventArgs e) {
            if ((this.selected_prop is null) || (this.selected_prop is CharTextProperty)) { return; }
            CharProperty add_prop = null;
            if (this.selected_prop is CharNumProperty num_prop) {
                double? new_value = SimpleDialog.askFloat(this.selected_path[^1], "New value:", (double)(num_prop.value), this);
                if (new_value is not null) {
                    add_prop = new CharNumProperty(((decimal)(new_value.Value)) - num_prop.value);
                    num_prop.value = (decimal)(new_value.Value);
                }
                else { return; }
            }
            else if (this.selected_prop is CharSetProperty set_prop) {
                // set property selected; add an element
                string new_value = SimpleDialog.askString(this.selected_path[^1], "New element:", owner: this);
                if (new_value is not null) {
                    set_prop.value.Add(new_value);
                    add_prop = new CharSetProperty(new HashSet<string>() { new_value });
                }
                else { return; }
            }
            else if (this.selected_prop is CharDictProperty dict_prop) {
                // dict property selected; add a child property
                SimpleCharacterPropertyWindow prop_win = new SimpleCharacterPropertyWindow();
                prop_win.ShowDialog();
                if (!prop_win.valid) { return; }
                if (dict_prop.value.ContainsKey(prop_win.name)) {
                    MessageBox.Show("Property " + prop_win.name + " already exists.", "Error", MessageBoxButton.OK);
                    return;
                }
                CharProperty new_prop = prop_win.get_property();
                if (new_prop is not null) {
                    List<string> new_path = new List<string>(this.selected_path) { prop_win.name };
                    if (this.guid is not null) {
                        this.actions.Add(new ActionCharacterPropertySet(this.guid.Value, new_path, null, new_prop));
                    }
                    this.character.set_property(new_path, new_prop);
                    add_prop = new CharDictProperty(new Dictionary<string, CharProperty>() { [prop_win.name] = new_prop });
                }
                else { return; }
            }
            else { return; }
            if (this.guid is not null) {
                this.actions.Add(new ActionCharacterPropertyAdjust(this.guid.Value, this.selected_path, null, add_prop));
            }
            this.repopulate_property(this.selected_path);
        }

        private void do_set(object sender, RoutedEventArgs e) {
            if (this.selected_prop is null) { return; }
            CharProperty original_prop = this.selected_prop.copy();
            if (this.selected_prop is CharTextProperty text_prop) {
                string new_value = SimpleDialog.askString(this.selected_path[^1], "New value:", text_prop.value, this);
                if (new_value is not null) {
                    text_prop.value = new_value;
                }
                else { return; }
            }
            else if (this.selected_prop is CharNumProperty num_prop) {
                double? new_value = SimpleDialog.askFloat(this.selected_path[^1], "New value:", (double)(num_prop.value), this);
                if (new_value is not null) {
                    num_prop.value = (decimal)(new_value.Value);
                }
                else { return; }
            }
            else if (this.selected_prop is CharSetProperty set_prop) {
                if (this.selected_member is null) {
                    // set property selected; clear it
                    set_prop.value.Clear();
                }
                else {
                    // single member selected; set it
                    string new_value = SimpleDialog.askString(this.selected_path[^1], "New value:", this.selected_member, this);
                    if (new_value is not null) {
                        set_prop.value.Remove(this.selected_member);
                        set_prop.value.Add(new_value);
                    }
                    else { return; }
                }
            }
            else if (this.selected_prop is CharDictProperty dict_prop) {
                // dict property selected; clear it
                dict_prop.value.Clear();
            }
            else { return; }
            if (this.guid is not null) {
                this.actions.Add(new ActionCharacterPropertySet(this.guid.Value, this.selected_path, original_prop, this.selected_prop));
            }
            this.repopulate_property(this.selected_path);
        }

        private void do_rem(object sender, RoutedEventArgs e) {
            if (this.selected_prop is null) { return; }
            if (this.selected_member is null) {
                if (this.guid is not null) {
                    this.actions.Add(new ActionCharacterPropertySet(this.guid.Value, this.selected_path, this.selected_prop, null));
                }
                this.character.remove_property(this.selected_path);
            }
            else {
                CharSetProperty set_prop = this.selected_prop as CharSetProperty;
                if (set_prop is null) { return; }
                set_prop.value.Remove(this.selected_member);
                if (this.guid is not null) {
                    CharSetProperty sub_prop = new CharSetProperty(new HashSet<string>() { this.selected_member });
                    this.actions.Add(new ActionCharacterPropertyAdjust(this.guid.Value, this.selected_path, sub_prop, null));
                }
            }
            this.repopulate_property(this.selected_path);
        }

        //TODO: inventory association

        private void do_ok(object sender, RoutedEventArgs e) {
            if (this.name_box.Text != this.character.name) {
                if (this.guid is null) {
                    this.character.name = this.name_box.Text;
                }
                else {
                    ActionCharacterSet action = new ActionCharacterSet(this.guid.Value, this.character, this.character);
                    action.to.name = this.name_box.Text;
                    this.actions.Add(action);
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
