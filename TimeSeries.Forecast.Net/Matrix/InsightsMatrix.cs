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
   /// InsightsMatrix
   ///
   /// <para>
   /// A small set of linear algebra methods to be used in ARIMA
   /// </para>
   /// </summary>
   [Serializable]
   public class InsightsMatrix
   {
      public const long serialVersionUID = 42L;

      // Primary
      protected internal int _m = -1;

      protected internal int _n = -1;
      protected internal double[][] _data = null;
      protected internal bool _valid = false;

      // Secondary
      protected internal bool _cholZero = false;

      protected internal bool _cholPos = false;
      protected internal bool _cholNeg = false;
      protected internal double[] _cholD = null;
      protected internal double[][] _cholL = null;

      //=====================================================================
      // Constructors
      //=====================================================================

      /// <summary>
      /// Constructor for InsightsMatrix
      /// </summary>
      /// <param name="data"> 2-dimensional double array with pre-populated values </param>
      /// <param name="makeDeepCopy"> if TRUE, allocated new memory space and copy data over
      ///                     if FALSE, re-use the given memory space and overwrites on it </param>
      public InsightsMatrix(double[][] data, bool makeDeepCopy)
      {
         if (_valid = isValid2D(data))
         {
            _m = data.Length;
            _n = data[0].Length;
            if (!makeDeepCopy)
            {
               _data = data;
            }
            else
            {
               _data = copy2DArray(data);
            }
         }
      }

      //=====================================================================
      // END of Constructors
      //=====================================================================
      //=====================================================================
      // Helper Methods
      //=====================================================================

      /// <summary>
      /// Determine whether a 2-dimensional array is in valid matrix format.
      /// </summary>
      /// <param name="matrix"> 2-dimensional double array </param>
      /// <returns> TRUE, matrix is in valid format
      ///         FALSE, matrix is not in valid format </returns>
      private static bool isValid2D(double[][] matrix)
      {
         bool result = true;
         if (matrix == null || matrix[0] == null || matrix[0].Length == 0)
         {
            throw new Exception("[InsightsMatrix][constructor] null data given");
         }
         else
         {
            int row = matrix.Length;
            int col = matrix[0].Length;
            for (int i = 1; i < row; ++i)
            {
               if (matrix[i] == null || matrix[i].Length != col)
               {
                  result = false;
               }
            }
         }

         return result;
      }

      /// <summary>
      /// Create a copy of 2-dimensional double array by allocating new memory space and copy data over
      /// </summary>
      /// <param name="source"> source 2-dimensional double array </param>
      /// <returns> new copy of the source 2-dimensional double array </returns>
      private static double[][] copy2DArray(double[][] source)
      {
         if (source == null)
         {
            return null;
         }
         else if (source.Length == 0)
         {
            return new double[0][];
         }

         int row = source.Length;
         double[][] target = new double[row][];
         for (int i = 0; i < row; i++)
         {
            if (source[i] == null)
            {
               target[i] = null;
            }
            else
            {
               int rowLength = source[i].Length;
               target[i] = new double[rowLength];
               Array.Copy(source[i], 0, target[i], 0, rowLength);
            }
         }
         return target;
      }

      //=====================================================================
      // END of Helper Methods
      //=====================================================================
      //=====================================================================
      // Getters & Setters
      //=====================================================================

      /// <summary>
      /// Getter for number of rows of the matrix
      /// </summary>
      /// <returns> number of rows </returns>
      public virtual int NumberOfRows
      {
         get
         {
            return _m;
         }
      }

      /// <summary>
      /// Getter for number of columns of the matrix
      /// </summary>
      /// <returns> number of columns </returns>
      public virtual int NumberOfColumns
      {
         get
         {
            return _n;
         }
      }

      /// <summary>
      /// Getter for a particular element in the matrix
      /// </summary>
      /// <param name="i"> i-th row </param>
      /// <param name="j"> j-th column </param>
      /// <returns> the element from the i-th row and j-th column from the matrix </returns>
      public virtual double get(int i, int j)
      {
         return _data[i][j];
      }

      /// <summary>
      /// Setter to modify a particular element in the matrix
      /// </summary>
      /// <param name="i"> i-th row </param>
      /// <param name="j"> j-th column </param>
      /// <param name="val"> new value </param>
      public virtual void set(int i, int j, double val)
      {
         _data[i][j] = val;
      }

      //=====================================================================
      // END of Getters & Setters
      //=====================================================================
      //=====================================================================
      // Basic Linear Algebra operations
      //=====================================================================

      /// <summary>
      /// Multiply a InsightMatrix (n x m) by a InsightVector (m x 1)
      /// </summary>
      /// <param name="v"> a InsightVector </param>
      /// <returns> a InsightVector of dimension (n x 1) </returns>
      public virtual InsightsVector timesVector(InsightsVector v)
      {
         if (!_valid || !v._valid || _n != v._m)
         {
            throw new Exception("[InsightsMatrix][timesVector] size mismatch");
         }
         double[] data = new double[_m];
         double dotProduc;
         for (int i = 0; i < _m; ++i)
         {
            InsightsVector rowVector = new InsightsVector(_data[i], false);
            dotProduc = rowVector.dot(v);
            data[i] = dotProduc;
         }
         return new InsightsVector(data, false);
      }

      // More linear algebra operations

      /// <summary>
      /// Compute the Cholesky Decomposition
      /// </summary>
      /// <param name="maxConditionNumber"> maximum condition number </param>
      /// <returns> TRUE, if the process succeed
      ///         FALSE, otherwise </returns>
      //JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
      //ORIGINAL LINE: private boolean computeCholeskyDecomposition(final double maxConditionNumber)
      private bool computeCholeskyDecomposition(double maxConditionNumber)
      {
         _cholD = new double[_m];
         //JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
         //ORIGINAL LINE: _cholL = new double[_m][_n];
         _cholL = RectangularArrays.ReturnRectangularDoubleArray(_m, _n);
         int i;
         int j;
         int k;
         double val;
         double currentMax = -1;
         // Backward marching method
         for (j = 0; j < _n; ++j)
         {
            val = 0;
            for (k = 0; k < j; ++k)
            {
               val += _cholD[k] * _cholL[j][k] * _cholL[j][k];
            }
            double diagTemp = _data[j][j] - val;
            //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
            //ORIGINAL LINE: final int diagSign = (int)(Math.signum(diagTemp));
            int diagSign = (int)(Math.Sign(diagTemp));
            switch (diagSign)
            {
               case 0: // singular diagonal value detected
                  if (maxConditionNumber < -0.5)
                  { // no bound on maximum condition number
                     _cholZero = true;
                     _cholL = null;
                     _cholD = null;
                     return false;
                  }
                  else
                  {
                     _cholPos = true;
                  }
                  break;

               case 1:
                  _cholPos = true;
                  break;

               case -1:
                  _cholNeg = true;
                  break;
            }
            if (maxConditionNumber > -0.5)
            {
               if (currentMax <= 0.0)
               { // this is the first time
                  if (diagSign == 0)
                  {
                     diagTemp = 1.0;
                  }
               }
               else
               { // there was precedent
                  if (diagSign == 0)
                  {
                     diagTemp = Math.Abs(currentMax / maxConditionNumber);
                  }
                  else
                  {
                     if (Math.Abs(diagTemp * maxConditionNumber) < currentMax)
                     {
                        diagTemp = diagSign * Math.Abs(currentMax / maxConditionNumber);
                     }
                  }
               }
            }
            _cholD[j] = diagTemp;
            if (Math.Abs(diagTemp) > currentMax)
            {
               currentMax = Math.Abs(diagTemp);
            }
            _cholL[j][j] = 1;
            for (i = j + 1; i < _m; ++i)
            {
               val = 0;
               for (k = 0; k < j; ++k)
               {
                  val += _cholD[k] * _cholL[j][k] * _cholL[i][k];
               }
               val = ((_data[i][j] + _data[j][i]) / 2 - val) / _cholD[j];
               _cholL[j][i] = val;
               _cholL[i][j] = val;
            }
         }
         return true;
      }

      /// <summary>
      /// Solve SPD(Symmetric positive definite) into vector
      /// </summary>
      /// <param name="b"> vector </param>
      /// <param name="maxConditionNumber"> maximum condition number </param>
      /// <returns> solution vector of SPD </returns>
      //JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
      //ORIGINAL LINE: public InsightsVector solveSPDIntoVector(InsightsVector b, final double maxConditionNumber)
      public virtual InsightsVector solveSPDIntoVector(InsightsVector b, double maxConditionNumber)
      {
         if (!_valid || b == null || _n != b._m)
         {
            // invalid linear system
            throw new Exception("[InsightsMatrix][solveSPDIntoVector] invalid linear system");
         }
         if (_cholL == null)
         {
            // computing Cholesky Decomposition
            this.computeCholeskyDecomposition(maxConditionNumber);
         }
         if (_cholZero)
         {
            // singular matrix. returning null
            return null;
         }

         double[] y = new double[_m];
         double[] bt = new double[_n];
         int i;
         int j;
         for (i = 0; i < _m; ++i)
         {
            bt[i] = b._data[i];
         }
         double val;
         for (i = 0; i < _m; ++i)
         {
            val = 0;
            for (j = 0; j < i; ++j)
            {
               val += _cholL[i][j] * y[j];
            }
            y[i] = bt[i] - val;
         }
         for (i = _m - 1; i >= 0; --i)
         {
            val = 0;
            for (j = i + 1; j < _n; ++j)
            {
               val += _cholL[i][j] * bt[j];
            }
            bt[i] = y[i] / _cholD[i] - val;
         }
         return new InsightsVector(bt, false);
      }

      /// <summary>
      /// Computu the product of the matrix (m x n) and its transpose (n x m)
      /// </summary>
      /// <returns> matrix of size (m x m) </returns>
      public virtual InsightsMatrix computeAAT()
      {
         if (!_valid)
         {
            throw new Exception("[InsightsMatrix][computeAAT] invalid matrix");
         }
         //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
         //ORIGINAL LINE: final double[][] data = new double[_m][_m];
         //JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
         //ORIGINAL LINE: double[][] data = new double[_m][_m];
         double[][] data = RectangularArrays.ReturnRectangularDoubleArray(_m, _m);
         for (int i = 0; i < _m; ++i)
         {
            //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
            //ORIGINAL LINE: final double[] rowI = _data[i];
            double[] rowI = _data[i];
            for (int j = 0; j < _m; ++j)
            {
               //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
               //ORIGINAL LINE: final double[] rowJ = _data[j];
               double[] rowJ = _data[j];
               double temp = 0;
               for (int k = 0; k < _n; ++k)
               {
                  temp += rowI[k] * rowJ[k];
               }
               data[i][j] = temp;
            }
         }
         return new InsightsMatrix(data, false);
      }

      //=====================================================================
      // END of Basic Linear Algebra operations
      //=====================================================================
   }
}