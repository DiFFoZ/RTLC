using SmartFormat.Core.Extensions;

namespace RTLC.SmartFormat;
internal class AppendStringSource : Source
{
    public override bool TryEvaluateSelector(ISelectorInfo selectorInfo)
    {
        // {0}.Append_Aboba
        // -> XXXAboba
        if (selectorInfo.CurrentValue is string currentValue
             && selectorInfo.SelectorText.StartsWith("Append_"))
        {
            selectorInfo.Result = currentValue + selectorInfo.SelectorText[7..];
            return true;
        }

        return false;
    }
}
