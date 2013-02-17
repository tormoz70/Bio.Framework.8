using Bio.Helpers.Common.Types;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading;
using Bio.Helpers.DOA;

namespace Bio.Helpers.Common.Test
{
    
    
    /// <summary>
    ///This is a test class for CThreadsQueueTest and is intended
    ///to contain all CThreadsQueueTest Unit Tests
    ///</summary>
  [TestClass()]
  public class CThreadsQueueTest {


    private TestContext testContextInstance;

    /// <summary>
    ///Gets or sets the test context which provides
    ///information about and functionality for the current test run.
    ///</summary>
    public TestContext TestContext {
      get {
        return testContextInstance;
      }
      set {
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
    ///A test for addAction
    ///</summary>
    [TestMethod()]
    public void addActionTest() {
      var target = new CThreadsQueue(); // TODO: Initialize to an appropriate value
      target.OnBeforeAction += new EventHandler<OnBeforeActionEventArgs>((s, e) => {
        System.Console.WriteLine("Starting action [{0}]", e.item.name);
      });
      target.OnAfterAction += new EventHandler<OnAfterActionEventArgs>((s, e) => {
        System.Console.WriteLine("Ending action [{0}]", e.item.name);
      });
      target.OnErrorAction += new EventHandler<OnErrorActionEventArgs>((s, e) => {
        System.Console.WriteLine("Error action [{0}]. Err:", e.item.name, e.exception);
      });
      for (var i = 0; i < 10; i++) {
        System.Console.WriteLine("Adding action [{0}]", i);
        target.addAction("action-" + i, new Action(() => {
          for (var j = 0; j < 10; j++)
            Thread.Sleep(300);
        }));
        System.Console.WriteLine("Added action [{0}]", i);
      }

      while (target.IsActive) {
        Thread.Sleep(1000);
      }
      System.Console.WriteLine("Test done!");
      //Assert.Inconclusive("A method that does not return a value cannot be verified.");
    }
    [TestMethod()]
    public void connTest() {
      var sess = new DBSession("Password=j12;Persist Security Info=True;Connection Lifetime=10;Max Pool Size=15;User ID=GIVC_PUB;Data Source=GIVCDB_EKBS02");
      var cr = SQLCursor.CreateAndOpenCursor(sess, "select * from ORGM$ORGWWW", null, 60);
      while (cr.Next()) {
        System.Console.WriteLine("...");
      }
      cr.Close();
    }
  }
}
