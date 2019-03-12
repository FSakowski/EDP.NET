using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace EDPDotNet.EPI {
    public class EPICommand : IEnumerable<string> {
        
        public EPICommand(string cmdWord, uint actionId, string[] fields, bool completed) {
            CMDWord = cmdWord;
            ActionId = actionId;
            Fields = fields;
            Completed = completed;
        }

        public string CMDWord {
            get;
            private set;
        }

        public uint ActionId {
            get;
            private set;
            
        }

        public string[] Fields {
            get;
            private set;
        }

        public bool Completed {
            get;
            private set;
        }

        public string this[int i] {
            get {
                if (Fields.Length > i)
                    return Fields[i];

                return String.Empty;
            }
        }

        public IEnumerator<string> GetEnumerator() {
            return ((IEnumerable<string>)Fields).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return Fields.GetEnumerator();
        }

        public override string ToString() {
            StringBuilder sb = new StringBuilder();
            sb.Append("{");
            sb.AppendFormat("{0}|{1}|", CMDWord, ActionId);

            foreach (String f in Fields)
                sb.AppendFormat("{0}|", f);

            sb.Append("}");

            return sb.ToString();
        }
    }
}
