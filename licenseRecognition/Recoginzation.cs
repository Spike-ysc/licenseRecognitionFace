using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using System.Drawing; 
using System.Drawing.Imaging;

namespace licenseRecognition
{
    class Recoginzation
    {
        #region 车牌定位算法
        public static Bitmap licensePlateLocation(Bitmap m_Bitmap, Bitmap always_Bitmap, float[,] m)
        {
            Bitmap c_Bitmap = null;
            if (m_Bitmap != null)
            {
                BitmapData bmData = m_Bitmap.LockBits(new Rectangle(0, 0, m_Bitmap.Width, m_Bitmap.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

                int nWidth = m_Bitmap.Width;
                int nHeight = m_Bitmap.Height;
                int div = 7;
                int lv = 11;
                int pWR, pWL, pWHL, pWHR, pWH;
                long sM = 0;
                int ccm = 0;
                int Wmin = 1;
                int Wmax = 9;
                int Bmin = 1;
                int Bmax = 5;
                bool getStart;
                bool[] lineLabel = new bool[(int)(nHeight / div) + 1];
                double[] sumC = new double[(int)(nHeight / div) + 1];
                int[,] countMatch = new int[(int)(nHeight / div) + 1, (int)(nWidth / lv) + 1];
                int[,] mark = new int[(int)(nHeight / div) + 1, nWidth];
                unsafe
                {
                    for (int i = 0; i < (int)(nHeight / div) + 1; i++)
                    {
                        for (int j = 0; j < (int)(nWidth / lv) + 1; j++)
                        {
                            countMatch[i, j] = 0;
                        }
                    }
                    int stride = bmData.Stride;
                    System.IntPtr Scan0 = bmData.Scan0;
                    byte* p = (byte*)(void*)Scan0;
                    byte* pp;
                    for (int y = 2 * div; y < nHeight - 2 * div;)
                    {
                        for (int j = 0; j < nWidth; j++)
                        {
                            mark[y / div, j] = 0;
                        }
                        for (int i = 0; i < div; i++)
                        {
                            getStart = true;
                            for (pWR = pWL = pWHL = pWHR = pWH = 4; pWH < nWidth - 1; pWH++)
                            {
                                if (getStart) //标记每行的第一个白点
                                {
                                    if (m[y + i, pWH] > 0)
                                    {
                                        getStart = false;
                                        pWR = pWL = pWHL = pWHR = pWH;
                                    }
                                    else
                                        continue;
                                }
                                if (pWR - pWL > nWidth / 3 || pWHL - pWR > nWidth / 3 || pWHR - pWHL > nWidth / 3)
                                {
                                    goto L;
                                }
                                if (m[y + i, pWH - 1] > 0 && m[y + i, pWH] <= 0)//白－－>黑 
                                {
                                    pWHR = pWH - 1;
                                    if (pWL != pWHL)
                                    {
                                        if ((Wmin <= (pWR - pWL) && (pWR - pWL) <= Wmax) || (Bmin <= (pWHL - pWR - 1) && (pWHL - pWR - 1) <= Bmax) || (Wmin <= (pWHR - pWHL) && (pWHR - pWHL) <= Wmax))
                                        {
                                            if (-pWL + pWHR < 30) //记录该点 if (pWR-pWL+pWHR-pWHL<11) 
                                            {
                                                double rate1 = Wmax / (Math.Abs((pWR - pWL) - (Wmax - Wmin)) / 2 + 1);
                                                double rate2 = Wmax / (Math.Abs((pWHR - pWHL) - (Wmax - Wmin)) / 2 + 1);
                                                double rate3 = Bmax * 3 / (pWHL - pWR);
                                                mark[y / div, pWL + (pWR - pWL) / 2] += (int)(rate3 + rate2 + rate2);
                                            }
                                        }
                                        if (pWR - pWL > 2 * lv)
                                        {
                                            for (int t = pWL + lv / 2; t < pWR - lv / 2; t += lv) //连续白（或）宽于一个字符宽度
                                            {
                                                countMatch[y / div, t / lv] = -1;
                                            }
                                            //countMatch[y/div,pWR/lv]=-1;
                                        }
                                        if (pWHL - pWR - 1 > 2 * lv)
                                        {
                                            for (int t = pWR + lv / 2; t < pWHL - lv / 2; t += lv)
                                            {
                                                countMatch[y / div, t / lv] -= 1;
                                            }
                                            //countMatch[y/div,pWHL/lv]=-1;
                                        }
                                        if (pWHR - pWHL > 2 * lv)
                                        {
                                            for (int t = pWHL + lv / 2; t < pWHR - lv / 2; t += lv)
                                            {
                                                countMatch[y / div, t / lv] -= 1;
                                            }
                                            //countMatch[y/div,pWHR/lv]=-1;
                                        }
                                    }
                                    pWR = pWHR;
                                    pWL = pWHL;
                                }
                                else if (m[y + i, pWH - 1] <= 0 && m[y + i, pWH] > 0)//黑－－>白
                                {
                                    pWHL = pWH;
                                }
                            }
                        }
                        L: y += div;
                    }

                    //////////////////////////////////////////////////////////////////////////
                    //去除噪音
                    //////////////////////////////////////////////////////////////////////////
                    ///
                    //基与特征点水平间隔的去噪
                    int toCheck = -1;
                    foreach (int i in sumC)
                    {
                        sumC[i] = 0;
                    }
                    sM = 0;
                    ccm = 0;
                    //累计连续的特征点
                    for (int i = 2; i < (int)(nHeight / div) - 1; i++)
                    {
                        toCheck = -1;
                        lineLabel[i] = false;
                        //sumLX=0;
                        pWL = pWR = 1;
                        getStart = true;
                        for (int j = 1; j < nWidth; j++)
                        {
                            //标记每行的第一个白点
                            if (getStart)
                            {
                                if (m[i, j] > 0)
                                {
                                    getStart = false;
                                    pWR = pWL = j;
                                }
                                else
                                    continue;
                            }
                            ///*
                            if (mark[i, j] > 0)
                            {
                                if (toCheck == -1)
                                {
                                    toCheck = j;
                                    continue;
                                }
                                else
                                {
                                    if (j - toCheck <= 1)
                                    {//////////////////////////////////////////////////////////////////////////
                                        if (countMatch[i, j / lv] >= 0)
                                        {
                                            countMatch[i, j / lv] += (mark[i, toCheck] + mark[i, j]);//两个点相互匹配，累加2
                                        }
                                        toCheck = -1;
                                        //lineLabel[i]=true;
                                        continue;
                                    }
                                    else
                                    {
                                        //mark[i,toCheck]-=(int)(div*0.8);//除去该特征点
                                        if (mark[i, toCheck] < (div * 0.7))
                                        {
                                            mark[i, toCheck] = 0;
                                        }
                                        else
                                        {
                                            countMatch[i, j / lv] += 2 * mark[i, toCheck];
                                        }

                                        toCheck = j;
                                        continue;
                                    }
                                }
                            }
                        }

                    }

                    //阀值化		
                    sM = 0;
                    ccm = 0;
                    int va = (int)(lv * div / 3);
                    int[] countL = new int[(int)(nHeight / div) + 1];

                    for (int i = 0; i < (int)(nHeight / div) + 1; i++)
                    {
                        bool ok;
                        ok = false;
                        countL[i] = 0;
                        lineLabel[i] = false;

                        for (int j = 0; j < (int)(nWidth / lv) + 1; ++j)
                        {
                            //图像周边特征点为零
                            if (i == 0 || i == (int)(nHeight / div) || j == 0 || j == (int)(nWidth / lv))
                            {
                                countMatch[i, j] = 0;
                                continue;
                            }

                            if (countMatch[i, j] > va)
                            {//阀值去噪音 
                                if ((countMatch[i, j - 1] <= va && countMatch[i, j + 1] <= va) ||//去除孤立点(水平）
                                    (countMatch[i - 1, j] <= va && (countMatch[i + 1, j] <= va || (countMatch[i + 1, j - 1] <= va && countMatch[i + 1, j + 1] <= va))))//去除孤立点（垂直）			 
                                {
                                    countMatch[i, j] = 0;
                                }
                                else
                                {
                                    countL[i] += countMatch[i, j];
                                    ok = true;
                                }
                            }
                            else
                                countMatch[i, j] = 0;
                        }

                        if (ok)
                        {
                            lineLabel[i] = true;
                            sM += countL[i];
                            ccm++;
                        }
                    }

                    //去除上半部分大面积的噪音

                    int v1 = 0, v2 = 0;
                    int vm1 = 0, vm2 = 0;
                    int maxL = 0, cv = 0;
                    for (int i = 1; i < (int)(nHeight / div) + 1; i++)
                    {
                        if (lineLabel[i] == true && lineLabel[i - 1] == false)
                        {
                            v1 = i;
                            v2 = i;
                        }
                        else if (lineLabel[i] == false && lineLabel[i - 1] == true)
                        {
                            v2 = i;
                            cv++;
                            if (maxL < v2 - v1)
                            {
                                vm1 = v1;
                                vm2 = v2;
                                maxL = v2 - v1;
                            }
                        }
                    }
                    if (cv > 1 && vm2 - vm1 > 5 && vm1 + (vm2 - vm1 + 1) / 2 < (nHeight / div) / 3 || vm2 - vm1 > nHeight / div / 2)
                    {
                        for (int k = vm1; k <= vm2; k++)
                        {
                            lineLabel[k] = false;
                        }
                    }
                    int p1 = 0, p2 = 0;
                    for (int i = 0; i < (int)(nHeight / div) + 1; i++)
                    {
                        if (lineLabel[i] == true)
                        {
                            p1 = 0; p2 = 0;
                            bool ok = false;
                            for (int j = 1; j < (int)(nWidth / lv) + 1; j++)
                            {
                                if (countMatch[i, j - 1] == 0 && countMatch[i, j] > 0)
                                {
                                    p1 = p2 = j;
                                }
                                if (countMatch[i, j - 1] > 0 && countMatch[i, j] == 0)
                                {
                                    p2 = j - 1;
                                    if (p2 - p1 > 0)
                                    {
                                        ok = true;
                                    }
                                    else
                                    {
                                        p2 = p1 = 0;
                                        countMatch[i, j - 1] = 0;
                                    }
                                }
                            }
                            if (!ok && p2 == 0 && p1 == 0)
                            {
                                lineLabel[i] = false;
                            }
                        }
                    }


                    //////////////////////////////////////////////////////////////////////////
                    //使用2×6矩阵粗定位
                    //////////////////////////////////////////////////////////////////////////

                    int lLenght = 5, vLenght = 1;
                    int maxAverage = 0;
                    int maxX1 = 0;
                    int maxY1 = 0;
                    for (int i = 0; i < (int)(nHeight / div) + 1; i++)
                    {
                        if (lineLabel[i] == true && lineLabel[i + 1] == true)
                        {
                            for (int j = 0; j < (int)(nWidth / lv) - lLenght; ++j)
                            {
                                int average = countMatch[i, j] + countMatch[i, j + 1] + countMatch[i, j + 2]
                                    + countMatch[i, j + 3] + countMatch[i, j + 4] + countMatch[i, j + 5]// +countMatch[i,j+6]
                                    + countMatch[i + 1, j] + countMatch[i + 1, j + 1] + countMatch[i + 1, j + 2]
                                    + countMatch[i + 1, j + 3] + countMatch[i + 1, j + 4] + countMatch[i + 1, j + 5];// +countMatch[i+1,j+6] ;
                                average = average / (lLenght + 1) / (vLenght + 1);
                                if (maxAverage < average)
                                {
                                    maxAverage = average;
                                    maxX1 = i;
                                    maxY1 = j;
                                }
                            }
                        }
                    }





                    bool jx1 = true, jx2 = true;
                    int x1 = 0, x2 = 0;
                    for (int j = 0; jx1 || jx2; j++)
                    {
                        if (jx1 && lineLabel[maxX1 - j] == false)
                        {
                            jx1 = false;
                            x1 = maxX1 - j;
                        }
                        if (jx2 && lineLabel[maxX1 + j] == false)
                        {
                            jx2 = false;
                            x2 = maxX1 + j;
                        }
                    }
                    for (int i = 0; i < x1; i++)
                    {
                        lineLabel[i] = false;
                    }
                    for (int i = x2; i < (int)(nHeight / div) + 1; i++)
                    {
                        lineLabel[i] = false;
                    }



                    //////////////////////////////////////////////////////////////////////////
                    //寻找车牌的四边
                    //////////////////////////////////////////////////////////////////////////

                    // 位置调整	
                    int lKZValve = (int)(maxAverage / 3);
                    int vKZValve = (int)(maxAverage / 2.5);
                    //int kz1=0,kz2=0;
                    int pX1 = 0, pX2 = 0, pX3 = 0, pX4 = 0, pY1 = 0, pY2 = 0, pY3 = 0, pY4 = 0; //用于搜索边框的范围
                    int pXU = 0, pXD = 0, pYL = 0, pYR = 0, pXM = 0, pYM = 0;





                    //除去两边噪音
                    bool l = true, r = true;
                    pY1 = maxY1;
                    pY4 = maxY1 + lLenght;

                    for (int j = 1; l || r; j++)
                    {
                        if (maxY1 - j < 0 && l)
                        {
                            l = false;
                            //pY1=0;
                        }
                        else if (l && countMatch[maxX1, maxY1 - j] < vKZValve && countMatch[maxX1 + 1, maxY1 - j] < vKZValve)
                        {
                            if (maxY1 - j - 2 >= 0 && countMatch[maxX1, maxY1 - j - 2] < vKZValve && countMatch[maxX1 + 1, maxY1 - j - 2] < vKZValve)
                            {
                                l = false;
                                pY1 = maxY1 - j + 1;
                            }

                        }
                        if (maxY1 + lLenght + j > (int)(nWidth / lv) && r)
                        {
                            r = false;
                            pY4 = (int)(nWidth / lv);
                        }
                        else if (r && countMatch[maxX1, maxY1 + lLenght + j] < vKZValve && countMatch[maxX1 + 1, maxY1 + lLenght + j] < vKZValve)
                        {
                            if (maxY1 + lLenght + j + 2 < (int)(nWidth / lv) + 1 && countMatch[maxX1, maxY1 + lLenght + j + 2] < vKZValve && countMatch[maxX1 + 1, maxY1 + lLenght + j + 2] < vKZValve)
                            {

                                r = false;
                                pY4 = maxY1 + lLenght + j - 1;
                            }
                        }
                    }
                    pY2 = (pY1 + 1) * lv;
                    pY3 = (pY4 - 1) * lv;
                    // 进一步去除不必要的边线
                    bool u = true, d = true;
                    pX1 = maxX1;
                    pX4 = maxX1 + vLenght;
                    while (u || d)
                    {
                        if (u && pX1 - 1 < 0)
                        {
                            u = false;
                            //pX1=0;
                        }
                        else if (u && lineLabel[pX1 - 1])
                        {
                            bool ok = false;
                            for (int j = pY1; j <= pY4; j++)
                            {
                                if (pX1 - 1 >= 0 && countMatch[pX1 - 1, j] > lKZValve)
                                {
                                    ok = true;
                                    pX1--;
                                    break;
                                }
                            }
                            if (!ok)
                            {
                                u = false;
                            }
                        }
                        else
                            u = false;

                        if (d && pX4 + 1 > (int)(nHeight / div))
                        {
                            d = false;
                        }
                        else if (d && lineLabel[pX4 + 1])
                        {
                            bool ok = false;
                            for (int j = pY1; j <= pY4; j++)
                            {
                                if (pX4 + 1 < (int)(nHeight / div) + 1 && countMatch[pX4 + 1, j] > lKZValve)
                                {
                                    ok = true;
                                    pX4++;
                                    break;
                                }
                            }
                            if (!ok)
                            {
                                d = false;
                            }
                        }
                        else
                            d = false;
                    }

                    pXM = pX1 * div + (pX4 - pX1) / 2 * div;
                    pYM = pY1 * lv + (pY4 - pY1) / 2 * lv;
                    //maxX1=x1;
                    vLenght = x2 - x1;
                    //水平再调整
                    l = true; r = true;
                    while (l || r)
                    {
                        if (pY1 - 1 < 0 && l)
                        {
                            l = false;
                        }
                        else if (l)
                        {
                            bool match = false;
                            for (int i = 0; i <= vLenght; i++)
                            {
                                if (countMatch[x1 + i, pY1 - 1] > vKZValve)
                                {
                                    match = true;
                                    break;
                                }
                            }
                            if (!match)
                            {
                                l = false;
                            }
                            else
                                pY1--;


                        }
                        if (pY4 + 1 > (int)(nWidth / lv) && r)
                        {
                            r = false;
                        }
                        else if (r)
                        {
                            bool match = false;
                            for (int i = 0; i <= vLenght; i++)
                            {
                                if (countMatch[x1 + i, pY4 + 1] > vKZValve)
                                {
                                    match = true;
                                    break;
                                }
                            }
                            if (!match)
                            {
                                r = false;

                            }
                            else
                                pY4++;
                        }
                    }

                    for (int i = 0; i < pX1; i++)
                    {
                        lineLabel[i] = false;
                    }
                    for (int i = pX4 + 1; i < (int)(nHeight / div) + 1; i++)
                    {
                        lineLabel[i] = false;
                    }

                    pX2 = x1 * div - div / 2;
                    if (pX2 < 0)
                    {
                        pX2 = 0;
                    }
                    pX3 = x2 * div + div / 2;
                    if (pX3 >= nHeight)
                    {
                        pX3 = nHeight - 1;
                    }
                    pYL = pY1 * lv;//-lv;
                    bool kz = false;
                    for (int i = x1; i <= x2; i++)
                    {
                        if (countMatch[i, pY1] > vKZValve)
                        {
                            kz = true;
                            break;
                        }
                        if (pY1 - 1 >= 0 && countMatch[i, pY1 - 1] > vKZValve)
                        {
                            pYL -= lv;
                            break;
                        }
                    }
                    if (kz)
                    {
                        pYL -= lv / 2;
                    }
                    if (pYL <= 0)
                    {
                        pYL = 0;
                    }
                    pYR = (pY4 + 1) * lv + lv / 2;//+lv;
                    kz = false;
                    for (int i = x1; i <= x2; i++)
                    {
                        if (pY4 + 1 < (int)(nWidth / lv) + 1 && countMatch[i, pY4 + 1] > vKZValve)
                        {
                            kz = true;
                            break;
                        }
                        if (pY4 + 2 < (int)(nWidth / lv) + 1 && countMatch[i, pY4 + 2] > vKZValve)
                        {
                            pYR += lv;
                            break;
                        }
                    }
                    if (kz)
                    {
                        pYR += lv / 2;
                    }
                    if (pYR >= nWidth)
                    {
                        pYR = nWidth - 1;
                    }
                    if (pX4 - pX1 <= 3)
                    {
                        if (pX1 - 1 >= 0)
                        {
                            pXU = (pX1 - 1) * div;
                        }
                        else
                            pXU = 0;
                        if (pX4 + 2 >= (int)(nHeight / div))
                        {
                            pXD = nHeight;
                        }
                        else
                            pXD = (int)((pX4 + 1.5) * div + div);
                    }
                    else if (4 <= pX4 - pX1 && pX4 - pX1 <= 5)
                    {
                        pXU = pX1 * div - div / 2;
                        pXD = (pX4 + 1) * div + div / 2;
                    }
                    else
                        pXU = pX1 * div;
                    pXD = (int)((pX4 + 1.5) * div - div / 2);

                    pXD += div / 2;
                    if (pXD > nHeight - 1)
                    {
                        pXD = nHeight - 1;
                    }
                    pYL -= lv / 2;
                    if (pYL < 0)
                    {
                        pYL = 0;
                    }

                    //调整截取的边缘
                    LR(m, pX2, pX3, pYL, pYR, out pYL, out pYR);
                    UD(m, pXU, pXD, pYL, pYR, out pXU, out pXD);


                    ///////////////////////////////////////////////////////////////////////////
                    //在图像上添加辅助线
                    //////////////////////////////////////////////////////////////////////////	

                    //显示边框
                    p = (byte*)(void*)Scan0;
                    pp = p;
                    for (int i = 0; i < nHeight; i++)
                    {

                        if (i == pXU || i == pXD)
                        {
                            for (int j = pYL; j <= pYR; j++)
                            {
                                pp = p + i * stride + j * 3;
                                pp[2] = 255; pp[0] = pp[1] = 0;
                            }
                        }
                        else if (pXU < i && i < pXD)
                        {

                            pp = p + i * stride + pYL * 3;
                            pp[2] = 255; pp[0] = pp[1] = 0;

                            pp = p + i * stride + pYR * 3;
                            pp[2] = 255; pp[0] = pp[1] = 0;
                        }
                    }

                    //截取的行线显示在图上
                    /*p = (byte *)(void *)Scan0;
                    pp = p;
                    for (int i=0;i<(int)(nHeight/div)-1;i++) 
                    {
                        //画垂线
                        for (int k=0;k<nWidth+1;k+=lv) 
                        {
                            pp=p+(i*div+div/2)*stride+k*3;
                            pp[2]=255;
                        }

                        //在车牌所在水平区域画出横线
                        if (lineLabel[i]) 
                        {
					
                            for(int j=0; j < nWidth; ++j ) 
                            { 
                                pp=p+(i*div+div/2)*stride+j*3;
                                pp[2]=255;
                            }
                        }
                    }  */
                    int ccount;
                    ccount = ccm;
                    ccount = maxAverage;
                    m_Bitmap.UnlockBits(bmData);
                    //maxX = maxX1 * div - pXU;
                    // maxY = maxY1 * lv - pYL;
                    //if (name != null)
                    //{
                    //    Bitmap other_c_Bitmap = (Bitmap)Bitmap.FromFile(name, false);
                    //}
                    //else { 

                    //}
                    Rectangle sourceRectangle = new Rectangle(pYL, pXU, pYR - pYL, pXD - pXU);
                    c_Bitmap = always_Bitmap.Clone(sourceRectangle, PixelFormat.DontCare);
                    //extract_Bitmap_one = other_c_Bitmap.Clone(sourceRectangle, PixelFormat.DontCare);
                    //在内存中处理c_Bitmap，提取数据之后，在原来的图片提取彩色图片。

                }
            }
            if (c_Bitmap == null)
                Console.WriteLine("nulllllllllllllllllllllllllllllllllllllllll");
            return c_Bitmap;

        }
        private static bool LR(float[,] m, int xu, int xd, int yl, int yr, out int pYL, out int pYR)
        {
            int[] projection = new int[yr - yl + 1];
            foreach (int i in projection)
            {
                projection[i] = 0;
            }
            //垂直投影
            for (int i = xu; i <= xd; i++)
            {
                for (int j = yl; j <= yr; j++)
                {
                    if (m[i, j] > 0)
                    {
                        projection[j - yl]++;
                    }
                }
            }
            bool l = true, r = true;
            int temp_yr = yr, temp_yl = yl;
            while (l || r)
            {
                if (temp_yr - temp_yl <= 60)
                {
                    l = r = false;
                }
                if (l && projection[temp_yl - yl] < 5)
                {
                    temp_yl++;
                }
                else
                {
                    l = false;
                }

                if (r && projection[temp_yr - yl] < 5)
                {
                    temp_yr--;
                }
                else
                {
                    r = false;
                }
            }

            pYL = temp_yl;
            pYR = temp_yr;

            return true;
        }



        /*
         * 调整车牌上下位置
         */
        private static bool UD(float[,] m, int pXU, int pXD, int pYL, int pYR, out int xu, out int xd)
        {
            int[] projection = new int[pXD - pXU + 1];
            foreach (int i in projection)
            {
                projection[i] = 0;
            }
            //水平投影
            for (int i = pXU; i <= pXD; i++)
            {
                for (int j = pYL; j <= pYR; j++)
                {
                    if (m[i, j] > 0)
                    {
                        projection[i - pXU]++;
                    }
                }
            }
            bool u = true, d = true;
            int temp_xd = pXD - 1, temp_xu = pXU + 1;
            while (u || d)
            {
                if (temp_xd - temp_xu <= 60)
                {
                    u = d = false;
                }
                if (u && projection[temp_xu - pXU] < 2)
                {
                    temp_xu++;
                }
                else
                {
                    u = false;
                }

                if (d && projection[temp_xd - pXU] < 2)
                {
                    temp_xd--;
                }
                else
                {
                    d = false;
                }
            }
            xu = temp_xu;

            xd = temp_xd;

            return true;
        }




        #endregion
    }
}
