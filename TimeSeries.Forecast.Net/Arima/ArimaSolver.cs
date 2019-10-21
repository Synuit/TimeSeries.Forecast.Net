using System;
using TimeSeries.Forecast.Arima.Utils;
using TimeSeries.Forecast.Models;

/*
 * Copyright (c) 2017-present, Workday, Inc.
 * All rights reserved.
 *
 * This source code is licensed under the MIT license found in the LICENSE file in the root repository.
 */

namespace TimeSeries.Forecast.TimeSeries.Arima
{
   public sealed class ArimaSolver
   {
      private const int maxIterationForHannanRissanen = 5;

      private ArimaSolver()
      {
      } // pure static helper class

      /// <summary>
      /// Forecast ARMA
      /// </summary>
      /// <param name="params"> MODIFIED. ARIMA parameters </param>
      /// <param name="dataStationary"> UNMODIFIED. the time series AFTER differencing / centering </param>
      /// <param name="startIndex"> the index where the forecast starts. startIndex &le; data.length </param>
      /// <param name="endIndex"> the index where the forecast stops (exclusive). startIndex  endIndex </param>
      /// <returns> forecast ARMA data point </returns>
      //JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
      //ORIGINAL LINE: public static double[] forecastARMA(final TimeSeries.Forecast.TimeSeries.Arima.struct.ArimaParams params, final double[] dataStationary, final int startIndex, final int endIndex)
      public static double[] forecastARMA(ArimaParams @params, double[] dataStationary, int startIndex, int endIndex)
      {
         //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
         //ORIGINAL LINE: final int train_len = startIndex;
         int train_len = startIndex;
         //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
         //ORIGINAL LINE: final int total_len = endIndex;
         int total_len = endIndex;
         //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
         //ORIGINAL LINE: final double[] errors = new double[total_len];
         double[] errors = new double[total_len];
         //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
         //ORIGINAL LINE: final double[] data = new double[total_len];
         double[] data = new double[total_len];
         Array.Copy(dataStationary, 0, data, 0, train_len);
         //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
         //ORIGINAL LINE: final int forecast_len = endIndex - startIndex;
         int forecast_len = endIndex - startIndex;
         //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
         //ORIGINAL LINE: final double[] forecasts = new double[forecast_len];
         double[] forecasts = new double[forecast_len];
         //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
         //ORIGINAL LINE: final int _dp = params.getDegreeP();
         int _dp = @params.DegreeP;
         //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
         //ORIGINAL LINE: final int _dq = params.getDegreeQ();
         int _dq = @params.DegreeQ;
         //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
         //ORIGINAL LINE: final int start_idx = (_dp > _dq) ? _dp : _dq;
         int start_idx = (_dp > _dq) ? _dp : _dq;

         for (int j = 0; j < start_idx; ++j)
         {
            errors[j] = 0;
         }
         // populate errors and forecasts
         for (int j = start_idx; j < train_len; ++j)
         {
            //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
            //ORIGINAL LINE: final double forecast = params.forecastOnePointARMA(data, errors, j);
            double forecast = @params.forecastOnePointARMA(data, errors, j);
            //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
            //ORIGINAL LINE: final double error = data[j] - forecast;
            double error = data[j] - forecast;
            errors[j] = error;
         }
         // now we can forecast
         for (int j = train_len; j < total_len; ++j)
         {
            //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
            //ORIGINAL LINE: final double forecast = params.forecastOnePointARMA(data, errors, j);
            double forecast = @params.forecastOnePointARMA(data, errors, j);
            data[j] = forecast;
            errors[j] = 0;
            forecasts[j - train_len] = forecast;
         }
         // return forecasted values
         return forecasts;
      }

