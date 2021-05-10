using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;

namespace CampLog {
    public partial class ItemAddWindow : Window {
        public bool valid;
        public bool need_refresh;
        private CampaignSave state;
        private InventoryEntry entry;
        private ItemSpec item;
        private ObservableCollection<InventoryItemBaseRow> inventory_rows;
        private Dictionary<InventoryItemIdent, InventoryItemRow> inventory_row_index;
        private Dictionary<InventoryItemIdent, decimal> inventory_capacity;

        private bool populate_inventory_rows(
            ObservableCollection<InventoryItemBaseRow> inv_rows, InventoryItemIdent parent, Inventory inv, InventoryItemIdent selected
        ) {
            bool found_selected = false;
            if (parent is null) {
                inv_rows.Add(new InventoryItemHeaderRow());
            }
            foreach (Guid guid in inv.contents.Keys) {
                InventoryEntry ent = inv.contents[guid];
                if ((ent is SingleItem item) && (item.containers is not null)) {
                    InventoryItemIdent ent_ident = new InventoryItemIdent(guid: guid);
                    InventoryItemRow ent_row = new InventoryItemRow(parent, ent_ident, ent.name, "", "");
                    bool ent_found_selected = false;
                    ent_row._children = new ObservableCollection<InventoryItemBaseRow>();
                    for (int i = 0; i < item.containers.Length; i++) {
                        Inventory child_inv = item.containers[i];
                        InventoryItemIdent inv_ident = new InventoryItemIdent(guid: guid, idx: i);
                        decimal? capacity = item.item.containers[i].weight_capacity;
                        if (capacity is not null) {
                            capacity -= item.containers[i].weight;
                            inventory_capacity[inv_ident] = capacity.Value;
                        }
                        InventoryItemRow inv_row = new InventoryItemRow(
                            ent_ident,
                            inv_ident,
                            child_inv.name,
                            "",
                            capacity.ToString(),
                            new ObservableCollection<InventoryItemBaseRow>()
                        );
                        bool child_found_selected = this.populate_inventory_rows(inv_row.children, inv_ident, child_inv, selected);
                        if (inv_ident == selected) {
                            inv_row._is_selected = true;
                            child_found_selected = true;
                        }
                        else if (child_found_selected) {
                            inv_row._is_expanded = true;
                        }
                        ent_found_selected = ent_found_selected || child_found_selected;
                        ent_row.children.Add(inv_row);
                        this.inventory_row_index[inv_ident] = inv_row;
                    }
                    if (ent_ident == selected) {
                        ent_row._is_selected = true;
                        ent_found_selected = true;
                    }
                    else if (ent_found_selected) {
                        ent_row._is_expanded = true;
                    }
                    found_selected = found_selected || ent_found_selected;
                    inv_rows.Add(ent_row);
                    this.inventory_row_index[ent_ident] = ent_row;
                }
            }
            return found_selected;
        }

        private void populate_inventories(CampaignState state, Guid? guid, InventoryItemIdent selected) {
            this.inventory_rows.Add(new InventoryItemHeaderRow());
            List<Guid> invs;
            if (guid is null) {
                invs = new List<Guid>(state.inventories.inventories.Keys);
            }
            else {
                invs = new List<Guid>() { guid.Value };
            }
            invs.Sort((x, y) => state.inventories.inventories[x].name.CompareTo(state.inventories.inventories[x].name));
            foreach (Guid inv_guid in invs) {
                InventoryItemIdent inv_ident = new InventoryItemIdent(inv_guid);
                InventoryItemRow inv_row = new InventoryItemRow(
                    null, inv_ident, state.inventories.inventories[inv_guid].name, "", "", new ObservableCollection<InventoryItemBaseRow>()
                );
                bool found_selected = this.populate_inventory_rows(inv_row.children, inv_ident, state.inventories.inventories[inv_guid], selected);
                if (inv_ident == selected) {
                    inv_row._is_selected = true;
                }
                else if (found_selected) {
                    inv_row._is_expanded = true;
                }
                this.inventory_rows.Add(inv_row);
                this.inventory_row_index[inv_ident] = inv_row;
            }
        }

