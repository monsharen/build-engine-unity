using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace DefaultNamespace
{
    public class TextureManager
    {

        private readonly Dictionary<int, Texture2D> _textureCache = new Dictionary<int, Texture2D>();
        private readonly Texture2D _missingTexture;

        public TextureManager(Texture2D missingTexture)
        {
            _missingTexture = missingTexture;
        }

        public Texture2D GetMissingTexture()
        {
            return _missingTexture;
        }

        public Texture2D LoadTextureWithPicnum(int picNum)
        {
            if (_textureCache.TryGetValue(picNum, out var texture))
            {
                return texture;
            }
            
            var textureFileName = "tile" + picNum.ToString("D4");
            var texture2d = LoadTexture("Assets/Sprites/upscale/" + textureFileName + ".png");
            _textureCache.Add(picNum, texture2d);
            return texture2d;
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