      /// <summary>
      /// Produce forecast result based on input ARIMA parameters and forecast length.
      /// </summary>
      /// <param name="params"> UNMODIFIED. ARIMA parameters </param>
      /// <param name="data"> UNMODIFIED. the original time series before differencing / centering </param>
      /// <param name="forecastStartIndex"> the index where the forecast starts. startIndex &le; data.length </param>
      /// <param name="forecastEndIndex"> the index where the forecast stops (exclusive). startIndex &lt; endIndex </param>
      /// <returns> forecast result </returns>
      //JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
      //ORIGINAL LINE: public static TimeSeries.Forecast.TimeSeries.Arima.struct.ForecastResult forecastARIMA(final TimeSeries.Forecast.TimeSeries.Arima.struct.ArimaParams params, final double[] data, final int forecastStartIndex, final int forecastEndIndex)
      public static ForecastResult forecastARIMA(ArimaParams @params, double[] data, int forecastStartIndex, int forecastEndIndex)
      {
         if (!checkARIMADataLength(@params, data, forecastStartIndex, forecastEndIndex))
         {
            //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
            //ORIGINAL LINE: final int initialConditionSize = params.d + params.D * params.m;
            int initialConditionSize = @params.d + @params.D * @params.m;
            throw new Exception("not enough data for ARIMA. needed at least " + initialConditionSize + ", have " + data.Length + ", startIndex=" + forecastStartIndex + ", endIndex=" + forecastEndIndex);
         }

         //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
         //ORIGINAL LINE: final int forecast_length = forecastEndIndex - forecastStartIndex;
         int forecast_length = forecastEndIndex - forecastStartIndex;
         //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
         //ORIGINAL LINE: final double[] forecast = new double[forecast_length];
         double[] forecast = new double[forecast_length];
         //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
         //ORIGINAL LINE: final double[] data_train = new double[forecastStartIndex];
         double[] data_train = new double[forecastStartIndex];
         Array.Copy(data, 0, data_train, 0, forecastStartIndex);

         //=======================================
         // DIFFERENTIATE
         //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
         //ORIGINAL LINE: final boolean hasSeasonalI = params.D > 0 && params.m > 0;
         bool hasSeasonalI = @params.D > 0 && @params.m > 0;
         //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
         //ORIGINAL LINE: final boolean hasNonSeasonalI = params.d > 0;
         bool hasNonSeasonalI = @params.d > 0;
         double[] data_stationary = differentiate(@params, data_train, hasSeasonalI, hasNonSeasonalI); // currently un-centered
                                                                                                       // END OF DIFFERENTIATE
                                                                                                       //==========================================

         //=========== CENTERING ====================
         //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
         //ORIGINAL LINE: final double mean_stationary = TimeSeries.Forecast.timeseries.timeseriesutil.Integrator.computeMean(data_stationary);
         double mean_stationary = Integrator.computeMean(data_stationary);
         Integrator.shift(data_stationary, (-1) * mean_stationary);
         //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
         //ORIGINAL LINE: final double dataVariance = TimeSeries.Forecast.timeseries.timeseriesutil.Integrator.computeVariance(data_stationary);
         double dataVariance = Integrator.computeVariance(data_stationary);
         //==========================================

         //==========================================
         // FORECAST
         //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
         //ORIGINAL LINE: final double[] forecast_stationary = forecastARMA(params, data_stationary, data_stationary.length, data_stationary.length + forecast_length);
         double[] forecast_stationary = forecastARMA(@params, data_stationary, data_stationary.Length, data_stationary.Length + forecast_length);

         //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
         //ORIGINAL LINE: final double[] data_forecast_stationary = new double[data_stationary.length + forecast_length];
         double[] data_forecast_stationary = new double[data_stationary.Length + forecast_length];

         Array.Copy(data_stationary, 0, data_forecast_stationary, 0, data_stationary.Length);
         Array.Copy(forecast_stationary, 0, data_forecast_stationary, data_stationary.Length, forecast_stationary.Length);
         // END OF FORECAST
         //==========================================

         //=========== UN-CENTERING =================
         Integrator.shift(data_forecast_stationary, mean_stationary);
         //==========================================

         //===========================================
         // INTEGRATE
         double[] forecast_merged = integrate(@params, data_forecast_stationary, hasSeasonalI, hasNonSeasonalI);
         // END OF INTEGRATE
         //===========================================
         Array.Copy(forecast_merged, forecastStartIndex, forecast, 0, forecast_length);

         return new ForecastResult(forecast, dataVariance);
      }

