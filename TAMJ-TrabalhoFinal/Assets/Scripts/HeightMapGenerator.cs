using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class HeightMapGenerator
{

    // Método para gerar um mapa de altura baseado em configurações de ruído e curvas de altura
    public static HeightMap GenerateHeightMap(int width, int height, HeightMapSettings settings, Vector2 sampleCentre)
    {
        // Gera um mapa de ruído com as dimensões e configurações fornecidas
        float[,] values = Noise.GenerateNoiseMap(width, height, settings.noiseSettings, sampleCentre);

        // Cria uma cópia thread-safe da curva de altura
        AnimationCurve heightCurve_threadsafe = new AnimationCurve(settings.heightCurve.keys);

        // Inicializa as variáveis para armazenar os valores mínimo e máximo
        float minValue = float.MaxValue;
        float maxValue = float.MinValue;

        // Itera sobre o mapa de valores para aplicar a curva de altura e calcular os valores mínimo e máximo
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                // Aplica a curva de altura e o multiplicador de altura
                values[i, j] *= heightCurve_threadsafe.Evaluate(values[i, j]) * settings.heightMultiplier;

                // Atualiza os valores mínimo e máximo
                if (values[i, j] > maxValue)
                {
                    maxValue = values[i, j];
                }
                if (values[i, j] < minValue)
                {
                    minValue = values[i, j];
                }
            }
        }

        // Retorna o mapa de altura gerado com os valores mínimo e máximo
        return new HeightMap(values, minValue, maxValue);
    }

}

// Estrutura que representa um mapa de altura, incluindo os valores das alturas e os valores mínimo e máximo
public struct HeightMap
{
    public readonly float[,] values;
    public readonly float minValue;
    public readonly float maxValue;

    // Construtor para inicializar o mapa de altura com os valores, mínimo e máximo
    public HeightMap(float[,] values, float minValue, float maxValue)
    {
        this.values = values;
        this.minValue = minValue;
        this.maxValue = maxValue;
    }
}
