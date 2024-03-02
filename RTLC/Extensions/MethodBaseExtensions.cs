using System.Reflection;

namespace RTLC.Extensions;
internal static class MethodBaseExtensions
{
    public static string GetReadableString(this MethodBase method)
    {
        var type = method.DeclaringType;

        return $"{type.Namespace}.{type.Name}::{method.Name}";
    }
}
