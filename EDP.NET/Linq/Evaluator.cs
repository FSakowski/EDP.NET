using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace EDPDotNet.Linq {
    /// <summary>
    /// Evaluiert Teile eines Ausdrucks, um etwa lokale Variablen zu einen
    /// konstanten Wert zu transformieren.
    /// </summary>
    public static class Evaluator {

        public static Expression PartialEval(Expression expression, Func<Expression, bool> fnCanBeEvaluated) {
            return new SubtreeEvaluator(new Nominator(fnCanBeEvaluated).Nominate(expression)).Eval(expression);
        }

        public static Expression PartialEval(Expression expression) {
            return PartialEval(expression, Evaluator.CanBeEvaluatedLocally);
        }

        public static bool CanBeEvaluatedLocally(Expression expression) {
            return expression.NodeType != ExpressionType.Parameter;
        }

        class SubtreeEvaluator : ExpressionVisitor {
            HashSet<Expression> candidates;

            internal SubtreeEvaluator(HashSet<Expression> candidates) {
                this.candidates = candidates;
            }

            internal Expression Eval(Expression exp) {
                return Visit(exp);
            }

            public override Expression Visit(Expression e) {
                if (e == null)
                    return null;

                if (candidates.Contains(e))
                    return Evaluate(e);

                return base.Visit(e);
            }

            private Expression Evaluate(Expression e) {
                if (e.NodeType == ExpressionType.Constant)
                    return e;

                LambdaExpression lambda = Expression.Lambda(e);
                Delegate fn = lambda.Compile();

                return Expression.Constant(fn.DynamicInvoke(null), e.Type);
            }
        }

        class Nominator : ExpressionVisitor {
            Func<Expression, bool> fnCanBeEvaluated;
            HashSet<Expression> candidates;
            bool cannotBeEvaluated;

            internal Nominator(Func<Expression, bool> fnCanBeEvaluated) {
                this.fnCanBeEvaluated = fnCanBeEvaluated;
            }

            internal HashSet<Expression> Nominate(Expression expression) {
                candidates = new HashSet<Expression>();
                Visit(expression);
                return candidates;
            }

            public override Expression Visit(Expression e) {
                if (e != null) {
                    bool saveCannotBeEvaluated = cannotBeEvaluated;
                    cannotBeEvaluated = false;
                    base.Visit(e);

                    if (!cannotBeEvaluated) {
                        if (fnCanBeEvaluated(e)) {
                            candidates.Add(e);
                        } else {
                            cannotBeEvaluated = true;
                        }
                    }

                    cannotBeEvaluated |= saveCannotBeEvaluated;
                }

                return e;
            }
        }

    }
}
