using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace EDPDotNet.Linq {
    internal class TranslateResult {
        internal Selection Selection;

        internal LambdaExpression Projector;
    }
}
