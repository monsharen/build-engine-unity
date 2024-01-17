using BuildEngine;
using BuildEngineMapReader;
using DefaultNamespace;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class Game : MonoBehaviour
{
        
    public GameObject rootNode;

    private TextureManager _textureManager;
        
    private void Start()
    {
        _textureManager = new TextureManager();
        
        var mapFileReader = new MapFileReader();
        var fileName = AssetManager.GetMapAssetPath("THE_BASE.MAP");
        var map = mapFileReader.ReadFile(fileName);
        Debug.Log(map);
            
        var mapRenderer = new MapRenderer(rootNode, _textureManager);
        mapRenderer.Render(map);
    }

        

        
    
        

}