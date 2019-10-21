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
   /// <summary>
   /// ARIMA implementation
   /// </summary>
   public sealed class ArimaForecaster
   {
      private ArimaForecaster()
      {
      } // pure static class

      /// <summary>
      /// Raw-level ARIMA forecasting function.
      /// </summary>
      /// <param name="data"> UNMODIFIED, list of double numbers representing time-series with constant time-gap </param>
      /// <param name="forecastSize"> integer representing how many data points AFTER the data series to be
      ///        forecasted </param>
      /// <param name="params"> ARIMA parameters </param>
      /// <returns> a ForecastResult object, which contains the forecasted values and/or error message(s) </returns>
      //JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
      //ORIGINAL LINE: public static TimeSeries.Forecast.TimeSeries.Arima.struct.ForecastResult forecast_arima(final double[] data, final int forecastSize, TimeSeries.Forecast.TimeSeries.Arima.struct.ArimaParams params)
      public static ForecastResult forecast_arima(double[] data, int forecastSize, ArimaParams @params)
      {
         try
         {
            int p = @params.p;

            int d = @params.d;

            int q = @params.q;
            int P = @params.P;
            int D = @params.D;
            int Q = @params.Q;
            int m = @params.m;
            ArimaParams paramsForecast = new ArimaParams(p, d, q, P, D, Q, m);
            ArimaParams paramsXValidation = new ArimaParams(p, d, q, P, D, Q, m);
            // estimate ARIMA model parameters for forecasting
            ArimaModel fittedModel = ArimaSolver.estimateARIMA(paramsForecast, data, data.Length, data.Length + 1);

            // compute RMSE to be used in confidence interval computation
            double rmseValidation = ArimaSolver.computeRMSEValidation(data, ForecastUtil.testSetPercentage, paramsXValidation);
            fittedModel.RMSE = rmseValidation;
            ForecastResult forecastResult = fittedModel.Forecast(forecastSize);

            // populate confidence interval
            forecastResult.Sigma2AndPredicationInterval = fittedModel.Params;

            // add logging messages
            forecastResult.Log("{" + "\"Best ModelInterface Param\" : \"" + fittedModel.Params.summary() + "\"," + "\"Forecast Size\" : \"" + forecastSize + "\"," + "\"Input Size\" : \"" + data.Length + "\"" + "}");

            // successfully built ARIMA model and its forecast
            return forecastResult;
         }
         catch (Exception ex)
         {
            // failed to build ARIMA model
            throw new Exception("Failed to build ARIMA forecast: " + ex.Message);
         }
      }
   }
}