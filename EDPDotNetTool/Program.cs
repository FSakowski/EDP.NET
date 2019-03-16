using EDPDotNet;
using EDPDotNet.EPI;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace EDPDotNet.Tool {
    class Program {
        static void Main(string[] args) {

            string host = "172.32.15.200";
            ushort port = 6550;
            string mandand = "erp";
            string pwd = "system";

            using (Context ctx = new Context(host, mandand, port, pwd)) {                
                /*Console.WriteLine("fopmode: " + ctx.GetOptionValue(EDPOption.FOPMode));
                ctx.SetOption(EDPOption.FOPMode, "0");
                Console.WriteLine("fopmode: " + ctx.GetOptionValue(EDPOption.FOPMode));*/

                Selection sel = new Selection(0, 1, 2);
                sel.AddConditions(Condition.Between("zipCode", "20000", "39999"));
                sel.OrderField = "town";
                sel.Direction = OrderDirection.Descending;
                sel.FilingMode = FilingMode.Active;
                sel.LinkMode = ConditionLinkMode.Or;

                Query query = ctx.CreateQuery(sel);
                /*query.FieldList.Add("idno");
                query.FieldList.Add("descrOperLang");
                query.FieldList.Add("swd");
                query.FieldList.Add("zipCode");*/

                DataSet data = query.Execute();

                foreach(Record r in data) {
                    foreach(KeyValuePair<Field, string> item in r) {
                        Console.WriteLine(item.Key + " (" + item.Key.Description + ", " + item.Key.Type + ") = " + item.Value);
                    }
                }
            }

            Console.WriteLine("\n Press Enter to continue...");
            Console.Read();
        }
    }
}
