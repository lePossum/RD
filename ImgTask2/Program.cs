using System;
using System.Numerics;
using ImageReadCS;
using ImgTask2;

namespace RidgeDetection
{
    class Program
    {
        static ColorFloatImage img_expansion(ColorFloatImage inp_img, String mode, int radius)
        {
            ColorFloatImage out_img = new ColorFloatImage(inp_img.Width + 2 * radius, inp_img.Height + 2 * radius);
            for (int y = radius; y < out_img.Height - radius; y++) //centre part pf image
                for (int x = 0; x < out_img.Width; x++)
                    if (mode == "rep")//replicate
                        if (x < radius)
                            out_img[x, y] = inp_img[0, y - radius];
                        else if (x >= radius + inp_img.Width)
                            out_img[x, y] = inp_img[inp_img.Width - 1, y - radius];
                        else
                            out_img[x, y] = inp_img[x - radius, y - radius];
                    else if (mode == "odd")//odd
                        if (x < radius)
                            out_img[x, y] = 2 * inp_img[0, y - radius] + (-1) * inp_img[radius - x - 1, y - radius];
                        else if (x >= radius + inp_img.Width)
                            out_img[x, y] = 2 * inp_img[inp_img.Width - 1, y - radius] + (-1) * inp_img[radius + 2 * inp_img.Width - x - 1, y - radius];
                        else
                            out_img[x, y] = inp_img[x - radius, y - radius];
                    else if (mode == "even")//even
                        if (x < radius)
                            out_img[x, y] = inp_img[radius - x - 1, y - radius];
                        else if (x >= radius + inp_img.Width)
                            out_img[x, y] = inp_img[radius + 2 * inp_img.Width - x - 1, y - radius];
                        else
                            out_img[x, y] = inp_img[x - radius, y - radius];
            for (int y = 0; y < radius; y++) //upper part of image
                for (int x = 0; x < out_img.Width; x++)
                    if (mode == "rep")//replicate
                        out_img[x, y] = out_img[x, radius];
                    else if (mode == "odd") // odd
                        out_img[x, y] = 2 * out_img[x, radius] + (-1) * out_img[x, 2 * radius - y - 1];
                    else if (mode == "even") // even
                        out_img[x, y] = out_img[x, 2 * radius - y - 1];
            for (int y = inp_img.Height + radius; y < out_img.Height; y++) //lower part of image
                for (int x = 0; x < out_img.Width; x++)
                    if (mode == "rep")//replicate
                        out_img[x, y] = out_img[x, out_img.Height - radius - 1];
                    else if (mode == "odd")//odd
                        out_img[x, y] = 2 * out_img[x, out_img.Height - radius - 1] + (-1) * out_img[x, 2 * (out_img.Height - radius) - y - 1];
                    else if (mode == "even")//even
                        out_img[x, y] = out_img[x, 2 * (out_img.Height - radius) - y - 1];
            return out_img;
        }

