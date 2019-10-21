using System;

/*
 * Copyright (c) 2017-present, Workday, Inc.
 * All rights reserved.
 *
 * This source code is licensed under the MIT license found in the LICENSE file in the root repository.
 */

namespace TimeSeries.Forecast.Arima.Utils
{
   /// <summary>
   /// Pure Helper Class
   /// </summary>
   public class Integrator
   {
      private Integrator()
      {
      } // this is pure helper class

      ///
      /// <param name="src"> source array of data </param>
      /// <param name="dst"> destination array to store data </param>
      /// <param name="initial"> initial conditions </param>
      /// <param name="d"> length of initial conditions </param>
      //JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
      //ORIGINAL LINE: public static void differentiate(final double[] src, final double[] dst, final double[] initial, final int d)
      public static void differentiate(double[] src, double[] dst, double[] initial, int d)
      {
         if (initial == null || initial.Length != d || d <= 0)
         {
            throw new Exception("invalid initial size=" + initial.Length + ", d=" + d);
         }
         if (src == null || src.Length <= d)
         {
            throw new Exception("insufficient source size=" + src.Length + ", d=" + d);
         }
         if (dst == null || dst.Length != src.Length - d)
         {
            throw new Exception("invalid destination size=" + dst.Length + ", src=" + src.Length + ", d=" + d);
         }

         // copy over initial conditions
         Array.Copy(src, 0, initial, 0, d);
         // now differentiate source into destination
         //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
         //ORIGINAL LINE: final int src_len = src.length;
         int src_len = src.Length;
         for (int j = d, k = 0; j < src_len; ++j, ++k)
         {
            dst[k] = src[j] - src[k];
         }
      }

      ///
      /// <param name="src"> source array of data </param>
      /// <param name="dst"> destination array to store data </param>
      /// <param name="initial"> initial conditions </param>
      /// <param name="d"> length of initial conditions </param>
      //JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
      //ORIGINAL LINE: public static void integrate(final double[] src, final double[] dst, final double[] initial, final int d)
      public static void integrate(double[] src, double[] dst, double[] initial, int d)
      {
         if (initial == null || initial.Length != d || d <= 0)
         {
            throw new Exception("invalid initial size=" + initial.Length + ", d=" + d);
         }
         if (dst == null || dst.Length <= d)
         {
            throw new Exception("insufficient destination size=" + dst.Length + ", d=" + d);
         }
         if (src == null || src.Length != dst.Length - d)
         {
            throw new Exception("invalid source size=" + src.Length + ", dst=" + dst.Length + ", d=" + d);
         }

         // copy over initial conditions
         Array.Copy(initial, 0, dst, 0, d);
         // now integrate source into destination
         //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
         //ORIGINAL LINE: final int src_len = src.length;
         int src_len = src.Length;
         for (int j = d, k = 0; k < src_len; ++j, ++k)
         {
            dst[j] = dst[k] + src[k];
         }
      }

      /// <summary>
      /// Shifting the input data
      /// </summary>
      /// <param name="inputData"> MODIFIED. input data </param>
      /// <param name="shiftAmount"> shift amount </param>
      //JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
      //ORIGINAL LINE: public static void shift(double[] inputData, final double shiftAmount)
      public static void shift(double[] inputData, double shiftAmount)
      {
         for (int i = 0; i < inputData.Length; i++)
         {
            inputData[i] += shiftAmount;
         }
      }

      /// <summary>
      /// Compute the mean of input data
      /// </summary>
      /// <param name="data"> input data </param>
      /// <returns> mean </returns>
      //JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
      //ORIGINAL LINE: public static double computeMean(final double[] data)
      public static double computeMean(double[] data)
      {
         //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
         //ORIGINAL LINE: final int length = data.length;
         int length = data.Length;
         if (length == 0)
         {
            return 0.0;
         }
         double sum = 0.0;
         for (int i = 0; i < length; ++i)
         {
            sum += data[i];
         }
         return sum / length;
      }

      /// <summary>
      /// Compute the variance of input data
      /// </summary>
      /// <param name="data"> input data </param>
      /// <returns> variance </returns>
      //JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
      //ORIGINAL LINE: public static double computeVariance(final double[] data)
      public static double computeVariance(double[] data)
      {
         double variance = 0.0;
         double mean = computeMean(data);
         for (int i = 0; i < data.Length; i++)
         {
            //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
            //ORIGINAL LINE: final double diff = data[i] - mean;
            double diff = data[i] - mean;
            variance += diff * diff;
         }
         return variance / (double)(data.Length - 1);
      }
   }
}