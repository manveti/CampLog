using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

using GUIx;

namespace CampLog {
    public class ItemLibraryItemRow {
        public string _name;
        public bool is_category;
        public List<ItemLibraryItemRow> _items;
        public bool _is_expanded;
        public bool _is_selected;

        public string name { get => this._name; }
        public List<ItemLibraryItemRow> items { get => this._items; }
        public ItemLibraryItemRow self { get => this; }
        public bool is_expanded { get => this._is_expanded; set => this._is_expanded = value; }
        public bool is_selected { get => this._is_selected; set => this._is_selected = value; }

        public ItemLibraryItemRow(string name, bool is_category = false) {
            this._name = name;
            this.is_category = is_category;
            this._items = new List<ItemLibraryItemRow>();
        }
    }


    public class ItemSpecContainerRow {
        public string _name;
        public string _weight_factor;
        public string _weight_capacity;

        public string name { get => this._name; }
        public string weight_factor { get => this._weight_factor; }
        public string weight_capacity { get => this._weight_capacity; }

        public ItemSpecContainerRow(string name, string weight_factor, string weight_capacity) {
            this._name = name;
            this._weight_factor = weight_factor;
            this._weight_capacity = weight_capacity;
        }
    }


    public class ItemSpecPropertyRow {
        public string _name;
        public string _value;

        public string name { get => this._name; }
        public string value { get => this._value; }

        public ItemSpecPropertyRow(string name, string value) {
            this._name = name;
            this._value = value;
        }
    }


    public partial class ItemLibraryWindow : Window {
        public bool valid;
        public bool need_refresh;
        private bool dirty;
        private CampaignSave state;
        private List<string> categories;
        private List<ItemLibraryItemRow> item_rows;
        ItemLibraryItemRow selection;
        private List<ContainerSpec> containers;
        private List<ItemSpecContainerRow> container_rows;
        private Dictionary<string, string> properties;
        private List<ItemSpecPropertyRow> property_rows;

        public void populate_item_rows(string selection = null) {
            Dictionary<string, List<ItemLibraryItemRow>> cat_rows = new Dictionary<string, List<ItemLibraryItemRow>>();
            string selected_cat = null;

            foreach (string name in this.state.items.Keys) {
                string cat_name = this.state.items[name].element.category.name;
                if (!cat_rows.ContainsKey(cat_name)) { cat_rows[cat_name] = new List<ItemLibraryItemRow>(); }
                cat_rows[cat_name].Add(new ItemLibraryItemRow(name) { _is_selected = (name == selection) });
                if (name == selection) { selected_cat = cat_name; }
            }
            foreach (string name in this.state.categories.Keys) {
                if (!cat_rows.ContainsKey(name)) { cat_rows[name] = new List<ItemLibraryItemRow>(); }
            }

            this.categories = new List<string>(cat_rows.Keys);
            this.categories.Sort();
            foreach (string name in this.categories) {
                cat_rows[name].Sort((x, y) => x.name.CompareTo(y.name));
                ItemLibraryItemRow row = new ItemLibraryItemRow(name, true) { _items = cat_rows[name], _is_expanded = (name == selected_cat) };
                this.item_rows.Add(row);
            }
        }

        public ItemLibraryWindow(CampaignSave state, bool is_selector = false, string selection = null) {
            this.valid = false;
            this.need_refresh = false;
            this.dirty = false;
            this.state = state;
            this.item_rows = new List<ItemLibraryItemRow>();
            this.populate_item_rows(selection);
            this.selection = null;
            this.container_rows = new List<ItemSpecContainerRow>();
            this.property_rows = new List<ItemSpecPropertyRow>();
            InitializeComponent();
            this.item_list.ItemsSource = this.item_rows;
            this.cat_box.ItemsSource = this.categories;
            this.containers = new List<ContainerSpec>();
            this.container_list.ItemsSource = this.container_rows;
            this.properties = new Dictionary<string, string>();
            this.prop_list.ItemsSource = this.property_rows;
            if (is_selector) {
                this.ok_but.Visibility = Visibility.Visible;
                this.cancel_but.Content = "Cancel";
            }
            else {
                this.ok_but.Visibility = Visibility.Collapsed;
                this.cancel_but.Content = "Done";
            }
        }

