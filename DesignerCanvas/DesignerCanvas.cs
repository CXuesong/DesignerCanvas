using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Undefined.DesignerCanvas
{
    /// <summary>
    /// 用于承载绘图图面。
    /// </summary>
    public class DesignerCanvas : Canvas
    {
        static DesignerCanvas()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DesignerCanvas), new FrameworkPropertyMetadata(typeof(DesignerCanvas)));
        }
    }
}
