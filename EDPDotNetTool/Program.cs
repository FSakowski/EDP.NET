using EDPDotNet;
using EDPDotNet.EPI;
using System;
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
                Console.WriteLine("fopmode: " + ctx.GetOptionValue(EDPOption.FOPMode));
                ctx.SetOption(EDPOption.FOPMode, "0");
                Console.WriteLine("fopmode: " + ctx.GetOptionValue(EDPOption.FOPMode));

                Selection sel = new Selection(0, 1, 2);

            }

            Console.WriteLine("\n Press Enter to continue...");
            Console.Read();
        }
    }
}
