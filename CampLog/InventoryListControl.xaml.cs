using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace CampLog {
    public class InventoryRow {
        public Guid _guid;
        public string _name;
        public string _value;
        public string _weight;

        public Guid guid { get => this._guid; }
        public string name { get => this._name; }
        public string value { get => this._value; }
        public string weight { get => this._weight; }

        public InventoryRow(Guid guid, string name, string value, string weight) {
            this._guid = guid;
            this._name = name;
            this._value = value;
            this._weight = weight;
        }
    }


    public partial class InventoryListControl : UserControl {
        private Action<List<EntryAction>> change_callback;
        private CampaignState state;
        private ObservableCollection<InventoryRow> inventory_rows;

        public InventoryListControl(Action<List<EntryAction>> change_callback) {
            this.change_callback = change_callback;
            this.state = null;
            this.inventory_rows = null;
            InitializeComponent();
        }

        private void populate_inventory_rows() {
            List<InventoryRow> rows = new List<InventoryRow>();
            foreach (Guid guid in state.inventories.inventories.Keys) {
                string name = state.inventories.inventories[guid].name;
                if ((name is null) && (state.character_inventory.ContainsValue(guid))){
                    foreach (Guid char_guid in state.character_inventory.Keys) {
                        if (state.character_inventory[char_guid] == guid) {
                            name = state.characters.characters[char_guid].name + "'s Inventory";
                            break;
                        }
                    }
                }
                if (name is null) { name = "(Unnamed Inventory)"; }
                Inventory inv = state.inventories.inventories[guid];
                rows.Add(new InventoryRow(guid, name, inv.value.ToString(), inv.weight.ToString()));
            }
            rows.Sort((InventoryRow r1, InventoryRow r2) => r1.name.CompareTo(r2.name));
            this.inventory_rows = new ObservableCollection<InventoryRow>(rows);
            this.inventory_list.ItemsSource = this.inventory_rows;
        }

        public void set_state(CampaignState state) {
            this.state = state;
            if (this.state is null) {
                this.add_but.IsEnabled = false;
                this.rem_but.IsEnabled = false;
                this.view_but.IsEnabled = false;
                this.inventory_rows.Clear();
                return;
            }
            this.populate_inventory_rows();
            this.add_but.IsEnabled = true;
        }

        private void list_sel_changed(object sender, RoutedEventArgs e) {
            Guid? guid = this.inventory_list.SelectedValue as Guid?;
            if ((guid is null) || (!this.state.inventories.inventories.ContainsKey(guid.Value))) {
                this.rem_but.IsEnabled = false;
                this.view_but.IsEnabled = false;
                return;
            }
            this.rem_but.IsEnabled = true;
            this.view_but.IsEnabled = true;
        }

        private void do_add(object sender, RoutedEventArgs e) {
            if (this.state is null) { return; }
            //TODO:
#if false
            Window dialog_window = new InventoryWindow(this.state) { Owner = Window.GetWindow(this) };
            dialog_window.ShowDialog();
            if (this.change_callback is null) { return; }
            List<EntryAction> actions = dialog_window.get_actions();
            if ((actions is null) || (actions.Count <= 0)) { return; }
            this.change_callback(actions);
#endif
        }

        private void do_rem(object sender, RoutedEventArgs e) {
            if ((this.change_callback is null) || (this.state is null)) { return; }
            Guid? guid = this.inventory_list.SelectedValue as Guid?;
            if ((guid is null) || (!this.state.inventories.inventories.ContainsKey(guid.Value))) { return; }
            EntryAction action = new ActionInventoryRemove(guid.Value, this.state.inventories.inventories[guid.Value].name);
            List<EntryAction> actions = new List<EntryAction>() { action };
            this.change_callback(actions);
        }

        private void do_view(object sender, RoutedEventArgs e) {
            if (this.state is null) { return; }
            Guid? guid = this.inventory_list.SelectedValue as Guid?;
            if ((guid is null) || (!this.state.inventories.inventories.ContainsKey(guid.Value))) { return; }
            //TODO:
#if false
            Window dialog_window = new InventoryWindow(this.state, guid.Value) { Owner = Window.GetWindow(this) };
            dialog_window.Owner = Window.GetWindow(this);
            dialog_window.ShowDialog();
            if (this.change_callback is null) { return; }
            List<EntryAction> actions = dialog_window.get_actions();
            if ((actions is null) || (actions.Count <= 0)) { return; }
            this.change_callback(actions);
#endif
        }
    }
}
