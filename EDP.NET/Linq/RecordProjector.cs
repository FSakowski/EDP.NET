using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace EDPDotNet.Linq {
    internal class RecordProjector : ExpressionVisitor {

        FieldList list;

        ParameterExpression row;

        static MethodInfo miGetValue;

        internal RecordProjector() {
            if (miGetValue == null) {
                miGetValue = typeof(ProjectionRow).GetMethod("GetValue");
            }
        }

        internal RecordProjection ProjectRecords(Expression e, ParameterExpression row) {
            list = new FieldList();
            this.row = row;

            Expression selector = Visit(e);
            return new RecordProjection { List = list, Selector = selector };
        }

        protected override Expression VisitMember(MemberExpression m) {
            if (m.Expression != null && m.Expression.NodeType == ExpressionType.Parameter) {
                if (!list.Contains(m.Member.Name))
                    list.Add(m.Member.Name);

                return Expression.Convert(Expression.Call(row, miGetValue,
                    Expression.Constant(m.Member.Name)), m.Type);
            }

            return base.VisitMember(m);
        }

        protected override Expression VisitMethodCall(MethodCallExpression methodExpr) {
            if (methodExpr == null)
                return base.VisitMethodCall(methodExpr);

            if (methodExpr.Arguments.Count == 0)
                return base.VisitMethodCall(methodExpr);

            ConstantExpression fieldNameExpr;
            if (methodExpr.Method.DeclaringType == typeof(Extensions)) {
                fieldNameExpr = methodExpr.Arguments[1] as ConstantExpression;
            } else {
                fieldNameExpr = methodExpr.Arguments[0] as ConstantExpression;
            }

            if (fieldNameExpr != null && fieldNameExpr.Value is string) {
                string fieldName = (string)fieldNameExpr.Value;

                if (!String.IsNullOrEmpty(fieldName)) {
                    if (!list.Contains(fieldName))
                        list.Add(fieldName);

                    return Expression.Convert(Expression.Call(row, miGetValue,
                        Expression.Constant(fieldName)), methodExpr.Type);
                }

                return base.VisitMethodCall(methodExpr);
            }

            return base.VisitMethodCall(methodExpr);
        }
    }
}