      /// <summary>
      /// Creates the fitted ARIMA model based on the ARIMA parameters.
      /// </summary>
      /// <param name="params"> MODIFIED. ARIMA parameters </param>
      /// <param name="data"> UNMODIFIED. the original time series before differencing / centering </param>
      /// <param name="forecastStartIndex"> the index where the forecast starts. startIndex &le; data.length </param>
      /// <param name="forecastEndIndex"> the index where the forecast stops (exclusive). startIndex &lt; endIndex </param>
      /// <returns> fitted ARIMA model </returns>
      //JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
      //ORIGINAL LINE: public static TimeSeries.Forecast.TimeSeries.Arima.struct.ArimaModel estimateARIMA(final TimeSeries.Forecast.TimeSeries.Arima.struct.ArimaParams params, final double[] data, final int forecastStartIndex, final int forecastEndIndex)
      public static ArimaModel estimateARIMA(ArimaParams @params, double[] data, int forecastStartIndex, int forecastEndIndex)
      {
         if (!checkARIMADataLength(@params, data, forecastStartIndex, forecastEndIndex))
         {
            //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
            //ORIGINAL LINE: final int initialConditionSize = params.d + params.D * params.m;
            int initialConditionSize = @params.d + @params.D * @params.m;
            throw new Exception("not enough data for ARIMA. needed at least " + initialConditionSize + ", have " + data.Length + ", startIndex=" + forecastStartIndex + ", endIndex=" + forecastEndIndex);
         }

         //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
         //ORIGINAL LINE: final int forecast_length = forecastEndIndex - forecastStartIndex;
         int forecast_length = forecastEndIndex - forecastStartIndex;
         //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
         //ORIGINAL LINE: final double[] data_train = new double[forecastStartIndex];
         double[] data_train = new double[forecastStartIndex];
         Array.Copy(data, 0, data_train, 0, forecastStartIndex);

         //=======================================
         // DIFFERENTIATE
         //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
         //ORIGINAL LINE: final boolean hasSeasonalI = params.D > 0 && params.m > 0;
         bool hasSeasonalI = @params.D > 0 && @params.m > 0;
         //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
         //ORIGINAL LINE: final boolean hasNonSeasonalI = params.d > 0;
         bool hasNonSeasonalI = @params.d > 0;
         double[] data_stationary = differentiate(@params, data_train, hasSeasonalI, hasNonSeasonalI); // currently un-centered
                                                                                                       // END OF DIFFERENTIATE
                                                                                                       //==========================================

         //=========== CENTERING ====================
         //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
         //ORIGINAL LINE: final double mean_stationary = TimeSeries.Forecast.timeseries.timeseriesutil.Integrator.computeMean(data_stationary);
         double mean_stationary = Integrator.computeMean(data_stationary);
         Integrator.shift(data_stationary, (-1) * mean_stationary);
         //==========================================

         //==========================================
         // FORECAST
         HannanRissanen.estimateARMA(data_stationary, @params, forecast_length, maxIterationForHannanRissanen);

         return new ArimaModel(@params, data, forecastStartIndex);
      }

