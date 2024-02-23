using System;
using SmartFormat.Core.Extensions;

namespace RTLC.SmartFormat;
internal class SelectorTextToStringSource : Source
{
    public override bool TryEvaluateSelector(ISelectorInfo selectorInfo)
    {
        if (selectorInfo.SelectorText!.StartsWith('"')
            && selectorInfo.SelectorText.EndsWith('"'))
        {
            selectorInfo.Result = selectorInfo.SelectorText[1..^1];
            return true;
        }

        return false;
    }
}
