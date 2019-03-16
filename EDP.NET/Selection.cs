using System;
using System.Collections.Generic;
using System.Text;

namespace EDPDotNet {
    public class Selection {

        public const char CriteriaSeparator = ';';

        private List<Condition> conditions;

        #region Properties

        public int Database {
            get;
            private set;
        }

        public int[] Groups {
            get;
            private set;
        }

        public ConditionLinkMode LinkMode {
            get;
            set;
        }

        public string Key {
            get;
            set;
        }

        public string OrderField {
            get;
            set;
        }

        public OrderDirection Direction {
            get;
            set;
        }

        public bool IncludeRows {
            get;
            set;
        }

        public FilingMode FilingMode {
            get;
            set;
        }

        public uint Limit {
            get;
            set;
        }

        public bool Paging {
            get;
            set;
        }

        public int PageSize {
            get;
            set;
        }

        public int Offset {
            get;
            set;
        }

        public Language VariableLanguage {
            get;
            set;
        }

        #endregion

        public Selection (int db, params int[] groups) {
            Database = db;
            Groups = groups;
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

        public void AddConditions(Condition c) {
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
                res.AddConditions(c);

            if (s == null)
                return res;

            foreach (Condition c in s.conditions)
                res.AddConditions(c);

            return res;
        }

        public override string ToString() {
            StringBuilder sb = new StringBuilder();
            sb.Append(Database);

            if (Groups != null && Groups.Length > 0) {
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

            foreach (Condition c in conditions)
                sb.Append(c).Append(";");

            sb.AppendFormat("@link={0};", LinkModeToString(LinkMode));

            if (!String.IsNullOrEmpty(Key))
                sb.AppendFormat("@sort={0};", Key);

            if (!String.IsNullOrEmpty(OrderField)) {
                sb.AppendFormat("@order={0}", OrderField);

                if (Direction != OrderDirection.Undefined)
                    sb.Append(".").Append(DirectionToString(Direction, true));

                sb.Append(";");
            }

            if (Direction !=  OrderDirection.Undefined && String.IsNullOrEmpty(OrderField)) {
                sb.AppendFormat("@direction={0};", DirectionToString(Direction, false));
            }

            sb.AppendFormat("@rows={0};", IncludeRows ? "(Yes)" : "(No)");
            sb.AppendFormat("@filingmode={0};", FilingModeToString(FilingMode));

            if (Limit > 0)
                sb.AppendFormat("@maxhit={0};", Limit);

            sb.AppendFormat("@lang={0}", LanguageHelper.ToString(VariableLanguage));

            return sb.ToString();
        }

        private string LinkModeToString(ConditionLinkMode linkMode) {
            switch(linkMode) {
                case ConditionLinkMode.And: return "(And)";
                case ConditionLinkMode.Or: return "(Or)";
                default:
                    throw new NotSupportedException("link mode " + linkMode + " not supported");
            }
        }

        private string DirectionToString(OrderDirection dir, bool order) {
            switch(dir) {
                case OrderDirection.Ascending: return "forwards";
                case OrderDirection.Descending: return "backwards";
                default:
                    throw new NotSupportedException("order direction " + dir + " not supported");
            }
        }

        private string FilingModeToString(FilingMode mode) {
            switch (FilingMode) {
                case FilingMode.Both:
                    return "(Both)";
                case FilingMode.Filed:
                    return "(Filed)";
                case FilingMode.Versionied:
                    return "(Versionied)";
                case FilingMode.Active:
                    return "(Active)";
                default:
                    throw new NotSupportedException("filingmode " + mode + " not supported");
            }
        }
    }
}
