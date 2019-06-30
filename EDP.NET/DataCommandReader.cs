using System;
using System.Collections.Generic;
using System.Text;
using EDPDotNet.EPI;

namespace EDPDotNet {
    /// <summary>
    /// Interpretiert BOD, EOD, D, DC und DM Nachrichten und wandelt diese in <see cref="Record"/> Objekte um.
    /// </summary>
    public class DataCommandReader {

        private List<Record> data;

        private EPICommand currentCmd = null;
        private Record lastRecord = null;
        private Field lastField;

        private bool dataContinuation;

        private FieldList fields;

        public DataCommandReader() {
            data = new List<Record>();
            EndOfData = true;
        }

        public DataCommandReader(FieldList fields) : this() {
            this.fields = fields;
        }

        #region Properties

        public FieldList FieldList {
            get {
                if (fields == null)
                    throw new InvalidOperationException("First, the query must be executed so that a field list can be returned");

                return new FieldList(fields);
            }
        }

        public uint EstimatedRecordsCount {
            get;
            private set;
        }
        public bool Success {
            get;
            private set;
        }
        public bool EndOfData {
            get;
            private set;
        }

        #endregion

        public List<Record> Read(Queue<EPICommand> cmds) {
            Reset();

            while (cmds.Count > 0) {
                currentCmd = cmds.Dequeue();
                dataContinuation = false;

                switch (currentCmd.CMDWord) {
                    case CommandWords.Responses.BeginOfData:
                        ReadBeginOfData();
                        break;

                    case CommandWords.Responses.MetaData:
                        ReadMetaData();
                        break;

                    case CommandWords.Responses.Data:
                        ReadData();
                        break;

                    case CommandWords.Responses.DataContinuation:
                        dataContinuation = true;
                        ReadData();
                        break;

                    case CommandWords.Responses.EndOfData:
                        ReadEndOfData();
                        break;

                    default:
                        throw new NotSupportedException("command " + currentCmd.CMDWord + " is not supported");
                }
            }

            return data;
        }

        private void ReadBeginOfData() {
            string numRecords = currentCmd[CommandFields.Responses.BOD.NumRecordsTotal];

            EndOfData = false;

            if (!String.IsNullOrEmpty(numRecords))
                EstimatedRecordsCount = uint.Parse(numRecords);
        }

        private void ReadMetaData() {
            // RDP mit unterschiedlichen Variablentabellen wird aktuell nicht untersützt
            if (data.Count > 0)
                throw new NotSupportedException("Server responds with meta data after sending data commands. Subsequent changes of field lists are not yet supported.");

            MetaDataTypeHelper.TryParse(currentCmd[CommandFields.Responses.DM.MetaDataType], out MetaDataType type);

            // neue nicht untersützte Metadaten ignorieren
            if (type == MetaDataType.Undefined)
                return;

            if (fields == null)
                fields = new FieldList();

            for (int i = 1; i < currentCmd.Fields.Length; i++) {
                string value = currentCmd[i];
                // die erste Angabe in den Metadaten ist der Typ und wird hier übersprungen, die Feldliste beginnt aber bei 0
                int fieldIdx = i - 1;

                if (type == MetaDataType.Name && !fields.Contains(value)) {
                    fields.Add(new Field(value));
                } else if (fieldIdx < fields.Count) {
                    // Die Metadaten kommen ohne Bezug zu den Feldnamen aber immer in der gleichen Reihenfolge
                    Field f = fields[fieldIdx];
                    f.SetMetaData(type, value);
                }
            }
        }

        private void ReadData() {
            // Datenforsetzung, erstes Fragment zum letzen Feldwert hinzufügen
            if (dataContinuation) {
                string fragment = currentCmd[0];
                if (lastRecord != null && lastField != null)
                    lastRecord[lastField] = lastRecord[lastField] + fragment;
            }

            // Datenzeile
            Record rec = dataContinuation ? lastRecord : new Record();
            lastField = FillRecord(rec, currentCmd, fields, dataContinuation ? 1 : 0);

            if (!dataContinuation)
                data.Add(rec);

            if (!currentCmd.Completed)
                lastRecord = rec;
        }

        private Field FillRecord(IDictionary<Field, string> record, EPICommand data, FieldList fieldList, int offset) {
            Field lastField = null;
            string fieldValue;

            for (int i = offset; i < data.Fields.Length; i++) {
                if (i < fieldList.Count) {
                    Field f = fieldList[i];
                    fieldValue = data[i];
                    record.Add(f, fieldValue);

                    lastField = f;
                }
            }

            return lastField;
        }

        private void ReadEndOfData() {
            Success = Utilities.ToBool(currentCmd[CommandFields.Responses.EOD.OKFlag]);
            EndOfData = Utilities.ToBool(currentCmd[CommandFields.Responses.EOD.EOFFlag]);
        }

        private void Reset() {
            data.Clear();
            currentCmd = null;
            lastRecord = null;
            lastField = null;
        }
    }
}
