using System;
using System.Collections.Generic;
using System.Text;

namespace EDPDotNet {
    /// <summary>
    /// Stellt eine Selektion in abas dar, welche über
    /// die EDP-Schnittstelle ausgeführt werden kann. Eine Selektion
    /// kann immer nur für eine Datenbank erfolgen aber über mehrere Gruppen
    /// hinweg.
    /// </summary>
    public class Selection {

        public const char CriteriaSeparator = ';';

        private List<Condition> conditions;

        private FieldList fieldList;

        #region Properties

        /// <summary>
        /// Gibt die Datenbanknummer zurück.
        /// </summary>
        public int Database {
            get;
            private set;
        }

        /// <summary>
        /// Gibt die Gruppennummern zurück, auf die die Selektion
        /// eingeschränkt ist.
        /// </summary>
        public int[] Groups {
            get;
            private set;
        }

        /// <summary>
        /// Liefert den Verknüpfungsmodus zurück oder legt diesen fest.
        /// Die Selektionkriterien können nur als ganzes entweder mit UND oder ODER
        /// verknüpft werden. Der Standardwert ist UND.
        /// </summary>
        public ConditionLinkMode LinkMode {
            get;
            set;
        }

        /// <summary>
        /// Liefert einen zuvor festgelegten Schlüssel zurück, der für die 
        /// Selektion verwendet werden soll, oder legt diesen fest.
        /// Erfolgt keine Zuweisung, wird eine optimale Schlüsselwahl
        /// automatisch durchgeführt.
        /// </summary>
        public string Key {
            get;
            set;
        }

        /// <summary>
        /// Liefert das Feld zurück nachdem sortiert werden soll oder legt
        /// dieses Fest. Standardmäßig erfolgt die Sortierung anhand
        /// des Schlüssels, welcher entweder manuell oder automatisch
        /// ermittelt wird.
        /// </summary>
        public string OrderField {
            get;
            set;
        }

        /// <summary>
        /// Liefert die Sortierreihenfolge zurück oder legt diese fest.
        /// </summary>
        public OrderDirection Direction {
            get;
            set;
        }

        /// <summary>
        /// Legt fest, ob Tabellenzeilen berücksichtigt werden sollen.
        /// </summary>
        public bool IncludeRows {
            get;
            set;
        }

        /// <summary>
        /// Liefert die Ablageart zurück oder legt diese fest.
        /// Standardmäßig werden nur aktive Datensätze gelesen.
        /// </summary>
        public FilingMode FilingMode {
            get;
            set;
        }

        /// <summary>
        /// Legt fest, ob die Selektion auf eine bestimmte Anzahl an
        /// Datensätzen limitiert werden soll. Standardmäßig ist der Wert 0, d.h.
        /// es findet keine Limitierung statt.
        /// Eine Limitierung beschleunigt die Suche und die Datenübermittlung.
        /// </summary>
        public uint Limit {
            get;
            set;
        }

        /// <summary>
        /// Legt fest, ob bei Ausführung der Selektion nur Teile der Ergebnismenge
        /// übermittelt werden sollen. Die Datenabfrage bleibt dann solange
        /// lebendig, bis alle Daten abgerufen worden sind oder ein Abbruch erfolgt.
        /// Standardmäßig werden alle Daten sofort übertragen.
        /// </summary>
        public bool Paging {
            get;
            set;
        }

        /// <summary>
        /// Legt die Anzahl an Datensätzen fest, die beim aktivierten <see cref="Paging">Paging</see> maximal
        /// übermittelt werden sollen. 
        /// </summary>
        public int PageSize {
            get;
            set;
        }

        /// <summary>
        /// Legt fest, ob eine bestimmte Anzahl an Datensätzen am Anfang nicht 
        /// übertragen werden sollen und somit nicht Teil der Ergebnismenge sind.
        /// Standardmäßig ist das Offset = 0 und damit deaktiviert.
        /// </summary>
        public int Offset {
            get;
            set;
        }

        /// <summary>
        /// Liefert die Sprache der Variablennamen zurück oder legt diese fest.
        /// Standardmäßig ist die Variablensprache englisch.
        /// </summary>
        public Language VariableLanguage {
            get;
            set;
        }

