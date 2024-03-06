namespace RTLC;
internal static partial class Translation
{
    public static string Concat(string str0, string str1)
    {
        var builder = GetStringBuilder();
        builder.Append(str0);
        builder.Append(str1);

        return GetLocalizedText(builder.ToString());
    }

    public static string Concat(string str0, string str1, string str2)
    {
        var builder = GetStringBuilder();
        builder.Append(str0);
        builder.Append(str1);
        builder.Append(str2);

        return GetLocalizedText(builder.ToString());
    }

    public static string Concat(string str0, string str1, string str2, string str3)
    {
        var builder = GetStringBuilder();
        builder.Append(str0);
        builder.Append(str1);
        builder.Append(str2);
        builder.Append(str3);

        return GetLocalizedText(builder.ToString());
    }

    public static string Concat(params string[] values)
    {
        var builder = GetStringBuilder();
        foreach (var value in values)
        {
            builder.Append(value);
        }

        return GetLocalizedText(builder.ToString());
    }
}
