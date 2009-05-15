using System;
using Zetetic.Chain;
using Zetetic.Chain.Xml;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace Zetetic.Chain.UnitTest
{
    public class HelloCommandWProp : Zetetic.Chain.Command
    {
        public string SomethingExtra { get; set; }

        public DateTime SomeDateTime { get; set; }

        [ChainRequired]
        public Guid CrazyGuid { get; set; }

        public HelloCommandWProp()
        {
            this.SomethingExtra = "A little...";
            this.SomeDateTime = new DateTime(2009, 12, 25);
            this.CrazyGuid = Guid.NewGuid();
        }

        public override CommandResult Execute(IContext ctx)
        {
            System.Console.WriteLine("Hello from {0}", this.GetType().FullName);
            return CommandResult.Continue;
        }
    }

    public class AnotherCommand : Zetetic.Chain.Command
    {
        public override CommandResult Execute(IContext ctx)
        {
            System.Console.WriteLine("Hello from {0}", this.GetType().FullName);
            return CommandResult.Continue;
        }
    }

    /// <summary>
    ///This is a test class for XmlCatalogTest and is intended
    ///to contain all XmlCatalogTest Unit Tests
    ///</summary>
    [TestClass()]
    public class XmlCatalogTest
    {
        private XmlCatalog _catalog;

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
        [TestInitialize()]
        public void MyTestInitialize()
        {
            _catalog = new XmlCatalog();
            ICommand cmd = new HelloCommandWProp() { SomethingExtra = "howdy", Name = "cmd" };

            _catalog[cmd.Name] = cmd;

            IChain chain = ChainFactory.GetFactory().CreateChain();
            chain.Name = "chain";
            chain.Add(new HelloCommandWProp() { ShouldTerminateChain = true });

            IChain innerChain = ChainFactory.GetFactory().CreateChain();
            innerChain.Add(new AnotherCommand());
            chain.Add(innerChain);

            _catalog[chain.Name] = chain;
        }
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion

        /// <summary>
        ///A test for GetCommandNames
        ///</summary>
        [TestMethod()]
        public void XmlCatalogSerializationTest()
        {
            string fileOut = @"c:\temp\xmlcat.xml";
            System.Xml.Serialization.XmlSerializer ser = new System.Xml.Serialization.XmlSerializer(typeof(XmlCatalog));
            using (System.IO.FileStream fs = new System.IO.FileStream(fileOut, System.IO.FileMode.OpenOrCreate))
            {
                ser.Serialize(fs, _catalog);
            }
            Assert.IsTrue(System.IO.File.Exists(fileOut));
        }

        [TestMethod()]
        public void XmlCatalogDeSerializationTest()
        {
            System.Xml.Serialization.XmlSerializer ser = new System.Xml.Serialization.XmlSerializer(typeof(XmlCatalog));
            using (System.IO.FileStream fs = new System.IO.FileStream(@"c:\temp\xmlcat.xml", System.IO.FileMode.Open))
            {
                _catalog = (XmlCatalog)ser.Deserialize(fs);
            }
            ICommand cmd = _catalog["cmd"];
            Assert.IsNotNull(cmd);
        }

        [TestMethod]
        [ExpectedException(typeof(Zetetic.Chain.Generic.ContextNotRemoteableException))]
        public void RemoteCommandRejectsUnremoteableContext()
        {
            Zetetic.Chain.Generic.RemoteCommand cmd = new Zetetic.Chain.Generic.RemoteCommand();
            cmd.Execute(new NonRemoteableContext());
            Assert.Fail("Should not get here");
        }

        [TestMethod]
        public void RemoteCommandAcceptsRemoteableContext()
        {
            Zetetic.Chain.Generic.RemoteCommand cmd = new Zetetic.Chain.Generic.RemoteCommand();
            try
            {
                cmd.Execute(new Zetetic.Chain.ContextBase());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + " :: " + ex.StackTrace);
                Assert.IsNotInstanceOfType(ex, typeof(Zetetic.Chain.Generic.ContextNotRemoteableException));
                return;
            }
            Assert.Fail("Should not get here");
        }

        [TestMethod]
        [ExpectedException(typeof(ChainXmlSerializationException))]
        public void AddCommandWithMissingPropertyShouldThrowAppExceptionTest()
        {
            _catalog["foo"] = new Zetetic.Chain.Generic.CopyCommand() { Name = "foo" };

            Assert.Fail("Should not reach here without exception");
        }

        [TestMethod]
        public void CopyCommandShouldCopyValue()
        {
            IContext ctx = new ContextBase();
            Zetetic.Chain.Generic.CopyCommand cmd = new Zetetic.Chain.Generic.CopyCommand();
            ctx["in"] = "value";
            cmd.FromKey = "in";
            cmd.ToKey = "out";
            cmd.Execute(ctx);

            Assert.IsNotNull(ctx["out"], "'out' value must be present in modified context");
            Assert.AreEqual("value", ctx["out"]);
            
        }

        [TestMethod]
        [ExpectedException(typeof(ChainXmlSerializationException))]
        public void CatalogShouldRefuseMissingRequiredProperties()
        {
            IContext ctx = new ContextBase();
            Zetetic.Chain.Generic.CopyCommand cmd = new Zetetic.Chain.Generic.CopyCommand();
            ctx["in"] = "value";
            cmd.FromKey = null;
            cmd.ToKey = "out";
            cmd.Name = "test";

            ICatalog cat = new XmlCatalog();
            cat[cmd.Name] = cmd;

            Assert.Fail("Should have had exception");

        }

        [TestMethod]
        public void GeneralChainingTest()
        {
            IContext ctx = new ContextBase();
            ctx["in"] = "value";

            Zetetic.Chain.IChain chain = ChainFactory.GetFactory().CreateChain();
            chain.Name = "chain";

            Zetetic.Chain.Generic.CopyCommand cmd = new Zetetic.Chain.Generic.CopyCommand();            
            cmd.FromKey = "in";
            cmd.ToKey = "middle";

            chain.Add(cmd);

            cmd = new Zetetic.Chain.Generic.CopyCommand();
            cmd.FromKey = "middle";
            cmd.ToKey = "third";

            chain.Add(cmd);

            Zetetic.Chain.Generic.RemoveCommand remo = new Zetetic.Chain.Generic.RemoveCommand();
            remo.FromKey = "middle";

            chain.Add(remo);

            ICatalog cat = new CatalogBase();
            cat[chain.Name] = chain;

            cat["chain"].Execute(ctx);

            Assert.AreEqual("value", ctx["third"]);
            Assert.IsNull(ctx["middle"]);

        }

        [TestMethod]
        public void LookupCommandTest()
        {
            IContext ctx = new ContextBase();
            ctx["test"] = "value";

            Zetetic.Chain.Generic.CopyCommand toLookup = new Zetetic.Chain.Generic.CopyCommand();
            toLookup.FromKey = "test";
            toLookup.ToKey = "x";
            toLookup.Name = "findMe";

            Zetetic.Chain.Generic.LookupCommand cmd = new Zetetic.Chain.Generic.LookupCommand();
            cmd.Command = "findMe";
            cmd.Name = "finder";
            cmd.CatalogKey = "CATALOG";

            ICatalog cat = new CatalogBase();
            cat[toLookup.Name] = toLookup;
            cat[cmd.Name] = cmd;
            ctx[cmd.CatalogKey] = cat;

            cat["finder"].Execute(ctx);

            Assert.AreEqual(ctx["x"], "value");
        }

        /// <summary>
        ///A test for Get
        ///</summary>
        [TestMethod()]
        public void GetTest()
        {
            ICommand cmd = _catalog["cmd"];

            Assert.IsNotNull(cmd);
        }
    }

    public class NonRemoteableContext : IContext
    {

        #region IContext Members

        public object this[string key]
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public void Remove(string key)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
