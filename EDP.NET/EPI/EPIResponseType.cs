using System;
using System.Collections.Generic;
using System.Text;

namespace EDPDotNet.EPI {
    /// <summary>
    /// Ergebnistyp einer EPI-Abfrage also das Endergebnis von mehreren Serverantworten.
    /// </summary>
    public enum EPIResponseType {
        Undefined,
        Acknowledge,
        NegativeAcknowledge,
        Data,
        MetaData,
        ChangeNotification,
        Dialoginfo,
        End
    }

    public static class EPIResponseTypeHelper {
        public static EPIResponseType GetTypeOf(EPICommand cmd) {
            if (cmd == null)
                return EPIResponseType.Undefined;

            // Antworten wie Status oder Daten werden nie alleine gesendet, sondern es folgt
            // immer eine abschließende Antwort. Daher ist bei diesen Kommandos der Typ noch undefiniert.
            switch (cmd.CMDWord) {
                case CommandWords.Responses.StatusMessage:
                    return EPIResponseType.Undefined;

                case CommandWords.Responses.Acknowledge:
                    return EPIResponseType.Acknowledge;

                case CommandWords.Responses.NegativeAcknowledge:
                    return EPIResponseType.NegativeAcknowledge;

                case CommandWords.Responses.BeginOfData:
                    return EPIResponseType.Undefined;

                case CommandWords.Responses.ChangeNotification:
                    return EPIResponseType.ChangeNotification;

                case CommandWords.Responses.Data:
                    return EPIResponseType.Undefined;

                case CommandWords.Responses.DataContinuation:
                    return EPIResponseType.Undefined;

                case CommandWords.Responses.EndOfData:
                    return EPIResponseType.Data;

                case CommandWords.Responses.End:
                    return EPIResponseType.End;

                case CommandWords.Responses.MetaData:
                    return EPIResponseType.MetaData;

                case CommandWords.Responses.ProgressMessage:
                    return EPIResponseType.Undefined;

                case CommandWords.Responses.Error:
                    return EPIResponseType.Undefined;

                default:
                    return EPIResponseType.Undefined;
            }
        }
    }
}
