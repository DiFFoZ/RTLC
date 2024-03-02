using SmartFormat.ZString;

namespace RTLC;
internal static partial class Translation
{
    private static readonly ZStringBuilder s_ZStringBuilder = new(false);

    internal static string Format(string format, object arg0)
    {
        format = GetLocalizedText(format, true);

        var builder = GetStringBuilder();
        builder.AppendFormat(format, arg0);
        return GetLocalizedText(builder.ToString());
    }

    internal static string Format(string format, object arg0, object arg1)
    {
        format = GetLocalizedText(format, true);

        var builder = GetStringBuilder();
        builder.AppendFormat(format, arg0, arg1);
        return GetLocalizedText(builder.ToString());
    }

    internal static string Format(string format, object arg0, object arg1, object arg2)
    {
        format = GetLocalizedText(format, true);

        var builder = GetStringBuilder();
        builder.AppendFormat(format, arg0, arg1, arg2);
        return GetLocalizedText(builder.ToString());
    }

    internal static string Format(string format, params object[] args)
    {
        format = GetLocalizedText(format, true);

        var builder = GetStringBuilder();
        builder.AppendFormat(format, args);
        return GetLocalizedText(builder.ToString());
    }

    private static ZStringBuilder GetStringBuilder()
    {
        s_ZStringBuilder.Clear();
        return s_ZStringBuilder;
    }
}
