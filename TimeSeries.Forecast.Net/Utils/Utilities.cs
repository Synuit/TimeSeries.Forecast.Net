using System;

namespace TimeSeries.Forecast.Utils
{
   public class Utilities
   {
      public static int[] CopyOfRange(int[] src, int start, int end)
      {
         int len = end - start;
         int[] dest = new int[len];
         Array.Copy(src, start, dest, 0, len);
         return dest;
      }

      public static double[] CopyOfRange(double[] src, int start, int end)
      {
         int len = end - start;
         double[] dest = new double[len];
         Array.Copy(src, start, dest, 0, len);
         return dest;
      }
   }
}