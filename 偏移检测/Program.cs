using System;
using OpenCvSharp;


class Program
{
    static void Main()
    {
        Console.WriteLine("Already run");

        //路径设置
        string path_pad = @"..\..\..\后.bmp";

        //读取图像
        Mat originImage = Cv2.ImRead(path_pad, ImreadModes.Color);
        //Cv2.ImShow("1原图", originImage);

        //灰度转换
        Mat grayImage = new Mat();
        Cv2.CvtColor(originImage, grayImage, ColorConversionCodes.BGR2GRAY);
        Cv2.ImShow("2灰度图", grayImage);

        //梯度计算
        Mat gradientImage = ImageUtils.CalGradient(grayImage, 70);
        Cv2.ImShow("3梯度图", gradientImage);

        //获取坐标
        List<Point> edgePixel = ImageUtils.GetCoordinate(gradientImage);
        //ransac去噪
        List<Point> edgePixel2 = ImageUtils.RansacPoint(edgePixel);

        //直线拟合
        ImageUtils.FitLine(edgePixel2, originImage);
        Cv2.ImShow("4直线拟合", originImage);


        Cv2.WaitKey(); Cv2.DestroyAllWindows();
    }
}


static class ImageUtils
{
    /*
    public static Mat CalGradient(Mat grayImage, double threshold)
    {
        Mat gradientX = new Mat();
        Cv2.Sobel(grayImage, gradientX, MatType.CV_32F, 1, 0, ksize: 3);
        //Cv2.ConvertScaleAbs(gradientX, gradientX, 2, 0);

        Mat thresholdedGradientX = new Mat();
        Cv2.Threshold(gradientX, thresholdedGradientX, threshold, 255,ThresholdTypes.Binary);

        return thresholdedGradientX;
    }
    */
    public static Mat CalGradient(Mat grayImage, double threshold)
    {
        Mat kernel = new Mat(3, 3, MatType.CV_32F, new float[] { 1, 0, -1, 1, 0, -1, 1, 0, -1 });

        // 对图像进行卷积操作，计算梯度
        Mat gradientX = new Mat();
        Cv2.Filter2D(grayImage, gradientX, MatType.CV_32F, kernel);

        // 将结果转换为正数，并转换为8位图像
        //Cv2.ConvertScaleAbs(gradientX, gradientX);
        gradientX.ConvertTo(gradientX, MatType.CV_8UC1);

        //设置阈值
        Mat thresholdGradientX = new Mat();
        Cv2.Threshold(gradientX, thresholdGradientX, threshold, 255, ThresholdTypes.Binary);    //超过阈值则为255

        return thresholdGradientX;
    }

    //获取像素坐标
    public static List<Point> GetCoordinate(Mat image)
    {
        List<Point> coordinate = new List<Point>();
        for (int y = 0; y < image.Rows; y++)
        {
            for (int x = 0; x < image.Cols; x++)
            {
                if (image.Get<byte>(y, x) == 255)
                {
                    coordinate.Add(new Point(x, y));
                }
            }
        }
        return coordinate;
    }

    //使用ransac算法去除噪点
    public static List<Point> RansacPoint(List<Point> coordinate)
    {
        int numIterations = 100; // 迭代次数
        double thresholdDistance = 2.0; //内点的阈值距离
        int numPointsInLine = 2; // 用于拟合直线的最小内点数
        double bestM = 0, bestB = 0;
        int maxInliers = 0;

        Random rand = new Random();
        for (int i = 0; i < numIterations; i++)
        {
            // 随机选择两个点
            int index1 = rand.Next(coordinate.Count);
            int index2 = rand.Next(coordinate.Count);
            if (index1 == index2)
                continue;
            Point p1 = coordinate[index1];
            Point p2 = coordinate[index2];

            // 计算直线参数
            if ((p2.Y - p1.Y) == 0)     //判断分母是否为0
            {
                continue;
            }
            double m0 = (p2.X - p1.X) / (p2.Y - p1.Y);
            double b0 = p1.X - m0 * p1.Y;

            // 计算内点个数
            int inliers = 0;
            foreach (var point in coordinate)
            {
                double distance = Math.Abs(point.X - (m0 * point.Y + b0));

                if (distance < thresholdDistance)
                    inliers++;
            }
            if (inliers > maxInliers && inliers >= numPointsInLine)
            {
                maxInliers = inliers;
                bestM = m0;
                bestB = b0;
            }
        }

        //创建新list保存合格点

        List<Point> coordinate2 = new List<Point>();
        foreach (var point in coordinate)
        {
            double distance = Math.Abs(point.X - (bestM * point.Y + bestB));
            if (distance < thresholdDistance)
            {
                coordinate2.Add(point);
            }
        }


        //打印所有的边缘像素
        /*
        foreach (Point point in coordinate2)
        {
            Console.Write(point + " ");
        }
        Console.WriteLine();
        */

        //把点画出来
        Mat image = new Mat(new Size(335, 570), MatType.CV_8UC3, Scalar.All(255)); // 白色背景图像
        Scalar pointColor = new Scalar(0, 0, 255); // 红色画笔，可以根据需要更改颜色
        foreach (var point in coordinate2)
        {
            Cv2.Circle(image, point, 1, pointColor, -1);
        }
        Cv2.ImShow("Ransac筛选后的点", image);

        return coordinate2;
    }

    //绘制直线
    public static void FitLine(List<Point> coordinate, Mat originImage)
    {
        //将Point转换为数组
        int n = coordinate.Count;
        double[] x = new double[n];
        double[] y = new double[n];
        for (int i = 0; i < n; i++)
        {
            x[i] = coordinate[i].X;
            y[i] = coordinate[i].Y;
        }

        // 拟合的直线有概率是斜率无穷大，故把x，y调换进行计算，从而算出画线所需的两个点
        double x_avg = x.Sum() / n;
        double y_avg = y.Sum() / n;
        double sum1 = 0;
        double sum2 = 0;
        for (int i = 0; i < n; i++)
        {
            sum1 += ((y[i] - y_avg) - (x[i] - x_avg));
            sum2 += ((y[i] - y_avg) * (y[i] - y_avg));
        }
        double m = sum1 / sum2;
        double b = x_avg - m * y_avg;

        //绘制直线
        int y1 = 0;
        int x1 = (int)(m * y1 + b);
        int y2 = originImage.Rows - 1;
        int x2 = (int)(m * y2 + b);
        Point pt1 = new Point(x1, y1);
        Point pt2 = new Point(x2, y2);

        //在原图上画线
        Cv2.Line(originImage, pt1, pt2, new Scalar(0, 255, 0), 2);  //执行之后，originImage的值发生改变
    }
}