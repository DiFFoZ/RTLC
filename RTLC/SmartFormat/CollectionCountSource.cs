using System.Collections;
using SmartFormat.Core.Extensions;

namespace RTLC.SmartFormat;
internal class CollectionCountSource : Source
{
    public override bool TryEvaluateSelector(ISelectorInfo selectorInfo)
    {
        if (selectorInfo.CurrentValue is ICollection currentValue
             && selectorInfo.SelectorText.Equals("Count"))
        {
            selectorInfo.Result = currentValue.Count;
            return true;
        }

        return false;
    }
}
