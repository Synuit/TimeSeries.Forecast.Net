/*
 * Copyright (c) 2017-present, Workday, Inc.
 * All rights reserved.
 *
 * This source code is licensed under the MIT license found in the LICENSE file in the root repository.
 */

using TimeSeries.Forecast.Arima.Utils;
using TimeSeries.Forecast.Matrix;
using TimeSeries.Forecast.Utils;

namespace TimeSeries.Forecast.Models
{
   //using global::TimeSeries.Forecast.Utils;
   //using TimeSeries.Forecast.Utils;
   //using InsightsVector = TimeSeries.Forecast.Matrix.InsightsVector;
   //using Integrator = TimeSeries.Forecast.timeseries.timeseriesutil.Integrator;

   /// <summary>
   /// Simple wrapper for ARIMA parameters and fitted states
   /// </summary>
   public sealed class ArimaParams
   {
      public readonly int p;
      public readonly int d;
      public readonly int q;
      public readonly int P;
      public readonly int D;
      public readonly int Q;
      public readonly int m;

      // ARMA part
      private readonly BackShift _opAR;

      private readonly BackShift _opMA;
      private readonly int _dp;
      private readonly int _dq;
      private readonly int _np;
      private readonly int _nq;
      private readonly double[][] _init_seasonal;
      private readonly double[][] _diff_seasonal;
      private readonly double[][] _integrate_seasonal;
      private readonly double[][] _init_non_seasonal;
      private readonly double[][] _diff_non_seasonal;
      private readonly double[][] _integrate_non_seasonal;
      private int[] lagsAR = null;
      private double[] paramsAR = null;
      private int[] lagsMA = null;
      private double[] paramsMA = null;

      // I part
      private double _mean = 0.0;

      /// <summary>
      /// Constructor for ArimaParams
      /// </summary>
      /// <param name="p"> ARIMA parameter, the order (number of time lags) of the autoregressive model </param>
      /// <param name="d"> ARIMA parameter, the degree of differencing </param>
      /// <param name="q"> ARIMA parameter, the order of the moving-average model </param>
      /// <param name="P"> ARIMA parameter, autoregressive term for the seasonal part </param>
      /// <param name="D"> ARIMA parameter, differencing term for the seasonal part </param>
      /// <param name="Q"> ARIMA parameter, moving average term for the seasonal part </param>
      /// <param name="m"> ARIMA parameter, the number of periods in each season </param>
      public ArimaParams(int p, int d, int q, int P, int D, int Q, int m)
      {
         this.p = p;
         this.d = d;
         this.q = q;
         this.P = P;
         this.D = D;
         this.Q = Q;
         this.m = m;

         // dependent states
         this._opAR = NewOperatorAR;
         this._opMA = NewOperatorMA;
         _opAR.initializeParams(false);
         _opMA.initializeParams(false);
         this._dp = _opAR.Degree;
         this._dq = _opMA.Degree;
         this._np = _opAR.numParams();
         this._nq = _opMA.numParams();
         //JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
         //ORIGINAL LINE: this._init_seasonal = (D > 0 && m > 0) ? new double[D][m] : null;
         this._init_seasonal = (D > 0 && m > 0) ? RectangularArrays.ReturnRectangularDoubleArray(D, m) : null;
         //JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
         //ORIGINAL LINE: this._init_non_seasonal = (d > 0) ? new double[d][1] : null;
         this._init_non_seasonal = (d > 0) ? RectangularArrays.ReturnRectangularDoubleArray(d, 1) : null;
         this._diff_seasonal = (D > 0 && m > 0) ? new double[D][] : null;
         this._diff_non_seasonal = (d > 0) ? new double[d][] : null;
         this._integrate_seasonal = (D > 0 && m > 0) ? new double[D][] : null;
         this._integrate_non_seasonal = (d > 0) ? new double[d][] : null;
      }

      /// <summary>
      /// ARMA forecast of one data point.
      /// </summary>
      /// <param name="data"> input data </param>
      /// <param name="errors"> array of errors </param>
      /// <param name="index"> index </param>
      /// <returns> one data point </returns>
      //JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
      //ORIGINAL LINE: public double forecastOnePointARMA(final double[] data, final double[] errors, final int index)
      public double forecastOnePointARMA(double[] data, double[] errors, int index)
      {
         //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
         //ORIGINAL LINE: final double estimateAR = _opAR.getLinearCombinationFrom(data, index);
         double estimateAR = _opAR.getLinearCombinationFrom(data, index);
         //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
         //ORIGINAL LINE: final double estimateMA = _opMA.getLinearCombinationFrom(errors, index);
         double estimateMA = _opMA.getLinearCombinationFrom(errors, index);
         //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
         //ORIGINAL LINE: final double forecastValue = estimateAR + estimateMA;
         double forecastValue = estimateAR + estimateMA;
         return forecastValue;
      }

