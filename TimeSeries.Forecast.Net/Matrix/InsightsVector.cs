using System;

/*
 * Copyright (c) 2017-present, Workday, Inc.
 * All rights reserved.
 *
 * This source code is licensed under the MIT license found in the LICENSE file in the root repository.
 */

namespace TimeSeries.Forecast.Matrix
{
   /// <summary>
   /// InsightsVector
   ///
   /// <para> Vector of double entries </para>
   /// </summary>
   [Serializable]
   public class InsightsVector
   {
      private const long serialVersionUID = 43L;

      protected internal int _m = -1;
      protected internal double[] _data = null;
      protected internal bool _valid = false;

      //=====================================================================
      // Constructors
      //=====================================================================

      /// <summary>
      /// Constructor for InsightVector
      /// </summary>
      /// <param name="m"> size of the vector </param>
      /// <param name="value"> initial value for all entries </param>
      public InsightsVector(int m, double value)
      {
         if (m <= 0)
         {
            throw new Exception("[InsightsVector] invalid size");
         }
         else
         {
            _data = new double[m];
            for (int j = 0; j < m; ++j)
            {
               _data[j] = value;
            }
            _m = m;
            _valid = true;
         }
      }

      /// <summary>
      /// Constructor for InsightVector
      /// </summary>
      /// <param name="data"> 1-dimensional double array with pre-populated values </param>
      /// <param name="deepCopy"> if TRUE, allocated new memory space and copy data over
      ///                 if FALSE, re-use the given memory space and overwrites on it </param>
      public InsightsVector(double[] data, bool deepCopy)
      {
         if (data == null || data.Length == 0)
         {
            throw new Exception("[InsightsVector] invalid data");
         }
         else
         {
            _m = data.Length;
            if (deepCopy)
            {
               _data = new double[_m];
               Array.Copy(data, 0, _data, 0, _m);
            }
            else
            {
               _data = data;
            }
            _valid = true;
         }
      }

      //=====================================================================
      // END of Constructors
      //=====================================================================
      //=====================================================================
      // Helper Methods
      //=====================================================================

      /// <summary>
      /// Create and allocate memory for a new copy of double array of current elements in the vector
      /// </summary>
      /// <returns> the new copy </returns>
      public virtual double[] deepCopy()
      {
         double[] dataDeepCopy = new double[_m];
         Array.Copy(_data, 0, dataDeepCopy, 0, _m);
         return dataDeepCopy;
      }

      //=====================================================================
      // END of Helper Methods
      //=====================================================================
      //=====================================================================
      // Getters & Setters
      //=====================================================================

      /// <summary>
      /// Getter for the i-th element in the vector
      /// </summary>
      /// <param name="i"> element index </param>
      /// <returns> the i-th element </returns>
      public virtual double get(int i)
      {
         if (!_valid)
         {
            throw new Exception("[InsightsVector] invalid Vector");
         }
         else if (i >= _m)
         {
            throw new System.IndexOutOfRangeException(string.Format("[InsightsVector] Index: {0:D}, Size: {1:D}", i, _m));
         }
         return _data[i];
      }

      /// <summary>
      /// Getter for the size of the vector
      /// </summary>
      /// <returns> size of the vector </returns>
      public virtual int size()
      {
         if (!_valid)
         {
            throw new Exception("[InsightsVector] invalid Vector");
         }

         return _m;
      }

      /// <summary>
      /// Setter to modify a element in the vector
      /// </summary>
      /// <param name="i"> element index </param>
      /// <param name="val"> new value </param>
      public virtual void set(int i, double val)
      {
         if (!_valid)
         {
            throw new Exception("[InsightsVector] invalid Vector");
         }
         else if (i >= _m)
         {
            throw new System.IndexOutOfRangeException(string.Format("[InsightsVector] Index: {0:D}, Size: {1:D}", i, _m));
         }
         _data[i] = val;
      }

      //=====================================================================
      // END of Getters & Setters
      //=====================================================================
      //=====================================================================
      // Basic Linear Algebra operations
      //=====================================================================

      /// <summary>
      /// Perform dot product operation with another vector of the same size
      /// </summary>
      /// <param name="vector"> vector of the same size </param>
      /// <returns> dot product of the two vector </returns>
      public virtual double dot(InsightsVector vector)
      {
         if (!_valid || !vector._valid)
         {
            throw new Exception("[InsightsVector] invalid Vector");
         }
         else if (_m != vector.size())
         {
            throw new Exception("[InsightsVector][dot] invalid vector size.");
         }

         double sumOfProducts = 0;
         for (int i = 0; i < _m; i++)
         {
            sumOfProducts += _data[i] * vector.get(i);
         }
         return sumOfProducts;
      }

      //=====================================================================
      // END of Basic Linear Algebra operations
      //=====================================================================
   }
}