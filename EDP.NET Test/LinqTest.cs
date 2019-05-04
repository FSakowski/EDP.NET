using EDPDotNet;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace EDP.NET_Test
{
    [TestClass]
    public class LinqTest
    {
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
        public void QuerySingleCustomer1()
        {
            var customers = from c in ctx.Get(0, 1, 2)
                            where c["swd"] == "VAAG"
                            select c;

            Record customer = customers.FirstOrDefault();

            Assert.AreNotEqual(null, customer);
            Assert.AreEqual("VAAG", customer["swd"]);
        }

        [TestMethod]
        public void QuerySingleCustomer2() {
            var customers = from c in ctx.Get(0, new int[] {}, "id", "idno", "swd", "zipCode")
                            where c.Field<Int32>("zipCode") == 38440
                            select c;                                

            var customer = customers.FirstOrDefault();

            Assert.AreNotEqual(null, customer);
            Assert.AreEqual(70032, customer.Field<Int32>("idno"));
        }

        [TestMethod]
        public void QueryContacts() {
            var contacts = from c in ctx.Get(0, new int[] { 2 }, "id", "idno", "swd", "descrOperLang")
                           where "70032" == c["companyARAP"]
                           select c;

            Assert.AreEqual(3, contacts.ToList().Count());
            Assert.AreEqual("ML Hannover - Volksauto AG, 38440 Wolfsburg", contacts.First()["descrOperLang"]);
        }

        [TestCleanup]
        public void Cleanup() {
            ctx.Dispose();
        }
    }
}