        private void item_list_sel_changed(object sender, RoutedEventArgs e) {
            this.selection = this.item_list.SelectedValue as ItemLibraryItemRow;
            if (this.selection is null) {
                this.item_rem_but.IsEnabled = false;
                this.name_box.Text = "";
                this.cat_label.Visibility = Visibility.Collapsed;
                this.cat_box.Visibility = Visibility.Collapsed;
                this.cost_box.Text = "";
                this.value_label.Visibility = Visibility.Collapsed;
                this.value_box.Visibility = Visibility.Collapsed;
                this.weight_label.Visibility = Visibility.Collapsed;
                this.weight_box.Visibility = Visibility.Collapsed;
                this.cat_apply_but.Visibility = Visibility.Collapsed;
                this.cat_revert_but.Visibility = Visibility.Collapsed;
                this.container_group.Visibility = Visibility.Collapsed;
                this.prop_group.Visibility = Visibility.Collapsed;
                this.item_apply_but.Visibility = Visibility.Collapsed;
                this.item_revert_but.Visibility = Visibility.Collapsed;
                return;
            }

            this.name_box.Text = this.selection.name;

            if (this.selection.is_category) {
                if (!this.state.categories.ContainsKey(this.selection.name)) { return; }
                ElementReference<ItemCategory> cat_ref = this.state.categories[this.selection.name];
                this.item_rem_but.IsEnabled = cat_ref.ref_count <= 0;
                this.cat_label.Visibility = Visibility.Collapsed;
                this.cat_box.Visibility = Visibility.Collapsed;
                this.cost_label.Content = "Value Factor:";
                this.cost_box.SmallChange = 0.125;
                this.cost_box.Value = (double)(cat_ref.element.sale_value);
                this.value_label.Visibility = Visibility.Collapsed;
                this.value_box.Visibility = Visibility.Collapsed;
                this.weight_label.Visibility = Visibility.Collapsed;
                this.weight_box.Visibility = Visibility.Collapsed;
                this.cat_apply_but.IsEnabled = false;
                this.cat_apply_but.Visibility = Visibility.Visible;
                this.cat_revert_but.IsEnabled = false;
                this.cat_revert_but.Visibility = Visibility.Visible;
                this.container_group.Visibility = Visibility.Collapsed;
                this.prop_group.Visibility = Visibility.Collapsed;
                this.item_apply_but.Visibility = Visibility.Collapsed;
                this.item_revert_but.Visibility = Visibility.Collapsed;
                return;
            }

            if (!this.state.items.ContainsKey(this.selection.name)) { return; }
            ElementReference<ItemSpec> item_ref = this.state.items[this.selection.name];
            bool is_unreferenced = item_ref.ref_count <= 0;
            this.item_rem_but.IsEnabled = is_unreferenced;
            this.cat_label.Visibility = Visibility.Visible;
            this.cat_box.SelectedValue = item_ref.element.category.name;
            this.cat_box.Visibility = Visibility.Visible;
            this.cost_label.Content = "Cost:";
            this.cost_box.SmallChange = 1;
            this.cost_box.Value = (double)(item_ref.element.cost);
            this.value_label.Visibility = Visibility.Visible;
            this.value_box.Value = (double)(item_ref.element.value);
            this.value_box.Visibility = Visibility.Visible;
            this.weight_label.Visibility = Visibility.Visible;
            this.weight_box.Value = (double)(item_ref.element.weight);
            this.weight_box.Visibility = Visibility.Visible;
            this.cat_apply_but.Visibility = Visibility.Collapsed;
            this.cat_revert_but.Visibility = Visibility.Collapsed;
            this.containers.Clear();
            this.container_rows.Clear();
            if (item_ref.element.containers is not null) {
                foreach (ContainerSpec cont in item_ref.element.containers) {
                    this.containers.Add(cont);
                    this.container_rows.Add(new ItemSpecContainerRow(cont.name, cont.weight_factor.ToString(), cont.weight_capacity.ToString()));
                }
            }
            this.container_list.Items.Refresh();
            this.container_add_but.IsEnabled = is_unreferenced;
            this.container_edit_but.IsEnabled = is_unreferenced;
            this.container_up_but.IsEnabled = is_unreferenced;
            this.container_down_but.IsEnabled = is_unreferenced;
            this.container_rem_but.IsEnabled = is_unreferenced;
            this.container_group.Visibility = Visibility.Visible;
            this.properties.Clear();
            this.property_rows.Clear();
            if (item_ref.element.properties is not null) {
                foreach (string prop_name in item_ref.element.properties.Keys) {
                    this.properties[prop_name] = item_ref.element.properties[prop_name];
                    this.property_rows.Add(new ItemSpecPropertyRow(prop_name, item_ref.element.properties[prop_name]));
                }
                this.property_rows.Sort((x, y) => x.name.CompareTo(y.name));
            }
            this.prop_list.Items.Refresh();
            this.prop_group.Visibility = Visibility.Visible;
            this.item_apply_but.IsEnabled = false;
            this.item_apply_but.Visibility = Visibility.Visible;
            this.item_revert_but.IsEnabled = false;
            this.item_revert_but.Visibility = Visibility.Visible;
        }

