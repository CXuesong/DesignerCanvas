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
using System.Windows.Threading;
using Undefined.DesignerCanvas;
using Undefined.DesignerCanvas.ObjectModel;

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
            var g1 = new Entity(10, 10, 32, 32, LoadImageResource("1.png"));
            var g2 = new Entity(50, 10, 32, 32, LoadImageResource("2.png"));
            var bumpingItem = new Entity(50, 50, 32, 32, LoadImageResource("3.png"));
            dcvs.Items.AddRange(new[] {g1, g2, bumpingItem});
            dcvs.Items.AddRange(new[]
            {
                new Connection(g1.Connectors[0], g2.Connectors[0]),
                new Connection(g2.Connectors[1], bumpingItem.Connectors[2]),
            });
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
                RenderedChildrenCounter.Text = "" + dcvs.RenderedChildrenCount;
            };
            statTimer.Start();
        }

        private void TestButton_Click(object sender, RoutedEventArgs e)
        {
            //var b = BindingOperations.GetBinding(TestButton, Button.CommandProperty);
        }

        private void AddItemsButton_Click(object sender, RoutedEventArgs e)
        {
            var columns = 50;
            var res = new[] {"1.png", "2.png", "3.png"};
            var rnd = new Random();
            var offset = dcvs.Items.Count;
            var objs = new List<Entity>();
            for (int i = offset; i < offset + 10000; i++)
            {
                var row = i/columns;
                var col = i%columns;
                var obj = new Entity(10 + col * 40, 100 + row * 40, 32, 32,
                    LoadImageResource(res[rnd.Next(0, res.Length)]));
                dcvs.Items.Add(obj);
                if (objs.Count > 0 && rnd.NextDouble() < 0.2)
                {
                    var anotherObj = objs[rnd.Next(Math.Max(0, objs.Count - 10), objs.Count)];
                    var conn = new Connection(obj.Connectors[rnd.Next(0, obj.Connectors.Count)],
                        anotherObj.Connectors[rnd.Next(0, anotherObj.Connectors.Count)]);
                    dcvs.Items.Add(conn);
                }
                objs.Add(obj);
            }
        }
    }
}
