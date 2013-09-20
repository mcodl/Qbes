using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Qbes.Common.Logic;

namespace Qbes.UnitTests
{
   /// <summary>
   /// Summary description for Point3DTest
   /// </summary>
   [TestClass]
   public class Point3DTest
   {
      /// <summary>
      /// Creates a new test instance.
      /// </summary>
      public Point3DTest()
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
      // Use ClassInitialize to run code before running the first test in the class
      // [ClassInitialize()]
      // public static void MyClassInitialize(TestContext testContext) { }
      //
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
      /// Test for the Point3D.GetRectangleIntersectionTest(...) method.
      /// </summary>
      [TestMethod]
      public void GetRectangleIntersectionTest()
      {
         Point3D pl1 = new Point3D(-5, 0, -5);
         Point3D pl2 = new Point3D(5, 0, -5);
         Point3D pl3 = new Point3D(5, 0, 5);
         Point3D pl4 = new Point3D(-5, 0, 5);
         var pl = Tuple.Create(pl1, pl2, pl3, pl4);

         // one intersection at [-4; 0; 4]
         Point3D u1 = new Point3D(-4, 2, 4);
         Point3D u2 = new Point3D(-4, -2, 4);
         var u = new Vector3D(u1, u2);

         var result = Point3D.GetRectangleIntersection(pl, u);
         Assert.IsTrue(result.Item1 == 1);
         Assert.AreEqual<Point3D>(result.Item2, new Point3D(-4, 0, 4));

         // one intersection at [-4; 0; 4]
         u1 = new Point3D(-5, 2, 5);
         u2 = new Point3D(-3, -2, 3);
         u = new Vector3D(u1, u2);

         result = Point3D.GetRectangleIntersection(pl, u);
         Assert.IsTrue(result.Item1 == 1);
         Assert.AreEqual<Point3D>(result.Item2, new Point3D(-4, 0, 4));

         // one intersection at [0; 0; 0]
         u1 = new Point3D(0, 2, 0);
         u2 = new Point3D(0, -2, 0);
         u = new Vector3D(u1, u2);

         result = Point3D.GetRectangleIntersection(pl, u);
         Assert.IsTrue(result.Item1 == 1);
         Assert.AreEqual<Point3D>(result.Item2, new Point3D(0, 0, 0));

         // no intersection
         u1 = new Point3D(-4, -1, 4);
         u2 = new Point3D(-4, -2, 4);
         u = new Vector3D(u1, u2);

         result = Point3D.GetRectangleIntersection(pl, u);
         Assert.IsTrue(result.Item1 == 0);
         Assert.IsNull(result.Item2);

         // no intersection
         u1 = new Point3D(-5.5, 2, 5);
         u2 = new Point3D(-5, -2, 5);
         u = new Vector3D(u1, u2);

         result = Point3D.GetRectangleIntersection(pl, u);
         Assert.IsTrue(result.Item1 == 0);
         Assert.IsNull(result.Item2);

         // line is in the plane
         u1 = new Point3D(4, 0, 4);
         u2 = new Point3D(3, 0, 3);
         u = new Vector3D(u1, u2);

         result = Point3D.GetRectangleIntersection(pl, u);
         Assert.IsTrue(result.Item1 == 2);
         Assert.IsNull(result.Item2);
      }

      /// <summary>
      /// Test for the Point3D.GetTriangleIntersection(...) method.
      /// </summary>
      [TestMethod]
      public void GetTriangleIntersectionTest()
      {
         Point3D pl1 = new Point3D(-5, 0, -5);
         Point3D pl2 = new Point3D(5, 0, 5);
         Point3D pl3 = new Point3D(-5, 0, 5);
         var pl = Tuple.Create(pl1, pl2, pl3);

         // one intersection at [-4; 0; 4]
         Point3D u1 = new Point3D(-4, 2, 4);
         Point3D u2 = new Point3D(-4, -2, 4);
         var u = new Vector3D(u1, u2);

         var result = Point3D.GetTriangleIntersection(pl, u);
         Assert.IsTrue(result.Item1 == 1);
         Assert.AreEqual<Point3D>(result.Item2, new Point3D(-4, 0, 4));

         // one intersection at [-4; 0; 4]
         u1 = new Point3D(-5, 2, 5);
         u2 = new Point3D(-3, -2, 3);
         u = new Vector3D(u1, u2);

         result = Point3D.GetTriangleIntersection(pl, u);
         Assert.IsTrue(result.Item1 == 1);
         Assert.AreEqual<Point3D>(result.Item2, new Point3D(-4, 0, 4));

         // one intersection at [0; 0; 0]
         u1 = new Point3D(0, 2, 0);
         u2 = new Point3D(0, -2, 0);
         u = new Vector3D(u1, u2);

         result = Point3D.GetTriangleIntersection(pl, u);
         Assert.IsTrue(result.Item1 == 1);
         Assert.AreEqual<Point3D>(result.Item2, new Point3D(0, 0, 0));

         // no intersection
         u1 = new Point3D(-4, -1, 4);
         u2 = new Point3D(-4, -2, 4);
         u = new Vector3D(u1, u2);

         result = Point3D.GetTriangleIntersection(pl, u);
         Assert.IsTrue(result.Item1 == 0);
         Assert.IsNull(result.Item2);

         // no intersection
         u1 = new Point3D(-5.5, 2, 4);
         u2 = new Point3D(-5, -2, 4);
         u = new Vector3D(u1, u2);

         result = Point3D.GetTriangleIntersection(pl, u);
         Assert.IsTrue(result.Item1 == 0);
         Assert.IsNull(result.Item2);

         // line is in the plane
         u1 = new Point3D(-4, 0, 4);
         u2 = new Point3D(-3, 0, 3);
         u = new Vector3D(u1, u2);

         result = Point3D.GetTriangleIntersection(pl, u);
         Assert.IsTrue(result.Item1 == 2);
         Assert.IsNull(result.Item2);
      }
   }
}
