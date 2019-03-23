using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace ImageReadCS
{
    [StructLayout(LayoutKind.Sequential, Pack=1)]
    public struct ColorFloatPixel : IComparable
    {
        public float b, g, r, a;

        public int CompareTo (object o)
        {
            ColorFloatPixel p = (ColorFloatPixel)o;
            float sum1 = this.b * this.b + this.g * this.g + this.r * this.r;
            float sum2 = p.b * p.b + p.g * p.g + p.r * p.r;
            float dif = sum1 - sum2;
            return dif >= 0 ? ( dif > 0 ? -1 : 0 ) : 1;
        }

        public static ColorFloatPixel operator *(float multiplier, ColorFloatPixel pix)
        {
            ColorFloatPixel out_p;
            out_p.b = pix.b * multiplier;
            out_p.g = pix.g * multiplier;
            out_p.r = pix.r * multiplier;
            out_p.a = pix.a /* multiplier*/;
            return out_p;
        }

        public static ColorFloatPixel operator *(ColorFloatPixel pix, float multiplier)
        {
            ColorFloatPixel out_p;
            out_p.b = pix.b * multiplier;
            out_p.g = pix.g * multiplier;
            out_p.r = pix.r * multiplier;
            out_p.a = pix.a /* multiplier*/;
            return out_p;
        }

        public static ColorFloatPixel operator *(ColorFloatPixel pix1, ColorFloatPixel pix2)
        {
            ColorFloatPixel out_p;
            out_p.b = pix1.b * pix2.b;
            out_p.g = pix1.g * pix2.g;
            out_p.r = pix1.r * pix2.r;
            out_p.a = pix1.a /* multiplier*/;
            return out_p;
        }

        public static ColorFloatPixel operator /(ColorFloatPixel pix, float multiplier)
        {
            ColorFloatPixel out_p;
            out_p.b = pix.b / multiplier;
            out_p.g = pix.g / multiplier;
            out_p.r = pix.r / multiplier;
            out_p.a = pix.a /* multiplier*/;
            return out_p;
        }

        public static ColorFloatPixel operator +(ColorFloatPixel pix, float added)
        {
            ColorFloatPixel out_p;
            out_p.b = pix.b + added;
            out_p.g = pix.g + added;
            out_p.r = pix.r + added;
            out_p.a = pix.a /*+ added*/;
            return out_p;
        }

        public static ColorFloatPixel operator +(ColorFloatPixel pix1, ColorFloatPixel pix2)
        {
            ColorFloatPixel out_p;
            out_p.b = pix1.b + pix2.b;
            out_p.g = pix1.g + pix2.g;
            out_p.r = pix1.r + pix2.r;
            out_p.a = pix1.a + pix2.a;
            return out_p;
        }
    }

    public class ColorFloatImage
    {
        public int Width { get; private set; }
        public int Height { get; private set; }
        public readonly ColorFloatPixel[] rawdata;

        public ColorFloatImage(int Width, int Height)
        {
            this.Width = Width;
            this.Height = Height;
            rawdata = new ColorFloatPixel[Width * Height];
        }

        public ColorFloatPixel this[int x, int y]
        {
            get
            {
#if DEBUG
                if (x < 0 || x >= Width || y < 0 || y >= Height)
                    throw new IndexOutOfRangeException(string.Format("Trying to access pixel ({0}, {1}) in {2}x{3} image", x, y, Width, Height));
#endif
                return rawdata[y * Width + x];
            }
            set
            {
#if DEBUG
                if (x < 0 || x >= Width || y < 0 || y >= Height)
                    throw new IndexOutOfRangeException(string.Format("Trying to access pixel ({0}, {1}) in {2}x{3} image", x, y, Width, Height));
#endif
                rawdata[y * Width + x] = value;
            }
        }

        public GrayscaleFloatImage ToGrayscaleFloatImage()
        {
            //res[i, j] = 0.114f * b + 0.587f * g + 0.299f * r
            GrayscaleFloatImage res = new GrayscaleFloatImage(Width, Height);
            for (int i = 0; i < res.rawdata.Length; i++)
                res.rawdata[i] = 0.114f * rawdata[i].b + 0.587f * rawdata[i].g + 0.299f * rawdata[i].r;
            return res;
        }
    }
}
