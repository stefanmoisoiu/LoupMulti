using UnityEditor;
using UnityEngine;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor.U2D.Aseprite;

public class AsepriteSplitter : Editor
{
    [MenuItem("Assets/Split Aseprite Sprites", false, 1500)]
    private static void SplitAsepriteSprites()
    {
        // Get the selected object
        Object selectedObject = Selection.activeObject;
        if (selectedObject == null)
        {
            Debug.LogError("No asset selected.");
            return;
        }

        // Get the AsepriteImporter
        string assetPath = AssetDatabase.GetAssetPath(selectedObject);
        var importer = AssetImporter.GetAtPath(assetPath) as AsepriteImporter;
        if (importer == null)
        {
            Debug.LogError("Selected asset is not an Aseprite file.");
            return;
        }

        // Load all sprites from the Aseprite asset
        Sprite[] sprites = AssetDatabase.LoadAllAssetsAtPath(assetPath).OfType<Sprite>().ToArray();
        if (sprites.Length == 0)
        {
            Debug.LogError("No sprites found in the Aseprite file.");
            return;
        }

        // Prepare output folder
        string directory = Path.GetDirectoryName(assetPath);
        string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(assetPath);
        string outputFolder = Path.Combine(directory, fileNameWithoutExtension + "_Sprites");
        Directory.CreateDirectory(outputFolder);


        string defaultMatPath = Path.Combine(outputFolder, "_default.mat");
        Material defaultMat = AssetDatabase.LoadAssetAtPath<Material>(defaultMatPath);
        if (defaultMat == null)
        {
            defaultMat = new Material(Shader.Find("MK/Toon/URP/Standard/Simple"));
            AssetDatabase.CreateAsset(defaultMat, defaultMatPath);
        }

        // Process each sprite
        foreach (Sprite sprite in sprites)
        {
            string texturePath = Path.Combine(outputFolder, sprite.name);
            Rect rect = sprite.rect;
            Texture2D sourceTexture = sprite.texture;

            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath + ".png");
            bool spriteExists = texture != null;

            if (!spriteExists)
            {
                texture = new Texture2D((int)rect.width, (int)rect.height, TextureFormat.RGBA32, true);
            }
            else
            {
                texture.Reinitialize(texture.width, texture.height, TextureFormat.RGBA32, true);
            }

            texture.filterMode = FilterMode.Point; // Set filter mode to Point
            texture.SetPixels(sourceTexture.GetPixels(
                (int)rect.x,
                (int)rect.y,
                (int)rect.width,
                (int)rect.height
            ));
            texture.Apply(true);

            if (spriteExists)
            {
                EditorUtility.SetDirty(texture);
                
                byte[] pngData = texture.EncodeToPNG();
                if (pngData != null)
                {
                    File.WriteAllBytes(texturePath + ".png", pngData);
                }
                AssetDatabase.Refresh();
            }
            else
            {
                // Encode texture to PNG format
                byte[] pngData = texture.EncodeToPNG();
                if (pngData != null)
                {
                    // Write the PNG file to disk
                    File.WriteAllBytes(texturePath + ".png", pngData);
                    
                    AssetDatabase.Refresh();
                    
                    File.WriteAllText(texturePath + ".png.meta", 
                        Regex.Replace(File.ReadAllText(texturePath + ".png.meta"), "filterMode: 1", "filterMode: 0"));
                    File.WriteAllText(texturePath + ".png.meta", 
                        Regex.Replace(File.ReadAllText(texturePath + ".png.meta"), "textureCompression: 0", "textureCompression: 1"));
                    File.WriteAllText(texturePath + ".png.meta", 
                        Regex.Replace(File.ReadAllText(texturePath + ".png.meta"), "isReadable: 0", "isReadable: 1"));
                } 
            }
            AssetDatabase.Refresh();
            string matPath = Path.Combine(outputFolder, sprite.name + ".mat");
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            bool matExists = mat != null;
            if (!matExists) mat = new Material(defaultMat);
            mat.parent = defaultMat;
            mat.mainTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath + ".png");
            mat.name = sprite.name;
            mat.mainTextureOffset = defaultMat.mainTextureOffset;
            mat.mainTextureScale = defaultMat.mainTextureScale;

            if (matExists)
            {
                EditorUtility.SetDirty(mat);
            }
            else AssetDatabase.CreateAsset(mat, matPath);
            AssetDatabase.Refresh();
        }
    }
}
