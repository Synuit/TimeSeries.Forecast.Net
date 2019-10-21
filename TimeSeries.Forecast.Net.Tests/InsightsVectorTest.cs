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

   public class InsightsVectorTest
   {
      [Fact]
      public virtual void ConstructorTests()
      {
         InsightsVector iv = new InsightsVector(10, 3.0);

         Assert.True(iv.size() == 10);
         for (int i = 0; i < iv.size(); i++)
         {
            Assert.True(iv.get(i) == 3.0);
         }

         double[] data = new double[] { 3.0, 3.0, 3.0, 3.0, 3.0, 3.0, 3.0, 3.0, 3.0, 3.0 };
         InsightsVector iv3 = new InsightsVector(data, false);
         for (int i = 0; i < iv3.size(); i++)
         {
            Assert.True(iv3.get(i) == 3.0);
         }
      }

      //
      [Fact]
      public virtual void DotOperationTest()
      {
         InsightsVector rowVec = new InsightsVector(3, 1.1);
         InsightsVector colVec = new InsightsVector(3, 2.2);

         double expect = 1.1 * 2.2 * 3;
         double actual = rowVec.dot(colVec);
         Assert.Equal(expect, actual, 0);
      }

      //
      [Fact]
      public virtual void DotOperationExceptionTest()
      {
         InsightsVector rowVec = new InsightsVector(4, 1);
         InsightsVector colVec = new InsightsVector(3, 2);
         Exception ex = Assert.Throws<Exception>(() => rowVec.dot(colVec));
         Assert.Equal("[InsightsVector][dot] invalid vector size.", ex.Message);
      }
   }
}