        static ColorFloatImage gradient(ColorFloatImage image, String mode)
        {
            if (mode != "rep" && mode != "odd" && mode != "even")
            {
                Console.WriteLine("Wrong edge mode");
                return image;
            }

            ColorFloatImage temp_image = img_expansion(image, mode, 1);

            ColorFloatImage temp_image_x = new ColorFloatImage(image.Width, image.Height);
            ColorFloatImage temp_image_y = new ColorFloatImage(image.Width, image.Height);

            for (int y = 0; y < temp_image_x.Height; y++)
                for (int x = 0; x < temp_image_x.Width; x++)
                    temp_image_x[x, y] = temp_image[x + 1, y] + (-1) * temp_image[x, y];
            for (int y = 0; y < temp_image_y.Height; y++)
                for (int x = 0; x < temp_image_y.Width; x++)
                    temp_image_y[x, y] = temp_image[x, y + 1] + (-1) * temp_image[x, y];

            ColorFloatImage out_img = new ColorFloatImage(image.Width, image.Height);

            float max_color_r = 0;
            float max_color_g = 0;
            float max_color_b = 0;

            for (int y = 0; y < image.Height; y++)
                for (int x = 0; x < image.Width; x++)
                {
                    double r_x = temp_image_x[x, y].r, r_y = temp_image_y[x, y].r;
                    double g_x = temp_image_x[x, y].g, g_y = temp_image_y[x, y].g;
                    double b_x = temp_image_x[x, y].b, b_y = temp_image_y[x, y].b;
                    ColorFloatPixel temp_pixel;
                    temp_pixel.r = (float)(Math.Sqrt(r_x * r_x + r_y * r_y));
                    if (temp_pixel.r > max_color_r) max_color_r = temp_pixel.r;
                    temp_pixel.g = (float)(Math.Sqrt(g_x * g_x + g_y * g_y));
                    if (temp_pixel.g > max_color_g) max_color_g = temp_pixel.g;
                    temp_pixel.b = (float)(Math.Sqrt(b_x * b_x + b_y * b_y));
                    if (temp_pixel.r > max_color_b) max_color_b = temp_pixel.b;
                    temp_pixel.a = 0;
                    out_img[x, y] = temp_pixel;
                }
            //contrast increasing block
            double multiplier_r = 255 / max_color_r;
            double multiplier_g = 255 / max_color_g;
            double multiplier_b = 255 / max_color_b;
            float mul = 1;
            if ((multiplier_r <= multiplier_g) && (multiplier_r <= multiplier_b))
                mul = (float)multiplier_r;
            else if (multiplier_g <= multiplier_r && multiplier_g <= multiplier_b)
                mul = (float)multiplier_g;
            else if (multiplier_b <= multiplier_r && multiplier_b <= multiplier_g)
                mul = (float)multiplier_b;
            for (int y = 0; y < image.Height; y++)
                for (int x = 0; x < image.Width; x++)
                    out_img[x, y] = out_img[x, y] * mul;

            return out_img;
        }

        static float[,] g_kernel_deriv(float sigma, string axis = "a")
        {
            int rad = (int)Math.Round(3 * sigma);
            int kernelsize = 2 * rad + 1;
            float[,] kernel = new float[kernelsize, kernelsize];

            int x_axis = 1;
            int y_axis = 1;

            if (axis == "x")
            {
                x_axis = 1;
                y_axis = 0;
            }

            else if (axis == "y")
            {
                x_axis = 0;
                y_axis = 1;
            }

            float sum = 0.0f;

            for (int x = -rad; x < rad + 1; x++)
                for (int y = -rad; y < rad + 1; y++)
                {
                    kernel[x + rad, y + rad] = (float)((x * x_axis + y * y_axis) *
                        Math.Exp(-(x * x + y * y) / (2.0f * sigma * sigma)));
                    sum += kernel[x + rad, y + rad];
                }
            /*
            for (int x = -rad; x < rad + 1; x++)
                for (int y = -rad; y < rad + 1; y++)
                    kernel[x + rad, y + rad] /= sum;
            */
            return kernel;
        }

        static GrayscaleFloatImage dir(ColorFloatImage image, float sigma)
        {
            string mode = "even";
            int rad = (int)Math.Round(3 * sigma);
            ColorFloatImage temp_img = img_expansion(image, mode, rad);

            /*
            for (int y = 1; y < temp_image_x.Height + 1; y++)
                for (int x = 1; x < temp_image_x.Width + 1; x++)
                    temp_image_x[x - 1, y - 1] = temp_image[x + 1, y] + (-1) * temp_image[x, y];
            for (int y = 1; y < temp_image_y.Height + 1; y++)
                for (int x = 1; x < temp_image_y.Width + 1; x++)
                    temp_image_y[x - 1, y - 1] = temp_image[x, y + 1] + (-1) * temp_image[x, y];
            */
            GrayscaleFloatImage temp_image = temp_img.ToGrayscaleFloatImage();
            GrayscaleFloatImage out_img = new GrayscaleFloatImage(image.Width, image.Height);

            float[,] Gx = g_kernel_deriv(sigma, "x");
            float[,] Gy = g_kernel_deriv(sigma, "y");

            for (int y = rad; y < image.Height + rad; y++)
                for (int x = rad; x < image.Width + rad; x++)
                {
                    float val_x = 0, val_y = 0;

                    int m = 0;
                    for (int j = y - rad; j < y + rad + 1; j++, m++)
                    {
                        int n = 0;
                        for (int k = x - rad; k < x + rad + 1; k++, n++)
                        {
                            val_x = val_x + Gx[n, m] * temp_image[k, j];
                            val_y = val_y + Gy[n, m] * temp_image[k, j];
                        }
                    }

                    if (val_x == 0 && val_y != 0)
                    {
                        out_img[x - rad, y - rad] = 64;
                        continue;
                    }
                    double theta = Math.Atan2(val_y - rad, val_x - rad) * (180 / Math.PI);
                    if (theta <= 22.5 && theta > -22.5 || theta <= -157.5 && theta > 157.5)
                        out_img[x - rad, y - rad] = 64; // ->   
                    else if (theta <= 67.5 && theta > 22.5 || theta >= -157.5 && theta < -112.5)
                        out_img[x - rad, y - rad] = 192; // /
                    else if (theta > 67.5 && theta <= 112.5 || theta >= -112.5 && theta < -67.5)
                        out_img[x - rad, y - rad] = 128; // ^
                    else if (theta > 112.5 && theta <= 157.5 || theta >= -67.5 && theta < -22.5)
                        out_img[x - rad, y - rad] = 255; // \
                }
            return out_img;
        }

