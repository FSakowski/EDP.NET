using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Linq;

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

            List<Record> data;

            Func<ProjectionRow, T> projector;

            internal Enumerator(Query query, Func<ProjectionRow, T> projector) {
                this.query = query;
                this.projector = projector;
            }

            public override object GetValue(string field, Type type) {
                if (String.IsNullOrEmpty(field))
                    throw new IndexOutOfRangeException();

                return Convert.ChangeType(record[field], type);
            }

            public T Current => current;

            object IEnumerator.Current => current;

            public void Dispose() {
                query.Dispose();
            }

            public bool MoveNext() {
                if (data == null) {
                    ReadData();
                }

                if (index >= data.Count) {
                    return false;
                }

                record = data[index];
                current = projector(this);
                index++;
                return true;
            }

            private void ReadData() {
                data = query.ToList();
                index = 0;
            }

            public void Reset() {
                query.BreakExecution();
            }
        }
    }
}