        public ItemAddWindow(CampaignSave save_state, CampaignState state, Guid? guid = null, InventoryItemIdent selected = null, InventoryEntry entry = null) {
            this.valid = false;
            this.need_refresh = false;
            this.state = save_state;
            this.entry = entry;
            this.item = entry?.item;
            this.inventory_rows = new ObservableCollection<InventoryItemBaseRow>();
            this.inventory_row_index = new Dictionary<InventoryItemIdent, InventoryItemRow>();
            this.inventory_capacity = new Dictionary<InventoryItemIdent, decimal>();
            this.populate_inventories(state, guid, selected);
            InitializeComponent();
            Visibility item_visibility = (guid is null ? Visibility.Collapsed : Visibility.Visible);
            this.item_label.Visibility = item_visibility;
            this.item_box.Visibility = item_visibility;
            this.item_set_but.Visibility = item_visibility;
            this.count_label.Visibility = item_visibility;
            this.count_box.Visibility = item_visibility;
            this.unidentified_label.Visibility = item_visibility;
            this.unidentified_box.Visibility = item_visibility;
            this.inventory_list.ItemsSource = this.inventory_rows;
        }

        private void update_weight_capacity(object sender = null, RoutedEventArgs e = null) {
            if ((this.item is null) || (this.weight_box is null)) { return; }
            decimal weight = this.item.weight * (decimal)(this.count_box.Value);
            this.weight_box.Text = weight.ToString();
            InventoryItemIdent inv_sel = this.inventory_list.SelectedValue as InventoryItemIdent;
            if ((inv_sel is null) || (!this.inventory_capacity.ContainsKey(inv_sel))) {
                this.count_box.Maximum = long.MaxValue;
                this.capacity_box.Text = "";
            }
            else {
                decimal capacity = this.inventory_capacity[inv_sel];
                long max_count = long.MaxValue;
                if (this.item.weight > 0) {
                    max_count = (long)(capacity / this.item.weight);
                }
                this.count_box.Maximum = max_count;
                this.capacity_box.Text = (capacity - weight).ToString();
            }
            if (this.item.containers is not null) {
                this.count_box.Value = 1;
                this.count_box.Maximum = 1;
                if (this.unidentified_box.Value > 1) { this.unidentified_box.Value = 1; }
            }
            long max_unidentified = (long)(this.count_box.Value);
            if (this.count_box.Maximum < max_unidentified) { max_unidentified = (long)(this.count_box.Maximum); }
            this.unidentified_box.Maximum = max_unidentified;
            if (this.unidentified_box.Value > max_unidentified) { this.unidentified_box.Value = max_unidentified; }
        }

        private void item_sel(object sender, RoutedEventArgs e) {
            string sel = this.item_box.Text;
            if (sel == "") { sel = null; }
            ItemLibraryWindow item_library_window = new ItemLibraryWindow(this.state, true, sel) { Owner = this };
            item_library_window.ShowDialog();
            this.need_refresh = this.need_refresh || item_library_window.need_refresh;
            if (!item_library_window.valid) { return; }
            ItemSpec item = item_library_window.get_selected_item();
            if (item is null) { return; }
            this.item = item;
            this.item_box.Text = item.name;
            this.update_weight_capacity();
        }

        private void do_ok(object sender, RoutedEventArgs e) {
            this.valid = true;
            this.Close();
        }

        private void do_cancel(object sender, RoutedEventArgs e) {
            this.Close();
        }

        public InventoryItemIdent get_destination() {
            if (!this.valid) { return null; }
            InventoryItemIdent inv_sel = this.inventory_list.SelectedValue as InventoryItemIdent;
            if ((inv_sel is null) || (inv_sel.guid is null)) { return null; }
            return inv_sel;
        }

        public InventoryEntry get_entry() {
            if ((!this.valid) || (this.item is null)) { return null; }
            if (this.entry is not null) { return this.entry; }
            if (this.item.containers is not null) {
                return new SingleItem(this.item, this.unidentified_box.Value > 0);
            }
            return new ItemStack(this.item, (long)(this.count_box.Value), (long)(this.unidentified_box.Value));
        }
    }
}
