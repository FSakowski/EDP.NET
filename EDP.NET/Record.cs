using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace EDPDotNet {
    public class Record : IDictionary<Field, string> {

        private Dictionary<Field, string> dataDictionary;
        private Dictionary<string, Field> lookupDictionary;

        public string this[string name] {
            get {
                if (name == null)
                    throw new ArgumentNullException("name");

                if (!lookupDictionary.ContainsKey(name))
                    throw new UnknownFieldException(name);

                return this[lookupDictionary[name]];
            }
        }

        public Record() {
            dataDictionary = new Dictionary<Field, string>();
            lookupDictionary = new Dictionary<string, Field>();
        }

        #region IDictionary-Implementierung

        public string this[Field field] { get => dataDictionary[field]; set => dataDictionary[field] = value; }

        public ICollection<Field> Keys => dataDictionary.Keys;

        public ICollection<string> Values => dataDictionary.Values;

        public int Count => dataDictionary.Count;

        public bool IsReadOnly => false;

        public void Add(Field field, string value) {
            if (field == null)
                throw new ArgumentNullException("field");

            dataDictionary.Add(field, value);
            lookupDictionary.Add(field.Name, field);
        }

        public void Add(KeyValuePair<Field, string> item) {
            if (item.Key == null)
                throw new ArgumentNullException("item", "Key of item must not be null");

            dataDictionary.Add(item.Key, item.Value);
            lookupDictionary.Add(item.Key.Name, item.Key);
        }

        public void Clear() {
            dataDictionary.Clear();
            lookupDictionary.Clear();
        }

        public bool Contains(KeyValuePair<Field, string> item) {
            return dataDictionary.ContainsKey(item.Key);
        }

        public bool ContainsKey(Field field) {
            return dataDictionary.ContainsKey(field);
        }

        public void CopyTo(KeyValuePair<Field, string>[] array, int arrayIndex) {
            ((ICollection<KeyValuePair<Field, string>>)dataDictionary).CopyTo(array, arrayIndex);
        }

        public IEnumerator<KeyValuePair<Field, string>> GetEnumerator() {
            return dataDictionary.GetEnumerator();
        }

        public bool Remove(Field field) {
            if (field == null)
                throw new ArgumentNullException("field");

            bool removed = dataDictionary.Remove(field);

            if (removed)
                lookupDictionary.Remove(field.Name);

            return removed;
        }

        public bool Remove(KeyValuePair<Field, string> item) {
            if (item.Key == null)
                throw new ArgumentNullException("item", "Key of item must not be null");

            bool removed = dataDictionary.Remove(item.Key);

            if (removed)
                lookupDictionary.Remove(item.Key.Name);

            return removed;
        }

        public bool TryGetValue(Field field, out string value) {
            return dataDictionary.TryGetValue(field, out value);
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return dataDictionary.GetEnumerator();
        }

        #endregion

        public ICollection<string> NameKeys => lookupDictionary.Keys;

        public bool ContainsField(string name) {
            return lookupDictionary.ContainsKey(name);
        }

        public bool Remove(string name) {
            if (!ContainsField(name))
                return false;

            return dataDictionary.Remove(lookupDictionary[name]);
        }

        public void Add(string name, string value) {
            Add(new Field(name), value);
        }
    }
}
