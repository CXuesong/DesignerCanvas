using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

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

        private void ShowAdorner()
        {
            var aLayer = AdornerLayer.GetAdornerLayer(this);
            if (aLayer == null) return;
            // The adorner should be abandoned in HideAdorner.
            Debug.Assert(adorner == null);
            var designerItem = DataContext as Control;
            if (designerItem == null) return;
            adorner = new ResizeRotateAdorner(designerItem);
            aLayer.Add(adorner);
        }

        private void HideAdorner()
        {
            if (adorner == null) return;
            var aLayer = AdornerLayer.GetAdornerLayer(this);
            if (aLayer == null) return;
            aLayer.Remove(adorner);
            adorner = null;
        }
        
        private void ComponentDesigningDecorator_Unloaded(object sender, RoutedEventArgs e)
        {
            if (adorner != null)
            {
                AdornerLayer aLayer = AdornerLayer.GetAdornerLayer(this);
                aLayer?.Remove(this.adorner);
                adorner = null;
            }
        }

        private static void DecoratorVisibleProperty_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var decorator = (ContainerDesigningDecorator)d;
            var DecoratorVisible = (bool) e.NewValue;
            if (DecoratorVisible)
                decorator.ShowAdorner();
            else 
                decorator.HideAdorner();
        }
        public ContainerDesigningDecorator()
        {
            Unloaded += ComponentDesigningDecorator_Unloaded;
        }
    }

}
