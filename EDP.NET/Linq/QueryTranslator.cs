using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace EDPDotNet.Linq {

    /// <summary>
    /// Übersetzt eine Linq-Abfrage in eine abas-Selektion. Selektionen in abas sind nur schwach kompatibel zu Linq-Abfragen:
    /// <list type="bullet">
    /// <item>Es sind nur Vergleiche mit Konstanten möglich</item>
    /// <item>Abfragen können keine Unterabfragen beinhalten</item>
    /// <item>Und/Oder-Verknüpfungen können nicht kombiniert werden</item>
    /// <item>Keine Gruppierungen möglich</item>
    /// </list>
    /// Aus der Linq-Abfrage werden nur die in abas unterstützen Teile entnommen. Bei Kombinationen von und/oder- verknüpften Abfragen, wird die komplette Abfrage auf Oder umgestellt, damit 
    /// die Eregbnismenge alle möglichen Werte beinhaltet, welche dann lokal via Linq-to-Obects weiter eingeschränkt werden kann.
    /// </summary>
    internal class QueryTranslator : ExpressionVisitor {

        private Selection selection;
        private bool negateCondition;

        internal QueryTranslator() {

        }

        internal Selection Translate(int db, int[] groups, Expression expression) {
            selection = new Selection(db, groups);
            this.Visit(expression);
            return this.selection;
        }

        private static Expression StripQuotes(Expression e) {
            while (e.NodeType == ExpressionType.Quote) {
                e = ((UnaryExpression)e).Operand;
            }

            return e;
        }

        private static string TrimValue(string s) {
            return s.Replace(";", "").Replace("@", "");
        }

        protected override Expression VisitMethodCall(MethodCallExpression node) {
            if (node.Method.DeclaringType == typeof(Queryable) && node.Method.Name == "Where") {
                Visit(node.Arguments[0]);

                LambdaExpression lambda = (LambdaExpression)StripQuotes(node.Arguments[1]);
                Visit(lambda.Body);
                return node;
            }

            if (node.Method.DeclaringType == typeof(Queryable) && node.Method.Name == "FirstOrDefault") {
                selection.Limit = 1;

                Visit(node.Arguments[0]);

                return node;
            }

            if (node.Method.DeclaringType == typeof(Queryable) && node.Method.Name == "Select") {
                Visit(node.Arguments[0]);
                return node;
            }

            return base.VisitMethodCall(node);
        }

        protected override Expression VisitUnary(UnaryExpression node) {
            switch (node.NodeType) {
                case ExpressionType.Not:
                    negateCondition = !negateCondition;

                    if (IsSupportedFieldExpression(node.Operand)) {
                        string fieldName = ExtractFieldName(node.Operand);
                        AddBoolean(fieldName);
                    }

                    Visit(node.Operand);

                    negateCondition = !negateCondition;
                    break;
            }

            return node;
        }

        private void AddBoolean(string fieldName) {
            if (!String.IsNullOrEmpty(fieldName)) {

                // Sonderfall Ablageart, diese steht als Selektionsoption zur Verfügung
                if (fieldName == "recordFiled") {
                    selection.FilingMode = negateCondition ? FilingMode.Active : FilingMode.Filed;
                } else {
                    selection.AddCondition(
                        Condition.Eq(fieldName, negateCondition ? "(No)" : "(Yes)"));
                }
            }
        }

        protected override Expression VisitBinary(BinaryExpression node) {
            bool validCondition = false;
            string fieldName = String.Empty;
            string field2Name = String.Empty;
            string value = String.Empty;

            ConstantExpression constExpr = null;

            // unterstützte Where-Klauseln herausfiltern
            if (IsSupportedFieldExpression(node.Left))
                fieldName = ExtractFieldName(node.Left);

            if (IsSupportedFieldExpression(node.Right) && String.IsNullOrEmpty(fieldName)) {
                fieldName = ExtractFieldName(node.Right);
            } else if (IsSupportedFieldExpression(node.Right)) {
                field2Name = ExtractFieldName(node.Right);
            }

            if (node.Left.NodeType == ExpressionType.Constant)
                constExpr = node.Left as ConstantExpression;

            if (node.Right.NodeType == ExpressionType.Constant)
                constExpr = node.Right as ConstantExpression;

            // Feld = Wert Abfrage
            if (constExpr != null) {
                value = ExtractConstantValue(constExpr);
                validCondition = !String.IsNullOrEmpty(fieldName);
            }

            switch(node.NodeType) {
                case ExpressionType.AndAlso:
                case ExpressionType.And:
                    AddBoolean(fieldName);
                    AddBoolean(field2Name);
                    break;

                case ExpressionType.OrElse:
                case ExpressionType.Or:
                    AddBoolean(fieldName);
                    AddBoolean(field2Name);

                    // beim ersten Auftreten dieses Typs die Selektion dauerhaft auf Oder umstellen
                    selection.LinkMode = ConditionLinkMode.Or;
                    break;

                case ExpressionType.Equal:
                    if (validCondition) {
                        if (String.IsNullOrEmpty(value)) {
                            selection.AddCondition(negateCondition ?
                               Condition.NotEmpty(fieldName) :
                               Condition.Empty(fieldName));
                            
                        } else {
                            selection.AddCondition(negateCondition ?
                                 Condition.Neq(fieldName, value) :
                                 Condition.Eq(fieldName, value));
                        }
                    }
                    break;

                case ExpressionType.NotEqual:
                    if (validCondition) {
                        if (String.IsNullOrEmpty(value)) {
                            selection.AddCondition(negateCondition ?
                                Condition.Empty(fieldName) :
                                Condition.NotEmpty(fieldName));
                        } else {
                            selection.AddCondition(negateCondition ?
                                Condition.Eq(fieldName, value) :
                                Condition.Neq(fieldName, value));
                        }
                    }
                    break;

                case ExpressionType.LessThan:
                    if (validCondition) {
                        selection.AddCondition(negateCondition ?
                            Condition.Geqt(fieldName, value) :
                            Condition.Lt(fieldName, value));
                    }
                    break;

                case ExpressionType.GreaterThan:
                    if (validCondition) {
                        selection.AddCondition(negateCondition ?
                            Condition.Leqt(fieldName, value) :
                            Condition.Gt(fieldName, value));
                    }
                    break;

                case ExpressionType.LessThanOrEqual:
                    if (validCondition) {
                        selection.AddCondition(negateCondition ?
                            Condition.Gt(fieldName, value) :
                            Condition.Leqt(fieldName, value));
                    }
                    break;

                case ExpressionType.GreaterThanOrEqual:
                    if (validCondition) {
                        selection.AddCondition(negateCondition ?
                            Condition.Lt(fieldName, value) :
                            Condition.Geqt(fieldName, value));
                    }
                    break;

                default:
                    throw new NotSupportedException($"The binary operator '{node.NodeType}' is not supported");
            }

            Visit(node.Left);
            Visit(node.Right);
            return node;
        }

        private string ExtractConstantValue(ConstantExpression node) {
            IQueryable q = node.Value as IQueryable;
            if (q != null) {
                // Unterabfrage
                throw new NotSupportedException("nested queries are not supported");
            } else if (node.Value == null) {
                return null;
            } else {
                switch (Type.GetTypeCode(node.Value.GetType())) {
                    case TypeCode.Boolean:
                        return ((bool)node.Value) ? "(Yes)" : "(No)";
                    case TypeCode.String:
                        return TrimValue((string)node.Value);
                    case TypeCode.Object:
                        throw new NotSupportedException($"The constant for '{node.Value}' is not supported");
                    default:
                        return node.Value.ToString();
                }
            }
        }

        private string ExtractFieldName(Expression node) {
            if (!(node is MethodCallExpression))
                return String.Empty;

            MethodCallExpression methodExpr = (MethodCallExpression)node;

            if (methodExpr.Arguments.Count == 0)
                return String.Empty;

            ConstantExpression fieldNameExpr;
            if (methodExpr.Method.DeclaringType == typeof(Extensions)) {
                fieldNameExpr = methodExpr.Arguments[1] as ConstantExpression;
            } else {
                fieldNameExpr = methodExpr.Arguments[0] as ConstantExpression;
            }
            
            if (fieldNameExpr != null && fieldNameExpr.Value is string) {
                string fieldName = (string)fieldNameExpr.Value;

                if (!String.IsNullOrEmpty(fieldName)) {
                    // Feld automatisch zur Feldliste hinzufügen
                    if (!selection.FieldList.Contains(fieldName))
                        selection.FieldList.Add(fieldName);
                }

                return fieldName;
            }

            return String.Empty;
        }

        private bool IsSupportedFieldExpression(Expression node) {
            MethodCallExpression methodExpr = node as MethodCallExpression;

            if (methodExpr == null)
                return false;

            if (methodExpr.Method.DeclaringType == typeof(Extensions) && methodExpr.Method.Name == "Field")
                return true;

            if (methodExpr.Method.DeclaringType == typeof(Record) && methodExpr.Method.Name == "get_Item")
                return true;

            return false;
        }
    }
}
