using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace DefaultNamespace
{
    public class TextureManager
    {
        private readonly Dictionary<int, Material> _materialCache = new Dictionary<int, Material>();
        private readonly Texture2D _missingTexture;

        public TextureManager(Texture2D missingTexture)
        {
            _missingTexture = missingTexture;
        }

        public Texture2D GetMissingTexture()
        {
            return _missingTexture;
        }

        public Material LoadMaterialWithPicnum(int picNum)
        {
            if (_materialCache.TryGetValue(picNum, out var cachedMaterial))
            {
                return cachedMaterial;
            }
            
            var textureFileName = "tile" + picNum.ToString("D4");
            var texture2d = LoadTexture("Assets/Sprites/upscale/" + textureFileName + ".png");
            var material = new Material(Shader.Find("Standard"));
            material.mainTexture = texture2d;
            _materialCache.Add(picNum, material);
            return material;
        }
        
        private Texture2D LoadTexture(string filePath)
        {
            if (File.Exists(filePath))
            {
                var fileData = File.ReadAllBytes(filePath);
                var texture = new Texture2D(64, 64); // Create new "empty" texture
                texture.LoadImage(fileData); // Load the image data into the texture
                return texture;
            }

            return _missingTexture;
        }
    }
}