        /// <summary>
        /// In der Feldliste können die zu selektierenden Felder bestimmt werden.
        /// ist die Feldliste leer, werden alle Felder übertragen.
        /// </summary>
        public FieldList FieldList {
            get {
                return fieldList;
            }
        }

        #endregion

        /// <summary>
        /// Erzeugt eine neue Selektion auf eine festzulegende Datenbank und Gruppe(n).
        /// </summary>
        /// <param name="db"></param>
        /// <param name="groups"></param>
        public Selection (int db, params int[] groups) {
            Database = db;
            Groups = groups;
            fieldList = new FieldList();
            Init();
        }

        private void Init() {
            conditions = new List<Condition>();
            LinkMode = ConditionLinkMode.And;
            Direction = OrderDirection.Undefined;
            IncludeRows = false;
            FilingMode = FilingMode.Active;
            Limit = 0;
            VariableLanguage = Language.English;
        }

        /// <summary>
        /// Fügt der Selektion ein Selektionskriterium hinzu.
        /// </summary>
        /// <param name="c"></param>
        public void AddCondition(Condition c) {
            if (c == null)
                throw new ArgumentNullException("c");

            conditions.Add(c);
        }

        /// <summary>
        /// Fügt dieser Selektion eine andere Selektion hinzu und übernimmt die Suchkriterien. Beide Selektionen 
        /// bleiben unverändert.
        /// </summary>
        /// <param name="s">Neue Selektion mit den zusammengesetzten Suchkriterien.</param>
        /// <returns></returns>
        public Selection Merge(Selection s) {
            Selection res = new Selection(Database, Groups) {
                LinkMode = this.LinkMode,
                Key = this.Key,
                OrderField = this.OrderField,
                Direction = this.Direction,
                IncludeRows = this.IncludeRows,
                FilingMode = this.FilingMode,
                Limit = this.Limit,
                VariableLanguage = this.VariableLanguage
            };

            foreach (Condition c in conditions)
                res.AddCondition(c);

            if (s == null)
                return res;

            foreach (Condition c in s.conditions)
                res.AddCondition(c);

            return res;
        }

        public override string ToString() {
            StringBuilder sb = new StringBuilder();

            if (Groups != null && Groups.Length > 0) {
                sb.Append(Database);
                sb.Append(":");
                bool first = true;

                foreach (int group in Groups) {
                    if (!first)
                        sb.Append(".");

                    sb.Append(group);
                    first = false;
                }
            }

            sb.Append(",");

            // ohne Gruppenangabe die Datenbank mithilfe der Option @database
            // angeben, da die Angabe <DBNr>:, ohne Gruppe nicht angenommen wird
            if (Groups == null || Groups.Length == 0) {
                sb.Append(String.Format("@database={0};", Database));
            }

            foreach (Condition c in conditions)
                sb.Append(c).Append(";");

            sb.AppendFormat("@link={0};", ConditionLinkModeHelper.ToString(LinkMode));

            if (!String.IsNullOrEmpty(Key))
                sb.AppendFormat("@sort={0};", Key);

            if (!String.IsNullOrEmpty(OrderField)) {
                sb.AppendFormat("@order={0}", OrderField);

                if (Direction != OrderDirection.Undefined)
                    sb.Append(".").Append(OrderDirectionHelper.ToString(Direction, true));

                sb.Append(";");
            }

            if (Direction !=  OrderDirection.Undefined && String.IsNullOrEmpty(OrderField)) {
                sb.AppendFormat("@direction={0};", OrderDirectionHelper.ToString(Direction, false));
            }

            sb.AppendFormat("@rows={0};", IncludeRows ? "(Yes)" : "(No)");
            sb.AppendFormat("@filingmode={0};", FilingModeHelper.ToString(FilingMode));

            if (Limit > 0)
                sb.AppendFormat("@maxhit={0};", Limit);

            sb.AppendFormat("@lang={0}", LanguageHelper.ToString(VariableLanguage));

            return sb.ToString();
        }
    }
}
