using System;
using System.Diagnostics;
using UnityEngine;

namespace PolymindGames
{
    [AttributeUsage(AttributeTargets.Field)]
    [Conditional("UNITY_EDITOR")]
    public sealed class SpritePreviewAttribute : PropertyAttribute
    {

    }
}