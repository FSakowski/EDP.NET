using System;
using System.Collections.Generic;
using System.Text;

namespace EDPDotNet.EPI {
    public static class CommandWords {

        public static class Session {

            public const string ChangeMandant = "CHM";
            public const string Logon = "LGN";
            public const string End = "END";
            public const string SetOption = "SET";
            public const string ShowOptions = "SHO";
            public const string SetDialogAnswer = "SDA";
            public const string Transaction = "TA";
            public const string Lock = "LCK";
            public const string ExecuteShell = "XSH";
        }

        public static class Selecting {
            public const string GetDatabaseName = "GDN";
            public const string GetGroupNames = "GGN";
            public const string GetInfosystemNames = "GIS";
            public const string GetEnumerations = "GAZ";
            public const string GetReferenceInfo = "GRI";
            public const string TestFieldValue = "TFV";
            public const string ExecuteQuery = "EXQ";
            public const string ReadPrimary = "RDP";
            public const string GetTableNames = "GTN";
            public const string GetFieldNames = "GFN";
            public const string GetKeyNames = "GKN";
            public const string GetMessageText = "GMT";
            public const string GetVariableReferences = "GVR";
            public const string GetTypeInfo = "GTI";
            public const string GetProcedureInfo = "GPI";
            public const string ExecuteProcedure = "XSP";
            public const string GetNextRecord = "GNR";
            public const string BreakQueryExecution = "BRQ";
            public const string GetTransactionState = "GTS";

        }

        public static class Editing {
            public const string GetTransactionState = "GTS";
            public const string New = "NEW";
            public const string Update = "UPD";
            public const string Delete = "DEL";
            public const string View = "VIE";
            public const string Edit = "EDI";
            public const string ExecuteCommandString = "XCS";
            public const string SubEdit = "SUB";
            public const string Break = "BRQ";
            public const string Commit = "COM";
            public const string Cancel = "CAN";
            public const string Save = "SAV";
            public const string SetFieldValue = "SFV";
            public const string SetFreeText = "SFT";
            public const string ResetFieldValue = "RFV";
            public const string GetFieldValue = "GFV";
            public const string GetFreeText = "GFT";
            public const string RowInsert = "RIN";
            public const string RowDelete = "RDL";
            public const string RowMove = "RMV";
            public const string SetContextField = "SCF";
            public const string SetEditorObservation = "SEO";
            public const string GetEditorObservation = "GEO";
            public const string GetEditorChanges = "GEC";
        }

        public static class Responses {
            public const string Acknowledge = "ACK";
            public const string NegativeAcknowledge = "NAK";
            public const string BeginOfData = "BOD";
            public const string EndOfData = "EOD";
            public const string Data = "D";
            public const string DataContinuation = "DC";
            public const string MetaData = "DM";
            public const string Error = "E";
            public const string StatusMessage = "S";
            public const string ProgressMessage = "P";
            public const string ChangeNotification = "CN";
            public const string Dialoginfo = "DLG";
            public const string End = "END";
        }
    }
}
