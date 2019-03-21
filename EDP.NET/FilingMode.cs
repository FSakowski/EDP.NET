using System;
using System.Collections.Generic;
using System.Text;

namespace EDPDotNet {
    public enum FilingMode {
        Active,
        Filed,
        Both,
        Versionied
    }

    public static class FilingModeHelper {
        public static string ToString(FilingMode mode) {
            switch (mode) {
                case FilingMode.Both:
                    return "(Both)";
                case FilingMode.Filed:
                    return "(Filed)";
                case FilingMode.Versionied:
                    return "(Versionied)";
                case FilingMode.Active:
                    return "(Active)";
                default:
                    throw new NotSupportedException("filingmode " + mode + " not supported");
            }
        }
    }
}
