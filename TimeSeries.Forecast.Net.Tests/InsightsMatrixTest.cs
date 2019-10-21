/*
 * Copyright (c) 2017-present, Workday, Inc.
 * All rights reserved.
 *
 * This source code is licensed under the MIT license found in the LICENSE file in the root repository.
 */

namespace TimeSeries.Forecast.Tests
{
   using global::TimeSeries.Forecast.Matrix;
   using System;
   using Xunit;

   public class InsightsMatrixTest
   {
      [Fact]
      public virtual void ConstructorTests()
      {
         double[][] data = new double[][]
         {
            new double[] {3.0, 3.0, 3.0},
            new double[] {3.0, 3.0, 3.0},
            new double[] {3.0, 3.0, 3.0}
         };

         InsightsMatrix im1 = new InsightsMatrix(data, false);
         Assert.True(im1.NumberOfColumns == 3);
         Assert.True(im1.NumberOfRows == 3);
         for (int i = 0; i < im1.NumberOfColumns; i++)
         {
            for (int j = 0; j < im1.NumberOfColumns; j++)
            {
               Assert.True(im1.get(i, j) == 3.0);
            }
         }
         im1.set(0, 0, 0.0);
         Assert.True(im1.get(0, 0) == 0.0);
         im1.set(0, 0, 3.0);

         InsightsVector iv = new InsightsVector(3, 3.0);
         for (int i = 0; i < im1.NumberOfColumns; i++)
         {
            Assert.True(im1.timesVector(iv).get(i) == 27.0);
         }
      }

      //
      [Fact]
      public virtual void SolverTestSimple()
      {
         double[][] A = new double[][]
         {
            new double[] {2.0}
         };
         double[] B = new double[] { 4.0 };
         double[] solution = new double[] { 2.0 };

         InsightsMatrix im = new InsightsMatrix(A, true);
         InsightsVector iv = new InsightsVector(B, true);

         InsightsVector solved = im.solveSPDIntoVector(iv, -1);
         for (int i = 0; i < solved.size(); i++)
         {
            Assert.True(solved.get(i) == solution[i]);
         }
      }

      //
      [Fact]
      public virtual void SolverTestOneSolution()
      {
         double[][] A = new double[][]
         {
            new double[] {1.0, 1.0},
            new double[] {1.0, 2.0}
         };

         double[] B = new double[] { 2.0, 16.0 };
         double[] solution = new double[] { -12.0, 14.0 };

         InsightsMatrix im = new InsightsMatrix(A, true);
         InsightsVector iv = new InsightsVector(B, true);

         InsightsVector solved = im.solveSPDIntoVector(iv, -1);
         for (int i = 0; i < solved.size(); i++)
         {
            Assert.True(solved.get(i) == solution[i]);
         }
      }

      //
      [Fact]
      public virtual void TimesVectorTestSimple()
      {
         double[][] A = new double[][]
         {
            new double[] {1.0, 1.0},
            new double[] {2.0, 2.0}
         };

         double[] x = new double[] { 3.0, 4.0 };
         double[] solution = new double[] { 7.0, 14.0 };

         InsightsMatrix im = new InsightsMatrix(A, true);
         InsightsVector iv = new InsightsVector(x, true);

         InsightsVector solved = im.timesVector(iv);
         for (int i = 0; i < solved.size(); i++)
         {
            Assert.True(solved.get(i) == solution[i]);
         }
      }

      //
      [Fact]
      public virtual void TimesVectorTestIncorrectDimension()
      {
         double[][] A = new double[][]
         {
            new double[] {1.0, 1.0, 1.0},
            new double[] {2.0, 2.0, 2.0},
            new double[] {3.0, 3.0, 3.0}
         };

         double[] x = new double[] { 4.0, 4.0, 4.0, 4.0 };

         InsightsMatrix im = new InsightsMatrix(A, true);
         InsightsVector iv = new InsightsVector(x, true);

         Exception ex = Assert.Throws<Exception>(() => im.timesVector(iv));
         //InsightsVector solved =

         Assert.Equal("[InsightsMatrix][timesVector] size mismatch", ex.Message);
      }
   }
}