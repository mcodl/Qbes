using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Qbes.Common.Logic.DataStructures;

namespace Qbes.UnitTests
{
    
    
    /// <summary>
    ///This is a test class for BinaryMatrix3DTest and is intended
    ///to contain all BinaryMatrix3DTest Unit Tests
    ///</summary>
   [TestClass()]
   public class BinaryMatrix3DTest
   {
      private TestContext testContextInstance;

      /// <summary>
      ///Gets or sets the test context which provides
      ///information about and functionality for the current test run.
      ///</summary>
      public TestContext TestContext
      {
         get
         {
            return testContextInstance;
         }
         set
         {
            testContextInstance = value;
         }
      }

      #region Additional test attributes
      // 
      //You can use the following additional attributes as you write your tests:
      //
      //Use ClassInitialize to run code before running the first test in the class
      //[ClassInitialize()]
      //public static void MyClassInitialize(TestContext testContext)
      //{
      //}
      //
      //Use ClassCleanup to run code after all tests in a class have run
      //[ClassCleanup()]
      //public static void MyClassCleanup()
      //{
      //}
      //
      //Use TestInitialize to run code before running each test
      //[TestInitialize()]
      //public void MyTestInitialize()
      //{
      //}
      //
      //Use TestCleanup to run code after each test has run
      //[TestCleanup()]
      //public void MyTestCleanup()
      //{
      //}
      //
      #endregion

      /// <summary>
      ///A test for BinaryMatrix3D Constructor
      ///</summary>
      [TestMethod()]
      public void BinaryMatrix3DConstructorTest()
      {
         bool[][][] initialData = new bool[8][][];
         for (int x = 0; x < 8; x++)
         {
            initialData[x] = new bool[8][];
            for (int y = 0; y < 8; y++)
            {
               initialData[x][y] = new bool[8];
            }
         }

         BinaryMatrix3D target = new BinaryMatrix3D(initialData);
         
         // check all zeros
         for (int x = 0; x < 8; x++)
         {
            for (int y = 0; y < 8; y++)
            {
               for (int z = 0; z < 8; z++)
               {
                  bool received = target.Get(x, y, z);
                  Assert.IsTrue(!received, "Excepted false but received " + received.ToString());
               }
            }
         }

         // create with a random matrix
         Random r = new Random();
         for (int x = 0; x < 8; x++)
         {
            for (int y = 0; y < 8; y++)
            {
               for (int z = 0; z < 8; z++)
               {
                  initialData[x][y][z] = (r.Next(2) == 1);
               }
            }
         }

         target = new BinaryMatrix3D(initialData);
         // check against expected
         for (int x = 0; x < 8; x++)
         {
            for (int y = 0; y < 8; y++)
            {
               for (int z = 0; z < 8; z++)
               {
                  bool received = target.Get(x, y, z);
                  Assert.IsTrue(received == initialData[x][y][z], "Excepted " + initialData[x][y][z] + " but received " + received.ToString());
               }
            }
         }
      }

      /// <summary>
      ///A test for Get and set
      ///</summary>
      [TestMethod()]
      public void GetSetTest()
      {
         BinaryMatrix3D target = new BinaryMatrix3D();
         bool[][][] expected = new bool[8][][];
         for (int x = 0; x < 8; x++)
         {
            expected[x] = new bool[8][];
            for (int y = 0; y < 8; y++)
            {
               expected[x][y] = new bool[8];
            }
         }

         Random r = new Random();
         for (int pass = 1; pass < 4; pass++)
         {
            // store values
            for (int x = 0; x < 8; x++)
            {
               for (int y = 0; y < 8; y++)
               {
                  for (int z = 0; z < 8; z++)
                  {
                     expected[x][y][z] = (r.Next(2) == 1);
                     target.Set(x, y, z, expected[x][y][z]);
                  }
               }
            }

            // check values
            for (int x = 0; x < 8; x++)
            {
               for (int y = 0; y < 8; y++)
               {
                  for (int z = 0; z < 8; z++)
                  {
                     bool received = target.Get(x, y, z);
                     Assert.IsTrue(received == expected[x][y][z], "Pass " + pass + " Excepted " + expected[x][y][z] + " but received " + received.ToString());
                  }
               }
            }
         }
      }

      /// <summary>
      ///A test for UpdateMatrix
      ///</summary>
      [TestMethod()]
      public void UpdateMatrixTest()
      {
         BinaryMatrix3D target = new BinaryMatrix3D();

         bool[][][] expected = new bool[8][][];
         for (int x = 0; x < 8; x++)
         {
            expected[x] = new bool[8][];
            for (int y = 0; y < 8; y++)
            {
               expected[x][y] = new bool[8];
            }
         }

         target.UpdateMatrix(expected);

         // check all zeros
         for (int x = 0; x < 8; x++)
         {
            for (int y = 0; y < 8; y++)
            {
               for (int z = 0; z < 8; z++)
               {
                  bool received = target.Get(x, y, z);
                  Assert.IsTrue(!received, "Excepted false but received " + received.ToString());
               }
            }
         }

         // create with a random matrix
         Random r = new Random();
         for (int x = 0; x < 8; x++)
         {
            for (int y = 0; y < 8; y++)
            {
               for (int z = 0; z < 8; z++)
               {
                  expected[x][y][z] = (r.Next(2) == 1);
               }
            }
         }

         target.UpdateMatrix(expected);
         // check against expected
         for (int x = 0; x < 8; x++)
         {
            for (int y = 0; y < 8; y++)
            {
               for (int z = 0; z < 8; z++)
               {
                  bool received = target.Get(x, y, z);
                  Assert.IsTrue(received == expected[x][y][z], "Excepted " + expected[x][y][z] + " but received " + received.ToString());
               }
            }
         }
      }
   }
}
