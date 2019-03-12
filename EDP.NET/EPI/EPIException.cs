using System;
using System.Collections.Generic;
using System.Text;

namespace EDPDotNet.EPI
{
    public class EPIException : Exception
    {
        public EPICommand cmd {
            get;
        }

        public EPIException() {
        }

        public EPIException(String message) : base(message) {
        }

        public EPIException(String message, EPICommand cmd) : base(message) {
            this.cmd = cmd;
        }

        public EPIException(String message, Exception inner) : base (message, inner) {
        }

        public EPIException(String message, EPICommand cmd, Exception inner) : base(message, inner) {
            this.cmd = cmd;
        }

        public override string Message {
            get {
                if (cmd == null)
                    return base.Message;

                return base.Message + ", CMD: " + cmd;
            }
        }
    }
}
