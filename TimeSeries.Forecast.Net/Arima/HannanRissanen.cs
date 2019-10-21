using System;
using TimeSeries.Forecast.Arima.Utils;
using TimeSeries.Forecast.Matrix;
using TimeSeries.Forecast.Models;
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
   /// Hannan-Rissanen algorithm for estimating ARMA parameters
   /// </summary>
   public sealed class HannanRissanen
   {
      private HannanRissanen()
      {
      }

      /// <summary>
      /// Estimate ARMA(p,q) parameters, i.e. AR-parameters: \phi_1, ... , \phi_p
      ///                                     MA-parameters: \theta_1, ... , \theta_q
      /// Input data is assumed to be stationary, has zero-mean, aligned, and imputed
      /// </summary>
      /// <param name="data_orig"> original data </param>
      /// <param name="params"> ARIMA parameters </param>
      /// <param name="forecast_length"> forecast length </param>
      /// <param name="maxIteration"> maximum number of iteration </param>
      //JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
      //ORIGINAL LINE: public static void estimateARMA(final double[] data_orig, final TimeSeries.Forecast.TimeSeries.Arima.struct.ArimaParams params, final int forecast_length, final int maxIteration)
      public static void estimateARMA(double[] data_orig, ArimaParams @params, int forecast_length, int maxIteration)
      {
         //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
         //ORIGINAL LINE: final double[] data = new double[data_orig.length];
         double[] data = new double[data_orig.Length];
         //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
         //ORIGINAL LINE: final int total_length = data.length;
         int total_length = data.Length;
         Array.Copy(data_orig, 0, data, 0, total_length);
         //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
         //ORIGINAL LINE: final int r = (params.getDegreeP() > params.getDegreeQ()) ? 1 + params.getDegreeP() : 1 + params.getDegreeQ();
         int r = (@params.DegreeP > @params.DegreeQ) ? 1 + @params.DegreeP : 1 + @params.DegreeQ;
         //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
         //ORIGINAL LINE: final int length = total_length - forecast_length;
         int length = total_length - forecast_length;
         //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
         //ORIGINAL LINE: final int size = length - r;
         int size = length - r;
         if (length < 2 * r)
         {
            throw new Exception("not enough data points: length=" + length + ", r=" + r);
         }

         // step 1: apply Yule-Walker method and estimate AR(r) model on input data
         //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
         //ORIGINAL LINE: final double[] errors = new double[length];
         double[] errors = new double[length];
         //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
         //ORIGINAL LINE: final double[] yuleWalkerParams = applyYuleWalkerAndGetInitialErrors(data, r, length, errors);
         double[] yuleWalkerParams = applyYuleWalkerAndGetInitialErrors(data, r, length, errors);
         for (int j = 0; j < r; ++j)
         {
            errors[j] = 0;
         }

         // step 2: iterate Least-Square fitting until the parameters converge
         // instantiate Z-matrix
         //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
         //ORIGINAL LINE: final double[][] matrix = new double[params.getNumParamsP() + params.getNumParamsQ()][size];
         //JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
         //ORIGINAL LINE: double[][] matrix = new double[params.NumParamsP + params.NumParamsQ][size];
         double[][] matrix = RectangularArrays.ReturnRectangularDoubleArray(@params.NumParamsP + @params.NumParamsQ, size);

         double bestRMSE = -1; // initial value
         int remainIteration = maxIteration;
         InsightsVector bestParams = null;
         while (--remainIteration >= 0)
         {
            //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
            //ORIGINAL LINE: final TimeSeries.Forecast.matrix.InsightsVector estimatedParams = iterationStep(params, data, errors, matrix, r, length, size);
            InsightsVector estimatedParams = iterationStep(@params, data, errors, matrix, r, length, size);
            //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
            //ORIGINAL LINE: final TimeSeries.Forecast.matrix.InsightsVector originalParams = params.getParamsIntoVector();
            InsightsVector originalParams = @params.ParamsIntoVector;
            @params.ParamsFromVector = estimatedParams;

            // forecast for validation data and compute RMSE
            //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
            //ORIGINAL LINE: final double[] forecasts = ArimaSolver.forecastARMA(params, data, length, data.length);
            double[] forecasts = ArimaSolver.forecastARMA(@params, data, length, data.Length);
            //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
            //ORIGINAL LINE: final double anotherRMSE = ArimaSolver.computeRMSE(data, forecasts, length, 0, forecast_length);
            double anotherRMSE = ArimaSolver.ComputeRMSE(data, forecasts, length, 0, forecast_length);
            // update errors
            //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
            //ORIGINAL LINE: final double[] train_forecasts = ArimaSolver.forecastARMA(params, data, r, data.length);
            double[] train_forecasts = ArimaSolver.forecastARMA(@params, data, r, data.Length);
            for (int j = 0; j < size; ++j)
            {
               errors[j + r] = data[j + r] - train_forecasts[j];
            }
            if (bestRMSE < 0 || anotherRMSE < bestRMSE)
            {
               bestParams = estimatedParams;
               bestRMSE = anotherRMSE;
            }
         }
         @params.ParamsFromVector = bestParams;
      }

      //JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
      //ORIGINAL LINE: private static double[] applyYuleWalkerAndGetInitialErrors(final double[] data, final int r, final int length, final double[] errors)
      private static double[] applyYuleWalkerAndGetInitialErrors(double[] data, int r, int length, double[] errors)
      {
         //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
         //ORIGINAL LINE: final double[] yuleWalker = YuleWalker.fit(data, r);
         double[] yuleWalker = YuleWalker.fit(data, r);
         //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
         //ORIGINAL LINE: final TimeSeries.Forecast.TimeSeries.Arima.struct.BackShift bsYuleWalker = new TimeSeries.Forecast.TimeSeries.Arima.struct.BackShift(r, true);
         BackShift bsYuleWalker = new BackShift(r, true);
         bsYuleWalker.initializeParams(false);
         // return array from YuleWalker is an array of size r whose
         // 0-th index element is lag 1 coefficient etc
         // hence shifting lag index by one and copy over to BackShift operator
         for (int j = 0; j < r; ++j)
         {
            bsYuleWalker.setParam(j + 1, yuleWalker[j]);
         }
         int m = 0;
         // populate error array
         while (m < r)
         {
            errors[m++] = 0;
         } // initial r-elements are set to zero
         while (m < length)
         {
            // from then on, initial estimate of error terms are
            // Z_t = X_t - \phi_1 X_{t-1} - \cdots - \phi_r X_{t-r}
            errors[m] = data[m] - bsYuleWalker.getLinearCombinationFrom(data, m);
            ++m;
         }
         return yuleWalker;
      }

      //JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
      //ORIGINAL LINE: private static TimeSeries.Forecast.matrix.InsightsVector iterationStep(final TimeSeries.Forecast.TimeSeries.Arima.struct.ArimaParams params, final double[] data, final double[] errors, final double[][] matrix, final int r, final int length, final int size)
      private static InsightsVector iterationStep(ArimaParams @params, double[] data, double[] errors, double[][] matrix, int r, int length, int size)
      {
         int rowIdx = 0;
         // copy over shifted timeseries data into matrix
         //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
         //ORIGINAL LINE: final int[] offsetsAR = params.getOffsetsAR();
         int[] offsetsAR = @params.OffsetsAR;
         foreach (int pIdx in offsetsAR)
         {
            Array.Copy(data, r - pIdx, matrix[rowIdx], 0, size);
            ++rowIdx;
         }
         // copy over shifted errors into matrix
         //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
         //ORIGINAL LINE: final int[] offsetsMA = params.getOffsetsMA();
         int[] offsetsMA = @params.OffsetsMA;
         foreach (int qIdx in offsetsMA)
         {
            Array.Copy(errors, r - qIdx, matrix[rowIdx], 0, size);
            ++rowIdx;
         }

         // instantiate matrix to perform least squares algorithm
         //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
         //ORIGINAL LINE: final TimeSeries.Forecast.matrix.InsightsMatrix zt = new TimeSeries.Forecast.matrix.InsightsMatrix(matrix, false);
         InsightsMatrix zt = new InsightsMatrix(matrix, false);

         // instantiate target vector
         //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
         //ORIGINAL LINE: final double[] vector = new double[size];
         double[] vector = new double[size];
         Array.Copy(data, r, vector, 0, size);
         //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
         //ORIGINAL LINE: final TimeSeries.Forecast.matrix.InsightsVector x = new TimeSeries.Forecast.matrix.InsightsVector(vector, false);
         InsightsVector x = new InsightsVector(vector, false);

         // obtain least squares solution
         //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
         //ORIGINAL LINE: final TimeSeries.Forecast.matrix.InsightsVector ztx = zt.timesVector(x);
         InsightsVector ztx = zt.timesVector(x);
         //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
         //ORIGINAL LINE: final TimeSeries.Forecast.matrix.InsightsMatrix ztz = zt.computeAAT();
         InsightsMatrix ztz = zt.computeAAT();
         //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
         //ORIGINAL LINE: final TimeSeries.Forecast.matrix.InsightsVector estimatedVector = ztz.solveSPDIntoVector(ztx, TimeSeries.Forecast.timeseries.timeseriesutil.ForecastUtil.maxConditionNumber);
         InsightsVector estimatedVector = ztz.solveSPDIntoVector(ztx, ForecastUtil.maxConditionNumber);

         return estimatedVector;
      }
   }
}