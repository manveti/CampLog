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
            //TODO: populate this.charsheet_box
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
    }
}
