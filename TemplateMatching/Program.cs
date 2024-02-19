// 归一化的差值平方和算法

using System;
using System.Drawing;
using System.Linq.Expressions;
using OpenCvSharp;

namespace TemplateMatchingExample;

static class Program
{
    static void Main(string[] args)
    {
        // 读取待匹配的图像和模板图像
        Mat image = Cv2.ImRead("..\\..\\..\\connector.png", ImreadModes.Color);
        Mat template = Cv2.ImRead("..\\..\\..\\connector_pattern.png", ImreadModes.Color);

        // 创建一个用于存储匹配结果的矩阵
        Mat result = new Mat();

        // 使用模板匹配方法，这里使用归一化相关系数匹配法
        Cv2.MatchTemplate(image, template, result, TemplateMatchModes.CCoeffNormed);

        // 查找匹配度最高的区域
        double minVal, maxVal;
        OpenCvSharp.Point minLoc, maxLoc;
        Cv2.MinMaxLoc(result, out minVal, out maxVal, out minLoc, out maxLoc);

        // 设置匹配程度阈值
        double threshold = 0.9;
        List<Rectangle> matchRectangles = new List<Rectangle>();
        for (int j = 0; j < result.Rows; j++)
        {
            for (int i = 0; i < result.Cols; i++)
            {
                var matchValue = result.At<float>(j, i);
                if (matchValue > threshold)
                {
                    Console.WriteLine($"{matchValue}");
                    Rectangle matchRectangle = new Rectangle(i, j, template.Width, template.Height);
                    matchRectangles.Add(matchRectangle);
                     Cv2.Rectangle(image, new OpenCvSharp.Point(matchRectangle.X, matchRectangle.Y), new OpenCvSharp.Point(matchRectangle.X + template.Width, matchRectangle.Y + template.Height), new Scalar(0, 255, 255), 1);
                }
            }
        }

        // 在原图像上绘制匹配区域的矩形框
        Rectangle matchRect = new Rectangle((int)minLoc.X, (int)minLoc.Y, template.Width, template.Height);
        Cv2.Rectangle(image, maxLoc, new OpenCvSharp.Point(maxLoc.X + template.Width, maxLoc.Y + template.Height), new Scalar(0, 255, 0), 1);

        // 显示匹配结果
        Console.WriteLine($"其中最大匹配值：{maxVal}");

        Cv2.ImShow("Matched Template", image);
        Cv2.WaitKey(0);
        Cv2.DestroyAllWindows();

        // 释放资源
        image.Dispose();
        template.Dispose();
        result.Dispose();
    }
}