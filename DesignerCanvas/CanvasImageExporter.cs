using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Undefined.DesignerCanvas
{
    class CanvasImageExporter
    {
        public const double WpfDpi = 96;

        private static void DoEvents()
        {
            var frame = new DispatcherFrame(true);
            Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Loaded,
                (SendOrPostCallback) (arg =>
                {
                    var f = (DispatcherFrame) arg;
                    f.Continue = false;
                }), frame);
            Dispatcher.PushFrame(frame);
        }

        public static BitmapSource CreateImage(DesignerCanvas canvas, double dpiX, double dpiY)
        {
            var image = new RenderTargetBitmap((int) (canvas.ExtentWidth*dpiX/WpfDpi),
                (int) (canvas.ExtentHeight*dpiX/WpfDpi), dpiX, dpiY,
                PixelFormats.Pbgra32);
            canvas.ShowContainers();
            // Wait for item rendering.
            DoEvents();
            image.Render(canvas);
            canvas.HideCoveredContainers();
            return image;
            /*
            var drawing = new DrawingVisual();
            var aaa = new RenderTargetBitmap(100, 100, dpiX, dpiY,
                PixelFormats.Pbgra32);
            //aaa.Render(new Label {Content = "bbb"});
            ExportImage(aaa, "D:\\test.png");
            using (var dc = drawing.RenderOpen())
            {
                dc.DrawRectangle(Brushes.CadetBlue, null, new Rect(0, 0, 100, 100));
                foreach (var item in canvas.Items)
                {
                    var container = (FrameworkElement)canvas.ItemContainerGenerator.ContainerFromItem(item);
                    var needDestryContainer = false;
                    var alreadyGenerated = container != null;
                    if (!alreadyGenerated)
                    {
                        container = (FrameworkElement)canvas.ItemContainerGenerator.CreateContainer(item);
                        needDestryContainer = true;
                    }
                    var itemImage = new RenderTargetBitmap((int) (canvas.ExtentWidth*dpiX/WpfDpi),
                        (int) (container.ActualHeight*dpiX/WpfDpi), dpiX, dpiY,
                        PixelFormats.Pbgra32);
                    itemImage.Render(container);
                    dc.DrawImage(itemImage, item.Bounds);
                    if (needDestryContainer) canvas.ItemContainerGenerator.Recycle(container);
                }
            }
            var image = new RenderTargetBitmap((int) (canvas.ExtentWidth*dpiX/WpfDpi),
                (int) (canvas.ExtentHeight*dpiX/WpfDpi), dpiX, dpiY,
                PixelFormats.Pbgra32);
            image.Render(drawing);
            return image;
            */
            /*
            var image = new WriteableBitmap((int) canvas.ExtentWidth, (int)canvas.ExtentHeight, dpiX, dpiY, PixelFormats.Bgr32, BitmapPalette.);
            foreach (var item in canvas.Items)
            {
                var container = (FrameworkElement) canvas.ItemContainerGenerator.ContainerFromItem(item);
                var needDestryContainer = false;
                var alreadyGenerated = container != null;
                if (!alreadyGenerated)
                {
                    container = (FrameworkElement) canvas.ItemContainerGenerator.CreateContainer(item);
                    needDestryContainer = true;
                }
                var itemImage = new RenderTargetBitmap((int) container.ActualWidth, (int) container.ActualHeight, dpiX, dpiY,
                    PixelFormats.Bgr32);
                itemImage.Render(container);
                if (needDestryContainer) canvas.ItemContainerGenerator.Recycle(container);

            }
            */
        }

        public static BitmapEncoder EncoderFromFileName(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                throw new ArgumentException("Argument is null or empty", nameof(fileName));
            var ext = Path.GetExtension(fileName);
            switch (ext.ToLowerInvariant())
            {
                case ".bmp":
                    return new BmpBitmapEncoder();
                case ".jpg":
                case ".jpeg":
                    return new JpegBitmapEncoder();
                case ".png":
                    return new PngBitmapEncoder();
                case ".tif":
                case ".tiff":
                    return new TiffBitmapEncoder();
            }
            return null;
        }

        public static void ExportImage(DesignerCanvas canvas, Stream s, BitmapEncoder encoder, double dpiX, double dpiY)
        {
            if (canvas == null) throw new ArgumentNullException(nameof(canvas));
            if (s == null) throw new ArgumentNullException(nameof(s));
            if (encoder == null) throw new ArgumentNullException(nameof(encoder));
            var image = CreateImage(canvas, dpiX, dpiY);
            encoder.Frames.Add(BitmapFrame.Create(image));
            encoder.Save(s);
        }

        public static void ExportImage(DesignerCanvas canvas, string fileName, double dpiX, double dpiY)
        {
            if (canvas == null) throw new ArgumentNullException(nameof(canvas));
            if (string.IsNullOrEmpty(fileName))
                throw new ArgumentException("Argument is null or empty", nameof(fileName));
            var encoder = EncoderFromFileName(fileName);
            if (encoder == null) throw new NotSupportedException("Extension of specified fileName is not supported.");
            using (var fs = File.OpenWrite(fileName))
            {
                ExportImage(canvas, fs, encoder, dpiX, dpiY);
            }
        }

        public static void ExportImage(BitmapSource source, string fileName)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (string.IsNullOrEmpty(fileName))
                throw new ArgumentException("Argument is null or empty", nameof(fileName));
            var encoder = EncoderFromFileName(fileName);
            if (encoder == null) throw new NotSupportedException("Extension of specified fileName is not supported.");
            encoder.Frames.Add(BitmapFrame.Create(source));
            using (var fs = File.OpenWrite(fileName))
            {
                encoder.Save(fs);
            }
        }
    }
}
