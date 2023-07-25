//@"..\\..\\..\\B.bmp"
//@"..\\..\\..\\B_cr.png"


using System;
using OpenCvSharp;

class Program
{
    static void Main(string[] args)
    {
        // 加载源图像和模板图像
        Mat sourceImage = new Mat(@"..\\..\\..\\B.bmp", ImreadModes.Color);
        Mat templateImage = new Mat(@"..\\..\\..\\B_cr.bmp", ImreadModes.Color);

        // 模板匹配
        Mat resultImage = new Mat();
        Cv2.MatchTemplate(sourceImage, templateImage, resultImage, TemplateMatchModes.CCoeffNormed);
        
        // 找到最佳匹配位置
        double minVal, maxVal;
        OpenCvSharp.Point minLoc, maxLoc;
        Cv2.MinMaxLoc(resultImage, out minVal, out maxVal, out minLoc, out maxLoc);

        // 在最佳匹配位置画出矩形
        OpenCvSharp.Point matchLoc = maxLoc;
        Cv2.Rectangle(sourceImage, matchLoc, new OpenCvSharp.Point(matchLoc.X + templateImage.Cols, matchLoc.Y + templateImage.Rows), Scalar.Red, 1);

        // 创建窗口显示结果
        Cv2.NamedWindow("Result", WindowFlags.Normal);
        Cv2.ImShow("Result", sourceImage);
        Cv2.WaitKey(0);
        Cv2.DestroyAllWindows();
    }
}
