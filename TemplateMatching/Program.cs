// @"..\\..\\..\\B.bmp"
// @"..\\..\\..\\B_cr.bmp"
// 归一化的差值平方和算法

using System;
using OpenCvSharp;

class Program
{
    static void Main(string[] args)
    {
        // Load source image and template image
        Mat sourceImage = new Mat(@"..\\..\\..\\B.bmp", ImreadModes.Color);
        Mat templateImage = new Mat(@"..\\..\\..\\B_cr.bmp", ImreadModes.Color);

        // Get the size of the source and template images
        int sourceWidth = sourceImage.Width;
        int sourceHeight = sourceImage.Height;
        int templateWidth = templateImage.Width;
        int templateHeight = templateImage.Height;

        // Create a result image to store the matching scores
        Mat resultImage = new Mat(sourceHeight - templateHeight + 1, sourceWidth - templateWidth + 1, MatType.CV_32FC1);

        // Calculate the squared differences (Sum of Squared Differences - SSD)
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

        // Normalize the result image
        Cv2.Normalize(resultImage, resultImage, 0, 1, NormTypes.MinMax);

        // Find the best match location
        double minValue, maxValue;
        OpenCvSharp.Point minLoc, maxLoc;
        Cv2.MinMaxLoc(resultImage, out minValue, out maxValue, out minLoc, out maxLoc);

        // Draw a rectangle around the best match
        Cv2.Rectangle(sourceImage, maxLoc, new OpenCvSharp.Point(maxLoc.X + templateWidth, maxLoc.Y + templateHeight), Scalar.Red, 2);

        // Display the result image
        Cv2.NamedWindow("Result", WindowFlags.Normal);
        Cv2.ImShow("Result", sourceImage);

        // Wait for a key press and then close the window
        Cv2.WaitKey(0);
        Cv2.DestroyAllWindows();
    }
}