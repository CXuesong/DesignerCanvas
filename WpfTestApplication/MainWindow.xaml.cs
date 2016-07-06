using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
using System.Windows.Threading;
using Undefined.DesignerCanvas;
using IOPath = System.IO.Path;

namespace WpfTestApplication
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private ImageSource LoadImageResource(string fileName)
        {
            return new BitmapImage(new Uri(@"Resources/Images/" + fileName, UriKind.RelativeOrAbsolute));
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var g1 = new CanvasItem(10, 10, 32, 32, LoadImageResource("1.png"));
            var g2 = new CanvasItem(50, 10, 32, 32, LoadImageResource("2.png")) {Angle = 90};
            var myG1 = new MyEntity {Bounds = new Rect(100, 10, 20, 20)};
            var bumpingItem = new CanvasItem(50, 50, 32, 32, LoadImageResource("3.png"));
            dcvs.Items.AddRange(new ICanvasItem[] {g1, g2, bumpingItem, myG1});
            var aniTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(10),
            };
            var rnd = new Random();
            aniTimer.Tick += (_, e1) =>
            {
                bumpingItem.Left += (rnd.NextDouble() - 0.5)*2;
                bumpingItem.Top += (rnd.NextDouble() - 0.5)*2;
            };
            aniTimer.Start();

            var statTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(500),
            };
            statTimer.Tick += (_, e1) =>
            {
#if DEBUG
                RenderedChildrenCounter.Text = "" + dcvs.RenderedChildrenCount;
#endif
                FocusLabel.Text = FocusManager.GetFocusedElement(this)?.ToString();
            };
            statTimer.Start();
        }

        private void TestButton_Click(object sender, RoutedEventArgs e)
        {
            GC.Collect();
            //var b = BindingOperations.GetBinding(TestButton, Button.CommandProperty);
        }

        private void AddItemsButton_Click(object sender, RoutedEventArgs e)
        {
            var columns = 50;
            var res = new[] {"1.png", "2.png", "3.png"};
            var rnd = new Random();
            var offset = dcvs.Items.Count;
            var objs = new List<CanvasItem>();
            for (int i = offset; i < offset + 10000; i++)
            {
                var row = i/columns;
                var col = i%columns;
                var obj = new CanvasItem(10 + col * 40, 100 + row * 40, 32, 32,
                    LoadImageResource(res[rnd.Next(0, res.Length)]));
                dcvs.Items.Add(obj);
                //if (objs.Count > 0 && rnd.NextDouble() < 0.2)
                //{
                //    var anotherObj = objs[rnd.Next(Math.Max(0, objs.Count - 10), objs.Count)];
                //    var conn = new Connection(obj.Connectors[rnd.Next(0, obj.Connectors.Count)],
                //        anotherObj.Connectors[rnd.Next(0, anotherObj.Connectors.Count)]);
                //    dcvs.Items.Add(conn);
                //}
                objs.Add(obj);
            }
        }

        private void dcvs_MouseDown(object sender, MouseButtonEventArgs e)
        {
            
        }

        private void dcvs_MouseMove(object sender, MouseEventArgs e)
        {
            MousePositionLabel.Content = dcvs.PointToCanvas(e.GetPosition(dcvs));
        }

        private void ExportImageButton_Click(object sender, RoutedEventArgs e)
        {
            var tempPath = IOPath.Combine(IOPath.GetTempPath(), "designerCanvasTemp.png");
            dcvs.ExportImage(tempPath);
            Process.Start(tempPath, null);
        }
    }
}
