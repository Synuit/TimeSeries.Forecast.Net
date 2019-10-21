using System;
using TimeSeries.Forecast.Matrix;

/*
 * Copyright (c) 2017-present, Workday, Inc.
 * All rights reserved.
 *
 * This source code is licensed under the MIT license found in the LICENSE file in the root repository.
 */

namespace TimeSeries.Forecast.Arima.Utils
{
   /// <summary>
   /// Time series forecasting Utilities
   /// </summary>
   public sealed class ForecastUtil
   {
      public const double testSetPercentage = 0.15;
      public const double maxConditionNumber = 100;
      public const double confidence_constant_95pct = 1.959963984540054;

      private ForecastUtil()
      {
      }

      /// <summary>
      /// Instantiates Toeplitz matrix from given input array
      /// </summary>
      /// <param name="input"> double array as input data </param>
      /// <returns> a Toeplitz InsightsMatrix </returns>
      public static InsightsMatrix initToeplitz(double[] input)
      {
         int length = input.Length;
         //JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
         //ORIGINAL LINE: double[][] toeplitz = new double[length][length];
         double[][] toeplitz = RectangularArrays.ReturnRectangularDoubleArray(length, length);

         for (int i = 0; i < length; i++)
         {
            for (int j = 0; j < length; j++)
            {
               if (j > i)
               {
                  toeplitz[i][j] = input[j - i];
               }
               else if (j == i)
               {
                  toeplitz[i][j] = input[0];
               }
               else
               {
                  toeplitz[i][j] = input[i - j];
               }
            }
         }
         return new InsightsMatrix(toeplitz, false);
      }

      /// <summary>
      /// Invert AR part of ARMA to obtain corresponding MA series
      /// </summary>
      /// <param name="ar"> AR portion of the ARMA </param>
      /// <param name="ma"> MA portion of the ARMA </param>
      /// <param name="lag_max"> maximum lag </param>
      /// <returns> MA series </returns>
      //JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
      //ORIGINAL LINE: public static double[] ARMAtoMA(final double[] ar, final double[] ma, final int lag_max)
      public static double[] ARMAtoMA(double[] ar, double[] ma, int lag_max)
      {
         //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
         //ORIGINAL LINE: final int p = ar.length;
         int p = ar.Length;
         //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
         //ORIGINAL LINE: final int q = ma.length;
         int q = ma.Length;
         //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
         //ORIGINAL LINE: final double[] psi = new double[lag_max];
         double[] psi = new double[lag_max];

         for (int i = 0; i < lag_max; i++)
         {
            double tmp = (i < q) ? ma[i] : 0.0;
            for (int j = 0; j < Math.Min(i + 1, p); j++)
            {
               tmp += ar[j] * ((i - j - 1 >= 0) ? psi[i - j - 1] : 1.0);
            }
            psi[i] = tmp;
         }
         //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
         //ORIGINAL LINE: final double[] include_psi1 = new double[lag_max];
         double[] include_psi1 = new double[lag_max];
         include_psi1[0] = 1;
         for (int i = 1; i < lag_max; i++)
         {
            include_psi1[i] = psi[i - 1];
         }
         return include_psi1;
      }

      /// <summary>
      /// Simple helper that returns cumulative sum of coefficients
      /// </summary>
      /// <param name="coeffs"> array of coefficients </param>
      /// <returns> array of cumulative sum of the coefficients </returns>
      //JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
      //ORIGINAL LINE: public static double[] getCumulativeSumOfCoeff(final double[] coeffs)
      public static double[] getCumulativeSumOfCoeff(double[] coeffs)
      {
         //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
         //ORIGINAL LINE: final int len = coeffs.length;
         int len = coeffs.Length;
         //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
         //ORIGINAL LINE: final double[] cumulativeSquaredCoeffSumVector = new double[len];
         double[] cumulativeSquaredCoeffSumVector = new double[len];
         double cumulative = 0.0;
         for (int i = 0; i < len; i++)
         {
            cumulative += Math.Pow(coeffs[i], 2);
            cumulativeSquaredCoeffSumVector[i] = Math.Pow(cumulative, 0.5);
         }
         return cumulativeSquaredCoeffSumVector;
      }
   }
}