using System;
using Zetetic.Chain.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Zetetic.Chain;

namespace Zetetic.Chain.UnitTest
{
    
    
    /// <summary>
    ///This is a test class for DispatchLookupCommandTest and is intended
    ///to contain all DispatchLookupCommandTest Unit Tests
    ///</summary>
    [TestClass()]
    public class DispatchLookupCommandTest
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
        ///A test for ResolveInnerCommand
        ///</summary>
        [TestMethod()]
        public void SelfDispatchTest()
        {
            ICatalog catalog = CatalogFactory.GetFactory().GetCatalog();
            DispatchLookupCommand dlc = new SelfDispatchingLookupCommand();
            dlc.Name = "cmd";
            dlc.CatalogKey = "cat";
            dlc.Command = "_self";
            dlc.DispatchMethod = "OtherMethod";

            IContext ctx = new ContextBase();
            ctx["cat"] = catalog;

            dlc.Execute(ctx);

            Assert.AreEqual(true, ctx["test"]);
        }
    }

    class SelfDispatchingLookupCommand : DispatchLookupCommand
    {
        public CommandResult OtherMethod(IContext ctx)
        {
            ctx["test"] = true;
            return CommandResult.Continue;
        }
    }
}
