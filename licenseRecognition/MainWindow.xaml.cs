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
//using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

using Microsoft.Win32;
using System.Drawing; //需要在解决方案的引用中添加“System.Drawing”
using System.Drawing.Imaging;
using System.IO;

namespace licenseRecognition
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private Bitmap m_Bitmap;//存放最先打开的图片 // 需要在解决方案的引用中添加“System.Drawing”
        private Bitmap always_Bitmap;//永远是彩色图片，用以备用
        private Bitmap other_c_Bitmap;
        private Bitmap extract_Bitmap_one;
        private Bitmap extract_Bitmap_two;
        private Bitmap z_Bitmap0;
        private Bitmap z_Bitmap1;
        private Bitmap z_Bitmap2;
        private Bitmap z_Bitmap3;
        private Bitmap z_Bitmap4;
        private Bitmap z_Bitmap5;
        private Bitmap z_Bitmap6;
        //private Bitmap z_Bitmap7;
        private Bitmap objNewPic;
        private Bitmap c_Bitmap; //车牌图像

        private int cHeight;
        private int cWidth;

        private Bitmap[] z_Bitmaptwo = new Bitmap[7];//用于储存最终的黑白字体

        private Bitmap[] charFont;
        private Bitmap[] provinceFont;
        string[] charString;//存储的路径
        string[] provinceString;//省份字体
        string[] charDigitalString;
        string[] provinceDigitalString;
        System.Drawing.Pen pen1 = new System.Drawing.Pen(System.Drawing.Color.Black);
        private String name;  // pictureName;
        private float count;
        private double ncount;
        private float[] gl = new float[256];
        int[] gray = new int[256]; //灰度化
        double[] ngray = new double[256]; //改进的灰度化
        int[] rr = new int[256];
        int[] gg = new int[256];
        int[] bb = new int[256];
        float[,] m = new float[5000, 5000];
        int flag = 0, flag1 = 0;
        int xx = -1;
        //private bool aline = false;
        public static string SourceBathOne = "G:\\licensePlate\\";//备用
        public static string charSourceBath = "MYsource\\char\\";
        public static string provinceSourceBath = "MYsource\\font\\";

        public MainWindow()
        {
            InitializeComponent();
        }


        //打开图片
        private void btnOpen_Click(object sender, RoutedEventArgs e)
        {   //打开图片
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Jpeg文件(*.jpg)|*.jpg|Bitmap文件(*.bmp)|*.bmp|所有合适文件(*.bmp/*.jpg)|*.bmp;*.jpg";
            openFileDialog.FilterIndex = 2;
            //该值指示对话框在关闭前是否还原当前目录
            openFileDialog.RestoreDirectory = true;
            if (openFileDialog.ShowDialog() == true)
            {
                name = openFileDialog.FileName;
                m_Bitmap = (Bitmap)Bitmap.FromFile(name, false);
                this.always_Bitmap = m_Bitmap.Clone(new Rectangle(0, 0,
                    m_Bitmap.Width, m_Bitmap.Height), PixelFormat.DontCare);
                IntPtr ip = m_Bitmap.GetHbitmap(); //将Bitmap转换为BitmapSource
                BitmapSource bitmapSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(ip, IntPtr.Zero,
                    Int32Rect.Empty, System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
                imgLoad.Source = bitmapSource;
            }


        }

        //保存图片
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {//保存图片
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Jpeg文件(*.jpg)|*.jpg|Bitmap文件(*.bmp)|*.bmp|所有合适文件(*.bmp/*.jpg)|*.bmp;*.jpg";
            saveFileDialog.FilterIndex = 1;
            saveFileDialog.RestoreDirectory = true;
            if (saveFileDialog.ShowDialog() == true)
            {
                name = saveFileDialog.FileName;
                if (c_Bitmap == null)
                    c_Bitmap = m_Bitmap;
                c_Bitmap.Save(saveFileDialog.FileName);


            }
        }


        //图像灰度化
        private void btnPictureGray_Click(object sender, RoutedEventArgs e)
        { //灰度化
            if (m_Bitmap != null)
            {
                int tt = 0;
                for (int i = 0; i < 256; i++)
                {
                    gray[i] = 0;
                    rr[i] = 0;
                    gg[i] = 0;
                    bb[i] = 0;
                }
                BitmapData bmData = m_Bitmap.LockBits(new Rectangle(0, 0, m_Bitmap.Width, m_Bitmap.Height),
                    ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
                int stride = bmData.Stride; //获取或设置Bitmap对象的跨距宽度（也称为扫描宽度）
                System.IntPtr Scan0 = bmData.Scan0; //获取或设置第一个像素数据的地址
                unsafe //"生成"选择“允许不安全代码”
                {
                    byte* p = (byte*)(void*)Scan0;
                    //这里stride是图片一行数据的长度， 
                    //sitide - m_Bitmap.Width*3 是获取一行元素中多余的部分
                    //因为每个m_Bitmap.Width获取一行的像素，乘三是因为每个像素由rgb三个值组成。
                    //参考https://www.cnblogs.com/zkwarrior/p/5665216.html
                    int nOffset = stride - m_Bitmap.Width * 3;
                    byte red, green, blue;
                    int nWidth = m_Bitmap.Width;
                    int nHeight = m_Bitmap.Height;
                    for (int y = 0; y < nHeight; y++)
                    {
                        for (int x = 0; x < nWidth; x++)
                        {
                            blue = p[0];
                            green = p[1];
                            red = p[2];
                            //加权平均值法
                            tt = p[0] = p[1] = p[2] = (byte)(.299 * red + .587 * green + .114 * blue);

                            //最大值法
                            //byte max = red > green ? red : green;
                            //max = max > blue ? max : blue;
                            //tt = p[0] = p[1] = p[2] = max;

                            //平均值法
                            //tt = p[0] = p[1] = p[2] = (byte)((red + green + blue) / 3);

                            rr[red]++;
                            gg[green]++;
                            bb[blue]++;
                            gray[tt]++; //统计灰度值tt的像素点数目
                            p += 3;
                        }
                        //加上多余的部分，跳到下一行的开头
                        p += nOffset;
                    }
                }
                m_Bitmap.UnlockBits(bmData);
                count = m_Bitmap.Width * m_Bitmap.Height;
                IntPtr ip = m_Bitmap.GetHbitmap(); //将Bitmap转换为BitmapSource
                BitmapSource bitmapSource = System.Windows.Interop.Imaging
                    .CreateBitmapSourceFromHBitmap(ip, IntPtr.Zero, Int32Rect.Empty,
                    System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
                imgLoad.Source = bitmapSource;
            }
        }


        //直方图均衡化代码
        private void btnGrayScales_Click(object sender, RoutedEventArgs e)
        { //传统直方图均衡化 -- 灰度均衡化
            BitmapData bmData = m_Bitmap.LockBits(new Rectangle(0, 0, m_Bitmap.Width, m_Bitmap.Height),
                   ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            int stride = bmData.Stride;
            System.IntPtr Scan0 = bmData.Scan0;
            int tt = 0;
            int[] SumGray = new int[256];
            double[] nSumGray = new double[256];
            for (int i = 0; i < 256; i++)
            {
                SumGray[i] = 0;
                nSumGray[i] = 0;
                ngray[i] = 0;
              
            }

            unsafe
            {
                //这里要将ncount初始为0，否则第二次打开的时候颜色偏暗
                ncount = 0;
                byte* p = (byte*)(void*)Scan0;
                int nOffset = stride - m_Bitmap.Width * 3;
                int nHeight = m_Bitmap.Height;
                int nWidth = m_Bitmap.Width;
                SumGray[0] = gray[0];    //灰度均衡化
                nSumGray[0] = gray[0];


                //改进开始
                //参考链接https://wenku.baidu.com/view/0e644522dd36a32d737581da.html
                for (int i = 0; i < 256; i++)
                {
                    ngray[i] = Math.Log(gray[i]+1);
                    ncount += ngray[i];
                }
                for (int i = 1; i < 256; i++)
                {
                    nSumGray[i] = nSumGray[i - 1] + ngray[i];
                }
                for (int i = 0; i < 256; i++)
                {
                    nSumGray[i] = (int)(nSumGray[i] * 255 / (ncount - ngray[i]));
                }
                //改进结束

                for (int i = 1; i < 256; ++i)
                {
                    SumGray[i] = SumGray[i - 1] + gray[i];
                }
                for (int i = 0; i < 256; ++i)
                {
                    //计算调整灰度值
                    //频率乘以灰度总级数得出该灰度变换后的灰度级
                    SumGray[i] = (int)(SumGray[i] * 255 / count);

                }
                for (int i = 0; i < 256; i++)
                {
                    gray[i] = 0;
                }
                for (int i = 0; i < nHeight; ++i)
                {
                    for (int j = 0; j < nWidth; ++j)
                    {
                        //改进前的方法
                        //tt = p[0] = p[1] = p[2] = (byte)(SumGray[p[0]]);
                        //改进后的方法
                        tt = p[0] = p[1] = p[2] = (byte)(nSumGray[p[0]]);
                        gray[tt]++;
                        p += 3;
                    }
                    p += nOffset;
                }

            }
            m_Bitmap.UnlockBits(bmData);
            count = m_Bitmap.Width * m_Bitmap.Height;
            IntPtr ip = m_Bitmap.GetHbitmap(); //将Bitmap转换为BitmapSource
            BitmapSource bitmapSource = System.Windows.Interop.Imaging
                .CreateBitmapSourceFromHBitmap(ip, IntPtr.Zero, Int32Rect.Empty,
                System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
            imgLoad.Source = bitmapSource;

        }

        // 高斯平滑滤波滤波去噪
        private void btnMedianFilter_Click(object sender, RoutedEventArgs e)
        {
            BitmapData bmData = m_Bitmap.LockBits(new Rectangle(0, 0, m_Bitmap.Width, m_Bitmap.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            for (int i = 0; i < 256; i++)
            {
                gray[i] = 0;
            }
            unsafe
            {
                int stride = bmData.Stride;
                System.IntPtr Scan0 = bmData.Scan0;
                byte* p = (byte*)(void*)Scan0;
                byte* pp;
                int tt;
                int nOffset = stride - m_Bitmap.Width * 3;
                int nWidth = m_Bitmap.Width;
                int nHeight = m_Bitmap.Height;

                long sum = 0;
                int[,] gaussianMatrix = { { 1, 2, 3, 2, 1 }, { 2, 4, 6, 4, 2 }, { 3, 6, 7, 6, 3 }, { 2, 4, 6, 4, 2 }, { 1, 2, 3, 2, 1 } };//高斯滤波器所选的n=5模板
                for (int y = 0; y < nHeight; ++y)
                {
                    for (int x = 0; x < nWidth; ++x)
                    {

                        if (!(x <= 1 || x >= nWidth - 2 || y <= 1 || y >= nHeight - 2))
                        {
                            pp = p;
                            sum = 0;
                            int dividend = 79;
                            for (int i = -2; i <= 2; i++)
                                for (int j = -2; j <= 2; j++)
                                {
                                    pp += (j * 3 + stride * i);
                                    sum += pp[0] * gaussianMatrix[i + 2, j + 2];
                                    if (i == 0 && j == 0)
                                    {
                                        if (pp[0] > 240)//如果模板中心的灰度大于240
                                        {
                                            sum += p[0] * 30;
                                            dividend += 30;
                                        }
                                        else if (pp[0] > 230)
                                        {
                                            sum += pp[0] * 20;
                                            dividend += 20;
                                        }
                                        else if (pp[0] > 220)
                                        {
                                            sum += p[0] * 15;
                                            dividend += 15;
                                        }
                                        else if (pp[0] > 210)
                                        {
                                            sum += pp[0] * 10;
                                            dividend += 10;
                                        }
                                        else if (p[0] > 200)
                                        {
                                            sum += pp[0] * 5;
                                            dividend += 5;
                                        }
                                    }
                                    pp = p;
                                }
                            sum = sum / dividend;
                            if (sum > 255)
                            {
                                sum = 255;
                            }

                            //高斯滤波
                            p[0] = p[1] = p[2] = (byte)(sum);

                            //中值滤波
                            //3*3
                            //byte[] zzP = new byte[9];
                            //zzP[0] = p[-stride - 3];
                            //zzP[1] = p[-stride];
                            //zzP[2] = p[-stride + 3];
                            //zzP[3] = p[-3];
                            //zzP[4] = p[0];
                            //zzP[5] = p[3];
                            //zzP[6] = p[stride - 3];
                            //zzP[7] = p[stride];
                            //zzP[8] = p[stride + 3];
                            //Array.Sort(zzP);
                            //p[0] = p[1] = p[2] = zzP[4];


                            //均值滤波
                            //p[0] = p[1] = p[2] = (byte)((p[-stride] + p[-stride - 3] + p[-stride + 3]
                            //    + p[0] + p[3] + p[-3] +
                            //    p[stride] + p[stride - 3] + p[stride + 3]) / 9);
                        }
                        tt = p[0];
                        gray[tt]++;
                        p += 3;
                    }
                    p += nOffset;
                }
            }
            m_Bitmap.UnlockBits(bmData);
            IntPtr ip = m_Bitmap.GetHbitmap();//将Bitmap转换为BitmapSource
            BitmapSource bitmapSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(ip, IntPtr.Zero, Int32Rect.Empty, System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
            imgLoad.Source = bitmapSource;

        }


        //边缘检测
        private void btnEdgeDetection_Click(object sender, RoutedEventArgs e)
        { // sobel边缘检测
            Rectangle rect = new Rectangle(0, 0, m_Bitmap.Width, m_Bitmap.Height);
            System.Drawing.Imaging.BitmapData bmpData = m_Bitmap.LockBits(rect,
                System.Drawing.Imaging.ImageLockMode.ReadWrite, m_Bitmap.PixelFormat);
            IntPtr ptr = bmpData.Scan0;
            int bytes = bmpData.Stride * bmpData.Height;
            byte[] grayValues = new byte[bytes];
            System.Runtime.InteropServices.Marshal.Copy(ptr, grayValues, 0, bytes);
            byte[] tempArray = new byte[bytes];
            double gradX, gradY, grad;

            //sobel
            //reb中的r
            //for (int i = 1; i < bmpData.Height - 1; i++)
            //{
            //    for (int j = 3; j < (bmpData.Width - 1) * 3; j += 3)
            //    {
            //        //几种不同的算子，结果一样
            //        //是因为这段代码不起任何作用
            //        //真正起作用的是在改进梯度幅值计算中的算子
            //        //http://blog.csdn.net/guanyuqiu/article/details/52993412

            //        //Sobel边缘检测算法
            //        //gradX =
            //        //    -1 * grayValues[(i - 1) * bmpData.Stride + (j - 1)] +
            //        //    0 * grayValues[(i - 1) * bmpData.Stride + (j) + 2] +
            //        //    1 * grayValues[(i - 1) * bmpData.Stride + (j + 5)] +
            //        //    -2 * grayValues[(i) * bmpData.Stride + (j - 1)] +
            //        //    0 * grayValues[(i) * bmpData.Stride + (j) + 2] +
            //        //    2 * grayValues[(i) * bmpData.Stride + (j + 5)] +
            //        //    -1 * grayValues[(i + 1) * bmpData.Stride + (j - 1)] +
            //        //    0 * grayValues[(i + 1) * bmpData.Stride + (j) + 2] +
            //        //    1 * grayValues[(i + 1) * bmpData.Stride + (j + 5)];

            //        //gradY =
            //        //      1 * grayValues[(i - 1) * bmpData.Stride + (j - 1)]
            //        //    + 2 * grayValues[(i - 1) * bmpData.Stride + (j) + 2]
            //        //    + 1 * grayValues[(i - 1) * bmpData.Stride + (j + 5)]
            //        //    + 0 * grayValues[(i) * bmpData.Stride + (j - 1)]
            //        //    + 0 * grayValues[(i) * bmpData.Stride + (j) + 2]
            //        //    + 0 * grayValues[(i) * bmpData.Stride + (j + 5)]
            //        //    - 1 * grayValues[(i + 1) * bmpData.Stride + (j - 1)]
            //        //    - 2 * grayValues[(i + 1) * bmpData.Stride + (j) + 2]
            //        //    - 1 * grayValues[(i + 1) * bmpData.Stride + (j + 5)];

            //        //Priwitt算子
            //        gradX =
            //            -1 * grayValues[(i - 1) * bmpData.Stride + (j - 1)] +
            //            0 * grayValues[(i - 1) * bmpData.Stride + (j) + 2] +
            //            1 * grayValues[(i - 1) * bmpData.Stride + (j + 5)] +
            //            -1 * grayValues[(i) * bmpData.Stride + (j - 1)] +
            //            0 * grayValues[(i) * bmpData.Stride + (j) + 2] +
            //            1 * grayValues[(i) * bmpData.Stride + (j + 5)] +
            //            -1 * grayValues[(i + 1) * bmpData.Stride + (j - 1)] +
            //            0 * grayValues[(i + 1) * bmpData.Stride + (j) + 2] +
            //            1 * grayValues[(i + 1) * bmpData.Stride + (j + 5)];

            //        gradY =
            //              -1 * grayValues[(i - 1) * bmpData.Stride + (j - 1)]
            //            - 1 * grayValues[(i - 1) * bmpData.Stride + (j) + 2]
            //            - 1 * grayValues[(i - 1) * bmpData.Stride + (j + 5)]
            //            + 0 * grayValues[(i) * bmpData.Stride + (j - 1)]
            //            + 0 * grayValues[(i) * bmpData.Stride + (j) + 2]
            //            + 0 * grayValues[(i) * bmpData.Stride + (j + 5)]
            //            + 1 * grayValues[(i + 1) * bmpData.Stride + (j - 1)]
            //           + 1 * grayValues[(i + 1) * bmpData.Stride + (j) + 2]
            //            + 1 * grayValues[(i + 1) * bmpData.Stride + (j + 5)];

            //        ////拉普拉斯算子
            //        //gradX =
            //        //    1 * grayValues[(i - 1) * bmpData.Stride + (j - 1)] +
            //        //    -2 * grayValues[(i - 1) * bmpData.Stride + (j) + 2] +
            //        //    1 * grayValues[(i - 1) * bmpData.Stride + (j + 5)] +
            //        //    -2 * grayValues[(i) * bmpData.Stride + (j - 1)] +
            //        //    4 * grayValues[(i) * bmpData.Stride + (j) + 2] +
            //        //    -2 * grayValues[(i) * bmpData.Stride + (j + 5)] +
            //        //    1 * grayValues[(i + 1) * bmpData.Stride + (j - 1)] +
            //        //    -2 * grayValues[(i + 1) * bmpData.Stride + (j) + 2] +
            //        //    1 * grayValues[(i + 1) * bmpData.Stride + (j + 5)];

            //        //gradY =
            //        //      1 * grayValues[(i - 1) * bmpData.Stride + (j - 1)]
            //        //    - 2 * grayValues[(i - 1) * bmpData.Stride + (j) + 2]
            //        //    + 1 * grayValues[(i - 1) * bmpData.Stride + (j + 5)]
            //        //    -2 * grayValues[(i) * bmpData.Stride + (j - 1)]
            //        //    +4 * grayValues[(i) * bmpData.Stride + (j) + 2]
            //        //    -2 * grayValues[(i) * bmpData.Stride + (j + 5)]
            //        //    + 1 * grayValues[(i + 1) * bmpData.Stride + (j - 1)]
            //        //   -2 * grayValues[(i + 1) * bmpData.Stride + (j) + 2]
            //        //    + 1 * grayValues[(i + 1) * bmpData.Stride + (j + 5)];

            //        grad = Math.Sqrt(gradX * gradX + gradY * gradY);

            //        if (grad < 0)
            //        {
            //            grad = 0;
            //        }
            //        if (grad > 255)
            //        {
            //            grad = 255;
            //        }
            //        tempArray[(i) * bmpData.Stride + (j) + 2] =
            //            tempArray[(i) * bmpData.Stride + (j) + 1] =
            //            tempArray[(i) * bmpData.Stride + (j) + 0] = (byte)grad;
            //    }
            //}
            //解除锁存图像，这里源代码没有
            m_Bitmap.UnlockBits(bmpData);
            //改进梯度幅值计算
            BitmapData bmData = m_Bitmap.LockBits(new Rectangle(0, 0, m_Bitmap.Width, m_Bitmap.Height),
                ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            float valve = 67;
            for (int i = 0; i < 256; i++)
            {
                gray[i] = 0;
            }
            unsafe
            {
                int stride = bmData.Stride;
                System.IntPtr Scan0 = bmData.Scan0;
                byte* p = (byte*)(void*)Scan0;
                byte* pp;
                int tt;
                int nOffset = stride - m_Bitmap.Width * 3;
                int nWidth = m_Bitmap.Width;
                int nHeight = m_Bitmap.Height;
                int Sx = 0;
                int Sy = 0;
                double sumM = 0;
                double sumCount = 0;
                //参考链接http://blog.csdn.net/guanyuqiu/article/details/52993412
                //Sobel边缘检测算法
                int[] marginalMx = { -1, 0, 1, -2, 0, 2, -1, 0, 1 };
                int[] marginalMy = { 1, 2, 1, 0, 0, 0, -1, -2, -1 };

                // Robert算子——无方向一阶锐化
                //int[] marginalMx = { 0, 0, 0, 0, 1, 0, 0, 0, -1 };
                //int[] marginalMy = { 0, 0, 0, 0, 0, 1, 0, -1, 0 };

                // 拉普拉斯算子边缘检测
                //int[] marginalMx = { 0, -1, 0, -1, 4, -1, 0, -1, 0 };
                //int[] marginalMy = { 0, -1, 0, -1, 4, -1, 0, -1, 0 };

                // 拉普拉斯算子改进边缘检测
                //int[] marginalMx = { -1, -1, -1, -1, 8, -1, -1, -1, -1 };
                //int[] marginalMy = { -1, -1, -1, -1, 8, -1, -1, -1, -1 };

                //Priwitt算子边缘检测
                //int[] marginalMx = { -1, 0, 1, -1, 0, 1, -1, 0, 1 };
                //int[] marginalMy = { -1, -1, -1, 0, 0, 0, 1, 1, 1 };

                int[,] dlta = new int[nHeight, nWidth];
                for (int y = 0; y < nHeight; y++)
                {
                    for (int x = 0; x < nWidth; x++)
                    {
                        if (!(x <= 0 || x >= nWidth - 1 || y <= 0 || y >= nHeight - 1))
                        {
                            pp = p;
                            Sx = 0;
                            Sy = 0;
                            for (int i = -1; i <= 1; i++)
                            {
                                for (int j = -1; j <= 1; j++)
                                {
                                    pp += (j * 3 + stride * i);
                                    Sx += pp[0] * marginalMx[(i + 1) * 3 + j + 1];
                                    Sy += pp[0] * marginalMy[(i + 1) * 3 + j + 1];
                                    pp = p;
                                }
                            }
                            m[y, x] = (int)(Math.Sqrt(Sx * Sx + Sy * Sy));
                            //增强白点
                            if (m[y, x] > valve / 2)
                            {
                                if (p[0] > 240)
                                {
                                    m[y, x] += valve;
                                }
                                else if (p[0] > 220)
                                {
                                    m[y, x] += (float)(valve * 0.8);
                                }
                                else if (p[0] > 200)
                                {
                                    m[y, x] += (float)(valve * 0.6);
                                }
                                else if (p[0] > 180)
                                {
                                    m[y, x] += (float)(valve * 0.4);
                                }
                                else if (p[0] > 160)
                                {
                                    m[y, x] += (float)(valve * 0.2);
                                }
                            }
                            float tan;
                            if (Sx != 0)
                            {
                                tan = Sy / Sx;
                            }
                            else
                            {
                                tan = 10000;
                            }

                            if (-0.41421356 <= tan && tan < 0.41421356)   //角度为-22.5度到22.5度之间
                            {
                                dlta[y, x] = 0;
                            }
                            else if (0.41421356 <= tan && tan < 2.41421356)//角度为22.5度到67.5度之间
                            {
                                dlta[y, x] = 1; //m[y,x] = 0;
                            }
                            else if (tan >= 2.41421356 || tan < -2.41421356)//角度为67.5度到90度之间或-90度到-67.5度
                            {
                                dlta[y, x] = 2;    //	m[y,x]+=valve;
                            }
                            else
                            {
                                dlta[y, x] = 3;//m[y,x] = 0;     
                            }
                        }
                        else
                        {
                            m[y, x] = 0;

                        }
                        p += 3;
                        if (m[y, x] > 0)
                        {
                            sumCount++;
                            sumM += m[y, x];
                        }
                    }
                    p += nOffset;
                }

                p = (byte*)(void*)Scan0;    //非极大值抑制和阈值
                for (int y = 0; y < nHeight; y++)
                {
                    for (int x = 0; x < nWidth; x++)
                    {
                        if (m[y, x] > sumM / sumCount * 1.2)
                        {
                            p[0] = p[1] = p[2] = (byte)(m[y, x]);
                        }
                        else
                        {
                            m[y, x] = 0;
                            p[0] = p[1] = p[2] = 0;
                        }
                        if (x >= 1 && x <= nWidth - 1 && y >= 1 && y <= nHeight - 1 && m[y, x] > valve)
                        {
                            switch (dlta[y, x])
                            {
                                case 0:
                                    if (m[y, x] >= m[y, x - 1] && m[y, x] >= m[y, x + 1])
                                    {
                                        p[0] = p[1] = p[2] = 255;
                                    }
                                    break;
                                case 1:
                                    if (m[y, x] >= m[y + 1, x - 1] && m[y, x] >= m[y - 1, x + 1])//正斜45度边缘
                                    {
                                        p[0] = p[1] = p[2] = 255;
                                    }
                                    break;

                                case 2:
                                    if (m[y, x] >= m[y - 1, x] && m[y, x] >= m[y + 1, x])//垂直边缘
                                    {
                                        p[0] = p[1] = p[2] = 255;
                                    }
                                    break;

                                case 3:
                                    if (m[y, x] >= m[y + 1, x + 1] && m[y, x] >= m[y - 1, x - 1])//反斜45度边缘
                                    {
                                        p[0] = p[1] = p[2] = 255;
                                    }
                                    break;

                            }
                        }
                        if (p[0] == 255)
                        {
                            m[y, x] = 1;
                        }
                        else
                        {
                            m[y, x] = 0;
                            p[0] = p[1] = p[2] = 0;
                        }
                        tt = p[0];
                        gray[tt]++;
                        p += 3;
                    }

                }
                m_Bitmap.UnlockBits(bmData);
                IntPtr ip = m_Bitmap.GetHbitmap();//将Bitmap转换为BitmapSource
                BitmapSource bitmapSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(ip, IntPtr.Zero, Int32Rect.Empty, System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
                imgLoad.Source = bitmapSource;

            }

        }


        //车牌定位
        private void btnLocation_Click(object sender, RoutedEventArgs e)
        { //车牌定位
            this.c_Bitmap = Recoginzation.licensePlateLocation(m_Bitmap, always_Bitmap, m);
            extract_Bitmap_one = c_Bitmap.Clone(new Rectangle(0, 0, c_Bitmap.Width, c_Bitmap.Height),
                PixelFormat.DontCare);
            IntPtr ip = m_Bitmap.GetHbitmap();
            BitmapSource bitmapSource = System.Windows.Interop.Imaging
              .CreateBitmapSourceFromHBitmap(ip, IntPtr.Zero, Int32Rect.Empty,
              System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
            imgLoad.Source = bitmapSource;

            IntPtr ip2 = c_Bitmap.GetHbitmap();
            BitmapSource bitmapSource2 = System.Windows.Interop.Imaging
           .CreateBitmapSourceFromHBitmap(ip2, IntPtr.Zero, Int32Rect.Empty,
           System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
            imgLicence.Source = bitmapSource2;



        }


        //车牌灰度化
        private void btnLicenceGray_Click(object sender, RoutedEventArgs e)
        {//车牌灰度化
            if (c_Bitmap != null)
            {
                int tt = 0;
                for (int i = 0; i < 256; i++)
                {
                    gray[i] = 0;
                    rr[i] = 0;
                    gg[i] = 0;
                    bb[i] = 0;
                }
                BitmapData bmData = c_Bitmap.LockBits(new Rectangle(0, 0, c_Bitmap.Width, c_Bitmap.Height),
                    ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
                int stride = bmData.Stride; //获取或设置Bitmap对象的跨距宽度（也称为扫描宽度）
                System.IntPtr Scan0 = bmData.Scan0; //获取或设置第一个像素数据的地址
                unsafe //"生成"选择“允许不安全代码”
                {
                    byte* p = (byte*)(void*)Scan0;
                    //这里stride是图片一行数据的长度， 
                    //sitide - m_Bitmap.Width*3 是获取一行元素中多余的部分
                    //因为每个m_Bitmap.Width获取一行的像素，乘三是因为每个像素由rgb三个值组成。
                    //参考https://www.cnblogs.com/zkwarrior/p/5665216.html
                    int nOffset = stride - c_Bitmap.Width * 3;
                    byte red, green, blue;
                    int nWidth = c_Bitmap.Width;
                    int nHeight = c_Bitmap.Height;
                    cWidth = c_Bitmap.Width;
                    cHeight = c_Bitmap.Height;
                    for (int y = 0; y < nHeight; y++)
                    {
                        for (int x = 0; x < nWidth; x++)
                        {
                            blue = p[0];
                            green = p[1];
                            red = p[2];
                            //加权平均值法
                            tt = p[0] = p[1] = p[2] = (byte)(.299 * red + .587 * green + .114 * blue);

                            //最大值法
                            //byte max = red > green ? red : green;
                            //max = max > blue ? max : blue;
                            //tt = p[0] = p[1] = p[2] = max;

                            //平均值法
                            //tt = p[0] = p[1] = p[2] = (byte)((red+green+blue)/3);

                            rr[red]++;
                            gg[green]++;
                            bb[blue]++;
                            gray[tt]++; //统计灰度值tt的像素点数目
                            p += 3;
                        }
                        //加上多余的部分，跳到下一行的开头
                        p += nOffset;
                    }
                }
                c_Bitmap.UnlockBits(bmData);
                IntPtr ip = c_Bitmap.GetHbitmap(); //将Bitmap转换为BitmapSource
                BitmapSource bitmapSource = System.Windows.Interop.Imaging
                    .CreateBitmapSourceFromHBitmap(ip, IntPtr.Zero, Int32Rect.Empty,
                    System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
                imgLicence.Source = bitmapSource;
            }
        }

        //车牌二值化
        private void btnLicenceBinary_Click(object sender, RoutedEventArgs e)
        { //车牌二值化
            int Mr = 0;
            long sum = 0;
            int count = 0;
            //像素个数与灰度等级的乘积除以像素个数
            //应该是为了求整个颜色灰度的平均值，以便区分黑白
            for (int i = 0; i < 256; i++)
            {
                sum += gray[i] * i;
                count += gray[i];
            }
            Mr = (int)(sum / count);
            int sum1 = 0;
            int count1 = 0;
            for (int i = 0; i < Mr; i++)
            {
                sum1 += gray[i] * i;
                count1 += gray[i];
            }
            int g1 = sum1 / count1;

            int sum2 = 0;
            int count2 = 0;
            for (int i = Mr; i < 255; i++)
            {
                sum2 += gray[i] * i;
                count2 += gray[i];
            }
            int g2 = sum2 / count2;

            //求阈值
            int va;
            if (count1 < count2)
            {
                //白底黑字
                va = Mr - count1 / count2 * Math.Abs(g1 - Mr);
            }
            else
            {
                va = Mr + count2 / count1 * Math.Abs(g2 - Mr);
            }
            //双峰法
            //双峰法是先分段，在左右两侧找出最大值，然后向中间遍历，
            //求出其中的最小值作为阀值
            //参考链接https://www.cnblogs.com/lonelyxmas/p/8554542.html
            //int h1 = 0, h2 = 0, t1 = 0, t2 = 0, t = 255, Th = 0;
            //for (int i = 0; i < 256; i++)
            //{
            //    if (i < 129)
            //    {
            //        if (gray[i] > t1)
            //        {
            //            h1 = i;
            //            t1 = gray[i];
            //        }
            //    }
            //    else
            //    {
            //        if (gray[i] > t2)
            //        {
            //            h2 = i;
            //            t2 = gray[i];
            //        }
            //    }
            //}
            //for (int n = h1; n <= h2; n++)
            //{
            //    if (gray[n] < t)
            //    {
            //        Th = n;
            //        t = gray[n];
            //    }
            //}
            //va = Th;

            //方法1，直接求所有颜色的平均值作为阀值
            // va = Mr;

            //OSTU算法
            //这个有问题,
            //不知道是因为其中有个size的大小不对
            //还是因为算法的原因
            //参考链接https://www.cnblogs.com/zhonghuasong/p/7250540.html

            //int threshold = 0;
            //long nsum0 = 0, nsum1 = 0; //存储前景的灰度总和及背景灰度总和
            //long cnt0 = 0, cnt1 = 0; //前景的总个数及背景的总个数
            //double w0 = 0, w1 = 0; //前景及背景所占整幅图像的比例
            //double u0 = 0, u1 = 0;  //前景及背景的平均灰度
            //double variance = 0; //最大类间方差
            //double u;

            //double maxVariance = 0;

            //for (int i = 0; i < 256; i++)
            //{
            //    nsum0 = 0;
            //    nsum1 = 0;
            //    cnt0 = 0;
            //    cnt1 = 0;
            //    w0 = 0;
            //    w1 = 0;
            //    u0 = 0;
            //    u1 = 0;
            //    u = 0;
            //    for (int j = 0; j < i; j++)
            //    {
            //        cnt0 += gray[j];
            //        nsum0 += j * gray[j];
            //    }
            //    u0 = (double)nsum0 / cnt0;
            //    w0 = (double)cnt0 / cHeight * cWidth;

            //    for (int j = i; j <= 255; j++)
            //    {
            //        cnt1 += gray[j];
            //        nsum1 += j * gray[j];
            //    }

            //    u1 = (double)nsum1 / cnt1;
            //    w1 = (double)cnt1 / cHeight * cWidth;
            //    u = nsum0 + nsum1;
            //    variance = w0 * (u0 - u) * (u0 - u) + w1 * (u1 - u) * (u1 - u);

            //    variance = w0 * w1 * (u0 - u1) * (u0 - u1);
            //    if (variance > maxVariance)
            //    {
            //        maxVariance = variance;
            //        threshold = i;
            //    }

            //}



            //Console.WriteLine("OSTU二值化算法" + threshold);
            //va = threshold;

            Console.WriteLine("二值化阈值为：" + va);

            BitmapData bmData = c_Bitmap.LockBits(new Rectangle(0, 0, c_Bitmap.Width,
                c_Bitmap.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            unsafe
            {
                int stride = bmData.Stride;
                System.IntPtr Scan0 = bmData.Scan0;
                byte* p = (byte*)(void*)Scan0;
                int nOffset = stride - c_Bitmap.Width * 3;

                int nWidth = c_Bitmap.Width;
                int nHeight = c_Bitmap.Height;

                for (int y = 0; y < nHeight; y++)
                {
                    for (int x = 0; x < nWidth; x++)
                    {
                        if (p[0] > va)
                        {
                            p[0] = p[1] = p[2] = 255;
                        }
                        else
                        {
                            p[0] = p[1] = p[2] = 0;
                        }
                        p += 3;
                    }
                    p += nOffset;
                }
            }
            c_Bitmap.UnlockBits(bmData);
            IntPtr ip = c_Bitmap.GetHbitmap(); //将Bitmap转换为BitmapSource
            BitmapSource bitmapSource = System.Windows.Interop.Imaging
                .CreateBitmapSourceFromHBitmap(ip, IntPtr.Zero, Int32Rect.Empty,
                System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
            imgLicence.Source = bitmapSource;

        }

        //字符切割
        private void btnCharSplit_Click(object sender, RoutedEventArgs e)
        {  // 字符切割

            //车牌切割
            flag = 1;
            BitmapData bmData = c_Bitmap.LockBits(new Rectangle(0, 0, c_Bitmap.Width, c_Bitmap.Height),
                ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            unsafe
            {
                int stride = bmData.Stride;
                System.IntPtr Scan0 = bmData.Scan0;
                byte* p = (byte*)(void*)Scan0;
                int nOffset = stride - c_Bitmap.Width * 3;

                int nWidth = c_Bitmap.Width;
                int nHeight = c_Bitmap.Height;
                int[] countHeight = new int[nHeight];
                int[] countWidth = new int[nWidth];
                int Yheight = nHeight, YBottom = 0;
                //这一步没有必要，因为默认的初值就是0
                //for (int i = 0; i < nHeight; i++)
                //{
                //    countHeight[i] = 0;
                //}
                for (int y = 0; y < nHeight; ++y)
                {
                    for (int x = 0; x < nWidth; x++)
                    {
                        if ((p[0] == 0 && p[3] == 255) || (p[0] == 255 && p[3] == 0))
                        {
                            countHeight[y]++;
                        }
                        p += 3;
                    }
                    Console.WriteLine(y + "********跳变数" + countHeight[y]);
                    p += nOffset;
                }
                //计算车牌号的上边缘
                for (int y = nHeight / 2; y > 0; y--)
                {
                    if (countHeight[y] >= 16 && countHeight[(y + 1) % nHeight] >= 12)
                    {
                        if (Yheight > y)
                        {
                            Yheight = y;
                        }
                        if ((Yheight - y) == 1)
                        {
                            Yheight = y - 3;
                            Console.WriteLine("--------" + Yheight);
                        }

                    }
                    Console.WriteLine("现在图片的顶部是：" + Yheight);
                }

                //计算车牌号的下边缘
                for (int y = nHeight / 2; y < nHeight; y++)
                {
                    if (countHeight[y] >= 12 && countHeight[(y + 1) % nHeight] >= 12)
                    {
                        if (YBottom < y)
                        {
                            YBottom = y;
                        }
                        if ((y - Yheight) == 1)
                        {
                            YBottom = y + 3;
                            Console.WriteLine("--------" + YBottom);
                        }

                    }
                    Console.WriteLine("现在图片的底部是：" + YBottom);
                }
                YBottom += 1;
                byte* p1 = (byte*)(void*)Scan0;

                p1 += stride * (Yheight - 1);   //跳到车牌的顶部
                for (int y = Yheight; y < YBottom; ++y)
                {
                    for (int x = 0; x < nWidth; x++)
                    {
                        if (p1[0] == 255)
                        {
                            countWidth[x]++;
                        }
                        p1 += 3;
                    }
                    p1 += nOffset;
                }
                int contg = 0, contd = 0, countRightEdge = 0, countLeftEdge = 0,
                    Y1 = nWidth, Yr = 0;
                int[] XLeft = new int[20];
                int[] xRight = new int[20];
                //这里不需要初始化，所以省去了。
                for (int y = 0; y < YBottom; y++)
                {
                    for (int x = 0; x < nWidth; x++)
                    {
                        if (countWidth[(x + 1) % nWidth] < y && countWidth[x] >= y &&
                            countWidth[Math.Abs((x - 1) % nWidth)] >= y && contg >= 2)
                        {
                            if (countRightEdge == 6)
                            {
                                Yr = x;
                            }
                            if ((countRightEdge == 2 && (x >= XLeft[2] && XLeft[2] > 0)))
                            {
                                xRight[countRightEdge] = x;
                                countRightEdge++;
                                contd = 0;
                            }
                            else
                            {
                                if ((countRightEdge != 2))
                                {
                                    if (countRightEdge == 0 && contg < 4)
                                    {
                                        XLeft[0] = 0;
                                        countLeftEdge = 0;
                                    }
                                    if ((x >= XLeft[0] && XLeft[0] > 0))
                                    {
                                        xRight[countRightEdge] = x;
                                        countRightEdge++;
                                        contd = 0;
                                    }
                                }
                            }
                        }
                        if (countWidth[Math.Abs((x - 1) % nWidth)] < y &&
                            countWidth[x] >= y && countWidth[(x + 1) % nWidth] >= y && contd >= 2)
                        {
                            if (countLeftEdge == 0 && countWidth[(x + 2) % nWidth] >= y)
                            {
                                Y1 = x;
                            }
                            if ((countLeftEdge == 2 && contd > 5))
                            {
                                XLeft[countLeftEdge] = x;
                                countLeftEdge++;
                            }
                            else
                            {
                                if ((countLeftEdge != 2))
                                {
                                    XLeft[countLeftEdge] = x;
                                    countLeftEdge++;
                                    contg = 0;
                                    if (countLeftEdge == 0 && countWidth[(x + 2) % nWidth] < y)
                                    {
                                        XLeft[0] = 0;
                                        countLeftEdge = 0;
                                    }
                                }
                            }
                        }
                        contg++;
                        contd++;
                    }
                    if (countRightEdge + countLeftEdge >= 14)
                    {
                        break;
                    }
                    countRightEdge = 0;
                    countLeftEdge = 0;
                    for (int i = 0; i < xRight.Length; i++)
                    {
                        xRight[i] = 0;
                    }
                    for (int i = 0; i < XLeft.Length; i++)
                    {
                        XLeft[i] = 0;
                    }
                }
                //字符切割
                c_Bitmap.UnlockBits(bmData);
                if ((YBottom - Yheight) > 1 && (Yr - Y1) > 1)
                {
                    Rectangle sourceRectangle = new Rectangle(Y1, Yheight, Yr - Y1, YBottom - Yheight);
                    extract_Bitmap_two = extract_Bitmap_one.Clone(sourceRectangle, PixelFormat.DontCare);
                    BitmapData bmData2 = extract_Bitmap_two.LockBits(new Rectangle(0, 0, extract_Bitmap_two.Width,
                        extract_Bitmap_two.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
                    int stride2 = bmData2.Stride;
                    System.IntPtr Scan02 = bmData2.Scan0;
                    byte* p2 = (byte*)(void*)Scan02;
                    int nOffset2 = stride2 - extract_Bitmap_two.Width * 3;

                    int nWidth2 = extract_Bitmap_two.Width;
                    int nHeight2 = extract_Bitmap_two.Height;
                    for (int y = 0; y < nHeight2; y++)
                    {
                        for (int x = 0; x < nWidth2; x++)
                        {
                            if (x == (xRight[0] - Y1) || x == (XLeft[0] - Y1) ||
                                x == (xRight[1] - Y1) || x == (XLeft[1] - Y1) ||
                                x == (xRight[2] - Y1) || x == (XLeft[2] - Y1) ||
                                x == (xRight[3] - Y1) || x == (XLeft[3] - Y1) ||
                                x == (xRight[4] - Y1) || x == (XLeft[4] - Y1) ||
                                x == (xRight[5] - Y1) || x == (XLeft[5] - Y1) ||
                                x == (xRight[6] - Y1) || x == (XLeft[6] - Y1) ||
                                x == (xRight[7] - Y1) || x == (XLeft[7] - Y1))
                            {
                                if (x != 0)
                                {
                                    p2[2] = 255;
                                    p2[0] = p2[1] = 0;
                                }
                            }
                            p2 += 3;

                        }
                        p2 += nOffset2;
                    }
                    extract_Bitmap_two.UnlockBits(bmData2);
                    IntPtr ip = extract_Bitmap_two.GetHbitmap(); //将Bitmap转换为BitmapSource
                    BitmapSource bitmapSource = System.Windows.Interop.Imaging
                        .CreateBitmapSourceFromHBitmap(ip, IntPtr.Zero, Int32Rect.Empty,
                        System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
                    imgSplit.Source = bitmapSource;
                    //这里不知道为什么源码要写7遍
                    //如果用for循环就写一遍就行了
                    //而且不知道为什么要把i=0的情况放到最后
                    for (int i = 0; i < 7; i++)
                    {
                        if ((YBottom - Yheight) > 1 && (xRight[i] - XLeft[i]) > 1)
                        {
                            Rectangle sourceRectangle2 = new Rectangle(XLeft[i], Yheight,
                                xRight[i] - XLeft[i], YBottom - Yheight);
                            z_Bitmap1 = extract_Bitmap_one.Clone(sourceRectangle2, PixelFormat.DontCare);
                            z_Bitmaptwo[i] = c_Bitmap.Clone(sourceRectangle2, PixelFormat.DontCare);
                            objNewPic = new System.Drawing.Bitmap(z_Bitmaptwo[i], 9, 16);
                            z_Bitmaptwo[i] = objNewPic;
                            ip = z_Bitmaptwo[i].GetHbitmap();
                            bitmapSource = System.Windows.Interop.Imaging
                            .CreateBitmapSourceFromHBitmap(ip, IntPtr.Zero, Int32Rect.Empty,
                            System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
                            switch (i)
                            {
                                case 0:
                                    imgChar0.Source = bitmapSource;
                                    break;
                                case 1:
                                    imgChar1.Source = bitmapSource;
                                    break;
                                case 2:
                                    imgChar2.Source = bitmapSource;
                                    break;
                                case 3:
                                    imgChar3.Source = bitmapSource;
                                    break;
                                case 4:
                                    imgChar4.Source = bitmapSource;
                                    break;
                                case 5:
                                    imgChar5.Source = bitmapSource;
                                    break;
                                case 6:
                                    imgChar6.Source = bitmapSource;
                                    break;

                            }

                            //imgChar1.Source = bitmapSource;
                        }
                    }
                    //if ((YBottom -Yheight) >1 && (xRight[1]-XLeft[1])>1)
                    //{
                    //    Rectangle sourceRectangle2 = new Rectangle(XLeft[1], Yheight,
                    //        xRight[1] - XLeft[1], YBottom - Yheight);
                    //    z_Bitmap1 = extract_Bitmap_one.Clone(sourceRectangle2, PixelFormat.DontCare);
                    //    z_Bitmaptwo[1] = c_Bitmap.Clone(sourceRectangle2, PixelFormat.DontCare);
                    //    objNewPic = new System.Drawing.Bitmap(z_Bitmaptwo[1], 9, 16);
                    //    z_Bitmaptwo[1] = objNewPic;
                    //    ip = z_Bitmaptwo[1].GetHbitmap();
                    //    bitmapSource = System.Windows.Interop.Imaging
                    //    .CreateBitmapSourceFromHBitmap(ip, IntPtr.Zero, Int32Rect.Empty,
                    //    System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
                    //    imgChar1.Source = bitmapSource;
                    //}

                }

            }



        }


        //字符识别
        private void btnCharIdentify_Click(object sender, RoutedEventArgs e)
        {

            //字符识别
            int charBmpCount = this.transformFiles(charSourceBath);
            int provinceBmpCount = this.transformFiles(provinceSourceBath);
            int[] charMatch = new int[charBmpCount];
            int[] provinceMatch = new int[provinceBmpCount];
            charFont = new Bitmap[charBmpCount];
            provinceFont = new Bitmap[provinceBmpCount];

            //初始化省去

            for (int i = 0; i < charBmpCount; i++)
            {
                charFont[i] = (Bitmap)Bitmap.FromFile(charString[i], false);
            }
            for (int i = 0; i < provinceBmpCount; i++)
            {
                provinceFont[i] = (Bitmap)Bitmap.FromFile(provinceString[i], false);
            }
            int matchIndex = 0;
            string[] digitalFont = new string[7];
            unsafe
            {
                if (z_Bitmaptwo[0] != null)
                {
                    BitmapData bmData = z_Bitmaptwo[0].LockBits(new Rectangle(0, 0, z_Bitmaptwo[0].Width, z_Bitmaptwo[0].Height),
                  ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
                    int stride = bmData.Stride;
                    System.IntPtr Scan = bmData.Scan0;
                    int nOffset = stride - z_Bitmaptwo[0].Width * 3;
                    int nWidth = z_Bitmaptwo[0].Width;
                    int nHeight = z_Bitmaptwo[0].Height;
                    int lv, lc = 30;
                    for (int i = 0; i < provinceBmpCount; i++)
                    {
                        byte* p = (byte*)(void*)Scan;
                        BitmapData bmData1 = provinceFont[i].LockBits(new Rectangle(0, 0, provinceFont[i].Width, provinceFont[i].Height),
                ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
                        int stride1 = bmData1.Stride;
                        System.IntPtr Scan1 = bmData1.Scan0;
                        byte* p1 = (byte*)(void*)Scan1;
                        int nOffset1 = stride1 - provinceFont[i].Width * 3;
                        int nWidth1 = provinceFont[i].Width;
                        int nHeight1 = provinceFont[i].Height;
                        int ccc0 = 0, ccc1 = 0;
                        lv = 0;
                        for (int y = 0; y < nHeight; y++)
                        {
                            for (int x = 0; x < nWidth; x++)
                            {
                                if ((p[0] - p1[0] != 0))
                                {
                                    provinceMatch[i]++;
                                }
                                p1 += 3;
                                p += 3;
                            }
                            p1 += nOffset;
                            p += nOffset;
                        }
                        //这里返回的是最小值
                        //需要的是最小值的下标

                        Console.WriteLine(charDigitalString[i] + "数字中不相同的像素数值" + provinceMatch[i]);
                        matchIndex = minNumber(provinceMatch);

                        //provinceMatch.Min();
                        digitalFont[0] = provinceDigitalString[matchIndex].Substring(0, 1);
                        provinceFont[i].UnlockBits(bmData1);
                    }

                    z_Bitmaptwo[0].UnlockBits(bmData);
                }
                if (z_Bitmaptwo[1] != null && z_Bitmaptwo[2] != null &&
                    z_Bitmaptwo[3] != null && z_Bitmaptwo[4] != null &&
                    z_Bitmaptwo[5] != null && z_Bitmaptwo[6] != null)
                {
                    for (int j = 1; j < 7; j++)
                    {
                        BitmapData bmData = z_Bitmaptwo[j].LockBits(new Rectangle(0, 0, z_Bitmaptwo[j].Width, z_Bitmaptwo[j].Height),
             ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
                        int stride = bmData.Stride;
                        System.IntPtr Scan = bmData.Scan0;
                        int nOffset = stride - z_Bitmaptwo[j].Width * 3;
                        int nWidth = z_Bitmaptwo[j].Width;
                        int nHeight = z_Bitmaptwo[j].Height;
                        int lv, lc = 0;
                        for (int i = 0; i < charBmpCount; i++)
                        {
                            charMatch[i] = 0;
                        }
                        for (int i = 0; i < charBmpCount; i++)
                        {
                            byte* p = (byte*)(void*)Scan;
                            BitmapData bmData1 = charFont[i].LockBits(new Rectangle(0, 0, charFont[i].Width, charFont[i].Height),
               ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
                            int stride1 = bmData1.Stride;
                            System.IntPtr Scan1 = bmData1.Scan0;
                            byte* p1 = (byte*)(void*)Scan1;
                            int nOffset1 = stride1 - charFont[i].Width * 3;
                            int nWidth1 = charFont[i].Width;
                            int nHeight1 = charFont[i].Height;
                            int ccc0 = 0, ccc1 = 0;
                            lv = 0;
                            for (int y = 0; y < nHeight; y++)
                            {
                                for (int x = 0; x < nWidth; x++)
                                {
                                    if ((p[0] - p1[0] != 0))
                                    {
                                        charMatch[i]++;
                                    }
                                    lv++;
                                    p1 += 3;
                                    p += 3;
                                }
                                p1 += nOffset;
                                p += nOffset;
                            }
                            //这里返回的是最小值
                            //需要的是最小值的下标
                            Console.WriteLine(charDigitalString[i] + "数字中不相同的像素数值" + charMatch[i]);
                            matchIndex = minNumber(charMatch);
                            
                            //charMatch
                            //provinceMatch.Min();
                            digitalFont[j] = charDigitalString[matchIndex].Substring(0, 1);
                            charFont[i].UnlockBits(bmData1);

                        }
                        z_Bitmaptwo[j].UnlockBits(bmData);
                    }

                }
            }
            string result = "";
            for (int i = 0; i < digitalFont.Length; i++)
            {
                result += digitalFont[i];
            }
            this.txtResult.Text = result;


        }

        //获取数组最小值下标
        private int minNumber(int[] promatch)
        {
            int min = promatch.Min();
            int n = 0;
            for (int i = 0; i < promatch.Length; i++)
            {
                if (promatch[i] ==min)
                {
                    n = i;
                }
            }
            return n;
        }

        //字符图像归一化
        private int transformFiles(string path)
        {
            DirectoryInfo dir = new DirectoryInfo(path);
            FileInfo[] files = dir.GetFiles("*.bmp");
            int i = 0, j = 0;
            try
            {
                foreach (FileInfo f in files)
                {
                    i++;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
            if (path.Equals(charSourceBath))
            {
                this.charString = new string[i];
                this.charDigitalString = new string[i];
                try
                {
                    foreach (FileInfo f in files)
                    {
                        charString[j] = (dir + f.ToString());
                        charDigitalString[j] =
                            Path.GetFileNameWithoutExtension(charString[j]);
                        j++;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message.ToString());
                    throw;
                }
            }
            else
            {
                provinceString = new string[i];
                provinceDigitalString = new string[i];
                try
                {
                    foreach (FileInfo f in files)
                    {
                        provinceString[j] = (dir + f.ToString());
                        provinceDigitalString[j] =
                            Path.GetFileNameWithoutExtension(provinceString[j]);
                        j++;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message.ToString());
                    throw;
                }
            }
            return i;
        }

        private void btnCSave_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.Image[] chars = { imgChar0, imgChar1, imgChar2, imgChar3, imgChar4, imgChar5, imgChar6 };
            for (int i = 0; i < 7; i++)
            {
                SaveFileDialog sfd = new SaveFileDialog();
                sfd.Filter = "Image Files (*.bmp, *.png, *.jpg)|*.bmp;*.png;*.jpg | All Files | *.*";
                sfd.RestoreDirectory = true;//保存对话框是否记忆上次打开的目录 
                if (sfd.ShowDialog() == true)
                {

                    var encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create((BitmapSource)chars[i].Source));
                    using (FileStream stream = new FileStream(sfd.FileName, FileMode.Create))
                        encoder.Save(stream);
                }

            }
        }


        //中值滤波
        //参考链接http://www.cnblogs.com/aaaSoft/archive/2009/12/14/1623628.html
        private byte[,] MedianFilterFunction(byte[,] m, int windowRadius)
        {
            int width = m.GetLength(0);
            int height = m.GetLength(1);
            byte[,] lightArray = new byte[width, height];
            //开始滤波
            for (int i = 0; i <= width - 1; i++)
            {
                for (int j = 0; j <= height - 1; j++)
                {
                    //得到过滤窗口矩形
                    Rectangle rectWindow = new Rectangle(i - windowRadius, j - windowRadius, 2 * windowRadius + 1, 2 * windowRadius + 1);
                    if (rectWindow.Left < 0) rectWindow.X = 0;
                    if (rectWindow.Top < 0) rectWindow.Y = 0;
                    if (rectWindow.Right > width - 1) rectWindow.Width = width - 1 - rectWindow.Left;
                    if (rectWindow.Bottom > height - 1) rectWindow.Height = height - 1 - rectWindow.Top;
                    //将窗口中的颜色取到列表中
                    List<byte> windowPixelColorList = new List<byte>();
                    for (int oi = rectWindow.Left; oi <= rectWindow.Right - 1; oi++)
                    {
                        for (int oj = rectWindow.Top; oj <= rectWindow.Bottom - 1; oj++)
                        {
                            windowPixelColorList.Add(m[oi, oj]);
                        }
                    }
                    //排序
                    windowPixelColorList.Sort();
                    //取中值
                    byte middleValue = 0;
                    if ((windowRadius * windowRadius) % 2 == 0)
                    {
                        //如果是偶数
                        middleValue = Convert.ToByte((windowPixelColorList[windowPixelColorList.Count / 2] + windowPixelColorList[windowPixelColorList.Count / 2 - 1]) / 2);
                    }
                    else
                    {
                        //如果是奇数
                        middleValue = windowPixelColorList[(windowPixelColorList.Count - 1) / 2];
                    }
                    //设置为中值
                    lightArray[i, j] = middleValue;
                }
            }
            return lightArray;
        }

    }
}
