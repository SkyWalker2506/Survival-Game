using UnityEngine;

namespace PolymindGamesEditor
{
    public interface IEditorDrawable
    {
        void Draw(Rect rect, EditorDrawableLayoutType layoutType);
    }
    
    public enum EditorDrawableLayoutType
    {
        Vertical,
        Horizontal
    }
}