        static GrayscaleFloatImage nonmax(ColorFloatImage image, float sigma)
        {
            int offset = 2;
            string mode = "even";

            ColorFloatImage temp_image = img_expansion(image, mode, offset);
            temp_image = gradient(image, mode);
            temp_image = img_expansion(temp_image, mode, offset);

            GrayscaleFloatImage gray_grad = temp_image.ToGrayscaleFloatImage();
            GrayscaleFloatImage dir_img = dir(image, sigma);
            GrayscaleFloatImage out_img = new GrayscaleFloatImage(image.Width, image.Height);
            float max_value = 0;

            for (int y = 0; y < out_img.Height; y++)
                for (int x = 0; x < out_img.Width; x++)
                {
                    float M = gray_grad[x + offset, y + offset];
                    if (M > max_value)
                        max_value = M;
                    switch (dir_img[x, y])
                    {
                        case 0: // o
                                //break;
                        case 64: // ->
                            if (M < gray_grad[x + offset + 1, y + offset] ||
                                M < gray_grad[x + offset - 1, y + offset])
                                out_img[x, y] = 0;
                            else
                                out_img[x, y] = M;
                            break;
                        case 128: // ^
                            if (M < gray_grad[x + offset, y + offset + 1] ||
                                M < gray_grad[x + offset, y + offset - 1])
                                out_img[x, y] = 0;
                            else
                                out_img[x, y] = M;
                            break;
                        case 192: // /
                            if (M < gray_grad[x + offset + 1, y + offset + 1] ||
                                M < gray_grad[x + offset - 1, y + offset - 1])
                                out_img[x, y] = 0;
                            else
                                out_img[x, y] = M;
                            break;
                        case 255: // \
                            if (M < gray_grad[x + offset - 1, y + offset + 1] ||
                                M < gray_grad[x + offset + 1, y + offset - 1])
                                out_img[x, y] = 0;
                            else
                                out_img[x, y] = M;
                            break;
                    }
                }
            float mult = 255 / max_value;
            for (int y = 0; y < out_img.Height; y++)
                for (int x = 0; x < out_img.Width; x++)
                    out_img[x, y] *= mult;
            return out_img;
        }

        static float[,] g_kernel(float sigma, string axis)
        {
            int rad = (int)Math.Round(3 * sigma);
            int kernelsize = 2 * rad + 1;
            float[,] kernel = new float[kernelsize, kernelsize];
            int xx = 0, yy = 0, xy = 0;
            if (axis == "xx")
                xx = 1;
            else if (axis == "yy")
                yy = 1;
            else if (axis == "xy")
                xy = 1;

            float sum = 0.0f;
            double denominator = 2 * Math.PI * Math.Pow(sigma, 6);
            for (int x = -rad; x < rad + 1; x++)
                for (int y = -rad; y < rad + 1; y++)
                {
                    kernel[x + rad, y + rad] = (float)
                        ((xx * (x * x ) + //- sigma * sigma) +
                          xy * x * y +
                          yy * (y * y )) * //- sigma * sigma)) * 
                        Math.Exp(-(x * x + y * y) / (2.0f * sigma * sigma))/denominator);
                    sum += kernel[x + rad, y + rad];
                }
            /*
            for (int x = -rad; x < rad + 1; x++)
                for (int y = -rad; y < rad + 1; y++)
                    kernel[x + rad, y + rad] /= sum;
                    */
            return kernel;
        }

