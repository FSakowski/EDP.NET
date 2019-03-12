using EDPDotNet.EPI;
using System;
using System.Collections.Generic;
using System.Text;

namespace EDPDotNet {
    public class Query {

        private EPIConnection connection;

        public Query(EPIConnection connection) {
            this.connection = connection ?? throw new ArgumentNullException("connection must not be null");

            if (!connection.Connected)
                connection.Open();
        }

        
    }
}
