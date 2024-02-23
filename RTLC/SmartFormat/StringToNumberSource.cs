using System;
using SmartFormat.Core.Extensions;

namespace RTLC.SmartFormat;
internal class StringToNumberSource : Source
{
    public override bool TryEvaluateSelector(ISelectorInfo selectorInfo)
    {
        if (selectorInfo.CurrentValue is not string currentValue)
        {
            return false;
        }

        if (!selectorInfo.SelectorText.Equals("ToNumber"))
        {
            return false;
        }

        selectorInfo.Result = Convert.ToInt64(currentValue);
        return true;
    }
}
