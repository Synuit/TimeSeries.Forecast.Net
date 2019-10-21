using System;
using System.Text;
using TimeSeries.Forecast.TimeSeries.Arima;

/*
 * Copyright (c) 2017-present, Workday, Inc.
 * All rights reserved.
 *
 * This source code is licensed under the MIT license found in the LICENSE file in the root repository.
 */

namespace TimeSeries.Forecast.Models
{
   /// <summary>
   /// ARIMA Forecast Result
   /// </summary>
   public class ForecastResult
   {
      private readonly double[] _forecast;
      private readonly double[] _forecastUpperConf;
      private readonly double[] _forecastLowerConf;
      private readonly double _dataVariance;
      private readonly StringBuilder _log_Renamed;
      private double _rmse;
      private double _maxNormalizedVariance;

      /// <summary>
      /// Constructor for ForecastResult
      /// </summary>
      /// <param name="pForecast"> forecast data </param>
      /// <param name="pDataVariance"> data variance of the original data </param>
      //JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
      //ORIGINAL LINE: public ForecastResult(final double[] pForecast, final double pDataVariance)
      public ForecastResult(double[] pForecast, double pDataVariance)
      {
         this._forecast = pForecast;

         this._forecastUpperConf = new double[pForecast.Length];
         Array.Copy(pForecast, 0, _forecastUpperConf, 0, pForecast.Length);

         this._forecastLowerConf = new double[pForecast.Length];
         Array.Copy(pForecast, 0, _forecastLowerConf, 0, pForecast.Length);

         this._dataVariance = pDataVariance;

         this._rmse = -1;
         this._maxNormalizedVariance = -1;

         this._log_Renamed = new StringBuilder();
      }

      /// <summary>
      /// Compute normalized variance
      /// </summary>
      /// <param name="v"> variance </param>
      /// <returns> Normalized variance </returns>
      //JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
      //ORIGINAL LINE: private double getNormalizedVariance(final double v)
      private double GetNormalizedVariance(double v)
      {
         if (v < -0.5 || _dataVariance < -0.5)
         {
            return -1;
         }
         else if (_dataVariance < 0.0000001)
         {
            return v;
         }
         else
         {
            return Math.Abs(v / _dataVariance);
         }
      }

      /// <summary>
      /// Getter for Root Mean-Squared Error
      /// </summary>
      /// <returns> Root Mean-Squared Error </returns>
      public virtual double RMSE { get { return _rmse; } set { _rmse = value; } }

      //
      /// <summary>
      /// Getter for Max Normalized Variance
      /// </summary>
      /// <returns> Max Normalized Variance </returns>
      public virtual double MaxNormalizedVariance { get { return _maxNormalizedVariance; } }

      //
      /// <summary>
      /// Compute and set confidence intervals
      /// </summary>
      /// <param name="constant"> confidence interval constant </param>
      /// <param name="cumulativeSumOfMA"> cumulative sum of MA coefficients </param>
      /// <returns> Max Normalized Variance </returns>
      //JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
      //ORIGINAL LINE: public double setConfInterval(final double constant, final double[] cumulativeSumOfMA)
      public virtual double SetConfInterval(double constant, double[] cumulativeSumOfMA)
      {
         double maxNormalizedVariance = -1.0;
         double bound = 0;
         for (int i = 0; i < _forecast.Length; i++)
         {
            bound = constant * _rmse * cumulativeSumOfMA[i];
            this._forecastUpperConf[i] = this._forecast[i] + bound;
            this._forecastLowerConf[i] = this._forecast[i] - bound;
            //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
            //ORIGINAL LINE: final double normalizedVariance = getNormalizedVariance(Math.pow(bound, 2));
            double normalizedVariance = GetNormalizedVariance(Math.Pow(bound, 2));
            if (normalizedVariance > maxNormalizedVariance)
            {
               maxNormalizedVariance = normalizedVariance;
            }
         }
         return maxNormalizedVariance;
      }

      /// <summary>
      /// Compute and set Sigma2 and prediction confidence interval.
      /// </summary>
      /// <param name="params"> ARIMA parameters from the model </param>
      public virtual ArimaParams Sigma2AndPredicationInterval
      {
         set
         {
            _maxNormalizedVariance = ArimaSolver.setSigma2AndPredicationInterval(value, this, _forecast.Length);
         }
      }

      /// <summary>
      /// Getter for forecast data
      /// </summary>
      /// <returns> forecast data </returns>
      public virtual double[] Forecast { get { return _forecast; } }

      /// <summary>
      /// Getter for upper confidence bounds
      /// </summary>
      /// <returns> array of upper confidence bounds </returns>
      public virtual double[] ForecastUpperConf { get { return _forecastUpperConf; } }

      /// <summary>
      /// Getter for lower confidence bounds
      /// </summary>
      /// <returns> array of lower confidence bounds </returns>
      public virtual double[] ForecastLowerConf
      {
         get
         {
            return _forecastLowerConf;
         }
      }

      /// <summary>
      /// Append message to log of forecast result
      /// </summary>
      /// <param name="message"> string message </param>
      public virtual void Log(string message)
      {
         this._log_Renamed.Append(message + "\n");
      }

      /// <summary>
      /// Getter for log of the forecast result
      /// </summary>
      /// <returns> full log of the forecast result </returns>
      public virtual string ForecastLog
      {
         get
         {
            return _log_Renamed.ToString();
         }
      }
   }
}