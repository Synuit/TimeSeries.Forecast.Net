using System;
using TimeSeries.Forecast.Arima.Utils;
using TimeSeries.Forecast.Matrix;
using TimeSeries.Forecast.Utils;

/*
 * Copyright (c) 2017-present, Workday, Inc.
 * All rights reserved.
 *
 * This source code is licensed under the MIT license found in the LICENSE file in the root repository.
 */

namespace TimeSeries.Forecast.TimeSeries.Arima
{
   /// <summary>
   /// Yule-Walker algorithm implementation
   /// </summary>
   public sealed class YuleWalker
   {
      private YuleWalker()
      {
      }

      /// <summary>
      /// Perform Yule-Walker algorithm to the given timeseries data
      /// </summary>
      /// <param name="data"> input data </param>
      /// <param name="p"> YuleWalker Parameter </param>
      /// <returns> array of Auto-Regressive parameter estimates. Index 0 contains coefficient of lag 1 </returns>
      //JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
      //ORIGINAL LINE: public static double[] fit(final double[] data, final int p)
      public static double[] fit(double[] data, int p)
      {
         int length = data.Length;
         if (length == 0 || p < 1)
         {
            throw new Exception("fitYuleWalker - Invalid Parameters" + "length=" + length + ", p = " + p);
         }

         double[] r = new double[p + 1];
         foreach (double aData in data)
         {
            r[0] += Math.Pow(aData, 2);
         }
         r[0] /= length;

         for (int j = 1; j < p + 1; j++)
         {
            for (int i = 0; i < length - j; i++)
            {
               r[j] += data[i] * data[i + j];
            }
            r[j] /= (length);
         }

         //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
         //ORIGINAL LINE: final TimeSeries.Forecast.matrix.InsightsMatrix toeplitz = TimeSeries.Forecast.timeseries.timeseriesutil.ForecastUtil.initToeplitz(java.util.Arrays.copyOfRange(r, 0, p));
         InsightsMatrix toeplitz = ForecastUtil.initToeplitz(Utilities.CopyOfRange(r, 0, p));
         //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
         //ORIGINAL LINE: final TimeSeries.Forecast.matrix.InsightsVector rVector = new TimeSeries.Forecast.matrix.InsightsVector(java.util.Arrays.copyOfRange(r, 1, p + 1), false);
         InsightsVector rVector = new InsightsVector(Utilities.CopyOfRange(r, 1, p + 1), false);

         return toeplitz.solveSPDIntoVector(rVector, ForecastUtil.maxConditionNumber).deepCopy();
      }
   }
}