      /// <summary>
      /// Differentiate procedures for forecast and estimate ARIMA.
      /// </summary>
      /// <param name="params"> ARIMA parameters </param>
      /// <param name="trainingData"> training data </param>
      /// <param name="hasSeasonalI"> has seasonal I or not based on the parameter </param>
      /// <param name="hasNonSeasonalI"> has NonseasonalI or not based on the parameter </param>
      /// <returns> stationary data </returns>
      private static double[] differentiate(ArimaParams @params, double[] trainingData, bool hasSeasonalI, bool hasNonSeasonalI)
      {
         double[] dataStationary; // currently un-centered
         if (hasSeasonalI && hasNonSeasonalI)
         {
            @params.differentiateSeasonal(trainingData);
            @params.DifferentiateNonSeasonal(@params.LastDifferenceSeasonal);
            dataStationary = @params.LastDifferenceNonSeasonal;
         }
         else if (hasSeasonalI)
         {
            @params.differentiateSeasonal(trainingData);
            dataStationary = @params.LastDifferenceSeasonal;
         }
         else if (hasNonSeasonalI)
         {
            @params.DifferentiateNonSeasonal(trainingData);
            dataStationary = @params.LastDifferenceNonSeasonal;
         }
         else
         {
            dataStationary = new double[trainingData.Length];
            Array.Copy(trainingData, 0, dataStationary, 0, trainingData.Length);
         }

         return dataStationary;
      }

      /// <summary>
      /// Differentiate procedures for forecast and estimate ARIMA.
      /// </summary>
      /// <param name="params"> ARIMA parameters </param>
      /// <param name="dataForecastStationary"> stationary forecast data </param>
      /// <param name="hasSeasonalI"> has seasonal I or not based on the parameter </param>
      /// <param name="hasNonSeasonalI"> has NonseasonalI or not based on the parameter </param>
      /// <returns> merged forecast data </returns>
      private static double[] integrate(ArimaParams @params, double[] dataForecastStationary, bool hasSeasonalI, bool hasNonSeasonalI)
      {
         double[] forecast_merged;
         if (hasSeasonalI && hasNonSeasonalI)
         {
            @params.IntegrateSeasonal(dataForecastStationary);
            @params.IntegrateNonSeasonal(@params.LastIntegrateSeasonal);
            forecast_merged = @params.LastIntegrateNonSeasonal;
         }
         else if (hasSeasonalI)
         {
            @params.IntegrateSeasonal(dataForecastStationary);
            forecast_merged = @params.LastIntegrateSeasonal;
         }
         else if (hasNonSeasonalI)
         {
            @params.IntegrateNonSeasonal(dataForecastStationary);
            forecast_merged = @params.LastIntegrateNonSeasonal;
         }
         else
         {
            forecast_merged = new double[dataForecastStationary.Length];
            Array.Copy(dataForecastStationary, 0, forecast_merged, 0, dataForecastStationary.Length);
         }

         return forecast_merged;
      }

      /// <summary>
      /// Computes the Root Mean-Squared Error given a time series (with forecast) and true values
      /// </summary>
      /// <param name="left"> time series being evaluated </param>
      /// <param name="right"> true values </param>
      /// <param name="startIndex"> the index which to start evaluation </param>
      /// <param name="endIndex"> the index which to end evaluation </param> </param>
      /// <param name="leftIndexOffset"> the number of elements from <param name="startIndex"> to the index which
      /// evaluation begins </param>
      /// <returns> Root Mean-Squared Error </returns>
      //JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
      //ORIGINAL LINE: public static double computeRMSE(final double[] left, final double[] right, final int leftIndexOffset, final int startIndex, final int endIndex)
      public static double ComputeRMSE(double[] left, double[] right, int leftIndexOffset, int startIndex, int endIndex)
      {
         //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
         //ORIGINAL LINE: final int len_left = left.length;
         int len_left = left.Length;
         //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
         //ORIGINAL LINE: final int len_right = right.length;
         int len_right = right.Length;
         if (startIndex >= endIndex || startIndex < 0 || len_right < endIndex || len_left + leftIndexOffset < 0 || len_left + leftIndexOffset < endIndex)
         {
            throw new Exception("invalid arguments: startIndex=" + startIndex + ", endIndex=" + endIndex + ", len_left=" + len_left + ", len_right=" + len_right + ", leftOffset=" + leftIndexOffset);
         }
         double square_sum = 0.0;
         for (int i = startIndex; i < endIndex; ++i)
         {
            //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
            //ORIGINAL LINE: final double error = left[i + leftIndexOffset] - right[i];
            double error = left[i + leftIndexOffset] - right[i];
            square_sum += error * error;
         }
         return Math.Sqrt(square_sum / (double)(endIndex - startIndex));
      }

