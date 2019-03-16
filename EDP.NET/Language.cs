using System;
using System.Collections.Generic;
using System.Text;

namespace EDPDotNet {
    public enum Language {
        German,
        English
    }

    public sealed class LanguageHelper {
        public static string ToString(Language lang) {
            switch (lang) {
                case Language.German: return "DE";
                case Language.English: return "EN";
                default:
                    return String.Empty;
            }
        }
    }
}
