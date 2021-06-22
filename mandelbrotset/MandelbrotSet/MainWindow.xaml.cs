using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Drawing;
using System.IO;
using System;
using System.Diagnostics;

namespace MandelbrotSet
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private double zoom = .5;
        private double dx = 300, dy = 0;

        public MainWindow()
        {
            InitializeComponent();
            Calculations.Setup();
        }

        BitmapImage BitmapToImageSource(Bitmap bitmap)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                memory.Position = 0;
                BitmapImage bitmapimage = new BitmapImage();
                bitmapimage.BeginInit();
                bitmapimage.StreamSource = memory;
                bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapimage.EndInit();

                return bitmapimage;
            }
        }

        Bitmap bm;

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            bm = new Bitmap((int)e.NewSize.Width, (int)e.NewSize.Height);
            //Stopwatch sw = new Stopwatch();
            //sw.Start();
            Calculations.SetImage(bm, zoom, (int)dx, (int)dy);
            //sw.Stop();
            //Console.WriteLine($"GPU time: {sw.ElapsedMilliseconds} ms");
            //sw.Restart();
            //Calculations.SetImageCPU(bm, zoom, (int)dx, (int)dy);
            //sw.Stop();
            //Console.WriteLine($"CPU time: {sw.ElapsedMilliseconds} ms");

            ImageViewer.Source = BitmapToImageSource(bm);
        }

        System.Windows.Point pressPos;
        private void ImageViewer_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if(e.LeftButton == MouseButtonState.Pressed)
            {
                pressPos = e.GetPosition(this);
            }
        }

        System.Windows.Point newPos;
        private void ImageViewer_MouseMove(object sender, MouseEventArgs e)
        {
            if(e.LeftButton == MouseButtonState.Pressed)
            {
                newPos = e.GetPosition(this);
                double tmpdx = pressPos.X - newPos.X;
                double tmpdy = pressPos.Y - newPos.Y;
                Calculations.SetImage(bm, zoom, (int)(dx + tmpdx), (int)(dy + tmpdy));
                ImageViewer.Source = BitmapToImageSource(bm);
            }
        }

        private void ImageViewer_IsMouseDirectlyOverChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue == false) // mouse left window
            {
                dx += pressPos.X - newPos.X;
                dy += pressPos.Y - newPos.Y;
            }
        }

        private void ImageViewer_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if(e.Delta < 0) // scroll back to zoom out
            {
                zoom *= 1.1;
                dx /= 1.1;
                dy /= 1.1;
            }
            else // scroll in to zoom in
            {
                zoom *= .9;
                dx /= .9;
                dy /= .9;
            }
            Console.WriteLine($"zoom: {zoom}; dx: {dx}; dy: {dy}");
            Calculations.SetImage(bm, zoom, (int)dx, (int)dy);
            ImageViewer.Source = BitmapToImageSource(bm);
        }

        private void ImageViewer_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if(e.LeftButton == MouseButtonState.Released)
            {
                System.Windows.Point newPos = e.GetPosition(this);
                dx += pressPos.X - newPos.X;
                dy += pressPos.Y - newPos.Y;
            }
        }
    }
}
