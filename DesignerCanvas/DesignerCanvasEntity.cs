using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using Undefined.DesignerCanvas.ObjectModel;
using Undefined.DesignerCanvas.Primitive;

namespace Undefined.DesignerCanvas
{
    /// <summary>
    /// Used for rendering <see cref="Entity"/> in <see cref="DesignerCanvas" />.
    /// </summary>
    [TemplatePart(Name = "PART_Image", Type = typeof(Image))]
    [TemplatePart(Name = "PART_DragThumb", Type = typeof(DragThumb))]
    [TemplatePart(Name = "PART_DesigningDecorator", Type = typeof(ContainerDesigningDecorator))]
    public class DesignerCanvasEntity : ContentControl
    {
        public bool IsSelected
        {
            get { return (bool)GetValue(IsSelectedProperty); }
            set { SetValue(IsSelectedProperty, value); }
        }

        public static readonly DependencyProperty IsSelectedProperty = Selector.IsSelectedProperty;

        public DesignerCanvas ParentDesigner => DesignerCanvas.FindDesignerCanvas(this);

        #region Interactions

        protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseDown(e);
            ParentDesigner?.NotifyItemMouseDown(this);
            e.Handled = false;
            // Then the Source of MouseDown will be DesignerCanvasItem rather than
            // DesignerCanvas. This is especially to handle the case when 
            // the DesignerCanvasItem recieves a right click.
        }

        #endregion

        static DesignerCanvasEntity()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DesignerCanvasEntity), new FrameworkPropertyMetadata(typeof(DesignerCanvasEntity)));
            Selector.IsSelectedProperty.OverrideMetadata(typeof(DesignerCanvasEntity),
                new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    (sender, e) =>
                    {
                        var s = (DesignerCanvasEntity)sender;
                        s.ParentDesigner?.NotifyItemIsSelectedChanged(s);
                    }));
        }
    }
}
