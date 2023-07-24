//@"..\\..\\..\\image_template.png"


using System;
using OpenCvSharp;

namespace SimpleTemplateMatching
{
    class Program
    {
        static void Main(string[] args)
        {
            // 图像文件路径和模板文件路径
            string imagePath = @"..\\..\\..\\image.png";
            string templatePath = @"..\\..\\..\\image_template.png";

            // 读取图像和模板
            Mat image = Cv2.ImRead(imagePath, ImreadModes.Color);
            Mat template = Cv2.ImRead(templatePath, ImreadModes.Color);

            // 将图像和模板转换为灰度图像
            Mat grayImage = new Mat();
            Mat grayTemplate = new Mat();
            Cv2.CvtColor(image, grayImage, ColorConversionCodes.BGR2GRAY);
            Cv2.CvtColor(template, grayTemplate, ColorConversionCodes.BGR2GRAY);

            // 获取图像和模板的宽度和高度
            int imageWidth = grayImage.Width;
            int imageHeight = grayImage.Height;
            int templateWidth = grayTemplate.Width;
            int templateHeight = grayTemplate.Height;

            // 创建结果矩阵
            int resultWidth = imageWidth - templateWidth + 1;
            int resultHeight = imageHeight - templateHeight + 1;
            Mat result = new Mat(new Size(resultWidth, resultHeight), MatType.CV_32FC1);

            // 执行模板匹配
            for (int y = 0; y < resultHeight; y++)
            {
                for (int x = 0; x < resultWidth; x++)
                {
                    double sum = 0;

                    for (int j = 0; j < templateHeight; j++)
                    {
                        for (int i = 0; i < templateWidth; i++)
                        {
                            double diff = grayImage.At<byte>(y + j, x + i) - grayTemplate.At<byte>(j, i);
                            sum += diff * diff;
                        }
                    }

                    result.At<float>(y, x) = (float)sum;
                }
            }

            // 获取匹配结果中的最小值和位置
            double minVal, maxVal;
            OpenCvSharp.Point minLoc, maxLoc;
            Cv2.MinMaxLoc(result, out minVal, out maxVal, out minLoc, out maxLoc);

            // 绘制矩形框标记匹配位置
            OpenCvSharp.Point matchLoc = maxLoc;
            OpenCvSharp.Point bottomRight = new OpenCvSharp.Point(matchLoc.X + templateWidth, matchLoc.Y + templateHeight);
            Cv2.Rectangle(image, matchLoc, bottomRight, Scalar.Red, 2);

            // 显示结果
            using (new Window("Original Image", image))
            using (new Window("Template", template))
            using (new Window("Matching Result", result))
            {
                Cv2.WaitKey(0);
            }
        }
    }
}

