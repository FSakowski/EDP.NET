using EDPDotNet.EPI;
using System;
using System.Collections.Generic;
using System.Text;

namespace EDPDotNet {
    /// <summary>
    /// Repräsentiert ein Feld in Datensätzen und beinhaltet Metainformationen
    /// zu diesem Feld.
    /// </summary>
    public class Field {
        private string name;

        private string outName;

        private string origName;

        private string descr;

        private bool tableFld;

        private int length;

        private string type;

        private string originalType;

        #region Properties

        public string Name {
            get {
                return name;
            }
        }

        public string OutName {
            get {
                return outName;
            }
        }

        public string OriginalName {
            get {
                return origName;
            }
        }

        public string Description {
            get {
                return descr;
            }
        }

        public bool IsTableField {
            get {
                return tableFld;
            }
        }

        public int FieldLength {
            get {
                return length;
            }
        }

        public string Type {
            get {
                return type;
            }
        }

        public string OriginalType {
            get {
                return originalType;
            }
        }

        #endregion

        public Field(string name) {
            this.name = name ?? throw new ArgumentNullException("name");
        }

        public override string ToString() {
            return name;
        }

        public override int GetHashCode() {
            return name.GetHashCode();
        }

        public override bool Equals(object obj) {
            if (obj == null)
                return false;

            if (!(obj is Field))
                return false;

            Field f = (Field)obj;

            return f.name.Equals(name);
        }

        internal void SetMetaData(MetaDataType type, string value) {
            switch (type) {
                case MetaDataType.Name:
                    name = value;
                    break;

                case MetaDataType.OutName:
                    outName = value;
                    break;

                case MetaDataType.OriginalName:
                    origName = value;
                    break;

                case MetaDataType.Description:
                    descr = value;
                    break;

                case MetaDataType.TableFlag:
                    tableFld = Utilities.ToBool(value);
                    break;

                case MetaDataType.Length:
                    length = Int32.Parse(value);
                    break;

                case MetaDataType.Type:
                    this.type = value;
                    break;

                case MetaDataType.OriginalType:
                    originalType = value;
                    break;
            }
        }
    }
}
