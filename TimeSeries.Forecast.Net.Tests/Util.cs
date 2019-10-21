using Xunit;

namespace TimeSeries.Forecast.Tests
{
   public class Util
   {
      public static void Equal(double x, double y, double tolerance)
      {
         Assert.True((x == y) ? true : (x > y) ? ((x - tolerance) <= y) : ((y - tolerance) <= x));
      }
   }
}