using System;
using System.Collections.Generic;
using System.Text;

namespace EDPDotNet.EPI {
    public class Channel : Queue<EPICommand> {

        public uint ActionId {
            get;
            private set;
        }

        public Channel(uint actionId) {
            this.ActionId = actionId;
        }

        public override string ToString() {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat("{ ActionId: {0}", ActionId);

            if (Count > 0) {
                sb.AppendLine(",");

                foreach(EPICommand cmd in this) {
                    sb.AppendLine(cmd.ToString());
                }

                sb.AppendLine();
            }

            sb.Append("}");

            return sb.ToString();
        }
    }
}
