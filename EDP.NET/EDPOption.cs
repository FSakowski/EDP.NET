using System;
using System.Collections.Generic;

namespace EDPDotNet {
    public class EDPOption
    {
        private readonly string value;
        private static readonly Dictionary<string, EDPOption> predefinedValues;

        public static readonly EDPOption NumMode = new EDPOption("NUMMODE");
        public static readonly EDPOption DecimalPoint = new EDPOption("DECIMALPOINT");
        public static readonly EDPOption BoolMode = new EDPOption("BOOLMODE");
        public static readonly EDPOption EnumMode = new EDPOption("ENUMMODE");
        public static readonly EDPOption RefMode = new EDPOption("VERWMODE");
        public static readonly EDPOption DateMode = new EDPOption("DATEMODE");
        public static readonly EDPOption FreeTextMode = new EDPOption("FTMODE");
        public static readonly EDPOption Charset = new EDPOption("CHARSET");
        public static readonly EDPOption FieldSeparator = new EDPOption("FLDSEP");
        public static readonly EDPOption FieldSubstitution = new EDPOption("FLDSEPSUBST");
        public static readonly EDPOption EscapeChar = new EDPOption("ESCAPE");
        public static readonly EDPOption NewLine = new EDPOption("NEWLINE");
        public static readonly EDPOption Carriage = new EDPOption("CARRIAGE");
        public static readonly EDPOption NullByte = new EDPOption("NULBYTE");
        public static readonly EDPOption LineBreak = new EDPOption("LINEBREAK");
        public static readonly EDPOption ContinueLines = new EDPOption("CONTLINES");
        public static readonly EDPOption SubstitutionMode = new EDPOption("SUBSTMODE");
        public static readonly EDPOption TrimValues = new EDPOption("TRIMVALUES");
        public static readonly EDPOption TruncText = new EDPOption("TRUNCTEXT");
        public static readonly EDPOption AutoRound = new EDPOption("AUTOROUND");
        public static readonly EDPOption EchoMode = new EDPOption("ECHOMODE");
        public static readonly EDPOption UpdateMode = new EDPOption("UPDATEMODE");
        public static readonly EDPOption StoreRowMode = new EDPOption("STOREROWMODE");
        public static readonly EDPOption FOPMode = new EDPOption("FOPMODE");
        public static readonly EDPOption Language = new EDPOption("LANG");
        public static readonly EDPOption VarLanguage = new EDPOption("VARLANG");
        public static readonly EDPOption ShowNotes = new EDPOption("SHOWNOTES");
        public static readonly EDPOption ShowText = new EDPOption("SHOWTEXT");
        public static readonly EDPOption LongLockMessage = new EDPOption("LONGLOCKMSG");
        public static readonly EDPOption LockBehaviour = new EDPOption("LOCKBEHAVIOR");
        public static readonly EDPOption Progress = new EDPOption("PROGRESS");
        public static readonly EDPOption ChangeNotify = new EDPOption("CHANGENOTIFY");
        public static readonly EDPOption DialogMode = new EDPOption("DIALOGMODE");
        public static readonly EDPOption Context = new EDPOption("CONTEXT");
        public static readonly EDPOption OneTimePassword = new EDPOption("OTPWUSER");

        static EDPOption() {
            predefinedValues = new Dictionary<string, EDPOption>();
            predefinedValues.Add("NUMMODE", NumMode);
            predefinedValues.Add("DECIMALPOINT", DecimalPoint);
            predefinedValues.Add("BOOLMODE", BoolMode);
            predefinedValues.Add("ENUMMODE", EnumMode);
            predefinedValues.Add("VERWMODE", RefMode);
            predefinedValues.Add("DATEMODE", DateMode);
            predefinedValues.Add("FTMODE", FreeTextMode);
            predefinedValues.Add("CHARSET", Charset);
            predefinedValues.Add("FLDSEP", FieldSeparator);
            predefinedValues.Add("FLDSEPSUBST", FieldSubstitution);
            predefinedValues.Add("ESCAPE", EscapeChar);
            predefinedValues.Add("NEWLINE", NewLine);
            predefinedValues.Add("CARRIAGE", Carriage);
            predefinedValues.Add("NULBYTE", NullByte);
            predefinedValues.Add("LINEBREAK", LineBreak);
            predefinedValues.Add("CONTLINES", ContinueLines);
            predefinedValues.Add("SUBSTMODE", SubstitutionMode);
            predefinedValues.Add("TRIMVALUES", TrimValues);
            predefinedValues.Add("TRUNCTEXT", TruncText);
            predefinedValues.Add("AUTOROUND", AutoRound);
            predefinedValues.Add("ECHOMODE", EchoMode);
            predefinedValues.Add("UPDATEMODE", UpdateMode);
            predefinedValues.Add("STOREROWMODE", StoreRowMode);
            predefinedValues.Add("FOPMODE", FOPMode);
            predefinedValues.Add("LANG", Language);
            predefinedValues.Add("VARLANG", VarLanguage);
            predefinedValues.Add("SHOWNOTES", ShowNotes);
            predefinedValues.Add("SHOWTEXT", ShowText);
            predefinedValues.Add("LONGLOCKMSG", LongLockMessage);
            predefinedValues.Add("LOCKBEHAVIOR", LockBehaviour);
            predefinedValues.Add("PROGRESS", Progress);
            predefinedValues.Add("CHANGENOTIFY", ChangeNotify);
            predefinedValues.Add("DIALOGMODE", DialogMode);
            predefinedValues.Add("CONTEXT", Context);
            predefinedValues.Add("OTPWUSER", OneTimePassword);
        }

        private EDPOption(string value) {
            this.value = value;
        }

        public static EDPOption Parse(string value) {
            if (String.IsNullOrEmpty(value))
                throw new FormatException($"invalid value for type {nameof(EDPOption)}");

            string key = value.ToUpper();
            if (!predefinedValues.ContainsKey(key))
                throw new FormatException($"invalid value for type {nameof(EDPOption)}");

            return predefinedValues[key];
        }

        public string Value {
            get {
                return value;
            }
        }

        public override string ToString() {
            return Value;
        }
    }
}
