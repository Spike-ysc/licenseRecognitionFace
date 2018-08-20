# licenseRecognitionFace
车牌识别软件



### 车牌识别步骤

图像的载入 -> 图像的灰度化 -> 图像的灰度均衡化 -> 图像的滤波去噪 -> Sobel边缘检测 -> 车牌定位提取 -> 车牌二值化 -> 车牌字符分割 -> 车牌字符识别


### 细节

* 1.车牌灰度化分为三种方法，**加权平均灰度化**[R=G=B=max(WrR+WrG+WrB)/3]、**平均值灰度化**[R=G=B=(R+G+B)/3]、**最大值灰度化**[R=G=B=max(R,G,B)]。
* 2.使用**直方图均衡化**，将图片的直方图改为“均匀”分布直方图，达到增强图片整体对比度的效果。另外对直方图均衡化的方法进行改进可实现更好的效果。
* 3.滤波去噪完成三种方式实现，分别是，**高斯滤波**（使用高斯滤波器模板对图像进行处理）、**中值滤波**（用像素点邻域灰度值的中值来代替该像素点的灰度值）、**均值滤波**（用像素点邻域灰度值的平均值来代替该像素点的灰度值）。
* 4.使用不同的算子对图像进行边缘检测，具体有**Prewitts算子**、**Sobel算子**、**Laplacian算子**、**Roberts算子**、**Laplacian改进算子**。
* 5.车牌定位调用licensePlateLoccation方法。
* 6.车牌二值化三种方法实现，分别是**最大类间方差二值化**、**OSTU算法**、**双峰法**。
* 7.字符切割步骤是先确定车牌上下边界和左右边界，从左到右扫描黑色像素的个数，再分为七个区间，切割图片。
* 8.字符识别是采用**模板匹配**的方式，根据提供的模板和截取图片进行对比，相差最小的为结果。


### 软件部分截图


 #### 1.打开文件
![](https://github.com/Spike-ysc/licenseRecognitionFace/raw/master/软件部分截图/打开文件.png)


#### 2.灰度化
加权平均灰度化|平均值灰度化|最大值灰度化 
-|-|-
![](https://github.com/Spike-ysc/licenseRecognitionFace/raw/master/软件部分截图/加权平均灰度化.jpg)|![](https://github.com/Spike-ysc/licenseRecognitionFace/raw/master/软件部分截图/平均值灰度化.jpg)|![](https://github.com/Spike-ysc/licenseRecognitionFace/raw/master/软件部分截图/最大值灰度化.jpg)


#### 3.直方图均衡化
直方图均衡化|直方图均衡化改进 
-|-
![](https://github.com/Spike-ysc/licenseRecognitionFace/raw/master/软件部分截图/灰度均衡化一般2.jpg)|![](https://github.com/Spike-ysc/licenseRecognitionFace/raw/master/软件部分截图/灰度均衡化改进2.jpg)


#### 4.边缘检测
Prewitts算子|Sobel算子|Laplacian算子|Roberts算子|Laplacian算子改进
-|-|-|-|-
![](https://github.com/Spike-ysc/licenseRecognitionFace/raw/master/软件部分截图/Priwitt.jpg)|![](https://github.com/Spike-ysc/licenseRecognitionFace/raw/master/软件部分截图/sobel.jpg)|![](https://github.com/Spike-ysc/licenseRecognitionFace/raw/master/软件部分截图/拉普拉斯算子.jpg)|![](https://github.com/Spike-ysc/licenseRecognitionFace/raw/master/软件部分截图/robert.jpg)|![](https://github.com/Spike-ysc/licenseRecognitionFace/raw/master/软件部分截图/拉普拉斯算子改进.jpg)

#### 5.车牌二值化
最大类间方差|OSTU算法|双峰法
-|-|-
![](https://github.com/Spike-ysc/licenseRecognitionFace/raw/master/软件部分截图/最大类方差二值化.jpg)|![](https://github.com/Spike-ysc/licenseRecognitionFace/raw/master/软件部分截图/ostu.jpg)|![](https://github.com/Spike-ysc/licenseRecognitionFace/raw/master/软件部分截图/双峰法.jpg)

#### 6.字符识别效果（模板匹配）
![](https://github.com/Spike-ysc/licenseRecognitionFace/raw/master/软件部分截图/字符识别.png)

#### 7.使用openCV完成的图像处理软件界面截图
![](https://github.com/Spike-ysc/licenseRecognitionFace/raw/master/软件部分截图/opencv软件.png)
