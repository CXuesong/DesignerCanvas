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
            dcvs.Items.Add(new GraphicalObject(10, 10, 32, 32, LoadImageResource("1.png")));
            dcvs.Items.Add(new GraphicalObject(50, 10, 32, 32, LoadImageResource("2.png")));
            var bumpingItem = new GraphicalObject(50, 50, 32, 32, LoadImageResource("3.png"));
            dcvs.Items.Add(bumpingItem);
            var timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(10),
            };
            var rnd = new Random();
            timer.Tick += (_, e1) =>
            {
                bumpingItem.Left += (rnd.NextDouble() - 0.5)*2;
                bumpingItem.Top += (rnd.NextDouble() - 0.5)*2;
            };
            timer.Start();
        }

        private void TestButton_Click(object sender, RoutedEventArgs e)
        {
            //var b = BindingOperations.GetBinding(TestButton, Button.CommandProperty);
        }

        private void AddItemsButton_Click(object sender, RoutedEventArgs e)
        {
            var columns = 25;
            var res = new[] {"1.png", "2.png", "3.png"};
            var rnd = new Random();
            var offset = dcvs.Items.Count;
            for (int i = offset; i < offset + 1000; i++)
            {
                var row = i/columns;
                var col = i%columns;
                dcvs.Items.Add(new GraphicalObject(100 + row*40, 10 + col*40, 32, 32,
                    LoadImageResource(res[rnd.Next(0, res.Length)])));
            }
        }
    }
}
