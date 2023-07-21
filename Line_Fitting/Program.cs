using OpenCvSharp;
using System;

internal class Program
{
    static void Main()
    {
        //相对路径是以exe文件为根目录的，一般exe存放在\bin\Debug\net6.0中
        string inputPath = @"..\..\..\image.png";
        string inputPath2 = @"..\..\..\pad.png";
        string outputPath = @"..\..\..\image_out_gray.png";
        string outputPath2 = @"..\..\..\image_out.png";

        Mat originImage = Cv2.ImRead(inputPath, ImreadModes.Color);
        Mat originImage2 = Cv2.ImRead(inputPath2, ImreadModes.Color);
        Mat grayImage = ImageUtils.ConvertToGray(originImage);
        //创建窗口进行显示
        Cv2.NamedWindow("灰度图像", WindowFlags.Normal);
        Cv2.ImShow("灰度图像", grayImage);
        Cv2.WaitKey(0); //显示等待
        Cv2.DestroyAllWindows();
        Cv2.ImWrite(outputPath, grayImage);   //保存灰度图像

        List<Point> edgePixel = ImageUtils.GetPixelCoordinate(grayImage);
        foreach (Point point in edgePixel)  //打印所有的边缘像素
        {
            Console.Write(point+" ");
        }
        Console.WriteLine();

        ImageUtils.FitLine(originImage2, edgePixel);
        Cv2.NamedWindow("拟合图像", WindowFlags.Normal);
        Cv2.ImShow("拟合图像", originImage2);
        Cv2.ImWrite(outputPath2, originImage2);
        Cv2.WaitKey(0);
    }
}

/// <summary>
/// 存放图像处理的方法
/// </summary>
internal static class ImageUtils
{
    /// <summary>
    /// 灰度转换
    /// </summary>
    public static Mat ConvertToGray(Mat originImage)
    {
        Mat grayImage = new Mat();
        Cv2.CvtColor(originImage, grayImage, ColorConversionCodes.BGR2GRAY);
        return grayImage;
    }

    /// <summary>
    /// 获取像素坐标
    /// </summary>
    public static List<Point> GetPixelCoordinate(Mat image)
    {
        List<Point> coordinate = new List<Point>();
        for (int y = 0; y < image.Rows; y++)
        {
            for (int x = 0; x < image.Cols; x++)
            {
                if (image.Get<byte>(y, x) > 128)
                {
                    coordinate.Add(new Point(x, y));
                }
            }
        }
        return coordinate;
    }

    /// <summary>
    /// 拟合直线并画线
    /// </summary>
    public static void FitLine(Mat originImage, List<Point> coordinate) //变量1是原始图像，为了在上面画线；变量2是边缘坐标
    {
        //使用ransac算法去除噪点
        int numIterations = 10; // 迭代次数
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
            if ((p2.X - p1.X) == 0)     //判断分母是否为0
            {
                continue;
            }
            double m0 = (p2.Y - p1.Y) / (p2.X - p1.X);
            double b0 = p1.Y - m0 * p1.X;

            // 计算内点个数
            int inliers = 0;
            foreach (var point in coordinate)
            {
                double distance = Math.Abs(point.Y - (m0 * point.X + b0));

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
            double distance = Math.Abs(point.Y - (bestM * point.X + bestB));
            if (distance < thresholdDistance)
            {
                coordinate2.Add(point);
            }
        }
        //打印去噪后的像素
        Console.WriteLine("Ransac去噪后剩余坐标：");
        foreach (Point point in coordinate2)
        {
            Console.Write(point+" ");
        }

        //将Point转换为数组
        int n = coordinate2.Count;
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
        Cv2.Line(originImage, pt1, pt2, new Scalar(255, 0, 0), 1);  //执行之后，originImage的值发生改变
    }
}