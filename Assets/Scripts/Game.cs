using BuildEngine;
using BuildEngineMapReader;
using DefaultNamespace;
using UnityEngine;

public class Game : MonoBehaviour
{
        
    public Texture2D missingTexture;
    public GameObject rootNode;

    private TextureManager _textureManager;
        
    private void Start()
    {
        _textureManager = new TextureManager(missingTexture);
        
        var mapFileReader = new MapFileReader();
        var fileName = AssetManager.GetMapAssetPath("THE_BASE.MAP");
        var map = mapFileReader.ReadFile(fileName);
        Debug.Log(map);

        var staticBatchingManager = new StaticBatchingManager(rootNode);
        var mapRenderer = new MapRenderer(rootNode, _textureManager);
        mapRenderer.Render(map);
        
        staticBatchingManager.Combine();
    }
}