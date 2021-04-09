using System;
using System.Windows;

namespace CampLog {
    public partial class CampaignPropsWindow : Window {
        public bool valid;
        private CalendarParameters parameters;

        public CampaignPropsWindow() {
            this.valid = false;
            this.parameters = null;
            InitializeComponent();
            this.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            this.SizeToContent = SizeToContent.WidthAndHeight;
            this.calendar_box.ItemsSource = CalendarSpecs.specs.Keys;
            this.calendar_box.SelectedValue = "None";
            this.charsheet_box.ItemsSource = CharacterSheetSpecs.specs.Keys;
            this.charsheet_box.SelectedValue = "None";
        }

        private void do_ok(object sender, RoutedEventArgs e) {
            this.valid = true;
            this.Close();
        }

        private void do_cancel(object sender, RoutedEventArgs e) {
            this.Close();
        }

        public Calendar get_calendar() {
            string calendar_name = this.calendar_box.SelectedValue as string;
            if ((calendar_name is null) || (!CalendarSpecs.specs.ContainsKey(calendar_name))) { throw new InvalidOperationException(); }
            return CalendarSpecs.specs[calendar_name].get_calendar(this.parameters);
        }

        public CharacterSheet get_character_sheet() {
            string charsheet_name = this.charsheet_box.SelectedValue as string;
            if ((charsheet_name is null) || (!CharacterSheetSpecs.specs.ContainsKey(charsheet_name))) { throw new InvalidOperationException(); }
            return CharacterSheetSpecs.specs[charsheet_name].get_character_sheet();
        }
    }
}
