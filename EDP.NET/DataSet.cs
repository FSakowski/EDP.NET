using EDPDotNet.EPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace EDPDotNet {
    public class DataSet : List<Record> {

        private FieldList fieldList;

        #region Properties

        /// <summary>
        /// Zugehörige Transaktions-Id für die Kommunikation mit dem EPI-Dienst. Dient
        /// zur Unterscheidung zwischen mehreren Abfragen.
        /// </summary>
        public uint ActionId {
            get;
            private set;
        }

        /// <summary>
        /// Ruft einen Wert ab, der angibt, ob alle Daten des Datensets gesendet wurden oder
        /// die Abfrage vorzeitig beendet wurde.
        /// </summary>
        public bool Success {
            get;
            private set;
        }

        /// <summary>
        /// Ruft einen Wert ab, der angibt, ob von einer vorab durchgeführten Abfrage noch weitere 
        /// Daten zu erwarten sind oder nicht.
        /// </summary>
        public bool EOF {
            get;
            private set;
        }

        /// <summary>
        /// Schätzwert, wieviele Datensätze gefunden wurden.
        /// </summary>
        public uint EstimatedRecordsCount {
            get;
            private set;
        }

        public FieldList FieldList {
            get {
                if (fieldList == null)
                    fieldList = new FieldList();

                return fieldList;
            }
            private set {
                fieldList = value;
            }
        }

        #endregion

        /// <summary>
        /// Liest aus eine Warteschlange von EPI-Kommandos nacheinander die Daten im Form von
        /// Key-Value-Paaren ein. Fragmentierte Datensätze werden berücksichtigt. Alle Nachrichten bis zum
        /// EOD-Kommando werden aus der Warteschlange entnommen. 
        /// </summary>
        /// <param name="cmds"></param>
        /// <returns></returns>
        public static DataSet Fill(Queue<EPICommand> cmds, FieldList fieldList) {
            if (cmds == null)
                throw new ArgumentNullException("cmds");

            DataSet result = new DataSet();
            Field lastField = null;
            Record lastRecord = null;

            while(cmds.Count > 0) {
                EPICommand cmd = cmds.Dequeue();
                bool data = CommandWords.Responses.Data.Equals(cmd.CMDWord);
                bool continuation = CommandWords.Responses.DataContinuation.Equals(cmd.CMDWord);

                if (CommandWords.Responses.BeginOfData == cmd.CMDWord) {
                    result.ActionId = cmd.ActionId;
                    string numRecords = cmd[CommandFields.Responses.BOD.NumRecordsTotal];

                    if (!String.IsNullOrEmpty(numRecords))
                        result.EstimatedRecordsCount = UInt32.Parse(numRecords);
                }

                if (CommandWords.Responses.MetaData == cmd.CMDWord) {
                    // RDP mit unterschiedlichen Variablentabellen wird aktuell nicht untersützt
                    if (result.Count > 0)
                        throw new NotSupportedException("Server responds with meta data after sending data commands. Subsequent changes of field lists are not yet supported.");

                    SetMetaData(fieldList, cmd);
                }

                // Datenforsetzung, erstes Fragment zum letzen Feldwert hinzufügen
                if (continuation) {
                    string fragment = cmd[0];
                    if (lastRecord != null && lastField != null)
                        lastRecord[lastField] = lastRecord[lastField] + fragment;

                    continuation = true;
                }

                // Datenzeile
                if (data || continuation) {
                    Record rec = new Record();
                    lastField = FillDataSet(rec, cmd, fieldList, continuation ? 1 : 0);
                    result.Add(rec);

                    if (!cmd.Completed)
                        lastRecord = rec;
                }

                if (CommandWords.Responses.EndOfData == cmd.CMDWord) {
                    result.Success = Utilities.ToBool(cmd[CommandFields.Responses.EOD.OKFlag]);
                    result.EOF = Utilities.ToBool(cmd[CommandFields.Responses.EOD.EOFFlag]);
                    result.FieldList = new FieldList(fieldList);
                    return result;
                }
            }

            throw new EPIException("Server has not sent an end-of-data command. Data query was probably canceled.");
        }

        private static void SetMetaData(FieldList fieldList, EPICommand data) {
            MetaDataTypeHelper.TryParse(data[CommandFields.Responses.DM.MetaDataType], out MetaDataType type);

            // neue nicht untersützte Metadaten ignorieren
            if (type == MetaDataType.Undefined)
                return;

            for (int i = 1; i < data.Fields.Length; i++) {
                string value = data[i];
                // die erste Angabe in den Metadaten ist der Typ und wird hier übersprungen, die Feldliste beginnt aber bei 0
                int fieldIdx = i - 1;

                if (type == MetaDataType.Name && !fieldList.Contains(value)) {
                    fieldList.Add(new Field(value));
                } else if (fieldIdx < fieldList.Count) {
                    // Die Metadaten kommen ohne Bezug zu den Feldnamen aber immer in der gleichen Reihenfolge
                    Field f = fieldList[fieldIdx];
                    f.SetMetaData(type, value);
                }
            }
        }

        private static Field FillDataSet(IDictionary<Field, string> record, EPICommand data, FieldList fieldList, int offset) {
            Field lastField = null;
            string fieldValue;
            
            for(int i = offset; i < data.Fields.Length; i++) {
                if (i < fieldList.Count) {
                    Field f = fieldList[i];
                    fieldValue = data[i];
                    record.Add(f, fieldValue);

                    lastField = f;
                }
            }

            return lastField;
        }

    }
}
