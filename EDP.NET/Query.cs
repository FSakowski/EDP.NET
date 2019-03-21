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

        public uint ActionId {
            get;
            private set;
        }

        #endregion

        public Query(EPIConnection connection, Selection selection) {
            this.connection = connection ?? throw new ArgumentNullException("connection");
            this.selection = selection ?? throw new ArgumentNullException("selection");
            VariableLanguage = Language.English;
            fieldList = new FieldList();
            ActionId = connection.RegisterNewActionId();
        }

        private void EnsureConnection() {
            if (!connection.Connected)
                connection.Open();
        }

        public void Reset() {
            BreakExecution();
            lastDataSet = null;
        }

        /// <summary>
        /// Führt die Abfrage mit der hinterlegten Selektion durch und liefert ein <see cref="DataSet">DataSet</see> mit den vom Server zurückgelieferten Daten.
        /// Wurden nocht nicht alle zurückgeliefert, kann die Methode erneut aufgerufen werden, um die nächsten Datensätze abzurufen.
        /// Ist das Ende bereits erreicht, wird eine InvalidOperationExceeption ausgelöst.
        /// </summary>
        /// <returns></returns>
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

        public Record GetFirstRecord() {
            DataSet data = Execute();
            if (data.Count == 0)
                return null;

            if (!data.EOF)
                BreakExecution();

            return data[0];
        }

        private void BreakExecution() {
            if (lastDataSet == null)
                return;

            connection.BreakQueryExecution(ActionId);
        }

        private DataSet NewQuery() {
            int pageSize = selection.Paging ? selection.PageSize : 0;
            int offset = selection.Offset;

            return connection.ExecuteQuery(selection.ToString(), ActionId, FieldList, pageSize, offset, LanguageHelper.ToString(VariableLanguage));
        }

        private DataSet ContinueQuery() {
            return connection.GetNextRecord(FieldList, ActionId);
        }
    }
}
