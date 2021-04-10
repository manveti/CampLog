using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace CampLog {
    public class CharacterRow {
        public string _name;
        public Guid _guid;

        public string name { get => this._name; }
        public Guid guid { get => this._guid; }

        public CharacterRow(string name, Guid guid) {
            this._name = name;
            this._guid = guid;
        }
    }


    public partial class CharacterListControl : UserControl {
        private Action<List<EntryAction>> change_callback;
        private CharacterSheet char_sheet;
        private CampaignState state;
        private ObservableCollection<CharacterRow> character_rows;

        public CharacterListControl(Action<List<EntryAction>> change_callback) {
            this.change_callback = change_callback;
            this.char_sheet = new CharacterSheet();
            this.state = null;
            this.character_rows = null;
            InitializeComponent();
        }

        private void populate_character_rows() {
            List<CharacterRow> rows = new List<CharacterRow>();
            foreach (Guid guid in state.characters.active_characters) {
                rows.Add(new CharacterRow(state.characters.characters[guid].name, guid));
            }
            rows.Sort((CharacterRow r1, CharacterRow r2) => r1.name.CompareTo(r2.name));
            this.character_rows = new ObservableCollection<CharacterRow>(rows);
            this.character_list.ItemsSource = this.character_rows;
        }

        public void set_char_sheet(CharacterSheet char_sheet) {
            this.char_sheet = char_sheet;
        }

        public void set_state(CampaignState state) {
            this.state = state;
            if (this.state is null) {
                this.add_but.IsEnabled = false;
                this.rem_but.IsEnabled = false;
                this.view_but.IsEnabled = false;
                this.character_rows.Clear();
                return;
            }
            this.populate_character_rows();
            this.add_but.IsEnabled = true;
        }

        private void list_sel_changed(object sender, RoutedEventArgs e) {
            Guid? guid = this.character_list.SelectedValue as Guid?;
            if ((guid is null) || (!this.state.characters.active_characters.Contains(guid.Value))) {
                this.rem_but.IsEnabled = false;
                this.view_but.IsEnabled = false;
                return;
            }
            this.rem_but.IsEnabled = true;
            this.view_but.IsEnabled = true;
        }

        private void do_add(object sender, RoutedEventArgs e) {
            if ((this.char_sheet is null) || (this.state is null)) { return; }
            Window dialog_window = this.char_sheet.character_window(this.state);
            dialog_window.Owner = Window.GetWindow(this);
            dialog_window.ShowDialog();
            if (this.change_callback is null) { return; }
            ICharacterWindow char_window = dialog_window as ICharacterWindow;
            if (char_window is null) { return; }
            List<EntryAction> actions = char_window.get_actions();
            if ((actions is null) || (actions.Count <= 0)) { return; }
            this.change_callback(actions);
        }

        private void do_rem(object sender, RoutedEventArgs e) {
            if ((this.change_callback is null) || (this.state is null)) { return; }
            Guid? guid = this.character_list.SelectedValue as Guid?;
            if ((guid is null) || (!this.state.characters.active_characters.Contains(guid.Value))) { return; }
            EntryAction action = new ActionCharacterSet(guid.Value, this.state.characters.characters[guid.Value], null);
            List<EntryAction> actions = new List<EntryAction>() { action };
            this.change_callback(actions);
        }

        private void do_view(object sender, RoutedEventArgs e) {
            if ((this.char_sheet is null) || (this.state is null)) { return; }
            Guid? guid = this.character_list.SelectedValue as Guid?;
            if ((guid is null) || (!this.state.characters.active_characters.Contains(guid.Value))) { return; }
            Window dialog_window = this.char_sheet.character_window(this.state, guid.Value);
            dialog_window.Owner = Window.GetWindow(this);
            dialog_window.ShowDialog();
            if (this.change_callback is null) { return; }
            ICharacterWindow char_window = dialog_window as ICharacterWindow;
            if (char_window is null) { return; }
            List<EntryAction> actions = char_window.get_actions();
            if ((actions is null) || (actions.Count <= 0)) { return; }
            this.change_callback(actions);
        }
    }
}
