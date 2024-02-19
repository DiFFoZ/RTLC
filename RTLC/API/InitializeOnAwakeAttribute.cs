using System;

namespace RTLC.API;
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
internal class InitializeOnAwakeAttribute : Attribute
{
}
