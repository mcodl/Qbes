using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Qbes.Common.Logic.DataStructures;

namespace Qbes.UnitTests
{
    /// <summary>
    ///This is a test class for DataInUintTest and is intended
    ///to contain all DataInUintTest Unit Tests
    ///</summary>
   [TestClass()]
   public class DataInUIntTest
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
      ///A test for DataInUint Constructor
      ///</summary>
      [TestMethod()]
      public void DataInUintConstructorTest()
      {
         DataInInt target = new DataInInt();

         CheckErased(target);
      }

      /// <summary>
      ///A test for Erase
      ///</summary>
      [TestMethod()]
      public void EraseTest()
      {
         DataInInt target = new DataInInt();

         // fill with random values
         byte[] buffer = new byte[4];
         Random r = new Random();
         r.NextBytes(buffer);
         for (int i = 0; i < 4; i++)
         {
            target.StoreByte(buffer[i], i * 8);
         }

         // full erase test
         target.Erase(0, 32);
         CheckErased(target);

         // partial erase test
         for (int pass = 1; pass < 4; pass++)
         {
            r.NextBytes(buffer);
            for (int i = 0; i < 4; i++)
            {
               target.StoreByte(buffer[i], i * 8);
            }

            for (int i = 0; i < 4; i++)
            {
               target.Erase(i * 8, 8);
               byte received = target.GetByte(i * 8);
               Assert.IsTrue(received == 0, "Erase failed at position " + (i * 8));
            }
         }
      }

      /// <summary>
      ///A test for GetBool and SetBool
      ///</summary>
      [TestMethod()]
      public void GetSetBoolTest()
      {
         DataInInt target = new DataInInt();

         // prepare random bool array and write to target
         bool[] expected = new bool[32];
         Random r = new Random();
         for (int pass = 1; pass < 4; pass++)
         {
            for (int i = 0; i < 32; i++)
            {
               expected[i] = (r.Next(2) == 1);
               target.StoreBool(expected[i], i);
            }

            // read the values and compare with the expected
            for (int i = 0; i < 32; i++)
            {
               bool received = target.GetBool(i);
               Assert.IsTrue(received == expected[i], "Stored data do not match at index " + i + "; pass: " + pass + "; received: " + received.ToString() + "; expected: " + expected[i]);
            }
         }
      }

      /// <summary>
      ///A test for GetByte and SetByte
      ///</summary>
      [TestMethod()]
      public void GetSetByteTest()
      {
         DataInInt target = new DataInInt();

         // prepare random bool array and write to target
         byte[] expected = new byte[4];
         Random r = new Random();
         for (int pass = 1; pass < 4; pass++)
         {
            r.NextBytes(expected);
            for (int i = 0; i < 4; i++)
            {
               target.StoreByte(expected[i], i * 8);
            }

            // read the values and compare with the expected
            for (int i = 0; i < 4; i++)
            {
               byte received = target.GetByte(i * 8);
               Assert.IsTrue(received == expected[i], "Stored data do not match at index " + i + "; pass: " + pass + "; received: " + received.ToString() + "; expected: " + expected[i]);
            }
         }
      }

      /// <summary>
      ///A test for GetUshort and SetUshort
      ///</summary>
      [TestMethod()]
      public void GetSetUshortTest()
      {
         DataInInt target = new DataInInt();

         // prepare random short array and write to target
         ushort[] expected = new ushort[2];
         Random r = new Random();
         for (int pass = 1; pass < 4; pass++)
         {
            for (int i = 0; i < 2; i++)
            {
               expected[i] = (ushort)r.Next(65536);
               target.StoreUShort(expected[i], i * 16);
            }

            // read the values and compare with the expected
            for (int i = 0; i < 2; i++)
            {
               ushort received = target.GetUShort(i * 16);
               Assert.IsTrue(received == expected[i], "Stored data do not match at index " + i + "; pass: " + pass + "; received: " + received.ToString() + "; expected: " + expected[i]);
            }
         }
      }

      #region Helper methods
      private void CheckErased(DataInInt target)
      {
         for (int i = 0; i < 32; i++)
         {
            Assert.IsFalse(target.GetBool(i), "Data not erased at index: " + i);
         }
      }
      #endregion
   }
}