      /// <summary>
      /// Getter for the degree of parameter p
      /// </summary>
      /// <returns> degree of p </returns>
      public int DegreeP { get { return _dp; } }

      /// <summary>
      /// Getter for the degree of parameter q
      /// </summary>
      /// <returns> degree of q </returns>
      public int DegreeQ
      {
         get
         {
            return _dq;
         }
      }

      /// <summary>
      /// Getter for the number of parameters p </summary>
      /// <returns> number of parameters p </returns>
      public int NumParamsP
      {
         get
         {
            return _np;
         }
      }

      /// <summary>
      /// Getter for the number of parameters q
      /// </summary>
      /// <returns> number of parameters q </returns>
      public int NumParamsQ
      {
         get
         {
            return _nq;
         }
      }

      /// <summary>
      /// Getter for the parameter offsets of AR
      /// </summary>
      /// <returns> parameter offsets of AR </returns>
      public int[] OffsetsAR
      {
         get
         {
            return _opAR.paramOffsets();
         }
      }

      /// <summary>
      /// Getter for the parameter offsets of MA
      /// </summary>
      /// <returns> parameter offsets of MA </returns>
      public int[] OffsetsMA
      {
         get
         {
            return _opMA.paramOffsets();
         }
      }

      /// <summary>
      /// Getter for the last integrated seasonal data
      /// </summary>
      /// <returns> integrated seasonal data </returns>
      public double[] LastIntegrateSeasonal
      {
         get
         {
            return _integrate_seasonal[D - 1];
         }
      }

      /// <summary>
      /// Getter for the last integrated NON-seasonal data
      /// </summary>
      /// <returns> NON-integrated NON-seasonal data </returns>
      public double[] LastIntegrateNonSeasonal
      {
         get
         {
            return _integrate_non_seasonal[d - 1];
         }
      }

      /// <summary>
      /// Getter for the last differentiated seasonal data
      /// </summary>
      /// <returns> differentiate seasonal data </returns>
      public double[] LastDifferenceSeasonal
      {
         get
         {
            return _diff_seasonal[D - 1];
         }
      }

      /// <summary>
      /// Getter for the last differentiated NON-seasonal data
      /// </summary>
      /// <returns> differentiated NON-seasonal data </returns>
      public double[] LastDifferenceNonSeasonal
      {
         get
         {
            return _diff_non_seasonal[d - 1];
         }
      }

      /// <summary>
      /// Summary of the parameters
      /// </summary>
      /// <returns> String of summary </returns>
      public string summary()
      {
         return "ModelInterface ParamsInterface:" +
            ", p= " + p +
            ", d= " + d +
            ", q= " + q +
            ", P= " + P +
            ", D= " + D +
            ", Q= " + Q +
            ", m= " + m;
      }

      //==========================================================
      // MUTABLE STATES

      /// <summary>
      /// Setting parameters from a Insight Vector
      ///
      /// It is assumed that the input vector has _np + _nq entries first _np entries are AR-parameters
      ///      and the last _nq entries are MA-parameters
      /// </summary>
      /// <param name="paramVec"> a vector of parameters </param>
      //JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
      //ORIGINAL LINE: public void setParamsFromVector(final TimeSeries.Forecast.matrix.InsightsVector paramVec)
      public InsightsVector ParamsFromVector
      {
         set
         {
            int index = 0;
            //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
            //ORIGINAL LINE: final int[] offsetsAR = getOffsetsAR();
            int[] offsetsAR = OffsetsAR;
            //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
            //ORIGINAL LINE: final int[] offsetsMA = getOffsetsMA();
            int[] offsetsMA = OffsetsMA;
            foreach (int pIdx in offsetsAR)
            {
               _opAR.setParam(pIdx, value.get(index++));
            }
            foreach (int qIdx in offsetsMA)
            {
               _opMA.setParam(qIdx, value.get(index++));
            }
         }
      }

