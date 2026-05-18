using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using coursework.Models;
using coursework.Core;

namespace coursework.Services
{
    public class HeatmapRenderer
    {
        public int Width { get; }
        public int Height { get; }
        private const int HotspotRadius = 40;
        private readonly float[] _kernel;
        private readonly int _kernelSize;
        private readonly Color[] _colormap;

        public HeatmapRenderer(int width, int height)
        {
            Width = width;
            Height = height;

            _kernelSize = HotspotRadius * 2 + 1;
            _kernel = BuildGaussianKernel(HotspotRadius);

            _colormap = BuildColormap();
        }

        public WriteableBitmap Render(
            IEnumerable<Visitor> visitors,
            IEnumerable<BaseZone> zones)
        {
            float[] intensityBuffer = new float[Width * Height];
            float maxIntensity = 0f;

            foreach (var visitor in visitors)
            {
                int cx = (int)visitor.X;
                int cy = (int)visitor.Y;

                for (int ky = -HotspotRadius; ky <= HotspotRadius; ky++)
                {
                    int py = cy + ky;
                    if (py < 0 || py >= Height) continue;

                    for (int kx = -HotspotRadius; kx <= HotspotRadius; kx++)
                    {
                        int px = cx + kx;
                        if (px < 0 || px >= Width) continue;

                        int kernelIdx = (ky + HotspotRadius) * _kernelSize + (kx + HotspotRadius);
                        float val = _kernel[kernelIdx];

                        int bufIdx = py * Width + px;
                        intensityBuffer[bufIdx] += val;

                        if (intensityBuffer[bufIdx] > maxIntensity)
                            maxIntensity = intensityBuffer[bufIdx];
                    }
                }
            }

            var bitmap = new WriteableBitmap(Width, Height, 96, 96, PixelFormats.Bgra32, null);
            bitmap.Lock();

            unsafe
            {
                byte* buffer = (byte*)bitmap.BackBuffer;
                float norm = maxIntensity > 0 ? 1f / maxIntensity : 0f;

                for (int i = 0; i < Width * Height; i++)
                {
                    float normalized = intensityBuffer[i] * norm;
                    normalized = (float)Math.Pow(normalized, 0.6);

                    int colorIdx = (int)(normalized * 255);
                    colorIdx = Math.Max(0, Math.Min(255, colorIdx));

                    Color c = _colormap[colorIdx];
                    int pixelOffset = i * 4;
                    buffer[pixelOffset + 0] = c.B;
                    buffer[pixelOffset + 1] = c.G;
                    buffer[pixelOffset + 2] = c.R;
                    buffer[pixelOffset + 3] = c.A;
                }
            }

            bitmap.AddDirtyRect(new Int32Rect(0, 0, Width, Height));
            bitmap.Unlock();
            DrawShopMarkers(bitmap, zones);

            return bitmap;
        }


        private void DrawShopMarkers(WriteableBitmap bitmap, IEnumerable<BaseZone> zones)
        {
            bitmap.Lock();
            unsafe
            {
                byte* buffer = (byte*)bitmap.BackBuffer;
                int stride = bitmap.BackBufferStride;

                foreach (var zone in zones)
                {
                    foreach (var shop in zone.Shops)
                    {
                        int cx = (int)shop.X;
                        int cy = (int)shop.Y;
                        const int markerRadius = 10;

                        for (int dy = -markerRadius; dy <= markerRadius; dy++)
                        {
                            for (int dx = -markerRadius; dx <= markerRadius; dx++)
                            {
                                int dist2 = dx * dx + dy * dy;
                                if (dist2 > markerRadius * markerRadius) continue;

                                int px = cx + dx;
                                int py = cy + dy;
                                if (px < 0 || px >= Width || py < 0 || py >= Height) continue;

                                byte* pixel = buffer + py * stride + px * 4;

                                bool isEdge = dist2 > (markerRadius - 2) * (markerRadius - 2);

                                if (isEdge)
                                {
                                    pixel[0] = 20;
                                    pixel[1] = 20;
                                    pixel[2] = 20;
                                    pixel[3] = 255;
                                }
                                else
                                {
                                    pixel[0] = 255;
                                    pixel[1] = 255;
                                    pixel[2] = 255;
                                    pixel[3] = 230;
                                }
                            }
                        }
                    }
                }
            }

            bitmap.AddDirtyRect(new Int32Rect(0, 0, Width, Height));
            bitmap.Unlock();
        }

        private static float[] BuildGaussianKernel(int radius)
        {
            int size = radius * 2 + 1;
            float[] kernel = new float[size * size];
            double sigma = radius / 2.5;
            double twoSigmaSq = 2.0 * sigma * sigma;
            float sum = 0f;

            for (int y = -radius; y <= radius; y++)
            {
                for (int x = -radius; x <= radius; x++)
                {
                    float val = (float)Math.Exp(-(x * x + y * y) / twoSigmaSq);
                    kernel[(y + radius) * size + (x + radius)] = val;
                    sum += val;
                }
            }

            float maxVal = kernel[radius * size + radius];
            for (int i = 0; i < kernel.Length; i++)
                kernel[i] /= maxVal;

            return kernel;
        }

        private static Color[] BuildColormap()
        {
            var map = new Color[256];

            for (int i = 0; i < 256; i++)
            {
                double t = i / 255.0;

                byte r, g, b, a;

                if (t < 0.05)
                {
                    r = 0; g = 0; b = 128;
                    a = (byte)(t / 0.05 * 60);
                }
                else if (t < 0.35)
                {
                    double s = (t - 0.05) / 0.30;
                    r = 0;
                    g = (byte)(s * 200);
                    b = (byte)(128 + s * 127);
                    a = (byte)(60 + s * 100);
                }
                else if (t < 0.65)
                {
                    double s = (t - 0.35) / 0.30;
                    r = (byte)(s * 255);
                    g = (byte)(200 + s * 55);
                    b = (byte)(255 * (1 - s));
                    a = (byte)(160 + s * 60);
                }
                else
                {
                    double s = (t - 0.65) / 0.35;
                    r = 255;
                    g = (byte)(255 * (1 - s));
                    b = 0;
                    a = (byte)(220 + s * 35);
                }

                map[i] = Color.FromArgb(a, r, g, b);
            }

            return map;
        }
    }
}