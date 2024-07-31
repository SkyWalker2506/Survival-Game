using System;
using System.Reflection;
using JetBrains.Annotations;
using Toolbox.Editor.Internal;
using UnityEditor;
using UnityEngine;

namespace Toolbox.Editor.Drawers
{

    [UsedImplicitly]
    public sealed class ReorderableListWithCallbacksPropertyDrawer : ToolboxListPropertyDrawer<ReorderableListWithCallbacksAttribute>
    {
        private static readonly PropertyDataStorage<ReorderableListBase, ReorderableListWithCallbacksAttribute> s_Storage;
        

        static ReorderableListWithCallbacksPropertyDrawer()
        {
            s_Storage = new PropertyDataStorage<ReorderableListBase, ReorderableListWithCallbacksAttribute>(false, (p, a) =>
            {
                // Create list in the standard way
                var list = ToolboxEditorGui.CreateList(p,
                    a.ListStyle,
                    a.ElementLabel,
                    a.FixedSize,
                    a.Draggable,
                    a.HasHeader,
                    a.HasLabels,
                    a.Foldable);

                // Additionally subscribe callbacks
                ConnectAddCallback(list, a);
                ConnectRemoveCallback(list, a);
                return list;
            });
        }
        
        private static void ConnectRemoveCallback(ReorderableListBase list, ReorderableListWithCallbacksAttribute attribute)
        {
            var listTarget = list.SerializedObject;
            var methodInfo = FindRemoveMethod(listTarget, attribute.OverrideRemoveElementMethodName);
            if (methodInfo == null)
                return;

            list.onRemoveCallback = listBase =>
            {
                methodInfo.Invoke(listTarget.targetObject, new object[]
                {
                    listBase.Index
                });
                listBase.RemoveElement(listBase.Index);
            };
        }

        private static void ConnectAddCallback(ReorderableListBase list, ReorderableListWithCallbacksAttribute attribute)
        {
            var listTarget = list.SerializedObject;
            var fieldInfo = list.List.GetFieldInfo();
            var returnType = fieldInfo.FieldType.GetEnumeratedType();
            var methodName = attribute.OverrideNewElementMethodName;
            var methodInfo = FindAddMethod(listTarget, methodName, returnType);
            if (methodInfo == null)
                return;

            list.overrideNewElementCallback = _ => methodInfo.Invoke(listTarget.targetObject, null);
        }

        private static MethodInfo FindRemoveMethod(SerializedObject target, string methodName)
        {
            var listTarget = target.targetObject;
            var methodInfo = listTarget.GetType().GetMethod(methodName, ReflectionUtility.allBindings);

            if (methodInfo == null)
            {
                ToolboxEditorLog.AttributeUsageWarning(typeof(ReorderableListWithCallbacksAttribute),
                    $"{methodName} method not found.");
                return null;
            }

            return methodInfo;
        }

        private static MethodInfo FindAddMethod(SerializedObject target, string methodName, Type expectedReturnType = null)
        {
            if (string.IsNullOrEmpty(methodName))
                return null;

            var methodInfo = ReflectionUtility.GetObjectMethod(methodName, target);
            if (methodInfo == null)
            {
                ToolboxEditorLog.AttributeUsageWarning(typeof(ReorderableListWithCallbacksAttribute),
                    $"{methodName} method not found.");
                return null;
            }

            var parameters = methodInfo.GetParameters();
            if (parameters.Length > 0)
            {
                ToolboxEditorLog.AttributeUsageWarning(typeof(ReorderableListWithCallbacksAttribute),
                    $"{methodName} method not found.");
                return null;
            }

            if (expectedReturnType != null && expectedReturnType != methodInfo.ReturnType)
            {
                ToolboxEditorLog.AttributeUsageWarning(typeof(ReorderableListWithCallbacksAttribute),
                    $"{methodName} method returns invalid type. Expected - {expectedReturnType}.");
                return null;
            }

            return methodInfo;
        }
        
        protected override void OnGuiSafe(SerializedProperty property, GUIContent label, ReorderableListWithCallbacksAttribute attribute)
        {
            s_Storage.ReturnItem(property, attribute).DoList(label);
        }
    }
}