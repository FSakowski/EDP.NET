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
    /// liefert die Ergebnisse als <see cref="Record"/> zurück.
    /// </summary>
    public class Query : IEnumerable<Record>, IDisposable {

        private EPIConnection connection;
        private Selection selection;
        private DataCommandReader reader;        

        private bool disposed = false;
        private bool executed = false;

        #region Properties

        /// <summary>
        /// Ruft ab oder legt fest, ob die Feldnamen der Feldliste in englisch oder deutsch genannt werden sollen.
        /// </summary>
        private Language VariableLanguage {
            get;
            set;
        }

        /// <summary>
        /// Ruft ab oder legt fest, ob zusätzliche Metadaten über die Variablenfelder abgefragt werden sollen.
        /// Wurde keine Feldliste angegeben, werden die Daten immer mit Metadaten abgerufen, unabhängig davon, ob diese Option aktiv ist oder nicht.
        /// </summary>
        public bool WithMetaData {
            get;
            set;
        }

        /// <summary>
        /// Liefert die Action-Id zurück, die für die Ausführung der Datenbankabfrage verwendet wird.
        /// </summary>
        public uint ActionId {
            get;
            private set;
        }

        /// <summary>
        /// Liefert die Feldliste zurück, die je nach Selektion weitere Metadaten zu den Feldern beinhaltet.
        /// Wurde die Abfrage noch nicht an den Server gesendet, wird dies an dieser Stelle ausgeführt, um die Metadaten zu erhalten.  
        /// </summary>
        public FieldList FieldList {
            get {
                if (!executed)
                    Execute();

                return reader.FieldList;
            }
        }

        #endregion

        public Query(EPIConnection connection, Selection selection) {
            this.connection = connection ?? throw new ArgumentNullException("connection");
            this.selection = selection ?? throw new ArgumentNullException("selection");
            VariableLanguage = Language.English;
            ActionId = connection.RegisterNewActionId();
            reader = new DataCommandReader(selection.FieldList);
        }

        private void EnsureConnection() {
            if (!connection.Connected)
                connection.Open();
        }

        private void Execute() {
            if (!executed) {
                executed = true;
                int pageSize = selection.Paging ? selection.PageSize : 0;
                int offset = selection.Offset;
                reader.Reset();
                reader.Fill(connection.ExecuteQuery(selection.ToString(), ActionId, selection.FieldList.ToString(), WithMetaData, pageSize, offset, LanguageHelper.ToString(VariableLanguage)));
            } else {
                if (!reader.EndOfData)
                    reader.Fill(connection.GetNextRecord(ActionId));
            }
        }

        /// <summary>
        /// Führt die Abfrage aus und liefert den ersten Datensatz zurück oder NULL, wenn
        /// die Ergebnismenge leer ist. Enthält die Eregbnismenge weitere Datensätze, 
        /// wird die Abfrage automatisch abgebrochen. Die Selektionskriteren sollten möglichst nur
        /// zu einen Datensatz passen oder auf ein Element limitiert werden. 
        /// </summary>
        /// <returns></returns>
        public Record GetFirstRecord() {
            IEnumerator<Record> it = GetEnumerator();
            if (!it.MoveNext())
                return null;

            Record r = it.Current;
            BreakExecution();

            return r;
        }

        /**
         * Bricht eine bereits gestartete Abfrage ab.
         **/
        public void BreakExecution() {
            if (executed && !reader.EndOfData) {
                connection.BreakQueryExecution(ActionId);
            }

            reader.Reset();
            executed = false;
        }

        public IEnumerator<Record> GetEnumerator() {
            EnsureConnection();
            BreakExecution();

            do {
                Execute();

                while (reader.HasNext()) {
                    yield return reader.ReadNextRecord();
                }
            } while (!reader.EndOfData);
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        #region IDisposable-Implementierung

        protected virtual void Dispose(bool disposing) {
            if (!disposed) {
                if (disposing) {
                    BreakExecution();
                }

                disposed = true;
            }
        }

        public void Dispose() {
            Dispose(true);
        }

        #endregion
    }
}
