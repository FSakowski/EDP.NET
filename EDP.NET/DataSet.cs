using EDPDotNet.EPI;
using System;
using System.Collections.Generic;
using System.Text;

namespace EDPDotNet {
    public class DataSet : List<Record> {

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
        /// Liest aus eine Warteschlange von EPI-Kommandos nacheinander die Daten im Form von
        /// Key-Value-Paaren ein. Fragmentierte Datensätze werden berücksichtigt. Alle Nachrichten bis zum
        /// EOD-Kommando werden aus der Warteschlange entnommen. 
        /// </summary>
        /// <param name="cmds"></param>
        /// <returns></returns>
        public static DataSet Fill(Queue<EPICommand> cmds) {
            if (cmds == null)
                throw new ArgumentNullException("cmds must not be null");

            DataSet result = new DataSet();

            string lastField = String.Empty;
            Record lastRecord = null;

            while(cmds.Count > 0) {
                EPICommand cmd = cmds.Dequeue();
                bool data = CommandWords.Responses.Data.Equals(cmd.CMDWord);
                bool continuation = CommandWords.Responses.DataContinuation.Equals(cmd.CMDWord);

                if (CommandWords.Responses.BeginOfData.Equals(cmd.CMDWord)) {
                    result.ActionId = cmd.ActionId;
                }

                if (continuation) {
                    string fragment = cmd[0];
                    if (lastRecord != null)
                        lastRecord[lastField] = lastRecord[lastField] + fragment;

                    continuation = true;
                }

                if (data || continuation) {
                    Record rec = new Record();
                    lastField = fillDataSet(rec, cmd, continuation ? 1 : 0);
                    result.Add(rec);

                    if (!cmd.Completed)
                        lastRecord = rec;
                }

                if (CommandWords.Responses.EndOfData.Equals(cmd.CMDWord)) {
                    result.Success = Utiltities.ToBool(cmd[CommandFields.Responses.EOD.OKFlag]);
                    result.EOF = Utiltities.ToBool(cmd[CommandFields.Responses.EOD.EOFFlag]);
                    return result;
                }
            }

            throw new EPIException("Server has not sent an end-of-data command. Data query was probably canceled.");
        }

        private static string fillDataSet(Dictionary<string, string> record, EPICommand data, int offset) {
            string lastField = String.Empty;

            string fieldName = String.Empty;
            string fieldValue;
            int mod = 0;
            
            for(int i = offset; i < data.Fields.Length; i++) {
                if (mod++ % 2 == 0) {
                    fieldName = data[i];
                    fieldValue = String.Empty;
                } else {
                    fieldValue = data[i];
                    record.Add(fieldName, fieldValue);
                }

                lastField = fieldName;
            }

            return lastField;
        }

    }
}
