using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

using GUIx;

namespace CampLog {
    public class InventoryItemPropRow {
        public enum RemoveAction {
            NONE,
            RESET,
            REMOVE,
        }

        public string _name;
        public string _value;
        public RemoveAction remove_action;

        public InventoryItemPropRow self { get => this; }
        public string name { get => this._name; }
        public string value { get => this._value; }

        public InventoryItemPropRow(string name, string value, RemoveAction remove_action) {
            this._name = name;
            this._value = value;
            this.remove_action = remove_action;
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


    public class InventoryItemBaseRow {
        public bool _is_expanded = false;
        public bool _is_selected = false;
        public virtual bool is_expanded { get => this._is_expanded; set => this._is_expanded = value; }
        public virtual bool is_selected { get => this._is_selected; set => this._is_selected = value; }
    }
    public class InventoryItemHeaderRow : InventoryItemBaseRow { }
    public class InventoryItemRow : InventoryItemBaseRow, INotifyPropertyChanged {
        public InventoryItemIdent parent;
        public InventoryItemIdent _ident;
        public string _name;
        public string _value;
        public string _weight;
        public ObservableCollection<InventoryItemBaseRow> _children;

        public event PropertyChangedEventHandler PropertyChanged;

        public override bool is_expanded {
            get => base.is_expanded;
            set {
                base.is_expanded = value;
                this.NotifyPropertyChanged();
            }
        }
        public override bool is_selected {
            get => base.is_selected;
            set {
                base.is_selected = value;
                this.NotifyPropertyChanged();
            }
        }
        public InventoryItemIdent ident { get => this._ident; }
        public string name {
            get => this._name;
            set {
                this._name = value;
                this.NotifyPropertyChanged();
            }
        }
        public string value {
            get => this._value;
            set {
                this._value = value;
                this.NotifyPropertyChanged();
            }
        }
        public string weight {
            get => this._weight;
            set {
                this._weight = value;
                this.NotifyPropertyChanged();
            }
        }
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
        public bool need_refresh;
        private bool suppress_change_apply;
        private CampaignSave save_state;
        private CampaignState state;
        private Guid guid;
        private List<EntryAction> actions;
        private Inventory inventory;
        private ObservableCollection<InventoryItemBaseRow> inventory_rows;
        private Dictionary<InventoryItemIdent, InventoryItemRow> inventory_row_index;
        private SortedDictionary<string, string> item_props;
        private List<InventoryItemPropRow> item_prop_rows;
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

        public void repopulate_inventory_rows() {
            InventoryItemIdent sel = this.selected;
            this.inventory_rows.Clear();
            this.inventory_row_index.Clear();
            this.populate_inventory_rows(this.inventory_rows, null, this.inventory);
            if ((sel is not null) && (this.inventory_row_index.ContainsKey(sel))) {
                this.inventory_row_index[sel]._is_selected = true;
                InventoryItemIdent par = this.inventory_row_index[sel].parent;
                while (par is not null) {
                    if (!this.inventory_row_index.ContainsKey(par)) { break; }
                    InventoryItemRow par_row = this.inventory_row_index[par];
                    par_row._is_expanded = true;
                    par = par_row.parent;
                }
            }
        }

        private void fix_listview_column_widths(ListView list_view) {
            GridView grid_view = list_view.View as GridView;
            if (grid_view is null) { return; }
            foreach (GridViewColumn col in grid_view.Columns) {
                col.Width = col.ActualWidth;
                col.Width = double.NaN;
            }
        }

        public InventoryWindow(CampaignSave save_state, CampaignState state, Guid? guid = null) {
            this.valid = false;
            this.need_refresh = false;
            this.suppress_change_apply = false;
            this.save_state = save_state;
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
            this.item_props = new SortedDictionary<string, string>();
            this.item_prop_rows = new List<InventoryItemPropRow>();
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

        private void apply_changes() {
            if (this.suppress_change_apply) { return; }
            if ((this.selected?.guid is not null) && (this.state.inventories.entries.ContainsKey(this.selected.guid.Value))) {
                if (this.state.inventories.entries[this.selected.guid.Value] is SingleItem prev_item) {
                    bool? new_unidentified = null;
                    decimal? new_value = (decimal)(this.value_item_box.Value);
                    Dictionary<string, string> new_props = null, prev_props = new Dictionary<string, string>(prev_item.item.properties);
                    bool set_value = false, set_properties = false;
                    if (this.unidentified_checkbox.IsChecked != prev_item.unidentified) {
                        new_unidentified = this.unidentified_checkbox.IsChecked;
                    }
                    if (new_value == prev_item.item.value) {
                        new_value = null;
                        if (prev_item.value_override is not null) { set_value = true; }
                    }
                    else if (new_value != prev_item.value_override) { set_value = true; }
                    foreach (string key in prev_item.properties.Keys) {
                        prev_props[key] = prev_item.properties[key];
                    }
                    if (this.item_props.Count != prev_props.Count) { set_properties = true; }
                    else {
                        foreach (string key in this.item_props.Keys) {
                            if ((!prev_props.ContainsKey(key)) || (this.item_props[key] != prev_props[key])) {
                                set_properties = true;
                                break;
                            }
                        }
                    }
                    if (set_properties) {
                        new_props = new Dictionary<string, string>();
                        foreach (string key in this.item_props.Keys) {
                            if ((!prev_item.item.properties.ContainsKey(key)) || (this.item_props[key] != prev_item.item.properties[key])) {
                                new_props[key] = this.item_props[key];
                            }
                        }
                    }
                    if ((new_unidentified is not null) || (set_value) || (set_properties)) {
                        bool? unidentified_from = (new_unidentified is null ? null : prev_item.unidentified);
                        decimal? value_from = (set_value ? prev_item.value_override : null);
                        Dictionary<string, string> props_from = (set_properties ? prev_item.properties : null);
                        ActionSingleItemSet action = new ActionSingleItemSet(
                            this.selected.guid.Value, unidentified_from, value_from, props_from, new_unidentified, new_value, new_props, set_value
                        );
                        if (new_unidentified is not null) { prev_item.unidentified = new_unidentified.Value; }
                        if (set_value) { prev_item.value_override = new_value; }
                        if (set_properties) { prev_item.properties = new Dictionary<string, string>(this.item_props); }
                        if (this.inventory_row_index.ContainsKey(this.selected)) {
                            this.inventory_row_index[this.selected].value = prev_item.value.ToString();
                        }
                        this.actions.Add(action);
                    }
                }
                else if (this.state.inventories.entries[this.selected.guid.Value] is ItemStack prev_stack) {
                    long count_diff = (long)(this.count_box.Value - prev_stack.count),
                        unidentified_diff = (long)(this.unidentified_box.Value - prev_stack.unidentified);
                    if ((count_diff != 0) || (unidentified_diff != 0)) {
                        ActionItemStackAdjust action = new ActionItemStackAdjust(this.selected.guid.Value, count_diff, unidentified_diff);
                        prev_stack.count += count_diff;
                        prev_stack.unidentified += unidentified_diff;
                        if (this.inventory_row_index.ContainsKey(this.selected)) {
                            this.inventory_row_index[this.selected]._name = prev_stack.name;
                            this.inventory_row_index[this.selected].value = prev_stack.value.ToString();
                            this.inventory_row_index[this.selected].weight = prev_stack.weight.ToString();
                        }
                        this.actions.Add(action);
                    }
                }
            }
        }

        private void inventory_list_sel_changed(object sender, RoutedEventArgs e) {
            InventoryItemIdent sel = this.inventory_list.SelectedValue as InventoryItemIdent;
            if (sel == this.selected) { return; }
            this.apply_changes();

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
                            if ((item is null) || ((item.containers is null) && (item.properties.Count == 0))) {
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
                this.unidentified_box.Maximum = stack.count;
                this.unidentified_box.Value = stack.unidentified;
                this.unidentified_box.Visibility = Visibility.Visible;
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
                if ((is_item) && (item.value_override is not null)) {
                    this.value_item_box.Value = (double)(item.value_override.Value);
                }
                else {
                    this.value_item_box.Value = (double)(entry.item.value);
                }
                this.value_item_box.IsReadOnly = !is_item;
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
                this.item_props = new SortedDictionary<string, string>(entry.item.properties);
                if (item is not null) {
                    foreach (string key in item.properties.Keys) {
                        this.item_props[key] = item.properties[key];
                    }
                }
                this.item_prop_rows.Clear();
                foreach (string key in this.item_props.Keys) {
                    InventoryItemPropRow.RemoveAction remove_action = InventoryItemPropRow.RemoveAction.NONE;
                    if (!entry.item.properties.ContainsKey(key)) { remove_action = InventoryItemPropRow.RemoveAction.REMOVE; }
                    else if ((item is not null) && (item.properties.ContainsKey(key)) && (entry.item.properties[key] != item.properties[key])) {
                        remove_action = InventoryItemPropRow.RemoveAction.RESET;
                    }
                    this.item_prop_rows.Add(new InventoryItemPropRow(key, this.item_props[key], remove_action));
                }
                this.prop_list.Items.Refresh();
                this.fix_listview_column_widths(this.prop_list);
                this.props_group.Visibility = Visibility.Visible;
            }
            else {
                this.props_group.Visibility = Visibility.Collapsed;
            }
        }

        private void item_add(object sender, RoutedEventArgs e) {
            if (this.inv_name_box.Text != this.inventory.name) {
                ActionInventoryRename action = new ActionInventoryRename(this.guid, this.inventory.name, this.inv_name_box.Text);
                this.actions.Add(action);
                this.inventory.name = this.inv_name_box.Text;
            }
            ItemAddWindow item_add_window = new ItemAddWindow(this.save_state, this.state, this.guid) { Owner = this };
            item_add_window.ShowDialog();
            this.need_refresh = this.need_refresh || item_add_window.need_refresh;
            if (!item_add_window.valid) {
                if (item_add_window.need_refresh) { this.repopulate_inventory_rows(); }
                return;
            }
            InventoryItemIdent destination = item_add_window.get_destination();
            if ((destination is null) || (destination.guid is null)) {
                if (item_add_window.need_refresh) { this.repopulate_inventory_rows(); }
                return;
            }
            InventoryEntry entry = item_add_window.get_entry();
            if (entry is null) {
                if (item_add_window.need_refresh) { this.repopulate_inventory_rows(); }
                return;
            }
            Guid guid = this.state.inventories.add_entry(destination.guid.Value, destination.idx, entry.copy());
            this.actions.Add(new ActionInventoryEntryAdd(destination.guid.Value, destination.idx, guid, entry));
            this.repopulate_inventory_rows();
        }

        private void item_rem(object sender, RoutedEventArgs e) {
            if ((this.selected?.guid is null) || (!this.state.inventories.entries.ContainsKey(this.selected.guid.Value))) { return; }
            if (!this.inventory_row_index.ContainsKey(this.selected)) { return; }
            InventoryItemRow sel_row = this.inventory_row_index[this.selected];
            if ((sel_row.parent is null) || (sel_row.parent.guid is null)) { return; }
            this.state.inventories.remove_entry(this.selected.guid.Value, sel_row.parent.guid.Value, sel_row.parent.idx);
            this.actions.Add(new ActionInventoryEntryRemove(sel_row.parent.guid.Value, sel_row.parent.idx, this.selected.guid.Value));
            this.repopulate_inventory_rows();
        }

        private void item_move(object sender, RoutedEventArgs e) {
            if ((this.selected?.guid is null) || (!this.state.inventories.entries.ContainsKey(this.selected.guid.Value))) { return; }
            if (!this.inventory_row_index.ContainsKey(this.selected)) { return; }
            InventoryItemRow sel_row = this.inventory_row_index[this.selected];
            if ((sel_row.parent is null) || (sel_row.parent.guid is null)) { return; }
            this.apply_changes();
            InventoryEntry entry = this.state.inventories.entries[this.selected.guid.Value];
            ItemAddWindow item_add_window = new ItemAddWindow(this.save_state, this.state, selected: sel_row.parent, entry: entry) { Owner = this };
            item_add_window.ShowDialog();
            if (!item_add_window.valid) { return; }
            InventoryItemIdent dest = item_add_window.get_destination();
            if ((dest is null) || (dest.guid is null) || (dest == sel_row.parent)) { return; }
            this.state.inventories.move_entry(this.selected.guid.Value, sel_row.parent.guid.Value, sel_row.parent.idx, dest.guid.Value, dest.idx);
            this.actions.Add(new ActionInventoryEntryMove(this.selected.guid.Value, sel_row.parent.guid.Value, sel_row.parent.idx, dest.guid.Value, dest.idx));
            this.repopulate_inventory_rows();
        }

        private void item_merge(object sender, RoutedEventArgs e) {
            if ((this.selected?.guid is null) || (!this.state.inventories.entries.ContainsKey(this.selected.guid.Value))) { return; }
            if (!this.inventory_row_index.ContainsKey(this.selected)) { return; }
            InventoryItemRow sel_row = this.inventory_row_index[this.selected];
            if ((sel_row.parent is null) || (sel_row.parent.guid is null) || (!this.inventory_row_index.ContainsKey(sel_row.parent))) { return; }
            this.apply_changes();
            InventoryItemRow parent_row = this.inventory_row_index[sel_row.parent];
            InventoryEntry entry = this.state.inventories.entries[this.selected.guid.Value];
            List<InventoryItemRow> targets = new List<InventoryItemRow>();
            foreach (InventoryItemRow row in parent_row.children) {
                if (row.ident == this.selected) { continue; }
                if ((row.ident?.guid is null) && (!this.state.inventories.entries.ContainsKey(row.ident.guid.Value))) { continue; }
                InventoryEntry target_ent = this.state.inventories.entries[row.ident.guid.Value];
                if (is_merge_target(target_ent, entry.item)) {
                    targets.Add(row);
                }
            }
            if (targets.Count <= 0) { return; }
            ItemMergeWindow item_merge_window = new ItemMergeWindow(targets) { Owner = this };
            item_merge_window.ShowDialog();
            if (!item_merge_window.valid) { return; }
            InventoryItemIdent dest = item_merge_window.get_item();
            if ((dest is null) || (dest.guid is null)) { return; }
            Guid guid = this.state.inventories.merge_entries(sel_row.parent.guid.Value, sel_row.parent.idx, this.selected.guid.Value, dest.guid.Value);
            this.actions.Add(new ActionInventoryEntryMerge(sel_row.parent.guid.Value, sel_row.parent.idx, this.selected.guid.Value, dest.guid.Value, guid));
            this.suppress_change_apply = true;
            this.repopulate_inventory_rows();
            this.suppress_change_apply = false;
        }

        private void item_split(object sender, RoutedEventArgs e) {
            if ((this.selected?.guid is null) || (!this.state.inventories.entries.ContainsKey(this.selected.guid.Value))) { return; }
            if (!this.inventory_row_index.ContainsKey(this.selected)) { return; }
            InventoryItemRow sel_row = this.inventory_row_index[this.selected];
            if ((sel_row.parent is null) || (sel_row.parent.guid is null)) { return; }
            ItemStack stack = this.state.inventories.entries[this.selected.guid.Value] as ItemStack;
            if ((stack is null) || (stack.count < 1)) { return; }
            if (stack.count > 1) {
                QueryPrompt[] prompts = new QueryPrompt[] {
                    new QueryPrompt("Count:", QueryType.INT, min: 1, max: stack.count - 1),
                    new QueryPrompt("Unidentified:", QueryType.INT, min: 0, max: stack.unidentified),
                };
                object[] results = SimpleDialog.askCompound("New Stack", prompts, this);
                if (results is null) { return; }
                int? count = results[0] as int?, unidentified = results[1] as int?;
                Guid guid = this.state.inventories.split_entry(
                    sel_row.parent.guid.Value, sel_row.parent.idx, this.selected.guid.Value, count.Value, unidentified.Value
                );
                this.actions.Add(new ActionInventoryEntrySplit(
                    sel_row.parent.guid.Value, sel_row.parent.idx, this.selected.guid.Value, count.Value, unidentified.Value, guid
                ));
            }
            else {
                Guid guid = this.state.inventories.unstack_entry(sel_row.parent.guid.Value, sel_row.parent.idx, this.selected.guid.Value);
                this.actions.Add(new ActionInventoryEntryUnstack(sel_row.parent.guid.Value, sel_row.parent.idx, this.selected.guid.Value, guid));
            }
            this.suppress_change_apply = true;
            this.repopulate_inventory_rows();
            this.suppress_change_apply = false;
        }

        private void count_box_changed(object sender, RoutedEventArgs e) {
            if ((this.selected?.guid is null) || (!this.state.inventories.entries.ContainsKey(this.selected.guid.Value))) { return; }
            if (!this.inventory_row_index.ContainsKey(this.selected)) { return; }
            InventoryItemRow sel_row = this.inventory_row_index[this.selected];
            if ((sel_row.parent is null) || (sel_row.parent.guid is null)) { return; }
            ItemStack stack = this.state.inventories.entries[this.selected.guid.Value] as ItemStack;
            if (stack is null) { return; }
            long count = (long)(this.count_box.Value);
            this.unidentified_box.Maximum = count;
            if (this.unidentified_box.Value > count) { this.unidentified_box.Value = count; }
            this.value_total_box.Text = (stack.item.value * count).ToString();
            this.weight_total_box.Text = (stack.item.weight * count).ToString();
        }

        private void value_item_box_changed(object sender, RoutedEventArgs e) {
            if ((this.selected?.guid is null) || (!this.state.inventories.entries.ContainsKey(this.selected.guid.Value))) { return; }
            if (!this.inventory_row_index.ContainsKey(this.selected)) { return; }
            InventoryItemRow sel_row = this.inventory_row_index[this.selected];
            if ((sel_row.parent is null) || (sel_row.parent.guid is null)) { return; }
            SingleItem item = this.state.inventories.entries[this.selected.guid.Value] as SingleItem;
            if (item is null) { return; }
            decimal value = (decimal)(this.value_item_box.Value);
            this.value_total_box.Text = (value + item.contents_value).ToString();
            this.value_reset_but.IsEnabled = (value != item.item.value);
        }

        private void value_reset(object sender, RoutedEventArgs e) {
            if ((this.selected?.guid is null) || (!this.state.inventories.entries.ContainsKey(this.selected.guid.Value))) { return; }
            if (!this.inventory_row_index.ContainsKey(this.selected)) { return; }
            InventoryItemRow sel_row = this.inventory_row_index[this.selected];
            if ((sel_row.parent is null) || (sel_row.parent.guid is null)) { return; }
            SingleItem item = this.state.inventories.entries[this.selected.guid.Value] as SingleItem;
            if (item is null) { return; }
            this.value_item_box.Value = (double)(item.item.value);
            this.value_total_box.Text = (item.item.value + item.contents_value).ToString();
            this.value_reset_but.IsEnabled = false;
        }

        private void prop_sel_changed(object sender, RoutedEventArgs e) {
            InventoryItemPropRow sel = this.prop_list.SelectedValue as InventoryItemPropRow;
            if (sel is null) {
                this.prop_edit_but.IsEnabled = false;
                this.prop_rem_but.Content = "Remove";
                this.prop_rem_but.IsEnabled = false;
                return;
            }
            this.prop_edit_but.IsEnabled = true;
            this.prop_rem_but.Content = (sel.remove_action == InventoryItemPropRow.RemoveAction.RESET ? "Reset" : "Remove");
            this.prop_rem_but.IsEnabled = (sel.remove_action != InventoryItemPropRow.RemoveAction.NONE);
        }

        private void prop_add(object sender, RoutedEventArgs e) {
            if ((this.selected?.guid is null) || (!this.state.inventories.entries.ContainsKey(this.selected.guid.Value))) { return; }
            if (!this.inventory_row_index.ContainsKey(this.selected)) { return; }
            InventoryItemRow sel_row = this.inventory_row_index[this.selected];
            if ((sel_row.parent is null) || (sel_row.parent.guid is null)) { return; }
            SingleItem item = this.state.inventories.entries[this.selected.guid.Value] as SingleItem;
            if (item is null) { return; }
            QueryPrompt[] prompts = new QueryPrompt[] {
                new QueryPrompt("Property:", QueryType.STRING),
                new QueryPrompt("Value:", QueryType.STRING),
            };
            object[] results = SimpleDialog.askCompound("Add Property", prompts, this);
            if (results is null) { return; }
            string prop = results[0] as string, value = results[1] as string;
            if ((prop is null) || (value is null)) { return; }
            if (this.item_props.ContainsKey(prop)) {
                string prompt = "Property \"" + prop + "\" already exists. Replace it?";
                MessageBoxResult result = MessageBox.Show(prompt, "Duplicate Property", MessageBoxButton.YesNo);
                if (result != MessageBoxResult.Yes) { return; }
                for (int i = this.item_prop_rows.Count - 1; i >= 0; i--) {
                    if (this.item_prop_rows[i].name == prop) {
                        this.item_prop_rows.RemoveAt(i);
                        break;
                    }
                }
            }
            this.item_props[prop] = value;
            InventoryItemPropRow.RemoveAction remove_action = InventoryItemPropRow.RemoveAction.REMOVE;
            if (item.item.properties.ContainsKey(prop)) { remove_action = InventoryItemPropRow.RemoveAction.RESET; }
            this.item_prop_rows.Add(new InventoryItemPropRow(prop, value, remove_action));
            this.item_prop_rows.Sort((x, y) => x.name.CompareTo(y.name));
            this.prop_list.Items.Refresh();
            this.fix_listview_column_widths(this.prop_list);
        }

        private void prop_edit(object sender, RoutedEventArgs e) {
            InventoryItemPropRow sel = this.prop_list.SelectedValue as InventoryItemPropRow;
            if ((sel is null) || (!this.item_props.ContainsKey(sel.name))) { return; }
            if ((this.selected?.guid is null) || (!this.state.inventories.entries.ContainsKey(this.selected.guid.Value))) { return; }
            if (!this.inventory_row_index.ContainsKey(this.selected)) { return; }
            InventoryItemRow sel_row = this.inventory_row_index[this.selected];
            if ((sel_row.parent is null) || (sel_row.parent.guid is null)) { return; }
            SingleItem item = this.state.inventories.entries[this.selected.guid.Value] as SingleItem;
            if (item is null) { return; }
            string value = SimpleDialog.askString(sel.name, "Value:", sel.value, this);
            if (value is null) { return; }
            this.item_props[sel.name] = value;
            sel._value = value;
            InventoryItemPropRow.RemoveAction remove_action = InventoryItemPropRow.RemoveAction.REMOVE;
            if (item.item.properties.ContainsKey(sel.name)) {
                if (value == item.item.properties[sel.name]) { remove_action = InventoryItemPropRow.RemoveAction.NONE; }
                else { remove_action = InventoryItemPropRow.RemoveAction.RESET; }
            }
            sel.remove_action = remove_action;
            this.prop_list.Items.Refresh();
            this.fix_listview_column_widths(this.prop_list);
            this.prop_rem_but.Content = (remove_action == InventoryItemPropRow.RemoveAction.RESET ? "Reset" : "Remove");
            this.prop_rem_but.IsEnabled = (remove_action != InventoryItemPropRow.RemoveAction.NONE);
        }

        private void prop_rem(object sender, RoutedEventArgs e) {
            InventoryItemPropRow sel = this.prop_list.SelectedValue as InventoryItemPropRow;
            if ((sel is null) || (!this.item_props.ContainsKey(sel.name))) { return; }
            if ((this.selected?.guid is null) || (!this.state.inventories.entries.ContainsKey(this.selected.guid.Value))) { return; }
            if (!this.inventory_row_index.ContainsKey(this.selected)) { return; }
            InventoryItemRow sel_row = this.inventory_row_index[this.selected];
            if ((sel_row.parent is null) || (sel_row.parent.guid is null)) { return; }
            SingleItem item = this.state.inventories.entries[this.selected.guid.Value] as SingleItem;
            if (item is null) { return; }
            if (item.item.properties.ContainsKey(sel.name)) {
                sel._value = item.item.properties[sel.name];
                this.item_props[sel.name] = sel.value;
                sel.remove_action = InventoryItemPropRow.RemoveAction.NONE;
                this.prop_rem_but.Content = "Remove";
                this.prop_rem_but.IsEnabled = false;
            }
            else {
                this.item_props.Remove(sel.name);
                this.item_prop_rows.Remove(sel);
            }
            this.prop_list.Items.Refresh();
            this.fix_listview_column_widths(this.prop_list);
        }

        private void do_ok(object sender, RoutedEventArgs e) {
            if (this.inv_name_box.Text != this.inventory.name) {
                ActionInventoryRename action = new ActionInventoryRename(this.guid, this.inventory.name, this.inv_name_box.Text);
                this.actions.Add(action);
            }
            this.apply_changes();
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
