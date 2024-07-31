using System.Diagnostics;
using System;

namespace UnityEngine
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    [Conditional("UNITY_EDITOR")]
    public sealed class ShowForRenderPipelineAttribute : ToolboxConditionAttribute
    {
        public enum Type
        {
            BuiltIn = 0,
            Hdrp = 1,
            Urp = 2
        }
        
        public ShowForRenderPipelineAttribute(Type type)
        {
            PipelineType = type;
        }
        
        public Type PipelineType { get; }
    }
}