        private ItemLibraryItemRow do_add_category(string name, decimal sale_value) {
            ItemCategory cat = new ItemCategory(name, sale_value);
            this.state.categories[name] = new ElementReference<ItemCategory>(cat);

            this.categories.Add(name);
            this.categories.Sort();
            this.cat_box.Items.Refresh();
            ItemLibraryItemRow row = new ItemLibraryItemRow(name, true);
            this.item_rows.Add(row);
            this.item_rows.Sort((x, y) => x.name.CompareTo(y.name));
            return row;
        }

        private void add_category(object sender, RoutedEventArgs e) {
            QueryPrompt[] prompts = new QueryPrompt[] {
                new QueryPrompt("Name:", QueryType.STRING),
                new QueryPrompt("Value Factor:", QueryType.FLOAT, 1.0, step: 0.125),
            };
            object[] results = SimpleDialog.askCompound("Add Category", prompts, this);
            if (results is null) { return; }

            string name = results[0] as string;
            if ((name is null) || (this.state.categories.ContainsKey(name))){ return; }

            this.do_add_category(name, (decimal)(results[1] as double?));
            this.item_list.Items.Refresh();
        }

        private void add_item(object sender, RoutedEventArgs e) {
            string default_cat = null;
            if (this.selection is not null) {
                if (this.selection.is_category) { default_cat = this.selection.name; }
                else {
                    ElementReference<ItemSpec> sel_ref = this.state.items[this.selection.name];
                    default_cat = sel_ref.element.category.name;
                }
            }
            QueryPrompt[] prompts = new QueryPrompt[] {
                new QueryPrompt("Name:", QueryType.STRING),
                new QueryPrompt("Category:", QueryType.LIST, default_cat, values: this.categories.ToArray(), canEdit: true),
                new QueryPrompt("Cost:", QueryType.FLOAT),
                new QueryPrompt("Weight:", QueryType.FLOAT),
            };
            object[] results = SimpleDialog.askCompound("Add Item", prompts, this);
            if (results is null) { return; }

            string name = results[0] as string, cat_name = results[1] as string;
            double? cost = results[2] as double?, weight = results[3] as double?;
            if ((name is null) || (this.state.items.ContainsKey(name)) || (cat_name is null) || (cost is null) || (weight is null)) { return; }

            ItemLibraryItemRow cat_row;
            int cat_idx = this.categories.IndexOf(cat_name);
            if (cat_idx >= 0) {
                cat_row = this.item_rows[cat_idx];
            }
            else {
                double? sale_value = SimpleDialog.askFloat(cat_name, "Value Factor:", 1.0, step: 0.125, owner: this);
                if (sale_value is null) { return; }
                cat_row = this.do_add_category(cat_name, (decimal)sale_value);
            }

            ItemSpec item = new ItemSpec(name, this.state.categories[cat_name].element, (decimal)cost, (decimal)weight);
            this.state.items[name] = new ElementReference<ItemSpec>(item);
            this.state.add_category_reference(item);

            ItemLibraryItemRow row = new ItemLibraryItemRow(name);
            if (!this.dirty) {
                if (this.selection is not null) { this.selection._is_selected = false; }
                cat_row._is_expanded = true;
                row._is_selected = true;
            }
            cat_row._items.Add(row);
            cat_row._items.Sort((x, y) => x.name.CompareTo(y.name));
            this.item_list.Items.Refresh();
        }

