using System;
using System.Collections.Generic;
using System.Text;

namespace EDPDotNet {
    public enum MetaDataType {
        Undefined = -1,
        Name,
        OutName,
        OriginalName,
        Description,
        TableFlag,
        Length,
        Type,
        OriginalType
    }

    public static class MetaDataTypeHelper {
        private static readonly Dictionary<string, MetaDataType> predefinedValues;

        static MetaDataTypeHelper() {
            predefinedValues = new Dictionary<string, MetaDataType> {
                { "NAME", MetaDataType.Name },
                { "OUTNAME", MetaDataType.OutName },
                { "ORIGNAME", MetaDataType.OriginalName },
                { "DESC", MetaDataType.Description },
                { "TABLEFLAG", MetaDataType.TableFlag },
                { "LENGTH", MetaDataType.Length },
                { "TYPE", MetaDataType.Type },
                { "ORIGTYPE", MetaDataType.OriginalType }
            };
        }

        public static MetaDataType Parse(string value) {
            if (String.IsNullOrEmpty(value))
                throw new FormatException($"invalid value for type {nameof(MetaDataType)}");

            string key = value.ToUpper();
            if (!predefinedValues.ContainsKey(key))
                throw new FormatException($"invalid value for type {nameof(MetaDataType)}");

            return predefinedValues[key];
        }

        public static bool TryParse(string value, out MetaDataType type) {
            if (String.IsNullOrEmpty(value))
                throw new FormatException($"invalid value for type {nameof(MetaDataType)}");

            string key = value.ToUpper();
            if (!predefinedValues.ContainsKey(key)) {
                type = MetaDataType.Undefined;
                return false;
            }

            type = predefinedValues[key];
            return true;
        }
    }
}
