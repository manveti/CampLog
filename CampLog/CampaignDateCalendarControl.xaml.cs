using System;
using System.Windows;
using System.Windows.Controls;

namespace CampLog {
    public partial class CampaignDateCalendarControl : UserControl, ICalendarControl {
        private Action _value_changed;

        public CampaignDateCalendarControl() {
            InitializeComponent();
            this.day_box.textBox.VerticalContentAlignment = VerticalAlignment.Center;
            this.day_box.TextChanged += this.handleValueChange;
            this.time_box.textBox.VerticalContentAlignment = VerticalAlignment.Center;
            this.time_box.TextChanged += this.handleValueChange;
        }

        public bool IsReadOnly {
            get => this.day_box.IsReadOnly;
            set {
                this.day_box.IsReadOnly = value;
                this.time_box.IsReadOnly = value;
            }
        }

        public Action value_changed {
            get => this._value_changed;
            set => this._value_changed = value;
        }

        public decimal calendar_value {
            get => (decimal)((this.day_box.Value * CampaignDateCalendar.DAY_LENGTH) + this.time_box.Value);
            set {
                this.day_box.Value = (long)(value / CampaignDateCalendar.DAY_LENGTH);
                this.time_box.Value = (double)(value % CampaignDateCalendar.DAY_LENGTH);
            }
        }

        private void handleValueChange(object sender, TextChangedEventArgs e) {
            if (this._value_changed is not null) {
                this._value_changed();
            }
        }

        public void setTimestampMode() {
            this.pre_label.Visibility = Visibility.Visible;
            this.post_label.Visibility = Visibility.Collapsed;
        }

        public void setIntervalMode() {
            this.pre_label.Visibility = Visibility.Collapsed;
            this.post_label.Visibility = Visibility.Visible;
        }
    }
}
