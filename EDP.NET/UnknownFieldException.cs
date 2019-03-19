using System;
using System.Collections.Generic;
using System.Text;

namespace EDPDotNet {
    public class UnknownFieldException : Exception {
        public string Field {
            get;
        }

        public UnknownFieldException() {
        }

        public UnknownFieldException(string field) : base($"field {field} is unknown") {
            this.Field = field;
        }

        public UnknownFieldException(string field, string message) : base(message) {
            this.Field = field;
        }

        public UnknownFieldException(string field, Exception inner) : base($"field {field} is unknwon", inner) {
            this.Field = field;
        }

        public UnknownFieldException(string field, string message, Exception inner) : base(message, inner) {
            this.Field = field;
        }
    }
}
