using UnityEngine;

public static class TextureGenerator
{

    // Método para criar uma textura a partir de um mapa de cores
    public static Texture2D TextureFromColourMap(Color[] colourMap, int width, int height)
    {
        // Cria uma nova textura com as dimensões fornecidas
        Texture2D texture = new Texture2D(width, height);

        // Define o modo de filtro da textura (sem suavização)
        texture.filterMode = FilterMode.Point;

        // Define o modo de repetição da textura (sem repetição)
        texture.wrapMode = TextureWrapMode.Clamp;

        // Define os pixels da textura com base no mapa de cores fornecido
        texture.SetPixels(colourMap);

        // Aplica as mudanças feitas nos pixels
        texture.Apply();

        return texture; // Retorna a textura gerada
    }

    // Método para criar uma textura a partir de um mapa de altura
    public static Texture2D TextureFromHeightMap(HeightMap heightMap)
    {
        int width = heightMap.values.GetLength(0); // Largura do mapa de altura
        int height = heightMap.values.GetLength(1); // Altura do mapa de altura

        // Cria um array de cores para a textura
        Color[] colourMap = new Color[width * height];

        // Preenche o mapa de cores com base nos valores do mapa de altura
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                // Lerp entre preto e branco com base no valor da altura normalizado
                colourMap[y * width + x] = Color.Lerp(Color.black, Color.white, Mathf.InverseLerp(heightMap.minValue, heightMap.maxValue, heightMap.values[x, y]));
            }
        }

        // Chama o método TextureFromColourMap para gerar a textura
        return TextureFromColourMap(colourMap, width, height);
    }
}
