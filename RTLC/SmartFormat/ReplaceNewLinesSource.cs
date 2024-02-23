using System;
using SmartFormat.Core.Extensions;

namespace RTLC.SmartFormat;
internal class ReplaceNewLinesSource : Source
{
    public override bool TryEvaluateSelector(ISelectorInfo selectorInfo)
    {
        if (selectorInfo.CurrentValue is string currentValue
            && selectorInfo.SelectorText.Equals("NewLines"))
        {
            selectorInfo.Result = currentValue.Replace(@"\n", "\n");
            return true;
        }

        return false;
    }
}
