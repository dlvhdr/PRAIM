using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace Common.Controls
{
    public class NullableCombobox : ComboBox
    {
        public NullableCombobox()
            : base()
        {
            this.KeyUp += OnKeyUp;

            MenuItem item = new MenuItem();
            item.Header = "Remove Selection";
            item.Command = new Command(canExec, exec);
            ContextMenu ctx_menu = new ContextMenu();
            ctx_menu.Items.Add(item);
            this.ContextMenu = ctx_menu;
        }

        private void exec(object parameter)
        {
            this.SelectedItem = null;
        }

        private bool canExec(object parameter)
        {
            return true;
        }

        private void OnKeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Escape || e.Key == Key.Delete) {
                this.SelectedItem = null;
            }
        }
    }
}
