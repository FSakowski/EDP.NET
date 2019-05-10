using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace EDPDotNet.Linq {
    internal class RecordProjection {
        internal FieldList List;
        internal Expression Selector;
    }
}
