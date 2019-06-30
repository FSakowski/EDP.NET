using EDPDotNet;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace EDP.NET_Test {
    [TestClass]
    public class SelectTest {
        string host = "172.32.15.200";
        ushort port = 6550;
        string mandand = "erp";
        string pwd = "system";

        private Context ctx;

        [TestInitialize]
        public void Setup() {
            ctx = new Context(host, mandand, port, pwd);
        }

        [TestMethod]
        public void QuerySingleProduct() {
            Selection sel = new Selection(2, 1);
            sel.FilingMode = FilingMode.Active;
            sel.AddCondition(Condition.Eq("idno", "30045"));

            Query query = ctx.CreateQuery(sel);

            Assert.AreEqual(1, query.Count());
            Assert.AreEqual("SABA-KP5000", query.First()["swd"]);
            Assert.AreEqual("SWK2", query.FieldList.First(f => f.Name == "swd").Type);
            Assert.AreEqual(1, query.Count());
        }

        [TestMethod]
        public void QuerySingleProductWithFieldList() {
            Selection sel = new Selection(2, 1);
            sel.FilingMode = FilingMode.Active;
            sel.AddCondition(Condition.Eq("idno", "30045"));
            sel.FieldList.Add("idno");
            sel.FieldList.Add("swd");
            sel.FieldList.Add("descrOperLang");

            Query query = ctx.CreateQuery(sel);

            Assert.AreEqual(1, query.Count());
            Assert.AreEqual("SABA-KP5000", query.First()["swd"]);
            Assert.AreEqual(3, query.FieldList.Count);
            Assert.AreEqual(1, query.Count());
        }

        [TestMethod]
        public void QueryCustomersWithinZipCodeBoundsAndWithOrder() {
            Selection sel = new Selection(0, 1);
            sel.FilingMode = FilingMode.Active;
            sel.AddCondition(Condition.Between("zipCode", "20000", "29999"));
            sel.OrderField = "zipCode";
            sel.FieldList.Add("idno");
            sel.FieldList.Add("swd");
            sel.FieldList.Add("descrOperLang");
            sel.FieldList.Add("zipCode");

            Query query = ctx.CreateQuery(sel);

            var list = query.ToList();

            Assert.AreEqual(4, list.Count);
            Assert.AreEqual("22113", list[0]["zipCode"]);
            Assert.AreEqual("22589", list[1]["zipCode"]);
            Assert.AreEqual("28022", list[2]["zipCode"]);
            Assert.AreEqual("28777", list[3]["zipCode"]);

            sel.Direction = OrderDirection.Descending;
            query = ctx.CreateQuery(sel);

            list = query.ToList();

            Assert.AreEqual(4, list.Count);
            Assert.AreEqual("28777", list[0]["zipCode"]);
            Assert.AreEqual("28022", list[1]["zipCode"]);
            Assert.AreEqual("22589", list[2]["zipCode"]);
            Assert.AreEqual("22113", list[3]["zipCode"]);
        }

        [TestCleanup]
        public void Cleanup() {
            ctx.Dispose();
        }
    }
}
