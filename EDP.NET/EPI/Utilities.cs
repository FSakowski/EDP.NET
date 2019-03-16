using System;
using System.Collections.Generic;
using System.Text;

namespace EDPDotNet.EPI {
    public static class Utilities {
        public static bool ToBool(string value) {
            if (String.IsNullOrEmpty(value))
                return false;

            if ("0".Equals(value.Trim()))
                return false;

            if ("1".Equals(value.Trim()))
                return true;

            throw new EPIException("value " + value + " couldn't be parsed to a boolean");
        }
    }
}
