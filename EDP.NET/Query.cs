using EDPDotNet.EPI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace EDPDotNet {
    /// <summary>
    /// Führt eine Datenbankabfrage anhand von Selektionskriteren und Optionen aus und
    /// liefert die Ergebnisse als <see cref="DataSet"/> zurück.
    /// </summary>
    public class Query {

        private EPIConnection connection;

        private Selection selection;

        private DataSet lastDataSet;  

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

        public uint ActionId {
            get;
            private set;
        }

        #endregion

        public Query(EPIConnection connection, Selection selection) {
            this.connection = connection ?? throw new ArgumentNullException("connection");
            this.selection = selection ?? throw new ArgumentNullException("selection");
            VariableLanguage = Language.English;
            ActionId = connection.RegisterNewActionId();
        }

        private void EnsureConnection() {
            if (!connection.Connected)
                connection.Open();
        }

        /// <summary>
        /// Bricht eine bereits gestartete Abfrage ab und setzt diese zurück.
        /// </summary>
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

        /// <summary>
        /// Führt die Abfrage aus und liefert den ersten Datensatz zurück oder NULL, wenn
        /// die Ergebnismenge leer ist. Enthält die Eregbnismenge weitere Datensätze, 
        /// wird die Abfrage automatisch abgebrochen. Die Selektionskriteren sollten möglichst nur
        /// zu einen Datensazu passen oder auf ein Element limitiert werden. 
        /// </summary>
        /// <returns></returns>
        public Record GetFirstRecord() {
            DataSet data = Execute();
            if (data.Count == 0)
                return null;

            if (!data.EOF)
                BreakExecution();

            return data[0];
        }

        private void BreakExecution() {
            if (lastDataSet == null || EndOfData)
                return;

            connection.BreakQueryExecution(ActionId);
        }

        private DataSet NewQuery() {
            int pageSize = selection.Paging ? selection.PageSize : 0;
            int offset = selection.Offset;

            var cmds = connection.ExecuteQuery(selection.ToString(), ActionId, selection.FieldList.ToString(), pageSize, offset, LanguageHelper.ToString(VariableLanguage));
            return DataSet.Fill(cmds, selection.FieldList);
        }

        private DataSet ContinueQuery() {
            var cmds = connection.GetNextRecord(ActionId);
            return DataSet.Fill(cmds, selection.FieldList);
        }
    }
}