        private void remove_item(object sender, RoutedEventArgs e) {
            ItemLibraryItemRow sel = this.item_list.SelectedValue as ItemLibraryItemRow;
            if (sel is null) { return; }

            int cat_idx;
            if (sel.is_category) {
                if (!this.state.categories.ContainsKey(sel.name)) { return; }
                ElementReference<ItemCategory> cat_ref = this.state.categories[sel.name];
                if (cat_ref.ref_count > 0) { return; }
                cat_idx = this.categories.IndexOf(sel.name);
                if (cat_idx < 0) { return; }
                this.state.categories.Remove(sel.name);
                this.categories.RemoveAt(cat_idx);
                this.item_rows.RemoveAt(cat_idx);
                this.item_list.Items.Refresh();
                this.cat_box.Items.Refresh();
                return;
            }

            if (!this.state.items.ContainsKey(sel.name)) { return; }
            ElementReference<ItemSpec> item_ref = this.state.items[sel.name];
            if (item_ref.ref_count > 0) { return; }
            cat_idx = this.categories.IndexOf(item_ref.element.category.name);
            if (cat_idx < 0) { return; }
            ItemLibraryItemRow cat_row = this.item_rows[cat_idx];
            int item_idx = cat_row.items.IndexOf(sel);
            if (item_idx < 0) { return; }
            this.state.remove_category_reference(item_ref.element);
            this.state.items.Remove(sel.name);
            cat_row.items.RemoveAt(item_idx);
            this.item_list.Items.Refresh();
        }

        private void set_dirty(bool dirty = true) {
            this.dirty = dirty;
            this.cat_apply_but.IsEnabled = dirty;
            this.cat_revert_but.IsEnabled = dirty;
            this.item_apply_but.IsEnabled = dirty;
            this.item_revert_but.IsEnabled = dirty;
        }
        private void set_dirty(object sender, RoutedEventArgs e) {
            this.set_dirty();
        }

        private void cat_box_sel_changed(object sender, RoutedEventArgs e) {
            if ((this.selection is null) || (this.selection.is_category)) { return; }
            if (!this.state.items.ContainsKey(this.selection.name)) { return; }
            ElementReference<ItemSpec> item_ref = this.state.items[this.selection.name];
            string cat_name = this.cat_box.SelectedValue as string;
            if ((cat_name is null) || (!this.state.categories.ContainsKey(cat_name))) { return; }

            if (item_ref.element.sale_value is null) {
                this.value_box.Value = (double)(((decimal)(this.cost_box.Value)) * this.state.categories[cat_name].element.sale_value);
            }
            if (cat_name != item_ref.element.category.name) {
                this.set_dirty();
            }
        }

        private void cost_changed(object sender, RoutedEventArgs e) {
            if (this.selection is null) { return; }
            this.set_dirty();
            if (this.selection.is_category) { return; }
            if (!this.state.items.ContainsKey(this.selection.name)) { return; }
            ElementReference<ItemSpec> item_ref = this.state.items[this.selection.name];
            if (item_ref.element.sale_value is not null) { return; }
            string cat_name = this.cat_box.SelectedValue as string;
            if ((cat_name is null) || (!this.state.categories.ContainsKey(cat_name))) { return; }
            this.value_box.Value = (double)(((decimal)(this.cost_box.Value)) * this.state.categories[cat_name].element.sale_value);
        }

