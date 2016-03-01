using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Undefined.DesignerCanvas
{
    [TemplatePart(Name = "PART_Image", Type = typeof(Image))]
    public class DesignerCanvasItem : Control
    {
        public ImageSource Image
        {
            get { return (ImageSource)GetValue(ImageProperty); }
            set { SetValue(ImageProperty, value); }
        }

        public static readonly DependencyProperty ImageProperty =
            DependencyProperty.Register("Image", typeof(ImageSource), typeof(DesignerCanvasItem), new PropertyMetadata(null));



        static DesignerCanvasItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DesignerCanvasItem), new FrameworkPropertyMetadata(typeof(DesignerCanvasItem)));
        }
    }
}
