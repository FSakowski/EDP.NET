using System;
using System.Collections.Generic;
using System.Text;

namespace EDPDotNet {
    public enum ConditionLinkMode {
        And,
        Or
    }

    public static class ConditionLinkModeHelper {
        public static string ToString(ConditionLinkMode linkMode) {
            switch (linkMode) {
                case ConditionLinkMode.And: return "(And)";
                case ConditionLinkMode.Or: return "(Or)";
                default:
                    throw new NotSupportedException("link mode " + linkMode + " not supported");
            }
        }
    }
}