        private void apply_changes(object sender, RoutedEventArgs e) {
            if (this.selection is null) { return; }
            if (this.selection.is_category) {
                if (!this.state.categories.ContainsKey(this.selection.name)) { return; }
                ElementReference<ItemCategory> cat_ref = this.state.categories[this.selection.name];
                cat_ref.element.name = this.name_box.Text;
                cat_ref.element.sale_value = (decimal)(this.cost_box.Value);
                if (cat_ref.element.name != this.selection.name) {
                    this.state.categories.Remove(this.selection.name);
                    this.state.categories[cat_ref.element.name] = cat_ref;
                    this.categories.Remove(this.selection.name);
                    this.categories.Add(cat_ref.element.name);
                    this.selection._name = cat_ref.element.name;
                    this.categories.Sort();
                    this.item_rows.Sort((x, y) => x.name.CompareTo(y.name));
                    this.item_list.Items.Refresh();
                    this.cat_box.Items.Refresh();
                }
                this.need_refresh = false;
            }
            else {
                if (!this.state.items.ContainsKey(this.selection.name)) { return; }
                ElementReference<ItemSpec> item_ref = this.state.items[this.selection.name];
                string cat_name = this.cat_box.SelectedValue as string, old_cat_name = item_ref.element.category.name;
                if ((cat_name is null) || (!this.state.categories.ContainsKey(cat_name))) { return; }
                item_ref.element.name = this.name_box.Text;
                if (cat_name != old_cat_name) { this.state.remove_category_reference(item_ref.element); }
                item_ref.element.category = this.state.categories[cat_name].element;
                if (cat_name != old_cat_name) { this.state.add_category_reference(item_ref.element); }
                item_ref.element.cost = (decimal)(this.cost_box.Value);
                decimal sale_value = (decimal)(this.value_box.Value);
                if (sale_value == item_ref.element.cost * item_ref.element.category.sale_value) {
                    item_ref.element.sale_value = null;
                }
                else {
                    item_ref.element.sale_value = sale_value;
                }
                item_ref.element.weight = (decimal)(this.weight_box.Value);
                item_ref.element.containers = this.containers.ToArray();
                item_ref.element.properties = new Dictionary<string, string>(this.properties);
                if ((cat_name != old_cat_name) || (item_ref.element.name != this.selection.name)) {
                    if (item_ref.element.name != this.selection.name) {
                        this.state.items.Remove(this.selection.name);
                        this.state.items[item_ref.element.name] = item_ref;
                        this.selection._name = item_ref.element.name;
                    }
                    int cat_idx = this.categories.IndexOf(cat_name);
                    if (cat_name != old_cat_name) {
                        int old_cat_idx = this.categories.IndexOf(old_cat_name);
                        this.item_rows[old_cat_idx]._items.Remove(this.selection);
                        this.item_rows[cat_idx]._items.Add(this.selection);
                        this.item_rows[cat_idx]._is_expanded = true;
                    }
                    this.item_rows[cat_idx]._items.Sort((x, y) => x.name.CompareTo(y.name));
                    this.item_list.Items.Refresh();
                }
                this.need_refresh = false;
            }
            this.set_dirty(dirty: false);
        }

        private void revert_changes(object sender, RoutedEventArgs e) {
            if (this.selection is null) { return; }
            this.name_box.Text = this.selection.name;
            if (this.selection.is_category) {
                if (!this.state.categories.ContainsKey(this.selection.name)) { return; }
                ElementReference<ItemCategory> cat_ref = this.state.categories[this.selection.name];
                this.cost_box.Value = (double)(cat_ref.element.sale_value);
            }
            else {
                if (!this.state.items.ContainsKey(this.selection.name)) { return; }
                ElementReference<ItemSpec> item_ref = this.state.items[this.selection.name];
                this.cat_box.SelectedValue = item_ref.element.category.name;
                this.cost_box.Value = (double)(item_ref.element.cost);
                this.value_box.Value = (double)(item_ref.element.value);
                this.weight_box.Value = (double)(item_ref.element.weight);
                this.containers.Clear();
                this.container_rows.Clear();
                if (item_ref.element.containers is not null) {
                    foreach (ContainerSpec cont in item_ref.element.containers) {
                        this.containers.Add(cont);
                        this.container_rows.Add(new ItemSpecContainerRow(cont.name, cont.weight_factor.ToString(), cont.weight_capacity.ToString()));
                    }
                }
                this.container_list.Items.Refresh();
                this.properties.Clear();
                this.property_rows.Clear();
                if (item_ref.element.properties is not null) {
                    foreach (string prop_name in item_ref.element.properties.Keys) {
                        this.properties[prop_name] = item_ref.element.properties[prop_name];
                        this.property_rows.Add(new ItemSpecPropertyRow(prop_name, item_ref.element.properties[prop_name]));
                    }
                    this.property_rows.Sort((x, y) => x.name.CompareTo(y.name));
                }
                this.prop_list.Items.Refresh();
            }
            this.set_dirty(dirty = false);
        }

        private void fix_listview_column_widths(ListView list_view) {
            GridView grid_view = list_view.View as GridView;
            if (grid_view is null) { return; }
            foreach (GridViewColumn col in grid_view.Columns) {
                col.Width = col.ActualWidth;
                col.Width = double.NaN;
            }
        }

