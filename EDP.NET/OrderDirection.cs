using System;
using System.Collections.Generic;
using System.Text;

namespace EDPDotNet {
    public enum OrderDirection {
        /// <summary>
        /// Unbestimmt.
        /// </summary>
        Undefined,

        /// <summary>
        /// Aufsteigend.
        /// </summary>
        Ascending,

        /// <summary>
        /// Absteigend.
        /// </summary>
        Descending
    }

    public static class OrderDirectionHelper {
        public static string ToString(OrderDirection dir, bool order) {
            switch (dir) {
                case OrderDirection.Ascending: return "forwards";
                case OrderDirection.Descending: return "backwards";
                default:
                    throw new NotSupportedException("order direction " + dir + " not supported");
            }
        }
    }
}
