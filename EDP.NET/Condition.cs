using System;
using System.Collections.Generic;
using System.Text;

namespace EDPDotNet {
    public class Condition {

        public abstract class Operator {
            public string Format {
                get;
                private set;
            }

            internal Operator(string format) {
                this.Format = format;
            }

            public abstract string ToString(string field, params string[] values);
        }

        internal class UnaryOperator : Operator {
           public UnaryOperator(string format) : base(format) {
            }

            public override string ToString(string field, params string[] values) {
                return String.Format(Format, field);
            }
        }

        internal class BinaryOperator : Operator {
            public BinaryOperator(string format) : base(format) {
            }

            public override string ToString(string field, params string[] values) {
                return String.Format(Format, field, values[0]);
            }
        }

        internal class TernaryOperator : Operator {
            public TernaryOperator(string format) : base(format) {
            }

            public override string ToString(string field, params string[] values) {
                return String.Format(Format, field, values[0], values[1]);
            }
        }

        public static class Operators {
            public readonly static Operator Eq = new BinaryOperator("{0}=={1}");
            public readonly static Operator EqIgnoreCase = new BinaryOperator("{0}~{1}");
            public readonly static Operator Neq = new BinaryOperator("{0}<>{1}");
            public readonly static Operator NeqIgnoreCase = new BinaryOperator("{0}~<>{1}");
            public readonly static Operator Between = new TernaryOperator("{0}={1}!{2}");
            public readonly static Operator Empty = new UnaryOperator("{0}==`");
            public readonly static Operator NotEmpty = new UnaryOperator("{0}<>`");
            public readonly static Operator Contains = new BinaryOperator("{0}/{1}");
            public readonly static Operator ContainsIgnoreCase = new BinaryOperator("{0}~/{1}");
            public readonly static Operator ContainsWord = new BinaryOperator("{0}//{1}");
            public readonly static Operator ContainsWordIgnoreCase = new BinaryOperator("{0}~//{1}");
            public readonly static Operator Gt = new BinaryOperator("{0}={1}!!");
            public readonly static Operator Geqt = new BinaryOperator("{0}={1}!");
            public readonly static Operator Lt = new BinaryOperator("{0}=!!{1}");
            public readonly static Operator Leqt = new BinaryOperator("{0}=!{1}");
            public readonly static Operator Matchcode = new BinaryOperator("{0}=`{1}");
            public readonly static Operator MatchcodeIgnoreCase = new BinaryOperator("{0}~`{1}");
            public readonly static Operator NotMatchcode = new BinaryOperator("{0}<>`{1}");
            public readonly static Operator NotMatchcodeIgnoreCase = new BinaryOperator("{0}~<>`{1}");
            public readonly static Operator Expression = new BinaryOperator("{0}/=={1}");
            public readonly static Operator ExpressionIgnoreCase = new BinaryOperator("{0}~/=={1}");
            public readonly static Operator NotExpression = new BinaryOperator("{0}/<>{1}");
            public readonly static Operator NotExpressionIgnoreCase = new BinaryOperator("{0}~/<>{1}");
        }

        private string field;
        private Operator op;
        private string value1;
        private string value2;

        private Condition(string field, Operator op, string value1 = null, string value2 = null) {
            this.field = field;
            this.op = op;
            this.value1 = value1;
            this.value2 = value2;
        }

        public override string ToString() {
            return op.ToString(field, value1, value2);
        }

        /// <summary>
        /// Prüfung auf Gleicheit.
        /// </summary>
        /// <param name="field"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Condition Eq(string field, string value) {
            return new Condition(field, Operators.Eq, value);
        }

