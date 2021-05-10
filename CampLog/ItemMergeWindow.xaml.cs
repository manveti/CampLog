using System.Collections.Generic;
using System.Windows;

namespace CampLog {
    public partial class ItemMergeWindow : Window {
        public bool valid;

        public ItemMergeWindow(List<InventoryItemRow> item_rows) {
            this.valid = false;
            InitializeComponent();
            this.item_list.ItemsSource = item_rows;
        }

        private void do_ok(object sender, RoutedEventArgs e) {
            this.valid = true;
            this.Close();
        }

        private void do_cancel(object sender, RoutedEventArgs e) {
            this.Close();
        }

        public InventoryItemIdent get_item() {
            if (!this.valid) { return null; }
            return this.item_list.SelectedValue as InventoryItemIdent;
        }
    }
}
