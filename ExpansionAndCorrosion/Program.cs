using System;
using OpenCvSharp;

class Program
{
    static void Main()
    {
        string inputPath = @"..\..\..\example.bmp";
        Mat sourceImage = Cv2.ImRead(inputPath, ImreadModes.Color);
        if (sourceImage.Empty())
        {
            Console.WriteLine("未读取到图片");
        }
        Cv2.NamedWindow("原图", WindowFlags.Normal);
        Cv2.ImShow("原图", sourceImage);

        Mat imageExpansion = ImageUtils.ImageExpansion(sourceImage);
        Cv2.NamedWindow("膨胀", WindowFlags.Normal);
        Cv2.ImShow("膨胀", imageExpansion);

        Mat imageErode = ImageUtils.ErodeImage(sourceImage);
        Cv2.NamedWindow("腐蚀", WindowFlags.Normal);
        Cv2.ImShow("腐蚀", imageErode);
        Cv2.WaitKey(0);

    }
}

public static class ImageUtils
{
    //膨胀
    public static Mat ImageExpansion(Mat img)
    {
        Mat kernel = Cv2.GetStructuringElement(MorphShapes.Rect, new Size(5, 5));

        Mat ans = new Mat();
        Cv2.Dilate(img, ans, kernel);
        return ans;
    }
    //腐蚀
    public static Mat ErodeImage(Mat img)
    {
        Mat kernel = Cv2.GetStructuringElement(MorphShapes.Rect, new Size(5, 5));

        Mat ans = new Mat();
        Cv2.Erode(img, ans, kernel);
        return ans;
    }
}