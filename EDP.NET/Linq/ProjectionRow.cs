using System;
using System.Collections.Generic;
using System.Text;

namespace EDPDotNet.Linq {
    public abstract class ProjectionRow {
        public abstract object GetValue(string field, Type type);
    }
}
