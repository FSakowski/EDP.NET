using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace EDPDotNet.Linq {
    internal class RecordReader : IEnumerable<Record>, IEnumerable {
        Enumerator enumerator;


        internal RecordReader(Query query) {
            enumerator = new Enumerator(query);
        }

        public IEnumerator<Record> GetEnumerator() {
            Enumerator e = this.enumerator;
            
            if (e == null) {
                throw new InvalidOperationException("cannot enumerate more than once");
            }

            enumerator = null;
            return e;
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return this.GetEnumerator();
        }

        class Enumerator : IEnumerator<Record>, IEnumerator, IDisposable{
            Query query;

            Record current;

            int index = 0;

            DataSet data;

            internal Enumerator(Query query) {
                this.query = query;
            }

            public Record Current => current;

            object IEnumerator.Current => current;

            public void Dispose() {
                query.Reset();
            }

            public bool MoveNext() {
                if (data == null) {
                    ReadNextDataSet();
                }

                if (index >= data.Count) {
                    if (query.EndOfData)
                        return false;

                    ReadNextDataSet();

                    if (data.Count == 0)
                        return false;
                }

                current = data[index];
                index++;
                return true;
            }

            private void ReadNextDataSet() {
                data = query.Execute();
                index = 0;
            }

            public void Reset() {
                query.Reset();
            }
        }
    }

}