      /// <summary>
      /// Performs validation using Root Mean-Squared Error given a time series (with forecast) and
      /// true values
      /// </summary>
      /// <param name="data"> UNMODIFIED. time series data being evaluated </param>
      /// <param name="testDataPercentage"> percentage of data to be used to evaluate as test set </param>
      /// <param name="params"> MODIFIED. parameter of the ARIMA model </param>
      /// <returns> a Root Mean-Squared Error computed from the forecast and true data </returns>
      //JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
      //ORIGINAL LINE: public static double computeRMSEValidation(final double[] data, final double testDataPercentage, TimeSeries.Forecast.TimeSeries.Arima.struct.ArimaParams params)
      public static double computeRMSEValidation(double[] data, double testDataPercentage, ArimaParams @params)
      {
         int testDataLength = (int)(data.Length * testDataPercentage);
         int trainingDataEndIndex = data.Length - testDataLength;

         //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
         //ORIGINAL LINE: final TimeSeries.Forecast.TimeSeries.Arima.struct.ArimaModel result = estimateARIMA(params, data, trainingDataEndIndex, data.length);
         ArimaModel result = estimateARIMA(@params, data, trainingDataEndIndex, data.Length);

         //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
         //ORIGINAL LINE: final double[] forecast = result.forecast(testDataLength).getForecast();
         double[] forecast = result.Forecast(testDataLength).Forecast;

         return ComputeRMSE(data, forecast, trainingDataEndIndex, 0, forecast.Length);
      }

      /// <summary>
      /// Set Sigma2(RMSE) and Predication Interval for forecast result.
      /// </summary>
      /// <param name="params"> ARIMA parameters </param>
      /// <param name="forecastResult"> MODIFIED. forecast result </param>
      /// <param name="forecastSize"> size of forecast </param>
      /// <returns> max normalized variance </returns>
      //JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
      //ORIGINAL LINE: public static double setSigma2AndPredicationInterval(final TimeSeries.Forecast.TimeSeries.Arima.struct.ArimaParams params, final TimeSeries.Forecast.TimeSeries.Arima.struct.ForecastResult forecastResult, final int forecastSize)
      public static double setSigma2AndPredicationInterval(ArimaParams @params, ForecastResult forecastResult, int forecastSize)
      {
         //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
         //ORIGINAL LINE: final double[] coeffs_AR = params.getCurrentARCoefficients();
         double[] coeffs_AR = @params.CurrentARCoefficients;
         //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
         //ORIGINAL LINE: final double[] coeffs_MA = params.getCurrentMACoefficients();
         double[] coeffs_MA = @params.CurrentMACoefficients;
         return forecastResult.SetConfInterval(ForecastUtil.confidence_constant_95pct, ForecastUtil.getCumulativeSumOfCoeff(ForecastUtil.ARMAtoMA(coeffs_AR, coeffs_MA, forecastSize)));
      }

      /// <summary>
      /// Input checker
      /// </summary>
      /// <param name="params"> ARIMA parameter </param>
      /// <param name="data"> original data </param>
      /// <param name="startIndex"> start index of ARIMA operation </param>
      /// <param name="endIndex"> end index of ARIMA operation </param>
      /// <returns> whether the inputs are valid </returns>
      private static bool checkARIMADataLength(ArimaParams @params, double[] data, int startIndex, int endIndex)
      {
         bool result = true;

         //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
         //ORIGINAL LINE: final int initialConditionSize = params.d + params.D * params.m;
         int initialConditionSize = @params.d + @params.D * @params.m;

         if (data.Length < initialConditionSize || startIndex < initialConditionSize || endIndex <= startIndex)
         {
            result = false;
         }

         return result;
      }
   }
}