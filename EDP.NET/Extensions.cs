using System;
using System.Collections.Generic;
using System.Text;

namespace EDPDotNet
{
    public static class Extensions
    {
        public static T Field<T>(this Record r, string fieldName) {
            string value = r[fieldName];

            return (T)Convert.ChangeType(value, typeof(T));
        }
    }
}
