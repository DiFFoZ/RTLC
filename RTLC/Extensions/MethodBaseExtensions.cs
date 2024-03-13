using System.Reflection;
using System.Text;

namespace RTLC.Extensions;
internal static class MethodBaseExtensions
{
    public static string GetReadableString(this MethodBase method)
    {
        var type = method.DeclaringType;
        var @namespace = type.Namespace == null ? string.Empty : type.Namespace + ".";

        return $"{@namespace}{type.Name}::{method.Name}";
    }

    public static string GetFullName(this MethodInfo method)
    {
        var returnType = method.ReturnType;

        var sb = new StringBuilder();

        sb.Append(returnType.FullName);
        sb.Append(' ');

        sb.Append(method.GetReadableString());

        sb.Append('(');

        var parameters = method.GetParameters();
        for (var i = 0; i < parameters.Length; i++)
        {
            var parameter = parameters[i];
            if (i > 0)
                sb.Append(",");

            sb.Append(parameter.ParameterType.FullName);
        }

        sb.Append(')');

        return sb.ToString();
    }
}
