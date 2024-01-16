using System.IO;
using UnityEngine;

namespace DefaultNamespace
{
    public class TextureUtil
    {

        public static Texture2D LoadTextureWithPicnum(int picNum)
        {
            string textureFileName = "tile" + picNum.ToString("D4");
            Debug.Log("sector Floor texture: " + textureFileName);
            return LoadTexture("Assets/Sprites/upscale/" + textureFileName + ".png");
        }
        private static Texture2D LoadTexture(string filePath)
        {
            if (File.Exists(filePath))
            {
                var fileData = File.ReadAllBytes(filePath);
                var texture = new Texture2D(64, 64); // Create new "empty" texture
                texture.LoadImage(fileData); // Load the image data into the texture
                return texture;
            }

            return null;
        }
    }
}