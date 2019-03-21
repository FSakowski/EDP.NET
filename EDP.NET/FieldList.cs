using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace EDPDotNet {
    public class FieldList : IReadOnlyCollection<Field>, IList<Field> {
        private List<Field> set;
        private bool readOnly = false;

        /// <summary>
        /// Instanziert eine neue anpassbare Feldliste.
        /// </summary>
        public FieldList() {
            set = new List<Field>();
        }

        /// <summary>
        /// Instanziert eine nicht veränderbare Feldliste aus einer bestehenden.
        /// </summary>
        /// <param name="fieldList"></param>
        public FieldList(FieldList fieldList) {
            set = new List<Field>(fieldList);
            readOnly = true;
        }

        #region IList-Implementierung

        public int Count => set.Count;

        public bool IsReadOnly => readOnly;

        public Field this[int index] {
            get => set[index];
            set {
                if (readOnly)
                    throw new NotSupportedException("field list is read only");

                this[index] = value;
            }
        }

        public void Add(Field item) {
            if (readOnly)
                throw new NotSupportedException("field list is read only");

            set.Add(item);
        }

        public void Clear() {
            if (readOnly)
                throw new NotSupportedException("field list is read only");

            set.Clear();
        }

        public bool Contains(Field item) {
            return set.Contains(item);
        }

        public void CopyTo(Field[] array, int arrayIndex) {
            set.CopyTo(array, arrayIndex);
        }

        public IEnumerator<Field> GetEnumerator() {
            return set.GetEnumerator();
        }

        public bool Remove(Field item) {
            if (readOnly)
                throw new NotSupportedException("field list is read only");

            return set.Remove(item);
        }


        void ICollection<Field>.Add(Field item) {
            if (readOnly)
                throw new NotSupportedException("field list is read only");

            set.Add(item);
        }

        public int IndexOf(Field item) {
            return set.IndexOf(item);
        }

        public void Insert(int index, Field item) {
            if (readOnly)
                throw new NotSupportedException("field list is read only");

            set.Insert(index, item);
        }

        public void RemoveAt(int index) {
            if (readOnly)
                throw new NotSupportedException("field list is read only");

            set.RemoveAt(index);
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return set.GetEnumerator();
        }

        #endregion

        public FieldList Add(string fieldName) {
            Add(new Field(fieldName));
            return this;
        }

        public override string ToString() {
            StringBuilder sb = new StringBuilder();

            foreach (Field field in this) {
                if (sb.Length > 0)
                    sb.Append(",");

                sb.Append(field);
            }

            return sb.ToString();
        }
    }
}
