using UnityEngine;
using System.Collections;
using System.Linq;

// Define esta classe como um ScriptableObject que pode ser criado no editor do Unity
[CreateAssetMenu()]
public class TextureData : UpdatableData
{

    // Tamanho fixo para as texturas geradas (512x512)
    const int textureSize = 512;
    // Formato de textura a ser utilizado (RGB565)
    const TextureFormat textureFormat = TextureFormat.RGB565;

    // Array de camadas que contém informações sobre cada camada de textura
    public Layer[] layers;

    // Armazena as alturas mínima e máxima da malha
    float savedMinHeight;
    float savedMaxHeight;

    // Aplica as propriedades desta classe ao material passado como argumento
    public void ApplyToMaterial(Material material)
    {
        // Define o número de camadas no material
        material.SetInt("layerCount", layers.Length);

        // Define os valores de cor base, que são extraídos das camadas
        material.SetColorArray("baseColours", layers.Select(x => x.tint).ToArray());

        // Define as alturas de início das camadas
        material.SetFloatArray("baseStartHeights", layers.Select(x => x.startHeight).ToArray());

        // Define a força de mistura das camadas
        material.SetFloatArray("baseBlends", layers.Select(x => x.blendStrength).ToArray());

        // Define a intensidade da cor de matiz para cada camada
        material.SetFloatArray("baseColourStrength", layers.Select(x => x.tintStrength).ToArray());

        // Define a escala de textura para cada camada
        material.SetFloatArray("baseTextureScales", layers.Select(x => x.textureScale).ToArray());

        // Gera um array de texturas e o aplica ao material
        Texture2DArray texturesArray = GenerateTextureArray(layers.Select(x => x.texture).ToArray());
        material.SetTexture("baseTextures", texturesArray);

        // Atualiza as alturas da malha no material
        UpdateMeshHeights(material, savedMinHeight, savedMaxHeight);
    }

    // Atualiza as alturas mínimas e máximas da malha no material
    public void UpdateMeshHeights(Material material, float minHeight, float maxHeight)
    {
        savedMinHeight = minHeight;
        savedMaxHeight = maxHeight;

        // Aplica as alturas mínimas e máximas ao material
        material.SetFloat("minHeight", minHeight);
        material.SetFloat("maxHeight", maxHeight);
    }

    // Gera um Texture2DArray a partir de um array de texturas
    Texture2DArray GenerateTextureArray(Texture2D[] textures)
    {
        // Cria o array de texturas com as dimensões e formato definidos
        Texture2DArray textureArray = new Texture2DArray(textureSize, textureSize, textures.Length, textureFormat, true);

        // Adiciona as texturas ao array
        for (int i = 0; i < textures.Length; i++)
        {
            textureArray.SetPixels(textures[i].GetPixels(), i);
        }

        // Aplica as alterações no array de texturas
        textureArray.Apply();
        return textureArray;
    }

    // Classe interna que define os dados de cada camada de textura
    [System.Serializable]
    public class Layer
    {
        // A textura associada a esta camada
        public Texture2D texture;

        // Cor de matiz aplicada à textura
        public Color tint;

        // Intensidade da cor de matiz (0 a 1)
        [Range(0, 1)]
        public float tintStrength;

        // Altura inicial da camada (0 a 1)
        [Range(0, 1)]
        public float startHeight;

        // Intensidade da mistura da camada (0 a 1)
        [Range(0, 1)]
        public float blendStrength;

        // Escala da textura aplicada
        public float textureScale;
    }
}
