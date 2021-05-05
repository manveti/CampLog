using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace CampLog {
    public class InventoryItemPropRow {
        public string _name;
        public string _value;

        public string name { get => this._name; }
        public string value { get => this._value; }

        public InventoryItemPropRow(string name, string value) {
            this._name = name;
            this._value = value;
        }
    }


    public class InventoryItemIdent {
        public Guid? guid;
        public int? idx;
        public string category;

        public InventoryItemIdent(Guid? guid = null, int? idx = null, string category = null) {
            this.guid = guid;
            this.idx = idx;
            this.category = category;
        }

        public override bool Equals(object obj) => obj is InventoryItemIdent ident && EqualityComparer<Guid?>.Default.Equals(this.guid, ident.guid) && this.idx == ident.idx && this.category == ident.category;
        public override int GetHashCode() => HashCode.Combine(this.guid, this.idx, this.category);
        public static bool operator ==(InventoryItemIdent i1, InventoryItemIdent i2) {
            if (i1 is not null) { return i1.Equals(i2); }
            if (i2 is not null) { return i2.Equals(i1); }
            return true;
        }
        public static bool operator !=(InventoryItemIdent i1, InventoryItemIdent i2) => !(i1 == i2);
    }


    public class InventoryItemBaseRow { }
    public class InventoryItemHeaderRow : InventoryItemBaseRow { }
    public class InventoryItemRow : InventoryItemBaseRow, INotifyPropertyChanged {
        public InventoryItemIdent parent;
        public InventoryItemIdent _ident;
        public string _name;
        public string _value;
        public string _weight;
        public ObservableCollection<InventoryItemBaseRow> _children;

        public event PropertyChangedEventHandler PropertyChanged;

        public InventoryItemIdent ident { get => this._ident; }
        public string name { get => this._name; }
        public string value { get => this._value; }
        public string weight { get => this._weight; }
        public ObservableCollection<InventoryItemBaseRow> children { get => this._children; }

        public InventoryItemRow(
            InventoryItemIdent parent,
            InventoryItemIdent ident,
            string name,
            string value,
            string weight,
            ObservableCollection<InventoryItemBaseRow> children = null
        ) {
            this.parent = parent;
            this._ident = ident;
            this._name = name;
            this._value = value;
            this._weight = weight;
            this._children = children;
        }

        public void NotifyPropertyChanged([CallerMemberName] string propertyName = "") {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }


    public partial class InventoryWindow : Window {
        public bool valid;
        private CampaignState state;
        private Guid guid;
        private List<EntryAction> actions;
        private Inventory inventory;
        private ObservableCollection<InventoryItemBaseRow> inventory_rows;
        private Dictionary<InventoryItemIdent, InventoryItemRow> inventory_row_index;
        private ObservableCollection<InventoryItemPropRow> item_prop_rows;
        private InventoryItemIdent selected;

        private void populate_inventory_rows(ObservableCollection<InventoryItemBaseRow> inv_rows, InventoryItemIdent parent, Inventory inv) {
            if (parent is null) {
                inv_rows.Add(new InventoryItemHeaderRow());
            }
            SortedDictionary<string, List<Guid>> categories = new SortedDictionary<string, List<Guid>>();
            Dictionary<string, decimal> cat_values = new Dictionary<string, decimal>(), cat_weights = new Dictionary<string, decimal>();
            foreach (Guid guid in inv.contents.Keys) {
                if (!categories.ContainsKey(inv.contents[guid].item.category.name)) {
                    categories[inv.contents[guid].item.category.name] = new List<Guid>();
                    cat_values[inv.contents[guid].item.category.name] = 0;
                    cat_weights[inv.contents[guid].item.category.name] = 0;
                }
                categories[inv.contents[guid].item.category.name].Add(guid);
                cat_values[inv.contents[guid].item.category.name] += inv.contents[guid].value;
                cat_weights[inv.contents[guid].item.category.name] += inv.contents[guid].weight;
            }
            foreach (string cat in categories.Keys) {
                InventoryItemIdent cat_ident = new InventoryItemIdent(parent?.guid ?? this.guid, parent?.idx, cat);
                InventoryItemRow cat_row = new InventoryItemRow(
                    parent, cat_ident, cat, cat_values[cat].ToString(), cat_weights[cat].ToString(), new ObservableCollection<InventoryItemBaseRow>()
                );
                categories[cat].Sort((Guid e1, Guid e2) => inv.contents[e1].item.name.CompareTo(inv.contents[e2].item.name));
                foreach (Guid guid in categories[cat]) {
                    InventoryEntry ent = inv.contents[guid];
                    InventoryItemIdent ent_ident = new InventoryItemIdent(guid: guid);
                    InventoryItemRow ent_row = new InventoryItemRow(cat_ident, ent_ident, ent.name, ent.value.ToString(), ent.weight.ToString());
                    if ((ent is SingleItem item) && (item.containers is not null)) {
                        ent_row._children = new ObservableCollection<InventoryItemBaseRow>();
                        for (int i = 0; i < item.containers.Length; i++) {
                            Inventory child_inv = item.containers[i];
                            InventoryItemIdent inv_ident = new InventoryItemIdent(guid: guid, idx: i);
                            InventoryItemRow inv_row = new InventoryItemRow(
                                ent_ident,
                                inv_ident,
                                child_inv.name,
                                child_inv.value.ToString(),
                                child_inv.weight.ToString(),
                                new ObservableCollection<InventoryItemBaseRow>()
                            );
                            this.populate_inventory_rows(inv_row.children, inv_ident, child_inv);
                            ent_row.children.Add(inv_row);
                            this.inventory_row_index[inv_ident] = inv_row;
                        }
                    }
                    cat_row.children.Add(ent_row);
                    this.inventory_row_index[ent_ident] = ent_row;
                }
                inv_rows.Add(cat_row);
                this.inventory_row_index[cat_ident] = cat_row;
            }
        }

        public InventoryWindow(CampaignState state, Guid? guid = null) {
            this.valid = false;
            this.state = state.copy();
            this.actions = new List<EntryAction>();
            if (guid is null) {
                this.guid = Guid.NewGuid();
                ActionInventoryCreate add_action = new ActionInventoryCreate(this.guid, "");
                this.actions.Add(add_action);
                this.state.inventories.new_inventory(add_action.name, add_action.guid);
                this.inventory = this.state.inventories.inventories[add_action.guid];
            }
            else {
                this.guid = guid.Value;
                this.inventory = this.state.inventories.inventories[guid.Value];
            }
            this.inventory_rows = new ObservableCollection<InventoryItemBaseRow>();
            this.inventory_row_index = new Dictionary<InventoryItemIdent, InventoryItemRow>();
            this.populate_inventory_rows(this.inventory_rows, null, this.inventory);
            this.item_prop_rows = new ObservableCollection<InventoryItemPropRow>();
            this.selected = null;
            InitializeComponent();
            this.inv_name_box.Text = this.inventory.name;
            this.inventory_list.ItemsSource = this.inventory_rows;
            this.prop_list.ItemsSource = this.item_prop_rows;
        }

        private static bool is_merge_target(InventoryEntry ent, ItemSpec item) {
            if (ent.item != item) { return false; }
            if (ent is SingleItem ent_item) {
                if (ent_item.value_override is not null) { return false; }
                if (ent_item.properties.Count > 0) { return false; }
                if (ent_item.containers is not null) { return false; }
            }
            return true;
        }

        private void inventory_list_sel_changed(object sender, RoutedEventArgs e) {
            InventoryItemIdent sel = this.inventory_list.SelectedValue as InventoryItemIdent;
            if (sel == this.selected) { return; }
            //TODO: check for changes; update (and add actions if necessary) if so

            this.selected = sel;
            InventoryEntry entry = null;
            SingleItem item = null;
            ItemStack stack = null;
            ContainerSpec container_spec = null;
            InventoryItemRow sel_row = null;
            bool can_merge = false;
            if (sel is not null) {
                if ((sel.guid is not null) && (this.state.inventories.entries.ContainsKey(sel.guid.Value))) {
                    entry = this.state.inventories.entries[sel.guid.Value];
                    item = entry as SingleItem;
                    stack = entry as ItemStack;
                    if ((sel.idx is not null) && (item?.containers is not null) && (sel.idx.Value < item.containers.Length)) {
                        container_spec = item.item.containers[sel.idx.Value];
                    }
                }
                if (this.inventory_row_index.ContainsKey(sel)) {
                    sel_row = this.inventory_row_index[sel];
                    Inventory parent_inv = null;
                    if (sel_row.parent?.guid is not null) {
                        if (this.state.inventories.inventories.ContainsKey(sel_row.parent.guid.Value)) {
                            parent_inv = this.state.inventories.inventories[sel_row.parent.guid.Value];
                        }
                        else if ((sel_row.parent.idx is not null) && (this.state.inventories.entries.ContainsKey(sel_row.parent.guid.Value))) {
                            SingleItem parent_item = this.state.inventories.entries[sel_row.parent.guid.Value] as SingleItem;
                            if ((parent_item?.containers is not null) && (sel_row.parent.idx.Value < parent_item.containers.Length)) {
                                parent_inv = parent_item.containers[sel_row.parent.idx.Value];
                            }
                        }
                        if (parent_inv is not null) {
                            foreach (Guid guid in parent_inv.contents.Keys) {
                                if (guid == sel.guid) { continue; }
                                if (is_merge_target(parent_inv.contents[guid], entry.item)) {
                                    can_merge = true;
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            bool is_entry = (entry is not null) && (sel?.category is null) && (sel?.idx is null);
            bool is_item = is_entry && (item is not null), is_stack = is_entry && (stack is not null);

            this.item_rem_but.IsEnabled = is_entry;
            this.item_move_but.IsEnabled = is_entry;
            this.item_merge_but.IsEnabled = can_merge;
            this.item_split_but.Content = ((is_stack && (stack.count == 1)) ? "Unstack" : "Split...");
            this.item_split_but.IsEnabled = is_stack;
            this.item_name_box.Text = sel?.category ?? container_spec?.name ?? entry?.item?.name ?? sel_row?.name ?? "";
            if (is_stack) {
                this.count_grid.Visibility = Visibility.Visible;
                this.count_box.Value = stack.count;
                this.unidentified_grid.Visibility = Visibility.Visible;
                this.unidentified_box.Visibility = Visibility.Visible;
                this.unidentified_box.Value = stack.unidentified;
                this.unidentified_checkbox.Visibility = Visibility.Hidden;
            }
            else {
                this.count_grid.Visibility = Visibility.Collapsed;
                if (is_item) {
                    this.unidentified_grid.Visibility = Visibility.Visible;
                    this.unidentified_box.Visibility = Visibility.Hidden;
                    this.unidentified_checkbox.Visibility = Visibility.Visible;
                    this.unidentified_checkbox.IsChecked = item.unidentified;
                }
                else {
                    this.unidentified_grid.Visibility = Visibility.Collapsed;
                }
            }
            this.value_total_box.Text = sel_row?.value ?? "";
            if (is_entry) {
                this.value_item_label.Visibility = Visibility.Visible;
                this.value_item_box.Value = (double)(entry.item.value);
                this.value_item_box.Visibility = Visibility.Visible;
            }
            else {
                this.value_item_label.Visibility = Visibility.Collapsed;
                this.value_item_box.Visibility = Visibility.Collapsed;
            }
            this.value_reset_but.Visibility = (is_item ? Visibility.Visible : Visibility.Collapsed);
            this.value_reset_but.IsEnabled = is_item && (item.value_override is not null);
            if (is_item && (item.containers is not null)) {
                this.value_cont_label.Visibility = Visibility.Visible;
                this.value_cont_box.Text = item.contents_value.ToString();
                this.value_cont_box.Visibility = Visibility.Visible;
            }
            else {
                this.value_cont_label.Visibility = Visibility.Collapsed;
                this.value_cont_box.Visibility = Visibility.Collapsed;
            }
            this.weight_total_box.Text = sel_row?.weight ?? "";
            if ((sel?.category is null) && (container_spec is not null) && (container_spec.weight_capacity is not null)) {
                this.weight_item_label.Content = "Capacity:";
                this.weight_item_label.Visibility = Visibility.Visible;
                this.weight_item_box.Text = container_spec.weight_capacity.Value.ToString();
                this.weight_item_box.Visibility = Visibility.Visible;
            }
            else if (is_entry) {
                this.weight_item_label.Content = "Item:";
                this.weight_item_label.Visibility = Visibility.Visible;
                this.weight_item_box.Text = entry.item.weight.ToString();
                this.weight_item_box.Visibility = Visibility.Visible;
            }
            else {
                this.weight_item_label.Visibility = Visibility.Collapsed;
                this.weight_item_box.Visibility = Visibility.Collapsed;
            }
            if (is_item && (item.containers is not null)) {
                this.weight_cont_label.Visibility = Visibility.Visible;
                this.weight_cont_box.Text = item.contents_weight.ToString();
                this.weight_cont_box.Visibility = Visibility.Visible;
            }
            else {
                this.weight_cont_label.Visibility = Visibility.Collapsed;
                this.weight_cont_box.Visibility = Visibility.Collapsed;
            }
            if (is_entry) {
                this.prop_add_but.Visibility = (is_item ? Visibility.Visible : Visibility.Collapsed);
                this.prop_edit_but.IsEnabled = false;
                this.prop_edit_but.Visibility = (is_item ? Visibility.Visible : Visibility.Collapsed);
                this.prop_rem_but.IsEnabled = false;
                this.prop_rem_but.Visibility = (is_item ? Visibility.Visible : Visibility.Collapsed);
                SortedDictionary<string, string> props = new SortedDictionary<string, string>(entry.item.properties);
                if (item is not null) {
                    foreach (string key in item.properties.Keys) {
                        props[key] = item.properties[key];
                    }
                }
                this.item_prop_rows.Clear();
                foreach (string key in props.Keys) {
                    this.item_prop_rows.Add(new InventoryItemPropRow(key, props[key]));
                }
                this.props_group.Visibility = Visibility.Visible;
            }
            else {
                this.props_group.Visibility = Visibility.Collapsed;
            }
        }

        private void item_add(object sender, RoutedEventArgs e) {
            //TODO: add new item
        }

        private void item_rem(object sender, RoutedEventArgs e) {
            //TODO: remove selected item
        }

        private void item_move(object sender, RoutedEventArgs e) {
            //TODO: prompt for destination; move selected item
        }

        private void item_merge(object sender, RoutedEventArgs e) {
            //TODO: prompt for merge target; merge selected items
        }

        private void item_split(object sender, RoutedEventArgs e) {
            //TODO: split: { prompt for count & unidentified; split selected stack } unstack: { unstack selected stack }
        }

        private void do_ok(object sender, RoutedEventArgs e) {
            if (this.inv_name_box.Text != this.inventory.name) {
                ActionInventoryRename action = new ActionInventoryRename(this.guid, this.inventory.name, this.inv_name_box.Text);
                this.actions.Add(action);
            }
            this.valid = true;
            this.Close();
        }

        private void do_cancel(object sender, RoutedEventArgs e) {
            this.Close();
        }

        public List<EntryAction> get_actions() {
            if (!this.valid) { return null; }
            return this.actions;
        }
    }
}
