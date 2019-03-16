using EDPDotNet.EPI;
using System;
using System.Collections.Generic;
using System.Text;

namespace EDPDotNet {
    public class Context : IDisposable {
        private EPIConnection connection;

        #region Properties

        public EPIConnection Conection {
            get {
                EnsureOpenConnection();
                return connection;
            }
        }

        #endregion

        public Context(string host, string mandant, ushort port, string password) {
            connection = new EPIConnection(host, mandant, port, password);
        }

        private void EnsureOpenConnection() {
            if (!connection.Connected)
                connection.Open();
        }

        public string GetOptionValue(EDPOption option) {
            if (option == null)
                throw new ArgumentNullException("option");

            EnsureOpenConnection();
            return connection.GetOptionValue(option.Value);
        }

        public void SetOption(EDPOption option, string value) {
            if (option == null)
                throw new ArgumentNullException("option");

            EnsureOpenConnection();
            connection.SetOption(option.Value, value);
        }

        public Query CreateQuery(Selection selection) {
            if (selection == null)
                throw new ArgumentNullException("selection");

            return new Query(connection, selection);
        }


        #region Disposable-Implementierung
        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~Context() {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing) {
            if (disposing) {
                connection.Close();
            }
        }
        #endregion
    }
}
