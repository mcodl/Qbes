using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Qbes.Common.Logic.DataStructures;

namespace Qbes.UnitTests
{
    /// <summary>
    ///This is a test class for BinaryArrayInByteTest and is intended
    ///to contain all BinaryArrayInByteTest Unit Tests
    ///</summary>
   [TestClass()]
   public class BinaryArrayInByteTest
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
      ///A test for BinaryArrayInByte Constructor
      ///</summary>
      [TestMethod()]
      public void BinaryArrayInByteConstructorTest()
      {
         BinaryArrayInByte target = new BinaryArrayInByte();
         byte received = target.GetValue();
         Assert.IsTrue(received == 0, "Expected 0 but received " + received);

         target = new BinaryArrayInByte(new bool[] { true, false, true, false, true, false, true, false });
         received = target.GetValue();
         Assert.IsTrue(received == 85, "Expected 85 but received " + received);

         target = new BinaryArrayInByte(new bool[] { false, true, false, true, false, true, false, true });
         received = target.GetValue();
         Assert.IsTrue(received == 170, "Expected 170 but received " + received);
      }

      /// <summary>
      ///A test for UpdateArray
      ///</summary>
      [TestMethod()]
      public void UpdateArrayTest()
      {
         BinaryArrayInByte target = new BinaryArrayInByte();

         target.UpdateArray(new bool[] { false, false, false, false, false, false, false, false });
         byte received = target.GetValue();
         Assert.IsTrue(received == 0, "Expected 0 but received " + received);

         target.UpdateArray(new bool[] { true, false, true, false, true, false, true, false });
         received = target.GetValue();
         Assert.IsTrue(received == 85, "Expected 85 but received " + received);

         target.UpdateArray(new bool[] { false, true, false, true, false, true, false, true });
         received = target.GetValue();
         Assert.IsTrue(received == 170, "Expected 170 but received " + received);
      }

      /// <summary>
      ///A test for Item
      ///</summary>
      [TestMethod()]
      public void ItemTest()
      {
         BinaryArrayInByte target = new BinaryArrayInByte();
         Random r = new Random();

         for (int pass = 1; pass < 4; pass++)
         {
            bool[] expected = new bool[8];

            for (int i = 0; i < 8; i++)
            {
               expected[i] = (r.Next(2) == 1);
               target[i] = expected[i];
            }

            // read the values and compare with the expected
            for (int i = 0; i < 8; i++)
            {
               bool received = target[i];
               Assert.IsTrue(received == expected[i], "Stored data do not match at index " + i + "; pass: " + pass + "; received: " + received.ToString() + "; expected: " + expected[i]);
            }
         }
      }
   }
}
