using Cudafy;
using Cudafy.Host;
using Cudafy.Translator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace MandelbrotSet
{
    public static class Calculations
    {
        private const int maxIter = 1000;
        public const int threadsPerBlock = 256;
        public const int blocksPerGrid = 32;
        private static GPGPU gpu;

        public static void Setup()
        {
            CudafyModes.Target = eGPUType.OpenCL;
            CudafyTranslator.Language = eLanguage.OpenCL;
            CudafyModule km = CudafyTranslator.Cudafy();

            gpu = CudafyHost.GetDevice(CudafyModes.Target, CudafyModes.DeviceId);
            gpu.LoadModule(km);
        }

        public static void SetImageCPU(Bitmap image, double zoom, int offX, int offY)
        {
            for (int i = 0; i < image.Width; i++)
            {
                for (int j = 0; j < image.Height; j++)
                {
                    int it = GetIteration(
                        TransformNumber(i + offX, 0, image.Width, -zoom, zoom),
                        TransformNumber(j + offY, 0, image.Height, -zoom, zoom));
                    byte rounded = (byte)((float)it / maxIter * 255);
                    image.SetPixel(i, j, Color.FromArgb(rounded, rounded, rounded));
                }
            }
        }

        public static void SetImage(Bitmap image, double zoom, int offX, int offY)
        {
            Rectangle area = new Rectangle(0, 0, image.Width, image.Height);
            BitmapData bitmapData = image.LockBits(area, ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            int stride = bitmapData.Stride;
            IntPtr ptr = bitmapData.Scan0;
            int numBytes = bitmapData.Stride * image.Height;
            byte[] rgbValues = new byte[numBytes];

            try
            {
                byte[] dev_rgb = gpu.Allocate(rgbValues);
                gpu.Launch(blocksPerGrid, threadsPerBlock).GetValues(dev_rgb, zoom, stride, image.Height, offX, offY);
                gpu.CopyFromDevice(dev_rgb, rgbValues);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                gpu.FreeAll();
            }

            Marshal.Copy(rgbValues, 0, ptr, bitmapData.Stride * image.Height);
            image.UnlockBits(bitmapData);
        }

        [Cudafy]
        static int GetIteration(double x0, double y0)
        {
            double x = 0, y = 0;
            int iter = 0;
            while (x * x + y * y <= 4 && iter < maxIter)
            {
                double xtemp = x * x - y * y + x0;
                y = 2 * x * y + y0;
                x = xtemp;
                iter++;
            }
            return iter;
        }

        [Cudafy]
        public static double TransformNumber(double x, int startX, int endX, double toStartX, double toEndX)
        {
            int d = endX - startX;
            return x / d * (toEndX - toStartX) + toStartX;
        }

        [Cudafy]
        public static void GetValues(GThread thread, byte[] rgbValues, double zoom, int stride, int height, int offX, int offY)
        {
            int threadID = thread.threadIdx.x + thread.blockIdx.x * thread.blockDim.x;
            int numThreads = thread.blockDim.x * thread.gridDim.x;
            if (threadID < rgbValues.Length)
            {
                for (int i = threadID; i < stride / 3 * height; i += numThreads)
                {
                    int x = i % (stride / 3);
                    int y = i / (stride / 3);

                    int iters = GetIteration(
                        TransformNumber(x + offX, 0, stride / 3, -zoom, zoom),
                        TransformNumber(y + offY, 0, height, -zoom, zoom));
                    byte rounded = (byte)((float)iters / maxIter * 255);

                    int rPos = y * stride + x * 3;

                    rgbValues[rPos] = rounded;
                    rgbValues[rPos + 1] = rounded;
                    rgbValues[rPos + 2] = rounded;
                }
            }
        }
    }
}
