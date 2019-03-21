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
                sel.AddCondition(Condition.Between("zipCode", "20000", "39999"));
                sel.OrderField = "town";
                sel.Direction = OrderDirection.Descending;
                sel.FilingMode = FilingMode.Active;
                sel.LinkMode = ConditionLinkMode.Or;
                sel.Paging = true;
                sel.PageSize = 1;

                Query query = ctx.CreateQuery(sel);
                query.FieldList.Add("idno");
                query.FieldList.Add("descrOperLang");
                query.FieldList.Add("swd");
                query.FieldList.Add("zipCode");
                query.FieldList.Add("town");
                query.FieldList.Add("inhouseContact");

                do {
                    DataSet data = query.Execute();

                    foreach (Record r in data) {
                        Console.WriteLine(r["idno"] + " " + r["descrOperLang"] + " " + r["zipCode"] + " " + r["town"]);
                        string inhouseCtcNr = r["inhouseContact"];
                        string inhouseCtcName = GetInhouseContactName(ctx, inhouseCtcNr);
                        Console.WriteLine("Betreuer: " + inhouseCtcName);
                    }

                } while (!query.EndOfData);
            }

            Console.WriteLine("\n Press Enter to continue...");
            Console.Read();
        }

        static string GetInhouseContactName(Context ctx, string nr) {
            Selection sel = new Selection(11, 1);
            sel.AddCondition(Condition.Eq("idno", nr));

            Query query = ctx.CreateQuery(sel);
            query.FieldList.Add("idno").Add("descrOperLang");
            Record contact = query.GetFirstRecord();
            return contact["descrOperLang"];
        }
    }
}
