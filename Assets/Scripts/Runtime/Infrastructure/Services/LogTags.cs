namespace BloodMoonIdle.Infrastructure.Services
{
    public static class LogTags
    {
        // Core system tags
        public const string GAME_INIT = "GameInit";
        public const string GAMEPLAY_INIT = "GameplayInit";
        
        // Character system tags
        public const string CHARACTER_FACTORY = "CharacterFactory";
        
        // UI system tags
        public const string JOYSTICK = "Joystick";
        
        // Future expansion tags (currently unused)
        public const string MOVEMENT_SYSTEM = "MovementSystem";
        public const string INPUT_SERVICE = "InputService";
        public const string RESOURCE_LOADER = "ResourceLoader";
        public const string SCENE_SERVICE = "SceneService";
        public const string SAVE_SYSTEM = "SaveSystem";
    }
} 