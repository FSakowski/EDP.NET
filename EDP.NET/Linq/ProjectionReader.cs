using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace EDPDotNet.Linq {
    internal class ProjectionReader<T> : IEnumerable<T>, IEnumerable {
        Enumerator enumerator;
  

        internal ProjectionReader(Query query, Func<ProjectionRow, T> projector) {
            enumerator = new Enumerator(query, projector);
        }

        public IEnumerator<T> GetEnumerator() {
            Enumerator e = enumerator;

            if (e == null) {
                throw new InvalidOperationException("cannot enumerate more than once");
            }

            enumerator = null;
            return e;
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return this.GetEnumerator();
        }

        class Enumerator : ProjectionRow, IEnumerator<T>, IEnumerator, IDisposable {
            Query query;

            T current;

            Record record;

            int index = 0;

            DataSet data;

            Func<ProjectionRow, T> projector;

            internal Enumerator(Query query, Func<ProjectionRow, T> projector) {
                this.query = query;
                this.projector = projector;
            }

            public override object GetValue(string field) {
                if (String.IsNullOrEmpty(field))
                    throw new IndexOutOfRangeException();

                return record[field];
            }

            public T Current => current;

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

                record = data[index];
                current = projector(this);
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