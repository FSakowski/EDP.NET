using System;
using System.Collections.Generic;
using System.Text;

namespace EDPDotNet.EPI
{
    public class EPIException : Exception
    {
        public EPICommand Cmd {
            get;
        }

        public EPIException() {
        }

        public EPIException(String message) : base(message) {
        }

        public EPIException(String message, EPICommand cmd) : base(message) {
            this.Cmd = cmd;
        }

        public EPIException(String message, Exception inner) : base (message, inner) {
        }

        public EPIException(String message, EPICommand cmd, Exception inner) : base(message, inner) {
            this.Cmd = cmd;
        }

        public override string Message {
            get {
                if (Cmd == null)
                    return base.Message;

                return base.Message + ", CMD: " + Cmd;
            }
        }
    }
}