        private void container_add(object sender, RoutedEventArgs e) {
            if ((this.selection is null) || (this.selection.is_category) || (!this.state.items.ContainsKey(this.selection.name))) { return; }
            ElementReference<ItemSpec> item_ref = this.state.items[this.selection.name];
            if (item_ref.ref_count > 0) { return; }
            QueryPrompt[] prompts = new QueryPrompt[] {
                new QueryPrompt("Name:", QueryType.STRING),
                new QueryPrompt("Weight Factor:", QueryType.FLOAT, 1.0, step: 0.125),
                new QueryPrompt("Weight Capacity:", QueryType.FLOAT),
            };
            object[] results = SimpleDialog.askCompound("Add Container", prompts, this);
            if (results is null) { return; }

            string name = results[0] as string;
            double? weight_factor = results[1] as double?, weight_capacity = results[2] as double?;
            if ((name is null) || (weight_factor is null)) { return; }
            if (weight_capacity == 0) { weight_capacity = null; }

            ContainerSpec container = new ContainerSpec(name, (decimal)weight_factor, (decimal?)weight_capacity);
            this.containers.Add(container);
            this.container_rows.Add(new ItemSpecContainerRow(container.name, container.weight_factor.ToString(), container.weight_capacity.ToString()));
            this.container_list.Items.Refresh();
            this.fix_listview_column_widths(this.container_list);
            this.set_dirty();
        }

        private void container_edit(object sender, RoutedEventArgs e) {
            if ((this.selection is null) || (this.selection.is_category) || (!this.state.items.ContainsKey(this.selection.name))) { return; }
            ElementReference<ItemSpec> item_ref = this.state.items[this.selection.name];
            if (item_ref.ref_count > 0) { return; }
            int idx = this.container_list.SelectedIndex;
            if ((idx < 0) || (idx >= this.containers.Count)) { return; }
            ContainerSpec container = this.containers[idx];
            QueryPrompt[] prompts = new QueryPrompt[] {
                new QueryPrompt("Name:", QueryType.STRING, container.name),
                new QueryPrompt("Weight Factor:", QueryType.FLOAT, (double)(container.weight_factor), step: 0.125),
                new QueryPrompt("Weight Capacity:", QueryType.FLOAT, (double)(container.weight_capacity ?? 0)),
            };
            object[] results = SimpleDialog.askCompound("Edit Container", prompts, this);
            if (results is null) { return; }

            string name = results[0] as string;
            double? weight_factor = results[1] as double?, weight_capacity = results[2] as double?;
            if ((name is null) || (weight_factor is null)) { return; }
            if (weight_capacity == 0) { weight_capacity = null; }

            container.name = name;
            container.weight_factor = (decimal)weight_factor;
            container.weight_capacity = (decimal?)weight_capacity;
            ItemSpecContainerRow row = this.container_rows[idx];
            row._name = name;
            row._weight_factor = container.weight_factor.ToString();
            row._weight_capacity = container.weight_capacity.ToString();
            this.container_list.Items.Refresh();
            this.fix_listview_column_widths(this.container_list);
            this.set_dirty();
        }

        private void container_up(object sender, RoutedEventArgs e) {
            if ((this.selection is null) || (this.selection.is_category) || (!this.state.items.ContainsKey(this.selection.name))) { return; }
            ElementReference<ItemSpec> item_ref = this.state.items[this.selection.name];
            if (item_ref.ref_count > 0) { return; }
            int idx = this.container_list.SelectedIndex;
            if ((idx <= 0) || (idx >= this.containers.Count)) { return; }
            ContainerSpec container = this.containers[idx];
            this.containers[idx] = this.containers[idx - 1];
            this.containers[idx - 1] = container;
            ItemSpecContainerRow row = this.container_rows[idx];
            this.container_rows[idx] = this.container_rows[idx - 1];
            this.container_rows[idx - 1] = row;
            this.container_list.Items.Refresh();
            this.set_dirty();
        }

