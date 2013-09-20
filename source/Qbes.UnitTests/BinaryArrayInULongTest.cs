using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Qbes.Common.Logic.DataStructures;

namespace Qbes.UnitTests
{
    /// <summary>
    ///This is a test class for BinaryArrayInULongTest and is intended
    ///to contain all BinaryArrayInULongTest Unit Tests
    ///</summary>
   [TestClass()]
   public class BinaryArrayInULongTest
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
      ///A test for BinaryArrayInULong Constructor
      ///</summary>
      [TestMethod()]
      public void BinaryArrayInULongConstructorTest()
      {
         BinaryArrayInLong target = new BinaryArrayInLong();
         long received = target.GetValue();
         Assert.IsTrue(received == 0, "Expected 0 but received " + received);

         target = new BinaryArrayInLong(new bool[] { true, false, true, false, true, false, true, false, true, false, true, false });
         received = target.GetValue();
         Assert.IsTrue(received == 1365, "Expected 1365 but received " + received);

         target = new BinaryArrayInLong(new bool[] { false, true, false, true, false, true, false, true, false, true, false, true });
         received = target.GetValue();
         Assert.IsTrue(received == 2730, "Expected 2730 but received " + received);

         bool[] allTrue = new bool[64];
         for (int i = 0; i < 64; i++)
         {
            allTrue[i] = true;
         }
         target = new BinaryArrayInLong(allTrue);
         received = target.GetValue();
         Assert.IsTrue(received == -1, "Expected -1 but received " + received);
      }

      /// <summary>
      ///A test for UpdateArray
      ///</summary>
      [TestMethod()]
      public void UpdateArrayTest()
      {
         BinaryArrayInLong target = new BinaryArrayInLong();

         target.UpdateArray(new bool[] { false, false, false, false, false, false, false, false });
         long received = target.GetValue();
         Assert.IsTrue(received == 0, "Expected 0 but received " + received);

         target.UpdateArray(new bool[] { true, false, true, false, true, false, true, false, true, false, true, false });
         received = target.GetValue();
         Assert.IsTrue(received == 1365, "Expected 1365 but received " + received);

         target.UpdateArray(new bool[] { false, true, false, true, false, true, false, true, false, true, false, true });
         received = target.GetValue();
         Assert.IsTrue(received == 2730, "Expected 2730 but received " + received);

         bool[] allTrue = new bool[64];
         for (int i = 0; i < 64; i++)
         {
            allTrue[i] = true;
         }
         target.UpdateArray(allTrue);
         received = target.GetValue();
         Assert.IsTrue(received == -1, "Expected -1 but received " + received);
      }

      /// <summary>
      ///A test for Item
      ///</summary>
      [TestMethod()]
      public void ItemTest()
      {
         BinaryArrayInLong target = new BinaryArrayInLong();
         Random r = new Random();

         for (int pass = 1; pass < 4; pass++)
         {
            bool[] expected = new bool[64];

            for (int i = 0; i < 64; i++)
            {
               expected[i] = (r.Next(2) == 1);
               target[i] = expected[i];
            }

            // read the values and compare with the expected
            for (int i = 0; i < 64; i++)
            {
               bool received = target[i];
               Assert.IsTrue(received == expected[i], "Stored data do not match at index " + i + "; pass: " + pass + "; received: " + received.ToString() + "; expected: " + expected[i]);
            }
         }
      }
   }
}
