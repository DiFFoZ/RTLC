using SmartFormat.Core.Extensions;

namespace RTLC.SmartFormat;
internal class TranslationSource : Source
{
    public override bool TryEvaluateSelector(ISelectorInfo selectorInfo)
    {
        if (selectorInfo.CurrentValue is string currentValue
            && selectorInfo.SelectorText.Equals("Translate"))
        {
            selectorInfo.Result = Translation.GetLocalizedText(currentValue);
            return true;
        }

        return false;
    }
}
