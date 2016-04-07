using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    /// This decorator is used to render handles for resizing an item (or container, exactly).
    /// The DataContext property should be set to the control subject to changes.
    /// </summary>
    public class ContainerDesigningDecorator : Control
    {
        private Adorner adorner;

        /// <summary>
        /// Decides wheter to display the adorners used for resizing & rotation.
        /// </summary>
        public bool DecoratorVisible
        {
            get { return (bool)GetValue(DecoratorVisibleProperty); }
            set { SetValue(DecoratorVisibleProperty, value); }
        }

        public static readonly DependencyProperty DecoratorVisibleProperty =
            DependencyProperty.Register("DecoratorVisible", typeof (bool), typeof (ContainerDesigningDecorator),
                new FrameworkPropertyMetadata(false, DecoratorVisibleProperty_Changed));

        /// <summary>
        /// When overridden in a derived class, participates in rendering operations that are directed by the layout system. The rendering instructions for this element are not used directly when this method is invoked, and are instead preserved for later asynchronous use by layout and drawing. 
        /// </summary>
        /// <param name="drawingContext">The drawing instructions for a specific element. This context is provided to the layout system.</param>
        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
            var adornerParent = adorner?.Parent as AdornerLayer;
            if (DecoratorVisible)
            {
                var adornerLayer = AdornerLayer.GetAdornerLayer(this);
                if (adornerLayer == null) return;
                if (adorner != null && adornerParent != adornerLayer)
                {
                    adornerParent?.Remove(adorner);
                    adorner = null;
                }
                if (adorner == null)
                {
                    var designerItem = DataContext as Control;
                    if (designerItem == null) return;
                    adorner = new ResizeRotateAdorner(designerItem);
                    adornerLayer.Add(adorner);
                }
            }
            else
            {
                if (adorner == null) return;
                adornerParent?.Remove(adorner);
                adorner = null;
            }
        }

        private void ComponentDesigningDecorator_Unloaded(object sender, RoutedEventArgs e)
        {
            if (adorner != null)
            {
                var aLayer = AdornerLayer.GetAdornerLayer(this);
                aLayer?.Remove(adorner);
                adorner = null;
            }
        }

        private static void DecoratorVisibleProperty_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var decorator = (ContainerDesigningDecorator)d;
            decorator.InvalidateVisual();
        }

        public ContainerDesigningDecorator()
        {
            Unloaded += ComponentDesigningDecorator_Unloaded;
        }
    }

}