        static float[,][,] matrix_der2(GrayscaleFloatImage image, float sigma)
        {
            float[,][,] out_matr = new float[image.Width, image.Height][,];
            float[,] Gxx = g_kernel(sigma, "xx");
            float[,] Gxy = g_kernel(sigma, "xy");
            float[,] Gyy = g_kernel(sigma, "yy");

            int rad = (int)Math.Round(3 * sigma);
            GrayscaleFloatImage temp_image = img_expansion(image.ToColorFloatImage(), "odd", rad).ToGrayscaleFloatImage();
            for (int y = rad; y < image.Height + rad; ++y)
                for (int x = rad; x < image.Width + rad; ++x)
                {
                    float[,] q = new float[2,2];
                    for (int m = -rad; m <= rad; ++m)
                        for (int n = -rad; n <= rad; ++n)
                        {
                            q[0, 0] += temp_image[x + n, y + m] * Gxx[rad + n, rad + m];
                            q[0, 1] += temp_image[x + n, y + m] * Gxy[rad + n, rad + m];
                            q[1, 1] += temp_image[x + n, y + m] * Gyy[rad + n, rad + m];
                        }
                    q[1, 0] = q[0, 1];
                    out_matr[x - rad, y - rad] = q;
                }
            return out_matr;
        }

        static float[,] _solve_matr(float[,][,] matr)
        {
            int h = matr.GetUpperBound(1) + 1;
            int w = matr.GetUpperBound(0) + 1;
            float[,] solved_matr = new float[w, h];

            for (int y = 0; y < h; ++y)
                for (int x = 0; x < w; ++x)
                    solved_matr[x, y] = _solve_equation(matr[x, y]);

            return solved_matr;
        }

        static float _solve_equation(float[,] cm) // cm == current_matrix
        {
            double D = Math.Pow(cm[0, 0], 2) - 2 * cm[0, 0] * cm[1, 1] +
                      Math.Pow(cm[1, 1], 2) + 4 * cm[0, 1] * cm[1, 0];
            Complex c = Complex.Sqrt(D);
            double l_h = (cm[0, 0] + cm[1, 1]) / 2;
            Complex lambda1 = l_h + c;
            Complex lambda2 = l_h + new Complex(-c.Real, c.Imaginary);
            double big_one = Math.Max(lambda1.Magnitude, lambda2.Magnitude);
            double lil_one = Math.Min(lambda1.Magnitude, lambda2.Magnitude);
            float solvation = (float)(big_one / lil_one);

            float decision_param = 2;
            if (solvation > decision_param) // decision function 
                return (float)big_one;
            else
                return 0;
        }

        static GrayscaleFloatImage RD(float[,] matr)
        {
            int h = matr.GetUpperBound(1) + 1;
            int w = matr.GetUpperBound(0) + 1;
            GrayscaleFloatImage tmp_img = new GrayscaleFloatImage(w, h);
            float max_one = 0, min_one = 255;
            for (int y = 0; y < h; ++y)
                for (int x = 0; x < w; ++x)
                {
                    max_one = Math.Max(max_one, matr[x, y]);
                    min_one = Math.Min(min_one, matr[x, y]);
                }

            for (int y = 0; y < h; ++y)
                for (int x = 0; x < w; ++x)
                    /*if (matr[x, y] >= 2)
                        tmp_img[x, y] = 255;
                    else
                        tmp_img[x, y] = 0;
                    */
                    tmp_img[x, y] = (matr[x, y] - min_one) * 255 / (max_one - min_one);
            return tmp_img;
        }


