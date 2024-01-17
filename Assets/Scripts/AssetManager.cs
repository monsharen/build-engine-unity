using UnityEngine;

namespace DefaultNamespace
{
    public class AssetManager
    {
        public static string GetMapAssetPath(string mapName)
        {
            var mapPath = Application.dataPath + "/Maps/" + mapName;
            Debug.Log(mapPath);
            return mapPath;
        }
    }
}