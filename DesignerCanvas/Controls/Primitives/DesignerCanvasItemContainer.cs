using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace Undefined.DesignerCanvas.Controls.Primitives
{
    /// <summary>
    /// Used for rendering <see cref="CanvasItem"/> in <see cref="DesignerCanvas" />.
    /// </summary>
    [TemplatePart(Name = "PART_DragThumb", Type = typeof(DragThumb))]
    [TemplatePart(Name = "PART_ContentPresenter", Type = typeof(ContentPresenter))]
    public class DesignerCanvasItemContainer : ContentControl
    {
        private ContentPresenter contentPresenter;

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


        public DesignerCanvas ParentDesigner => Controls.DesignerCanvas.FindDesignerCanvas(this);

        /// <summary>
        /// When overridden in a derived class, is invoked whenever application code or internal processes call <see cref="M:System.Windows.FrameworkElement.ApplyTemplate"/>.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            contentPresenter = GetTemplateChild("PART_ContentPresenter") as ContentPresenter;
        }

        // Not supported yet.

        //public static Geometry GetContainerClip(DependencyObject obj)
        //{
        //    return (Geometry)obj.GetValue(ContainerClipProperty);
        //}

        //public static void SetContainerClip(DependencyObject obj, Geometry value)
        //{
        //    obj.SetValue(ContainerClipProperty, value);
        //}

        //public static readonly DependencyProperty ContainerClipProperty =
        //    DependencyProperty.RegisterAttached("ContainerClip",
        //        typeof(Geometry), typeof(DesignerCanvasItemContainer), new FrameworkPropertyMetadata(null, (d, e) =>
        //        {
        //            var container = (d as FrameworkElement)?.Parent as DesignerCanvasItemContainer;
        //            container?.NotifyContentContainerClipChanged((Geometry) e.NewValue);
        //        }));

        /// <summary>
        /// Raises when user attempt to start dragging the item.
        /// </summary>
        public static readonly RoutedEvent BeforeDraggingStartedEvent =
            EventManager.RegisterRoutedEvent("BeforeDraggingStarted", RoutingStrategy.Direct,
                typeof(RoutedEventHandler), typeof(DesignerCanvasItemContainer));

        public static void AddBeforeDraggingStartedHandler(DependencyObject d, RoutedEventHandler handler)
        {
            (d as UIElement)?.AddHandler(BeforeDraggingStartedEvent, handler);
        }

        public static void RemoveBeforeDraggingStartedHandler(DependencyObject d, RoutedEventHandler handler)
        {
            (d as UIElement)?.RemoveHandler(BeforeDraggingStartedEvent, handler);
        }

        #region Interactions

        protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseDown(e);
            ParentDesigner?.NotifyItemMouseDown(this);
            Focus();
            if (VisualTreeHelper.GetChildrenCount(contentPresenter) > 0)
            {
                var content = VisualTreeHelper.GetChild(contentPresenter, 0) as UIElement;
                if (content != null)
                {
                    var ee = new RoutedEventArgs(BeforeDraggingStartedEvent, this);
                    content.RaiseEvent(ee);
                }
            }
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

        private void NotifyContentContainerClipChanged(Geometry newClip)
        {
            this.Clip = newClip;
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

#if DEBUG
        // See OnApplyTemplate()
        private static readonly Brush BoundaryIndicatorBrush =
            new SolidColorBrush(Color.FromArgb(102, 0, 255, 255));
#endif

        public DesignerCanvasItemContainer()
        {
#if DEBUG
            // Display the boundary of the container. This is used for debugging.
            BorderThickness = new Thickness(1);
            BorderBrush = BoundaryIndicatorBrush;
#endif
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
