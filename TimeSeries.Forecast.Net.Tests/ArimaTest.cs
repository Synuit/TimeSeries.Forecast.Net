using System;
using System.Text;
using TimeSeries.Forecast.Arima.Utils;
using TimeSeries.Forecast.Models;
using TimeSeries.Forecast.TimeSeries.Arima;
using Xunit;

/*
 * Copyright (c) 2017-present, Workday, Inc.
 * All rights reserved.
 *
 * This source code is licensed under the MIT license found in the LICENSE file in the root repository.
 */

namespace TimeSeries.Forecast.Tests
{
   /// <summary>
   /// ARIMA Tests
   /// </summary>
   public class ArimaTest
   {
      //private readonly DecimalFormat df = new DecimalFormat("##.#####");

      //JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
      //ORIGINAL LINE: private double[] commonTestLogic(final String name, final double[] dataSet, final double forecastRatio, int p, int d, int q, int P, int D, int Q, int m)
      private double[] CommonTestLogic(string name, double[] dataSet, double forecastRatio, int p, int d, int q, int P, int D, int Q, int m)
      {
         //Compute forecast and training size
         int forecastSize = (int)(dataSet.Length * forecastRatio);
         int trainSize = dataSet.Length - forecastSize;
         //Separate data set into training data and test data
         double[] trainingData = new double[trainSize];
         Array.Copy(dataSet, 0, trainingData, 0, trainSize);
         double[] trueForecastData = new double[forecastSize];
         Array.Copy(dataSet, trainSize, trueForecastData, 0, forecastSize);

         return CommonTestSimpleForecast(name + " (common test)", trainingData, trueForecastData, forecastSize, p, d, q, P, D, Q, m);
      }

      private double ForecastSinglePointLogic(string name, double[] dataSet, int p, int d, int q, int P, int D, int Q, int m)
      {
         //Compute forecast and training size
         const int forecastSize = 1;
         int trainSize = dataSet.Length - forecastSize;
         //Separate data set into training data and test data
         double[] trainingData = new double[trainSize];
         Array.Copy(dataSet, 0, trainingData, 0, trainSize);
         double[] trueForecastData = new double[forecastSize];
         Array.Copy(dataSet, trainSize, trueForecastData, 0, forecastSize);

         return CommonTestSimpleForecast(name + " (forecast single point)", trainingData, trueForecastData, forecastSize, p, d, q, P, D, Q, m)[0];
      }

      private string dbl2str(double value)
      {
         //$!!$
         //string rep = df.format(value);
         //string padded = string.Format("{0,15}", rep);
         return "";// padded;
      }

      private double[] CommonTestSimpleForecast(string name, double[] trainingData, double[] trueForecastData, int forecastSize, int p, int d, int q, int P, int D, int Q, int m)
      {
         //Make forecast
         ForecastResult forecastResult = ArimaForecaster.forecast_arima(trainingData, forecastSize, new ArimaParams(p, d, q, P, D, Q, m));

         //Get forecast data and confidence intervals
         double[] forecast = forecastResult.Forecast;
         double[] upper = forecastResult.ForecastUpperConf;
         double[] lower = forecastResult.ForecastLowerConf;

         //Building output
         StringBuilder sb = new StringBuilder();
         sb.Append(name).Append("  ****************************************************\n");
         sb.Append("Input Params { ").Append("p: ").Append(p).Append(", d: ").Append(d).Append(", q: ").Append(q).Append(", P: ").Append(P).Append(", D: ").Append(D).Append(", Q: ").Append(Q).Append(", m: ").Append(m).Append(" }");
         sb.Append("\n\nFitted Model RMSE: ").Append(dbl2str(forecastResult.RMSE));
         sb.Append("\n\n      TRUE DATA    |     LOWER BOUND          FORECAST       UPPER BOUND\n");

         for (int i = 0; i < forecast.Length; ++i)
         {
            sb.Append(dbl2str(trueForecastData[i])).Append("    | ").Append(dbl2str(lower[i])).Append("   ").Append(dbl2str(forecast[i])).Append("   ").Append(dbl2str(upper[i])).Append("\n");
         }

         sb.Append("\n");

         //Compute RMSE against true forecast data
         double temp = 0.0;
         for (int i = 0; i < forecast.Length; ++i)
         {
            temp += Math.Pow(forecast[i] - trueForecastData[i], 2);
         }
         double rmse = Math.Pow(temp / forecast.Length, 0.5);
         sb.Append("RMSE = ").Append(dbl2str(rmse)).Append("\n\n");
         Console.WriteLine(sb.ToString());
         return forecast;
      }

      private void CommonAssertionLogic(double[] dataSet, double actualValue, double delta)
      {
         double lastTrueValue = dataSet[dataSet.Length - 1];
         //Assert.Equal(lastTrueValue, actualValue); //$!!$, delta);
         Util.Equal(lastTrueValue, actualValue, delta);
      }

      [Fact]
      public virtual void Arma2ma_test()
      {
         double[] ar = new double[] { 1.0, -0.25 };
         double[] ma = new double[] { 1.0, 2.0 };
         int lag = 10;
         double[] ma_coeff = ForecastUtil.ARMAtoMA(ar, ma, lag);
         double[] true_coeff = new double[] { 1.0, 2.0, 3.75, 3.25, 2.3125, 1.5, 0.921875, 0.546875, 0.31640625, 0.1796875 };

         Assert.Equal(ma_coeff, true_coeff); //$!!$, 1e-6);
         //Util.Equal(ma_coeff, true_coeff, 1e-6);
      }

