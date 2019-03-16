using EDPDotNet.EPI;
using System;
using System.Collections.Generic;
using System.Text;

namespace EDPDotNet {
    public class Query {

        private EPIConnection connection;

        private Selection selection;

        private DataSet lastDataSet;

        private FieldList fieldList;

        #region properties

        /// <summary>
        ///Ruft ab oder legt fest, ob die Feldnamen der Feldliste in englisch oder deutsch genannt werden sollen.
        /// </summary>
        private Language VariableLanguage {
            get;
            set;
        }

        /// <summary>
        /// Ruft einen Wert ab, der angibt, ob bereits alle Daten der Abfrage abgerufen wurden.
        /// </summary>
        public bool EndOfData {
            get {
                if (lastDataSet == null)
                    return false;

                return lastDataSet.EOF;
            }
        }

        public FieldList FieldList {
            get {
                return fieldList;
            }
        }

        #endregion

        public Query(EPIConnection connection, Selection selection) {
            this.connection = connection ?? throw new ArgumentNullException("connection");
            this.selection = selection ?? throw new ArgumentNullException("selection");
            VariableLanguage = Language.English;
            fieldList = new FieldList();
        }

        private void EnsureConnection() {
            if (!connection.Connected)
                connection.Open();
        }

        public void Reset() {
            lastDataSet = null;
        }

        public DataSet Execute() {
            EnsureConnection();

            if (EndOfData)
                throw new InvalidOperationException("end of query has already been reached");

            if (lastDataSet == null) {
                lastDataSet = NewQuery();
            } else {
                lastDataSet = ContinueQuery();
            }

            return lastDataSet;
        }

        private DataSet NewQuery() {
            int pageSize = selection.Paging ? selection.PageSize : 0;
            int offset = selection.Offset;

            return connection.ExecuteQuery(selection.ToString(), FieldList, pageSize, offset, LanguageHelper.ToString(VariableLanguage));
        }

        private DataSet ContinueQuery() {
            return connection.GetNextRecord(FieldList);
        }
    }
}
