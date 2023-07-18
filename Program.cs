﻿using System;
using OpenCvSharp;

class Program
{
    static void Main()
    {
        string inputPath = "D:/CSharpProject/ConsoleApp0706/直线拟合_OpenCv/image.png";
        string inputPath2 = "D:/CSharpProject/ConsoleApp0706/直线拟合_OpenCv/pad.png";
        string outputPath = "D:/CSharpProject/ConsoleApp0706/直线拟合_OpenCv/image_out.png";
        string outputPath2 = "D:/CSharpProject/ConsoleApp0706/直线拟合_OpenCv/image_out2.png";

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
        foreach (Point point in edgePixel)
        {
            Console.WriteLine(point);
        }
        ImageUtils.FitLine(originImage2, edgePixel);
        Cv2.NamedWindow("拟合图像", WindowFlags.Normal);
        Cv2.ImShow("拟合图像", originImage2);
        Cv2.ImWrite(outputPath2, originImage2);
        Cv2.WaitKey(0);
    }
}

static class ImageUtils //存放图像处理的方法
{
    public static Mat ConvertToGray(Mat originImage)    //灰度转换
    {
        Mat grayImage = new Mat();
        Cv2.CvtColor(originImage, grayImage, ColorConversionCodes.BGR2GRAY);
        return grayImage;
    }
    public static List<Point> GetPixelCoordinate(Mat image)
    {
        List<Point> coordinate = new List<Point>();
        for (int y=0; y<image.Rows; y++)
        {
            for (int x=0; x<image.Cols; x++)
            {
                if (image.Get<byte>(y,x)>128)
                {
                    coordinate.Add(new Point(x, y));
                }
            }
        }
        return coordinate;
    }
    public static void FitLine(Mat originImage, List<Point> coordinate) //变量1是原始图像，为了在上面画线；变量2是边缘坐标
    {
        //将Point转换为数组
        int n = coordinate.Count; //Point的数量
        double[] x = new double[n];
        double[] y = new double[n];
        for (int i=0; i<n; i++)
        {
            x[i] = coordinate[i].X;
            y[i] = coordinate[i].Y;
        }

        // 拟合的直线有概率是斜率无穷大，故把x，y调换进行计算，从而算出画线所需的两个点
        double x_avg = x.Sum() / n;
        double y_avg = y.Sum() / n;
        double sum1 = 0;
        double sum2 = 0;
        for (int i = 0;i<n;i++)
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