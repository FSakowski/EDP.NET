using EDPDotNet.EPI;
using EDPDotNet.Linq;
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

        /// <summary>
        /// Erzeugt eine Abfrage, welche über Linq angesteuert werden kann.
        /// </summary>
        /// <param name="db">Datenbanknummer</param>
        /// <param name="groups">Gruppennummern</param>
        /// <param name="fields">Feldnamen</param>
        /// <returns></returns>
        public QueryProxy<Record> Get(int db, int[] groups, params string[] fields) {
            FieldList list = new FieldList();

            foreach(string f in fields)
                list.Add(f);

            return Get(db, groups, list);
        }

        /// <summary>
        /// Erzeugt eine Abfrage, welche über Linq angesteuert werden kann.
        /// </summary>
        /// <param name="db">Datenbanknummer</param>
        /// <param name="group">Gruppennummer</param>
        /// <param name="fieldList">Feldliste</param>
        /// <returns></returns>
        public QueryProxy<Record> Get(int db, int group, FieldList fieldList) {
            return Get(db, new int[] { group }, fieldList);
        }

        /// <summary>
        /// Erzeugt eine Abfrage, welche über Linq angesteuert werden kann.
        /// </summary>
        /// <param name="db">Datenbanknummern</param>
        /// <param name="groups">Gruppennummern</param>
        /// <returns></returns>
        public QueryProxy<Record> Get(int db, params int[] groups) {
            return Get(db, groups, new FieldList());
        }

        /// <summary>
        /// Erzeugt eine Abfrage, welche über Linq angesteuert werden kann.
        /// </summary>
        /// <param name="db">Datenbanknummer</param>
        /// <param name="groups">Gruppennummern</param>
        /// <param name="fieldList">Feldliste</param>
        /// <returns></returns>
        public QueryProxy<Record> Get(int db, int[] groups, FieldList fieldList) {
            if (fieldList == null)
                fieldList = new FieldList();

            fieldList.Add("id");
            fieldList.Add("idno");

            return new QueryProxy<Record>(new QueryProvider(this, db, groups, fieldList));
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
