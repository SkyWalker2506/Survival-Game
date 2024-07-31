using System.Diagnostics;
using System;

namespace UnityEngine
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    [Conditional("UNITY_EDITOR")]
    public sealed class HideInPlayModeAttribute : ToolboxConditionAttribute
    { }
}