using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using HarmonyLib;

namespace RTLC.Helpers;
internal static class AccessToolsHelper
{
    public static MethodInfo AsyncMoveNext(Type type, string name)
    {
        var method = AccessTools.Method(type, name);
        if (method is null)
        {
            return null!;
        }

        var asyncAttribute = method.GetCustomAttribute<AsyncStateMachineAttribute>();
        if (asyncAttribute is null)
        {
            return null!;
        }

        var asyncStateMachineType = asyncAttribute.StateMachineType;
        var asyncMethodBody = AccessTools.DeclaredMethod(asyncStateMachineType, nameof(IAsyncStateMachine.MoveNext));
        if (asyncMethodBody is null)
        {
            return null!;
        }

        return asyncMethodBody;
    }
}