        private void container_down(object sender, RoutedEventArgs e) {
            if ((this.selection is null) || (this.selection.is_category) || (!this.state.items.ContainsKey(this.selection.name))) { return; }
            ElementReference<ItemSpec> item_ref = this.state.items[this.selection.name];
            if (item_ref.ref_count > 0) { return; }
            int idx = this.container_list.SelectedIndex;
            if ((idx < 0) || (idx >= this.containers.Count - 1)) { return; }
            ContainerSpec container = this.containers[idx];
            this.containers[idx] = this.containers[idx + 1];
            this.containers[idx + 1] = container;
            ItemSpecContainerRow row = this.container_rows[idx];
            this.container_rows[idx] = this.container_rows[idx + 1];
            this.container_rows[idx + 1] = row;
            this.container_list.Items.Refresh();
            this.set_dirty();
        }

        private void container_rem(object sender, RoutedEventArgs e) {
            if ((this.selection is null) || (this.selection.is_category) || (!this.state.items.ContainsKey(this.selection.name))) { return; }
            ElementReference<ItemSpec> item_ref = this.state.items[this.selection.name];
            if (item_ref.ref_count > 0) { return; }
            int idx = this.container_list.SelectedIndex;
            if ((idx < 0) || (idx >= this.containers.Count)) { return; }
            this.containers.RemoveAt(idx);
            this.container_rows.RemoveAt(idx);
            this.container_list.Items.Refresh();
            this.set_dirty();
        }

        private void prop_add(object sender, RoutedEventArgs e) {
            if ((this.selection is null) || (this.selection.is_category) || (!this.state.items.ContainsKey(this.selection.name))) { return; }
            QueryPrompt[] prompts = new QueryPrompt[] {
                new QueryPrompt("Name:", QueryType.STRING),
                new QueryPrompt("Value:", QueryType.STRING),
            };
            object[] results = SimpleDialog.askCompound("Add Property", prompts, this);
            if (results is null) { return; }

            string name = results[0] as string, value = results[1] as string;
            if ((name is null) || (value is null)) { return; }

            this.properties[name] = value;
            for (int i = 0; i < this.property_rows.Count; i++) {
                if (this.property_rows[i].name == name) {
                    this.property_rows.RemoveAt(i);
                    break;
                }
            }
            this.property_rows.Add(new ItemSpecPropertyRow(name, value));
            this.property_rows.Sort((x, y) => x.name.CompareTo(y.name));
            this.prop_list.Items.Refresh();
            this.fix_listview_column_widths(this.prop_list);
            this.set_dirty();
        }

        private void prop_edit(object sender, RoutedEventArgs e) {
            if ((this.selection is null) || (this.selection.is_category) || (!this.state.items.ContainsKey(this.selection.name))) { return; }
            int idx = this.prop_list.SelectedIndex;
            if ((idx < 0) || (idx >= this.property_rows.Count)) { return; }
            ItemSpecPropertyRow row = this.property_rows[idx];
            QueryPrompt[] prompts = new QueryPrompt[] {
                new QueryPrompt("Name:", QueryType.STRING, row.name),
                new QueryPrompt("Value:", QueryType.STRING, row.value),
            };
            object[] results = SimpleDialog.askCompound("Edit Property", prompts, this);
            if (results is null) { return; }

            string name = results[0] as string, value = results[1] as string;
            if ((name is null) || (value is null)) { return; }

            if (name != row.name) { this.properties.Remove(row.name); }
            this.properties[name] = value;
            row._name = name;
            row._value = value;
            this.property_rows.Sort((x, y) => x.name.CompareTo(y.name));
            this.prop_list.Items.Refresh();
            this.fix_listview_column_widths(this.prop_list);
            this.set_dirty();
        }

        private void prop_rem(object sender, RoutedEventArgs e) {
            if ((this.selection is null) || (this.selection.is_category) || (!this.state.items.ContainsKey(this.selection.name))) { return; }
            int idx = this.prop_list.SelectedIndex;
            if ((idx < 0) || (idx >= this.property_rows.Count)) { return; }
            this.properties.Remove(this.property_rows[idx].name);
            this.property_rows.RemoveAt(idx);
            this.prop_list.Items.Refresh();
            this.set_dirty();
        }

        private void do_ok(object sender, RoutedEventArgs e) {
            this.valid = true;
            //TODO: prompt if this.dirty?
            this.Close();
        }

        private void do_cancel(object sender, RoutedEventArgs e) {
            this.Close();
        }

        //TODO: get_selected_item
    }
}
