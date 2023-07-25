// @"..\\..\\..\\B.bmp"
// @"..\\..\\..\\B_cr.bmp"
// 归一化的差值平方和算法

using System;
using OpenCvSharp;

class Program
{
    static void Main(string[] args)
    {
        // 加载原图与模板
        Mat sourceImage = new Mat(@"..\\..\\..\\B.bmp", ImreadModes.Color);
        Mat templateImage = new Mat(@"..\\..\\..\\B_cr.bmp", ImreadModes.Color);

        // 获取原图与模板的规格
        int sourceWidth = sourceImage.Width;
        int sourceHeight = sourceImage.Height;
        int templateWidth = templateImage.Width;
        int templateHeight = templateImage.Height;

        // 创建空图来存储匹配结果
        Mat resultImage = new Mat(sourceHeight - templateHeight + 1, sourceWidth - templateWidth + 1, MatType.CV_32FC1);

        // 计算
        for (int y = 0; y < resultImage.Rows; y++)
        {
            for (int x = 0; x < resultImage.Cols; x++)
            {
                float sumSquaredDiffs = 0;

                for (int ty = 0; ty < templateHeight; ty++)
                {
                    for (int tx = 0; tx < templateWidth; tx++)
                    {
                        float diff = (sourceImage.At<float>(y + ty, x + tx) - templateImage.At<float>(ty, tx));
                        sumSquaredDiffs += diff * diff;
                    }
                }

                resultImage.Set(y, x, sumSquaredDiffs);
            }
        }
        Cv2.Normalize(resultImage, resultImage, 0, 1, NormTypes.MinMax);

        // 查找最匹配区域
        double minValue, maxValue;
        OpenCvSharp.Point minLoc, maxLoc;
        Cv2.MinMaxLoc(resultImage, out minValue, out maxValue, out minLoc, out maxLoc);

        // 用矩形圈出最匹配区域
        Cv2.Rectangle(sourceImage, maxLoc, new OpenCvSharp.Point(maxLoc.X + templateWidth, maxLoc.Y + templateHeight), Scalar.Red, 2);

        // 显示
        Cv2.NamedWindow("Result", WindowFlags.Normal);
        Cv2.ImShow("Result", sourceImage);
        Cv2.WaitKey(0);
        Cv2.DestroyAllWindows();
    }
}