      /// <summary>
      /// Create a Insight Vector that contains the parameters.
      ///
      /// It is assumed that the input vector has _np + _nq entries first _np entries are AR-parameters
      ///      and the last _nq entries are MA-parameters
      /// </summary>
      /// <returns> Insight Vector of parameters </returns>
      public InsightsVector ParamsIntoVector
      {
         get
         {
            int index = 0;
            //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
            //ORIGINAL LINE: final TimeSeries.Forecast.matrix.InsightsVector paramVec = new TimeSeries.Forecast.matrix.InsightsVector(_np + _nq, 0.0);
            InsightsVector paramVec = new InsightsVector(_np + _nq, 0.0);
            //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
            //ORIGINAL LINE: final int[] offsetsAR = getOffsetsAR();
            int[] offsetsAR = OffsetsAR;
            //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
            //ORIGINAL LINE: final int[] offsetsMA = getOffsetsMA();
            int[] offsetsMA = OffsetsMA;
            foreach (int pIdx in offsetsAR)
            {
               paramVec.set(index++, _opAR.GetParam(pIdx));
            }
            foreach (int qIdx in offsetsMA)
            {
               paramVec.set(index++, _opMA.GetParam(qIdx));
            }
            return paramVec;
         }
      }

      public BackShift NewOperatorAR
      {
         get
         {
            return mergeSeasonalWithNonSeasonal(p, P, m);
         }
      }

      public BackShift NewOperatorMA
      {
         get
         {
            return mergeSeasonalWithNonSeasonal(q, Q, m);
         }
      }

      public double[] CurrentARCoefficients
      {
         get
         {
            return _opAR.CoefficientsFlattened;
         }
      }

      public double[] CurrentMACoefficients
      {
         get
         {
            return _opMA.CoefficientsFlattened;
         }
      }

      private BackShift mergeSeasonalWithNonSeasonal(int nonSeasonalLag, int seasonalLag, int seasonalStep)
      {
         //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
         //ORIGINAL LINE: final BackShift nonSeasonal = new BackShift(nonSeasonalLag, true);
         BackShift nonSeasonal = new BackShift(nonSeasonalLag, true);
         //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
         //ORIGINAL LINE: final BackShift seasonal = new BackShift(seasonalLag * seasonalStep, false);
         BackShift seasonal = new BackShift(seasonalLag * seasonalStep, false);
         for (int s = 1; s <= seasonalLag; ++s)
         {
            seasonal.setIndex(s * seasonalStep, true);
         }
         //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
         //ORIGINAL LINE: final BackShift merged = seasonal.apply(nonSeasonal);
         BackShift merged = seasonal.apply(nonSeasonal);
         return merged;
      }

      //================================
      // Differentiation and Integration

      //JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
      //ORIGINAL LINE: public void differentiateSeasonal(final double[] data)
      public void differentiateSeasonal(double[] data)
      {
         double[] current = data;
         for (int j = 0; j < D; ++j)
         {
            //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
            //ORIGINAL LINE: final double[] next = new double[current.length - m];
            double[] next = new double[current.Length - m];
            _diff_seasonal[j] = next;
            //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
            //ORIGINAL LINE: final double[] init = _init_seasonal[j];
            double[] init = _init_seasonal[j];
            Integrator.differentiate(current, next, init, m);
            current = next;
         }
      }

      //JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
      //ORIGINAL LINE: public void differentiateNonSeasonal(final double[] data)
      public void DifferentiateNonSeasonal(double[] data)
      {
         double[] current = data;
         for (int j = 0; j < d; ++j)
         {
            //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
            //ORIGINAL LINE: final double[] next = new double[current.length - 1];
            double[] next = new double[current.Length - 1];
            _diff_non_seasonal[j] = next;
            //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
            //ORIGINAL LINE: final double[] init = _init_non_seasonal[j];
            double[] init = _init_non_seasonal[j];
            Integrator.differentiate(current, next, init, 1);
            current = next;
         }
      }

      //JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
      //ORIGINAL LINE: public void integrateSeasonal(final double[] data)
      public void IntegrateSeasonal(double[] data)
      {
         double[] current = data;
         for (int j = 0; j < D; ++j)
         {
            //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
            //ORIGINAL LINE: final double[] next = new double[current.length + m];
            double[] next = new double[current.Length + m];
            _integrate_seasonal[j] = next;
            //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
            //ORIGINAL LINE: final double[] init = _init_seasonal[j];
            double[] init = _init_seasonal[j];
            Integrator.integrate(current, next, init, m);
            current = next;
         }
      }

      //JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
      //ORIGINAL LINE: public void integrateNonSeasonal(final double[] data)
      public void IntegrateNonSeasonal(double[] data)
      {
         double[] current = data;
         for (int j = 0; j < d; ++j)
         {
            //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
            //ORIGINAL LINE: final double[] next = new double[current.length + 1];
            double[] next = new double[current.Length + 1];
            _integrate_non_seasonal[j] = next;
            //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
            //ORIGINAL LINE: final double[] init = _init_non_seasonal[j];
            double[] init = _init_non_seasonal[j];
            Integrator.integrate(current, next, init, 1);
            current = next;
         }
      }
   }
}