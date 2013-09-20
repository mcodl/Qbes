using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Qbes.Common.Logic;
using Qbes.UnitTests.Helpers;

namespace Qbes.UnitTests
{
   /// <summary>
   /// This test class contains tests for area functions.
   /// </summary>
   [TestClass]
   public class AreaTest
   {
      /// <summary>
      /// Creates a new test instance
      /// </summary>
      public AreaTest()
      {
         //
         // TODO: Add constructor logic here
         //
      }

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
      // You can use the following additional attributes as you write your tests:
      //

      /// <summary>
      /// Initializes the world size so that segment functions work properly.
      /// </summary>
      /// <param name="testContext"></param>
      [ClassInitialize()]
      public static void MyClassInitialize(TestContext testContext)
      {
         WorldSizeHelper.SetWorldSize();
      }

      // Use ClassCleanup to run code after all tests in a class have run
      // [ClassCleanup()]
      // public static void MyClassCleanup() { }
      //
      // Use TestInitialize to run code before running each test 
      // [TestInitialize()]
      // public void MyTestInitialize() { }
      //
      // Use TestCleanup to run code after each test has run
      // [TestCleanup()]
      // public void MyTestCleanup() { }
      //
      #endregion

      /// <summary>
      /// Tests the Area.GenerateKey() static method.
      /// </summary>
      [TestMethod]
      public void GenerateKeyTest()
      {
         Dictionary<int, bool> data = new Dictionary<int, bool>();

         // add all possible area keys
         for (int x = 0; x < 65536; x += 64)
         {
            for (int y = 0; y < 256; y += 64)
            {
               for (int z = 0; z < 65536; z += 64)
               {
                  int key = Segment.GenerateKey(x, y, z);
                  data.Add(key, true);
               }
            }
         }

         // check if shifts work
         Assert.AreEqual(Segment.GenerateKey(-64, 0, 0), Segment.GenerateKey(65536 - 64, 0, 0));
         Assert.AreEqual(Segment.GenerateKey(65536, 0, 0), Segment.GenerateKey(0, 0, 0));
         Assert.AreEqual(Segment.GenerateKey(0, 0, -64), Segment.GenerateKey(0, 0, 65536 - 64));
         Assert.AreEqual(Segment.GenerateKey(0, 0, 65536), Segment.GenerateKey(0, 0, 0));
      }
   }
}