        static GrayscaleFloatImage ExtendedRD(GrayscaleFloatImage img, int steps, int start = 1)
        {
            int[,] ridges_amount = new int[img.Width, img.Height];
            double sigma = Math.Pow(Math.Sqrt(2), start - 1);
            GrayscaleFloatImage[] image_array = new GrayscaleFloatImage[steps];
            GrayscaleFloatImage[] new_image_array = new GrayscaleFloatImage[steps];
            int rad = 0;
            for (int i = 0; i < steps; ++i, sigma *= Math.Sqrt(2))
            {
                var t = matrix_der2(img, (float)sigma);
                var matr = _solve_matr(t);
                image_array[i] = RD(matr);
                new_image_array[i] = nonmax(image_array[i].ToColorFloatImage(), (float)sigma);
                ImageIO.ImageToFile(new_image_array[i], "a_"+i.ToString()+".bmp");
                new_image_array[i] = img_expansion(new_image_array[i].ToColorFloatImage(), "odd", rad).ToGrayscaleFloatImage();
            }

            int MAGIC_NUMBER = 3;
            for (int i = 0; i < steps; ++i)
                for (int y = 0; y < img.Height; ++y)
                   for (int x = 0; x < img.Width; ++x)
                   {
                        if (image_array[i][x, y] > 0)
                            ridges_amount[x, y]++;
                   }

            GrayscaleFloatImage out_img = new GrayscaleFloatImage(img.Width, img.Height);
            for (int y = 0; y < img.Height; ++y)
                for (int x = 0; x < img.Width; ++x)
                {
                    if (ridges_amount[x,y] >= MAGIC_NUMBER)
                    {
                        for (int i = steps - MAGIC_NUMBER - 1; i >= 0; --i)
                        {
                            bool f = true;

                            for (int j = 0; j < MAGIC_NUMBER; ++j)
                            {
                                bool an_f = false;
                                for (int a = -rad; a <= rad; ++a)
                                    for (int b = - rad; b <= rad; ++b)
                                    {
                                        if (new_image_array[i][x + rad + a, y + rad + b] > 0 && !an_f)
                                        {
                                            if (out_img[x, y] == 0)
                                            out_img[x, y] = new_image_array[i + j][x + rad + a, y + rad + b];
                                            an_f = true;
                                        }

                                    }
                                if (!an_f)
                                {
                                    f = false;
                                    out_img[x, y] = 0;
                                }
                            }
                            if (f)
                            {
                                //out_img[x, y] /= MAGIC_NUMBER;
                                break;
                            }
                        }
                    }

                }
            return out_img;
        }

        static GrayscaleFloatImage temp_func (GrayscaleFloatImage image, float sigma)
        {
            GrayscaleFloatImage out_matr = new GrayscaleFloatImage(image.Width, image.Height);
            float[,] Gxx = g_kernel(sigma, "yy");
            //float[,] Gxy = g_kernel(sigma, "xy");
            //float[,] Gyy = g_kernel(sigma, "yy");

            int rad = (int)Math.Round(3 * sigma);
            GrayscaleFloatImage temp_image = img_expansion(image.ToColorFloatImage(), "odd", rad).ToGrayscaleFloatImage();
            float common_sum = 0;
            for (int y = rad; y < image.Height + rad; ++y)
                for (int x = rad; x < image.Width + rad; ++x)
                {
                    float[,] q = new float[2, 2];
                    for (int m = -rad; m <= rad; ++m)
                        for (int n = -rad; n <= rad; ++n)
                        {
                            out_matr[x-rad, y - rad] += temp_image[x + n, y + m] * Gxx[rad + n, rad + m];
                            //               q[0, 1] += temp_image[x + n, y + m] * Gxy[rad + n, rad + m];
                            //             q[1, 1] += temp_image[x + n, y + m] * Gyy[rad + n, rad + m];
                        }
                    common_sum += out_matr[x - rad, y - rad];
                }
            Console.WriteLine(common_sum);
            float max_one = 0;
            float min_one = 0;
            for (int y = 0; y < image.Height; ++y)
                for (int x = 0; x < image.Width; ++x)
                {
                    //out_matr[x, y] += 128;
                    if (out_matr[x, y] > max_one)
                        max_one = out_matr[x, y];
                    if (out_matr[x, y] < min_one)
                        min_one = out_matr[x, y];

                }

            min_one = -min_one;
            //if (min_one > max_one)
              //  max_one = min_one;

            for (int y = 0; y < image.Height; ++y)
                for (int x = 0; x < image.Width; ++x)
                {
                    out_matr[x, y] *= 255 / max_one / 2;
                    out_matr[x, y] += 128;

                }

            return out_matr;
        }

        static void Main(string[] args)
        {
            GrayscaleFloatImage input_image = ImageIO.FileToGrayscaleFloatImage(args[0]);
            input_image = ExtendedRD(input_image, 4);
            //input_image = temp_func(input_image, 2);
            ImageIO.ImageToFile(input_image, args[2]);
        }
    }
}
