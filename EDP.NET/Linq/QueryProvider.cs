using EDPDotNet.EPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace EDPDotNet.Linq {
    public class QueryProvider : IQueryProvider {

        Context context;

        private int db;
        private int[] groups;
        private FieldList fieldList;

        internal QueryProvider(Context context, int db, int[] groups, FieldList fieldList) {
            this.context = context ?? throw new ArgumentNullException("context");
            this.db = db;
            this.groups = groups;
            this.fieldList = fieldList;
        }

        public IQueryable CreateQuery(Expression expression) {
            Type elementType = TypeSystem.GetElementType(expression.Type);
            try {
                return (IQueryable)Activator.CreateInstance(typeof(QueryProxy<>).MakeGenericType(elementType), new object[] { this, expression });
            } catch (TargetInvocationException e) {
                throw e.InnerException;
            }
            
        }

        public IQueryable<T> CreateQuery<T>(Expression expression) {
            return new QueryProxy<T>(this, expression);
        }

        object IQueryProvider.Execute(Expression expression) {
            return this.Execute(expression);
        }

        T IQueryProvider.Execute<T>(Expression expression) {
            return (T)this.Execute(expression);
        }

        public Selection TranslateSelection(Expression expression) {
            return new QueryTranslator().Translate(db, groups, expression);
        }

        public object Execute(Expression expression) {
            Selection sel = this.TranslateSelection(expression);
            sel.FieldList.Merge(fieldList);

#if DEBUG
            Console.WriteLine(sel.ToString());
#endif

            Query query = context.CreateQuery(sel);

            Type elementType = expression.Type;
            if (elementType == typeof(Record)) {
                return query.GetFirstRecord();
            } else {
                elementType = TypeSystem.GetElementType(expression.Type);
                if (elementType == typeof(Record))
                    return new RecordReader(query);
                else
                    throw new NotSupportedException($"The expression type '{elementType}' is not supported");
            }  
        }
    }
}