        /// <summary>
        /// Prüfung auf Gleicheit ohne Berücksichtigung von Groß- und Kleinschreibung.
        /// </summary>
        /// <param name="field"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Condition EqIgnoreCase(string field, string value) {
            return new Condition(field, Operators.EqIgnoreCase, value);
        }

        /// <summary>
        /// Prüfung auf Ungleichheit.
        /// </summary>
        /// <param name="field"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Condition Neq(string field, string value) {
            return new Condition(field, Operators.Neq, value);
        }

        /// <summary>
        /// Prüfung auf Ungleichheit ohne Berücksichtigung von Groß- und Kleinschreibung.
        /// </summary>
        /// <param name="field"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Condition NeqIgnoreCase(string field, string value) {
            return new Condition(field, Operators.NeqIgnoreCase, value);
        }

        /// <summary>
        /// Bereichsprüfung.
        /// </summary>
        /// <param name="field"></param>
        /// <param name="value1"></param>
        /// <param name="value2"></param>
        /// <returns></returns>
        public static Condition Between(string field, string value1, string value2) {
            return new Condition(field, Operators.Between, value1, value2);
        }

        /// <summary>
        /// Prüfung auf Gefüllt.
        /// </summary>
        /// <param name="field"></param>
        /// <returns></returns>
        public static Condition Empty(string field) {
            return new Condition(field, Operators.Empty);
        }

        /// <summary>
        /// Prüfung auf Leer.
        /// </summary>
        /// <param name="field"></param>
        /// <returns></returns>
        public static Condition NotEmpty(string field) {
            return new Condition(field, Operators.Empty);
        }

        /// <summary>
        /// Pürfung auf Enthalten.
        /// </summary>
        /// <param name="field"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Condition Contains(string field, string value) {
            return new Condition(field, Operators.Contains, value);
        }

        /// <summary>
        /// Prüfung auf Enthalten ohne Berücksichtigung von Groß- und Kleinschreibung.
        /// </summary>
        /// <param name="field"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Condition ContainsIgnoreCase(string field, string value) {
            return new Condition(field, Operators.ContainsIgnoreCase, value);
        }

        /// <summary>
        /// Prüfung auf Enthalten mit ganzen Wörtern.
        /// </summary>
        /// <param name="field"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Condition ContainsWord(string field, string value) {
            return new Condition(field, Operators.ContainsWord, value);
        }

        /// <summary>
        /// Prüfung auf Enthalten mit ganzen Wörtern und ohne Berücksichtigung von Groß- und 
        /// Kleinschreibung.
        /// </summary>
        /// <param name="field"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Condition ContainsWordIgnoreCase(string field, string value) {
            return new Condition(field, Operators.ContainsWordIgnoreCase, value);
        }

        /// <summary>
        /// Prüfung auf Größer.
        /// </summary>
        /// <param name="field"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Condition Gt(string field, string value) {
            return new Condition(field, Operators.Gt, value);
        }

        /// <summary>
        /// Prüfung auf Größer oder Gleichheit.
        /// </summary>
        /// <param name="field"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Condition Geqt(string field, string value) {
            return new Condition(field, Operators.Geqt, value);
        }

        /// <summary>
        /// Prüfung auf Kleiner.
        /// </summary>
        /// <param name="field"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Condition Lt(string field, string value) {
            return new Condition(field, Operators.Lt, value);
        }

        /// <summary>
        /// Prüfung auf Kleiner oder Gleichheit.
        /// </summary>
        /// <param name="field"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Condition Leqt(string field, string value) {
            return new Condition(field, Operators.Leqt, value);
        }

        /// <summary>
        /// Prüfung per Matchcode.
        /// </summary>
        /// <param name="field"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Condition Matchcode(string field, string value) {
            return new Condition(field, Operators.Matchcode, value);
        }

        /// <summary>
        /// Prüfung per Matchcode und ohne Berücksichtigung von Groß- und
        /// Kleinschreibung.
        /// </summary>
        /// <param name="field"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Condition MatchcodeIgnoreCase(string field, string value) {
            return new Condition(field, Operators.MatchcodeIgnoreCase, value);
        }

        /// <summary>
        /// Prüfung per Matchcode negiert.
        /// </summary>
        /// <param name="field"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Condition NotMatchcode(string field, string value) {
            return new Condition(field, Operators.NotMatchcode, value);
        }

        /// <summary>
        /// Prüfung per Matchcode negiert und ohne Berücksichtigung von Groß- und
        /// Kleinschreibung.
        /// </summary>
        /// <param name="field"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Condition NotMatchcodeIgnoreCase(string field, string value) {
            return new Condition(field, Operators.NotMatchcodeIgnoreCase, value);
        }

        /// <summary>
        /// Prüfung per regulären Ausdruck.
        /// </summary>
        /// <param name="field"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Condition Expression(string field, string value) {
            return new Condition(field, Operators.Expression, value);
        }

        /// <summary>
        /// Prüfung per regulären Ausdruck negiert.
        /// </summary>
        /// <param name="field"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Condition ExpressionIgnoreCase(string field, string value) {
            return new Condition(field, Operators.ExpressionIgnoreCase, value);
        }

        /// <summary>
        /// Prüfung per regulären Asudruck negiert.
        /// </summary>
        /// <param name="field"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Condition NotExpression(string field, string value) {
            return new Condition(field, Operators.NotExpression, value);
        }

        /// <summary>
        /// Prüfung per regulären Ausdruck negiert und ohne Berücksichtigung von Groß- und
        /// Kleinschreibung.
        /// </summary>
        /// <param name="field"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Condition NotExpressionIgnoreCase(string field, string value) {
            return new Condition(field, Operators.NotExpressionIgnoreCase, value);
        }
    }
}
