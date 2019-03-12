using System;
using System.Collections.Generic;
using System.Text;

namespace EDPDotNet {
    public class Selection {

        public int Database {
            get;
            private set;
        }

        public int[] Groups {
            get;
            private set;
        }

        public Selection (int db, params int[] groups) {
            Database = db;
            Groups = groups;
        }


    }
}