      //
      [Fact]
      public virtual void Common_logic_fail_test()
      {
         Exception ex = Assert.Throws<Exception>(() => CommonTestLogic("simple12345", Datasets.simple12345, 0.1, 0, 0, 0, 0, 0, 0, 0));
         Assert.Equal("Failed to build ARIMA forecast: Index was outside the bounds of the array.", ex.Message);
      }

      //
      [Fact]
      public virtual void one_piont_fail_test()
      {
         Exception ex = Assert.Throws<Exception>(() => ForecastSinglePointLogic("simple12345", Datasets.simple12345, 0, 0, 0, 0, 0, 0, 0));
         Assert.Equal("Failed to build ARIMA forecast: Index was outside the bounds of the array.", ex.Message);
      }

      //
      [Fact]
      public virtual void a10_test()
      {
         CommonTestLogic("a10_test", Datasets.a10_val, 0.1, 3, 0, 0, 1, 0, 1, 12);
         double actualValue = ForecastSinglePointLogic("a10_test", Datasets.a10_val, 3, 0, 0, 1, 0, 1, 12);
         CommonAssertionLogic(Datasets.a10_val, actualValue, 5.05);
      }

      //
      [Fact]
      public virtual void Usconsumption_test()
      {
         CommonTestLogic("usconsumption_test", Datasets.usconsumption_val, 0.1, 1, 0, 3, 0, 0, 1, 42);
         double lastActualData = ForecastSinglePointLogic("usconsumption_test", Datasets.usconsumption_val, 1, 0, 3, 0, 0, 1, 42);
         CommonAssertionLogic(Datasets.usconsumption_val, lastActualData, 0.15);
      }

      //
      [Fact]
      public virtual void euretail_test()
      {
         CommonTestLogic("euretail_test", Datasets.euretail_val, 0.1, 3, 0, 3, 1, 1, 0, 0);
         double lastActualData = ForecastSinglePointLogic("euretail_test", Datasets.euretail_val, 3, 0, 3, 1, 1, 0, 0);
         CommonAssertionLogic(Datasets.euretail_val, lastActualData, 0.23);
      }

      //
      [Fact]
      public virtual void sunspots_test()
      {
         CommonTestLogic("sunspots_test", Datasets.sunspots_val, 0.1, 2, 0, 0, 1, 0, 1, 21);
         double actualValue = ForecastSinglePointLogic("sunspots_test", Datasets.sunspots_val, 2, 0, 0, 1, 0, 1, 21);
         CommonAssertionLogic(Datasets.sunspots_val, actualValue, 11.83);
      }

      //
      [Fact]
      public virtual void ausbeer_test()
      {
         CommonTestLogic("ausbeer_test", Datasets.ausbeer_val, 0.1, 2, 0, 1, 1, 0, 1, 8);
         double actualValue = ForecastSinglePointLogic("ausbeer_test", Datasets.ausbeer_val, 2, 0, 1, 1, 0, 1, 8);
         CommonAssertionLogic(Datasets.ausbeer_val, actualValue, 8.04);
      }

      //
      [Fact]
      public virtual void elecequip_test()
      {
         CommonTestLogic("elecequip_test", Datasets.elecequip_val, 0.1, 3, 0, 1, 1, 0, 1, 6);
         double actualValue = ForecastSinglePointLogic("elecequip_test", Datasets.elecequip_val, 3, 0, 1, 1, 0, 1, 6);
         CommonAssertionLogic(Datasets.elecequip_val, actualValue, 5.63);
      }

      //
      [Fact]
      public virtual void chicago_potholes_test()
      {
         CommonTestLogic("chicago_potholes_test", Datasets.chicago_potholes_val, 0.1, 3, 0, 3, 0, 1, 1, 14);
         double actualValue = ForecastSinglePointLogic("chicago_potholes_test", Datasets.chicago_potholes_val, 3, 0, 3, 0, 1, 1, 14);
         CommonAssertionLogic(Datasets.chicago_potholes_val, actualValue, 25.94);
      }

      //
      [Fact]
      public virtual void simple_data1_test()
      {
         double forecast = ForecastSinglePointLogic("simple_data1_test", Datasets.simple_data1_val, 3, 0, 3, 1, 1, 0, 0);
         Assert.True(forecast == 2);
      }

      //
      [Fact]
      public virtual void simple_data2_test()
      {
         double forecast = ForecastSinglePointLogic("simple_data2_test", Datasets.simple_data2_val, 0, 0, 1, 0, 0, 0, 0);
         Assert.True(forecast == 2);
      }

      //
      [Fact]
      public virtual void simple_data3_test()
      {
         double[] forecast = CommonTestSimpleForecast("simple_data3_test", Datasets.simple_data3_val, Datasets.simple_data3_answer, 7, 3, 0, 0, 1, 0, 1, 12);
         double lastActualData = forecast[forecast.Length - 1];
         CommonAssertionLogic(Datasets.simple_data3_answer, lastActualData, 0.31);
      }
   }
}