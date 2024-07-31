namespace PolymindGames
{
    /// <summary>
    /// Provides constants representing layer indices for commonly used layers in Unity.
    /// </summary>
    public static partial class LayerConstants
    {
        // Layer indices
        public const int DEFAULT = 0;
        public const int TRANSPARENT_FX = 1;
        public const int IGNORE_RAYCAST = 2;
        public const int WATER = 4;
        public const int UI = 5;
        public const int EFFECT = 7;
        public const int TRIGGER_ZONE = 8;
        public const int INTERACTABLE = 9;
        public const int VIEW_MODEL = 10;
        public const int POST_PROCESSING = 11;
        public const int HITBOX = 12;
        public const int CHARACTER = 13;
        public const int STATIC_OBJECTS = 14;
        public const int DYNAMIC_OBJECTS = 15;
        public const int STRUCTURE_INTERACTABLE = 16;

        // Layer masks
        public const int ALL_SOLID_OBJECTS_MASK = SIMPLE_SOLID_OBJECTS_MASK | CHARACTER_MASK | INTERACTABLES_MASK;
        public const int SIMPLE_SOLID_OBJECTS_MASK = 1 << DEFAULT | 1 << STATIC_OBJECTS | 1 << DYNAMIC_OBJECTS;
        public const int CHARACTER_MASK = 1 << CHARACTER | 1 << HITBOX;
        public const int INTERACTABLES_MASK = 1 << INTERACTABLE | STRUCTURE_MASK;
        public const int DAMAGEABLE_MASK = 1 << DEFAULT | INTERACTABLES_MASK | 1 << HITBOX | 1 << CHARACTER;
        public const int STRUCTURE_MASK = 1 << STRUCTURE_INTERACTABLE;
    }

    /// <summary>
    /// Provides constants representing commonly used tags in Unity.
    /// </summary>
    public static partial class TagConstants
    {
        // Tag names
        public const string MAIN_CAMERA = "MainCamera";
        public const string PLAYER = "Player";
        public const string GAME_CONTROLLER = "GameController";
    }

    /// <summary>
    /// Provides constants representing execution order values for script execution in Unity.
    /// </summary>
    public static partial class ExecutionOrderConstants
    {
        // Execution order values
        public const int SCRIPTABLE_SINGLETON = -100000;
        public const int SCENE_SINGLETON = -10000;
        public const int BEFORE_DEFAULT_3 = -1000;
        public const int BEFORE_DEFAULT_2 = -100;
        public const int BEFORE_DEFAULT_1 = -10;
        public const int AFTER_DEFAULT_1 = 10;
        public const int AFTER_DEFAULT_2 = 100;
    }
}