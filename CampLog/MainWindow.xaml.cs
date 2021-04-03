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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CampLog {
    public partial class MainWindow : Window {
        private CampaignSave state;
        private bool state_dirty;
        private string save_path;

        public MainWindow() {
            this.save_path = null;
            this.state = null;
            this.state_dirty = false;
            InitializeComponent();
        }

        private void new_campaign(object sender, RoutedEventArgs e) {
            if (this.state_dirty) {
                string msg = "Do you want to save changes to " + (this.save_path ?? "Untitled") + "?";
                MessageBoxResult result = MessageBox.Show(msg, "Campaign Log", MessageBoxButton.YesNoCancel);
                if (result == MessageBoxResult.Cancel) { return; }
                if (result != MessageBoxResult.No) {
                    //TODO: save this.state
                    this.state_dirty = false;
                }
            }

            CampaignPropsWindow props_window = new CampaignPropsWindow() { Owner = this };
            props_window.ShowDialog();
            if (!props_window.valid) { return; }

            Calendar cal;
            try {
                cal = props_window.get_calendar();
                //TODO: character sheet
            }
            catch (InvalidOperationException) { return; }

            this.state = new CampaignSave(cal);
            this.state_dirty = false;
            this.save_path = null;

            this.save_opt.IsEnabled = true;
            this.save_as_opt.IsEnabled = true;
            this.calendar_cfg_opt.IsEnabled = true;
            this.charsheet_cfg_opt.IsEnabled = true;
            this.item_library_opt.IsEnabled = true;
            this.revert_opt.IsEnabled = true;
            //TODO: characters_list, char_add_but, char_rem_but, char_view_but
            //TODO: inventories_list, inv_add_but, inv_rem_but, inv_view_but
            //TODO: session_num_box, current_timestamp_box
            //TODO: entries_list, ent_add_but, ent_rem_but, ent_view_but
            this.show_past_events_box.IsChecked = this.state.show_past_events;
            //TODO: events_list, event_add_but, event_rem_but, event_view_but
            this.show_inactive_tasks_box.IsChecked = this.state.show_inactive_tasks;
            //TODO: tasks_list, task_add_but, task_rem_but, task_resolve_but, task_view_but
            //TODO: topics_list, topic_add_but, topic_rem_but, topic_view_but
        }

        private void do_test(object sender, RoutedEventArgs e) {
            EntryWindow ew = new EntryWindow() { Owner = this };
            ew.ShowDialog();
        }
    }
}
