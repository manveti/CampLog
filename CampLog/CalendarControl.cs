using System;
using System.Windows;
using System.Windows.Controls;

using GUIx;

namespace CampLog {
    public interface ICalendarControl {
        public bool IsReadOnly { get; set; }
        public Action value_changed { get; set; }
        public decimal calendar_value { get; set; }
    }


    public class SimpleCalendarControl : SpinBox, ICalendarControl {
        private Action _value_changed;

        public SimpleCalendarControl() : base() {
            this.textBox.MinWidth = 50;
            this.textBox.VerticalContentAlignment = VerticalAlignment.Center;
        }

        public override void handleTextChange(object sender, TextChangedEventArgs e){
            base.handleTextChange(sender, e);
            if (this._value_changed is not null) {
                this._value_changed();
            }
        }

        public Action value_changed {
            get => this._value_changed;
            set => this._value_changed = value;
        }

        public decimal calendar_value {
            get => (decimal)(this.Value);
            set => this.Value = (double)value;
        }
    }
}