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

namespace CampLog {
    public enum PropertyType {
        text = 0,
        num,
        set,
        dict,
    }

    public partial class SimpleCharacterPropertyWindow : Window {
        private readonly string[] PROPERTY_TYPE_NAMES = { "Text", "Number", "Set", "Dictionary" };
        public bool valid;
        public string _name;
        public string name {
            get => this._name;
            set => this._name = value;
        }

        public SimpleCharacterPropertyWindow(string name = "") {
            this.valid = false;
            this.name = name;
            InitializeComponent();
            foreach (string type_name in PROPERTY_TYPE_NAMES) {
                this.type_list.Items.Add(type_name);
            }
            this.type_list.SelectedIndex = (int)(PropertyType.text);
        }

        private void type_list_sel_changed(object sender, RoutedEventArgs e) {
            int idx = this.type_list.SelectedIndex;
            bool is_text = (idx == ((int)(PropertyType.text))), is_num = (idx == ((int)(PropertyType.num)));
            this.value_label.Visibility = ((is_text || is_num) ? Visibility.Visible : Visibility.Hidden);
            this.text_value_box.Visibility = (is_text ? Visibility.Visible : Visibility.Hidden);
            this.num_value_box.Visibility = (is_num ? Visibility.Visible : Visibility.Hidden);
        }

        private void do_ok(object sender, RoutedEventArgs e) {
            if (this.name.Length <= 0) {
                MessageBox.Show("Property name must not be empty.", "Error", MessageBoxButton.OK);
                return;
            }
            this.valid = true;
            this.Close();
        }

        private void do_cancel(object sender, RoutedEventArgs e) {
            this.Close();
        }

        public CharProperty get_property() {
            return this.type_list.SelectedIndex switch {
                (int)(PropertyType.text) => new CharTextProperty(this.text_value_box.Text),
                (int)(PropertyType.num) => new CharNumProperty((decimal)(this.num_value_box.Value)),
                (int)(PropertyType.set) => new CharSetProperty(),
                (int)(PropertyType.dict) => new CharDictProperty(),
                _ => null,
            };
        }
    }
}
