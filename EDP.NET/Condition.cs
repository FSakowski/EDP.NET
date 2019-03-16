using System;
using System.Collections.Generic;
using System.Text;

namespace EDPDotNet {
    public class Condition {

        public static class Operators {
            public const string Eq = "{0}=={1}";
            public const string EqIgnoreCase = "{0}~{1}";
            public const string Neq = "{0}<>{1}";
            public const string NeqIgnoreCase = "{0}~<>{1}";
            public const string Between = "{0}={1}!{2}";
            public const string Empty = "{0}==`";
            public const string NotEmpty = "{0}<>`";
            public const string Contains = "{0}/{1}";
            public const string ContainsIgnoreCase = "{0}~/{1}";
            public const string ContainsWord = "{0}//{1}";
            public const string ContainsWordIgnoreCase = "~//";
            public const string Gt = "{0}={1}!!";
            public const string Geqt = "{0}={1}!";
            public const string Lt = "{0}=!!{1}";
            public const string Leqt = "{0}=!{1}";
            public const string Matchcode = "{0}=`{1}";
            public const string MatchcodeIgnoreCase = "{0}~`{1}";
            public const string NotMatchcode = "{0}<>`{1}";
            public const string NotMatchcodeIgnoreCase = "{0}~<>`{1}";
            public const string Expression = "{0}/=={1}";
            public const string ExpressionIgnoreCase = "{0}~/=={1}";
            public const string NotExpression = "{0}/<>{1}";
            public const string NotExpressionIgnoreCase = "{0}~/<>{1}";
        }

        private string field;
        private string operatorFormat;
        private string value1;
        private string value2;

        private Condition(string field, string op, string value1 = null, string value2 = null) {
            this.field = field;
            this.operatorFormat = op;
            this.value1 = value1;
            this.value2 = value2;
        }

        public override string ToString() {
            if (String.IsNullOrEmpty(value1))
                return String.Format(operatorFormat, field);

            if (String.IsNullOrEmpty(value2))
                return String.Format(operatorFormat, field, value1);

            return String.Format(operatorFormat, field, value1, value2);
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
