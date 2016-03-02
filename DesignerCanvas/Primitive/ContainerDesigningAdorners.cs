using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace Undefined.DesignerCanvas.Primitive
{
    /// <summary>
    /// Used for rendering a handle for resizing & rotation.
    /// </summary>
    public class ResizeRotateAdorner : Adorner
    {
        private readonly ResizeRotateChrome chrome;

        protected override int VisualChildrenCount => 1;

        public ResizeRotateAdorner(UIElement destControl) : base(destControl)
        {
            this.SnapsToDevicePixels = true;
            chrome = new ResizeRotateChrome();
            this.AddVisualChild(chrome);
            chrome.DataContext = destControl;
        }

        protected override Size ArrangeOverride(Size arrangeBounds)
        {
            chrome.Arrange(new Rect(arrangeBounds));
            return arrangeBounds;
        }

        protected override Visual GetVisualChild(int index)
        {
            if (index == 0)
                return chrome;
            throw new IndexOutOfRangeException();
        }
    }

    /// <summary>
    /// Used for rendering a handle for resizing & rotation.
    /// </summary>
    public class ResizeRotateChrome : Control
    {
        static ResizeRotateChrome()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ResizeRotateChrome), new FrameworkPropertyMetadata(typeof(ResizeRotateChrome)));
        }
    }

    public class SizeAdorner : Adorner
    {
        private readonly SizeChrome chrome;

        protected override int VisualChildrenCount => 1;

        public SizeAdorner(FrameworkElement designerItem) : base(designerItem)
        {
            this.SnapsToDevicePixels = true;
            chrome = new SizeChrome();
            this.AddVisualChild(chrome);
            chrome.DataContext = designerItem;
        }

        protected override Size ArrangeOverride(Size arrangeBounds)
        {
            chrome.Arrange(new Rect(arrangeBounds));
            return arrangeBounds;
        }

        protected override Visual GetVisualChild(int index)
        {
            if (index == 0) return chrome;
            throw new IndexOutOfRangeException();
        }
    }

    /// <summary>
    /// Used for displaying object dimension when resizing items.
    /// </summary>
    public class SizeChrome : Control
    {
        static SizeChrome()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SizeChrome), new FrameworkPropertyMetadata(typeof(SizeChrome)));
        }
    }

}
