using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using Undefined.DesignerCanvas.Controls;
using Undefined.DesignerCanvas.Controls.Primitives;

namespace Undefined.DesignerCanvas
{
    /// <summary>
    /// Used for rendering <see cref="CanvasItem"/> in <see cref="DesignerCanvas" />.
    /// </summary>
    [TemplatePart(Name = "PART_Image", Type = typeof(Image))]
    [TemplatePart(Name = "PART_DragThumb", Type = typeof(DragThumb))]
    public class DesignerCanvasItemContainer : ContentControl
    {
        public bool IsSelected
        {
            get { return (bool)GetValue(IsSelectedProperty); }
            set { SetValue(IsSelectedProperty, value); }
        }

        public static readonly DependencyProperty IsSelectedProperty = Selector.IsSelectedProperty;
    
        /// <summary>
        /// Determines whether the entity can be resized.
        /// </summary>
        public bool Resizeable
        {
            get { return (bool)GetValue(ResizeableProperty); }
            set { SetValue(ResizeableProperty, value); }
        }

        public static readonly DependencyProperty ResizeableProperty =
            DependencyProperty.Register("Resizeable", typeof(bool), typeof(DesignerCanvasItemContainer), new PropertyMetadata(true));


        public Controls.DesignerCanvas ParentDesigner => Controls.DesignerCanvas.FindDesignerCanvas(this);

        #region Interactions

        protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseDown(e);
            ParentDesigner?.NotifyItemMouseDown(this);
            Focus();
        }

        #endregion

        private CanvasAdorner designerAdorner;

        private void UpdateDesignerAdorner()
        {
            var pd = ParentDesigner;
            var obj = DataContext as ICanvasItem;
            if (!IsSelected || pd == null || obj == null)
            {
                if (designerAdorner != null)
                {
                    designerAdorner.ParentCanvas.RemoveAdorner(designerAdorner);
                    designerAdorner = null;
                }
            }
            else
            {
                if (designerAdorner == null)
                {
                    designerAdorner = ParentDesigner.GenerateDesigningAdornerFormItem(obj);
                    if (designerAdorner != null)
                    {
                        designerAdorner.SetCanvas(pd);
                        pd.AddAdorner(designerAdorner);
                    }
                }
            }
        }

        /// <summary>
        /// Invoked when the parent of this element in the visual tree is changed. Overrides <see cref="M:System.Windows.UIElement.OnVisualParentChanged(System.Windows.DependencyObject)"/>.
        /// </summary>
        /// <param name="oldParent">The old parent element. May be null to indicate that the element did not have a visual parent previously.</param>
        protected override void OnVisualParentChanged(DependencyObject oldParent)
        {
            base.OnVisualParentChanged(oldParent);
            UpdateDesignerAdorner();
        }

        static DesignerCanvasItemContainer()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DesignerCanvasItemContainer), new FrameworkPropertyMetadata(typeof(DesignerCanvasItemContainer)));
            Selector.IsSelectedProperty.OverrideMetadata(typeof(DesignerCanvasItemContainer),
                new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    (sender, e) =>
                    {
                        var s = (DesignerCanvasItemContainer)sender;
                        s.UpdateDesignerAdorner();
                        s.ParentDesigner?.NotifyItemIsSelectedChanged(s);
                    }));
            FocusableProperty.OverrideMetadata(typeof(DesignerCanvasItemContainer), new FrameworkPropertyMetadata(true));
        }
    }
}
