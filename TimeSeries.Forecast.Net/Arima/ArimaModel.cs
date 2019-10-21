/*
 * Copyright (c) 2017-present, Workday, Inc.
 * All rights reserved.
 *
 * This source code is licensed under the MIT license found in the LICENSE file in the root repository.
 */

using TimeSeries.Forecast.TimeSeries.Arima;

namespace TimeSeries.Forecast.Models
{
   /// <summary>
   /// ARIMA model
   /// </summary>
   public class ArimaModel
   {
      private readonly ArimaParams _params;
      private readonly double[] _data;
      private readonly int _trainDataSize;
      private double _rmse;

      /// <summary>
      /// Constructor for ArimaModel
      /// </summary>
      /// <param name="params"> ARIMA parameter </param>
      /// <param name="data"> original data </param>
      /// <param name="trainDataSize"> size of train data </param>
      public ArimaModel(ArimaParams pparams, double[] data, int trainDataSize)
      {
         _params = pparams;
         _data = data;
         _trainDataSize = trainDataSize;
      }

      /// <summary>
      /// Getter for Root Mean-Squared Error.
      /// </summary>
      /// <returns> Root Mean-Squared Error for the ARIMA model </returns>
      public virtual double RMSE { get { return _rmse; } set { _rmse = value; } }

      /// <summary>
      /// Getter for ARIMA parameters.
      /// </summary>
      /// <returns> ARIMA parameters for the model </returns>
      public virtual ArimaParams Params { get { return _params; } }

      /// <summary>
      /// Forecast data base on training data and forecast size.
      /// </summary>
      /// <param name="forecastSize"> size of forecast </param>
      /// <returns> forecast result </returns>
      //JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
      //ORIGINAL LINE: public ForecastResult forecast(final int forecastSize)
      public virtual ForecastResult Forecast(int forecastSize)
      {
         ForecastResult forecastResult = ArimaSolver.forecastARIMA(_params, _data, _trainDataSize, _trainDataSize + forecastSize);
         forecastResult.RMSE = this._rmse;

         return forecastResult;
      }
   }
}