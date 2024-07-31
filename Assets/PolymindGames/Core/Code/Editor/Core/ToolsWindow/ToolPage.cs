using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

namespace PolymindGamesEditor.ToolPages
{
    using Object = UnityEngine.Object;
    
    public interface IEditorToolPage : IComparable<IEditorToolPage>
    {
        int Order { get; }
        string DisplayName { get; }
        bool DisableInPlaymode { get; }

        void DrawPage(Rect rect);
        IEnumerable<IEditorToolPage> GetSubPages();
        bool IsCompatibleWithObject(Object unityObject);
    }

    public abstract class RootPage : ToolPage
    { }

    public abstract class ToolPage : IEditorToolPage
    {
        public abstract int Order { get; }
        public abstract string DisplayName { get; }
        public abstract bool DisableInPlaymode { get; }

        public abstract void DrawPage(Rect rect);
        public abstract bool IsCompatibleWithObject(Object unityObject);
        public virtual IEnumerable<IEditorToolPage> GetSubPages() => Array.Empty<IEditorToolPage>();

        protected static List<Type> GetAllSubTypes(Type type)
        {
            return type.Assembly.GetTypes().Where(t => !t.IsAbstract && t.IsSubclassOf(type)).ToList();
        }
        
        public int CompareTo(IEditorToolPage other) => Order.CompareTo(other.Order);
    }
    
    public sealed class SimpleToolPage : IEditorToolPage
    {
        private readonly Type _compatibleType;
        

        public SimpleToolPage(string name, Type compatibleType, int order, Func<IEditorDrawable> getContentFunc)
        {
            DisplayName = name;
            Order = order;
            Content = new Lazy<IEditorDrawable>(getContentFunc);
            _compatibleType = compatibleType;
        }

        public int Order { get; }
        public string DisplayName { get; }
        public bool DisableInPlaymode => false;
        public Lazy<IEditorDrawable> Content { get; }

        public void DrawPage(Rect rect) => Content?.Value.Draw(rect, EditorDrawableLayoutType.Vertical);

        public IEnumerable<IEditorToolPage> GetSubPages() => Array.Empty<IEditorToolPage>();
        
        public bool IsCompatibleWithObject(Object unityObject)
        {
            return unityObject != null && unityObject.GetType() == _compatibleType;
        }
        
        public int CompareTo(IEditorToolPage other) => Order.CompareTo(other.Order);
    }
}