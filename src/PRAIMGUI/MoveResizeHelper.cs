using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace PRAIM
{
    public class MoveResizeHelper : Thumb
    {
        public MoveResizeHelper()
        {
            DragDelta += OnDragDelta;
        }

        private void OnDragDelta(object sender, DragDeltaEventArgs e)
        {
            ContentControl control = this.DataContext as ContentControl;

            double left_pos = Canvas.GetLeft(control);
            double top_pos = Canvas.GetTop(control);

            double new_left = left_pos + e.HorizontalChange;
            double new_top = top_pos + e.VerticalChange;

            if (new_left >= 0) {
                Canvas.SetLeft(control, left_pos + e.HorizontalChange);
            }
            if (new_top >= 0) {
                Canvas.SetTop(control, top_pos + e.VerticalChange);
            }
            
        }
    }
}
