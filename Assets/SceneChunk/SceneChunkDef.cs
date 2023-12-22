namespace GameEngine
{
    public class SceneChunkDef
    {
        public static readonly string ITEM_SEPARATOR = ":";
        public static readonly string RESOURCE_SEPARATOR = "&";
        public static readonly string DEBUG_ASSETID_FILE_NAME = "debug_asset_id.txt";
        public static readonly string DEBUG_SCENEINFO_FILE_NAME = "debug_scene_info.txt";
        public static readonly string SAVE_MAP_INFO_FOLDER_NAME = "scene_chunk_info";
        
        public static readonly string ASSETID_FILE_NAME = "asset_id.bytes";
        public static readonly string SCENEINFO_FILE_NAME = "scene_info.bytes";
        
        public static readonly float DELETE_SCENE_PART_DELAY = 3f; // 单位秒
        public static readonly int DELETE_SCENE_PART_MAX_COUNT = 30; // must > 1
        
    }
}