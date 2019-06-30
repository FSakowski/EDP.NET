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
        public void QueryAndCountContacts() {
            var contacts = from c in ctx.Get(0, new int[] { 2 }, "id", "idno", "swd", "descrOperLang")
                           where "70032" == c["companyARAP"]
                           select c;

            Assert.AreEqual(3, contacts.Count());
            Assert.AreEqual("ML Hannover - Volksauto AG, 38440 Wolfsburg", contacts.First()["descrOperLang"]);
        }

        [TestMethod]
        public void QueryCustomerWithLocaleVariable() {
            string swd = "VAAG";

            var customers = from c in ctx.Get(0, 1, 2)
                            where c["swd"] == swd
                            select c;

            Record customer = customers.FirstOrDefault();

            Assert.AreNotEqual(null, customer);
            Assert.AreEqual("VAAG", customer["swd"]);
        }

        [TestMethod]
        public void QueryAndSelect() {
            var contacts = (from c in ctx.Get(0, 2)
                           where "70032" == c["companyARAP"]
                           select new { ID = c["id"], IdNo = c["idno"], Swd = c["swd"], Descr = c["descrOperLang"], ZipCode = c.Field<Int32>("zipCode") }).ToList();

            Assert.AreEqual(3, contacts.Count());
            Assert.AreEqual("ML Hannover - Volksauto AG, 38440 Wolfsburg", contacts.First().Descr);
            Assert.AreEqual(30419, contacts.First().ZipCode);
        }

        [TestMethod]
        public void QueryAndSelectWithSubType() {
            var contact = (from c in ctx.Get(0, 1)
                           where 70032 == c.Field<Int32>("idno")
                           select new {
                               ID = c["id"],
                               IdNo = c["idno"],
                               Swd = c["swd"],
                               Descr = c["descrOperLang"],
                               Location = new {
                                   ZipCode = c.Field<Int32>("zipCode"),
                                   Town = c["town"],
                                   Street = c["street"]
                               }
                           }).ToList().First();

            Assert.AreEqual("70032", contact.IdNo);
            Assert.AreEqual("Volksauto AG, 38440 Wolfsburg", contact.Descr);
            Assert.AreEqual(38440, contact.Location.ZipCode);
            Assert.AreEqual("Wolfsburg", contact.Location.Town);
            Assert.AreEqual("Hannover Ring 2", contact.Location.Street);
        }

        [TestMethod]
        public void QuerySelectBeforeWhere() {
            var customer = ctx.Get(0, 1).Select(c =>
                           new {
                               Name = c["descrOperLang"],
                               Location = new {
                                   ZipCode = c["zipCode"],
                                   Town = c["town"],
                                   Street = c["street"]
                               }
                           }
                          ).Where(x => x.Location.Town == "Wolfsburg").ToList().First();
                                                     
            Assert.AreEqual("Volksauto AG, 38440 Wolfsburg", customer.Name);
            Assert.AreEqual("38440", customer.Location.ZipCode);
            Assert.AreEqual("Wolfsburg", customer.Location.Town);
            Assert.AreEqual("Hannover Ring 2", customer.Location.Street);
        }

        [TestCleanup]
        public void Cleanup() {
            ctx.Dispose();
        }
    }
}
