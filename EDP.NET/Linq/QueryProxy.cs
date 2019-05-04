using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace EDPDotNet.Linq {
    public class QueryProxy<T> : IQueryable<T>, IQueryable, IEnumerable<T>, IEnumerable, IOrderedQueryable<T>, IOrderedQueryable {

        private IQueryProvider provider;

        Expression expression;

        public Type ElementType => typeof(T);

        public Expression Expression => this.expression;

        public IQueryProvider Provider => this.provider;


        public IEnumerator<T> GetEnumerator() {
            return ((IEnumerable<T>)provider.Execute(expression)).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return ((IEnumerable)provider.Execute(expression)).GetEnumerator();
        }

        public QueryProxy(IQueryProvider provider) {
            this.provider = provider ?? throw new ArgumentNullException("provider");
            expression = Expression.Constant(this);
        }

        public QueryProxy(IQueryProvider provider, Expression expression) {
            this.provider = provider ?? throw new ArgumentNullException("provider");

            if (expression == null)
                throw new ArgumentNullException("expression");

            if (!typeof(IQueryable<T>).IsAssignableFrom(expression.Type))
                throw new ArgumentOutOfRangeException("expression");

            this.expression = expression;
        }
    }


}
