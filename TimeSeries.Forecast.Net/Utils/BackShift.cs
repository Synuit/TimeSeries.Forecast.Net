using System;

/*
 * Copyright (c) 2017-present, Workday, Inc.
 * All rights reserved.
 *
 * This source code is licensed under the MIT license found in the LICENSE file in the root repository.
 */

namespace TimeSeries.Forecast.Utils
{
   /// <summary>
   /// Helper class that implements polynomial of back-shift operator
   /// </summary>
   public sealed class BackShift
   {
      private readonly int _degree; // maximum lag, e.g. AR(1) degree will be 1
      private readonly bool[] _indices;
      private int[] _offsets = null;
      private double[] _coeffs = null;

      //Constructor
      public BackShift(int degree, bool initial)
      {
         if (degree < 0)
         {
            throw new Exception("degree must be non-negative");
         }
         this._degree = degree;
         this._indices = new bool[_degree + 1];
         for (int j = 0; j <= _degree; ++j)
         {
            this._indices[j] = initial;
         }
         this._indices[0] = true; // zero index must be true all the time
      }

      public BackShift(bool[] indices, bool copyIndices)
      {
         if (indices == null)
         {
            throw new Exception("null indices given");
         }
         this._degree = indices.Length - 1;
         if (copyIndices)
         {
            this._indices = new bool[_degree + 1];
            Array.Copy(indices, 0, _indices, 0, _degree + 1);
         }
         else
         {
            this._indices = indices;
         }
      }

      public int Degree
      {
         get
         {
            return _degree;
         }
      }

      public double[] CoefficientsFlattened
      {
         get
         {
            if (_degree <= 0 || _offsets == null || _coeffs == null)
            {
               return new double[0];
            }
            int temp = -1;
            foreach (int offset in _offsets)
            {
               if (offset > temp)
               {
                  temp = offset;
               }
            }
            //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
            //ORIGINAL LINE: final int maxIdx = 1 + temp;
            int maxIdx = 1 + temp;
            //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
            //ORIGINAL LINE: final double[] flattened = new double[maxIdx];
            double[] flattened = new double[maxIdx];
            for (int j = 0; j < maxIdx; ++j)
            {
               flattened[j] = 0;
            }
            for (int j = 0; j < _offsets.Length; ++j)
            {
               flattened[_offsets[j]] = _coeffs[j];
            }
            return flattened;
         }
      }

      public void setIndex(int index, bool enable)
      {
         _indices[index] = enable;
      }

      public BackShift apply(BackShift another)
      {
         int mergedDegree = _degree + another._degree;
         bool[] merged = new bool[mergedDegree + 1];
         for (int j = 0; j <= mergedDegree; ++j)
         {
            merged[j] = false;
         }
         for (int j = 0; j <= _degree; ++j)
         {
            if (_indices[j])
            {
               for (int k = 0; k <= another._degree; ++k)
               {
                  merged[j + k] = merged[j + k] || another._indices[k];
               }
            }
         }
         return new BackShift(merged, false);
      }

      public void initializeParams(bool includeZero)
      {
         _indices[0] = includeZero;
         _offsets = null;
         _coeffs = null;
         int nonzeroCount = 0;
         for (int j = 0; j <= _degree; ++j)
         {
            if (_indices[j])
            {
               ++nonzeroCount;
            }
         }
         _offsets = new int[nonzeroCount]; // cannot be 0 as 0-th index is always true
         _coeffs = new double[nonzeroCount];
         int coeffIndex = 0;
         for (int j = 0; j <= _degree; ++j)
         {
            if (_indices[j])
            {
               _offsets[coeffIndex] = j;
               _coeffs[coeffIndex] = 0;
               ++coeffIndex;
            }
         }
      }

      // MAKE SURE to initializeParams before calling below methods
      public int numParams()
      {
         return _offsets.Length;
      }

      public int[] paramOffsets()
      {
         return _offsets;
      }

      //
      public double GetParam(int paramIndex)
      {
         for (int j = 0; j < _offsets.Length; ++j)
         {
            if (_offsets[j] == paramIndex)
            {
               return _coeffs[j];
            }
         }
         throw new Exception("invalid parameter index: " + paramIndex);
      }

      public double[] AllParam
      {
         get
         {
            return this._coeffs;
         }
      }

      //JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
      //ORIGINAL LINE: public void setParam(final int paramIndex, final double paramValue)
      public void setParam(int paramIndex, double paramValue)
      {
         int offsetIndex = -1;
         for (int j = 0; j < _offsets.Length; ++j)
         {
            if (_offsets[j] == paramIndex)
            {
               offsetIndex = j;
               break;
            }
         }
         if (offsetIndex == -1)
         {
            throw new Exception("invalid parameter index: " + paramIndex);
         }
         _coeffs[offsetIndex] = paramValue;
      }

      public void copyParamsToArray(double[] dest)
      {
         Array.Copy(_coeffs, 0, dest, 0, _coeffs.Length);
      }

      public double getLinearCombinationFrom(double[] timeseries, int tsOffset)
      {
         double linearSum = 0;
         for (int j = 0; j < _offsets.Length; ++j)
         {
            linearSum += timeseries[tsOffset - _offsets[j]] * _coeffs[j];
         }
         return linearSum;
      }
   }
}