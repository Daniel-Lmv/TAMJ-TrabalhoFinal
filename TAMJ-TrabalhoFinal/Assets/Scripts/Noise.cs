using UnityEngine;
using System.Collections;

public static class Noise
{

    // Enum para escolher entre normalização local e global
    public enum NormalizeMode { Local, Global };

    // Função para gerar um mapa de ruído
    public static float[,] GenerateNoiseMap(int mapWidth, int mapHeight, NoiseSettings settings, Vector2 sampleCentre)
    {
        // Inicializa o mapa de ruído com as dimensões fornecidas
        float[,] noiseMap = new float[mapWidth, mapHeight];

        // Gerador de números aleatórios para os offsets de cada oitava (octave)
        System.Random prng = new System.Random(settings.seed);
        Vector2[] octaveOffsets = new Vector2[settings.octaves];

        // Variáveis para calcular a altura máxima possível do ruído
        float maxPossibleHeight = 0;
        float amplitude = 1;
        float frequency = 1;

        // Gera os deslocamentos (offsets) para cada oitava e calcula a altura máxima possível
        for (int i = 0; i < settings.octaves; i++)
        {
            // Gera valores aleatórios para deslocamento baseado na semente e na posição central
            float offsetX = prng.Next(-100000, 100000) + settings.offset.x + sampleCentre.x;
            float offsetY = prng.Next(-100000, 100000) - settings.offset.y - sampleCentre.y;
            octaveOffsets[i] = new Vector2(offsetX, offsetY);

            maxPossibleHeight += amplitude; // Adiciona a amplitude ao valor máximo de altura
            amplitude *= settings.persistance;  // Modifica a amplitude para cada oitava
        }

        // Variáveis para armazenar a altura mínima e máxima local do ruído
        float maxLocalNoiseHeight = float.MinValue;
        float minLocalNoiseHeight = float.MaxValue;

        // Calcula metade da largura e da altura do mapa para centralizar o mapa
        float halfWidth = mapWidth / 2f;
        float halfHeight = mapHeight / 2f;

        // Gera os valores de ruído para cada ponto do mapa
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {

                // Reseta a amplitude e a frequência para cada ponto
                amplitude = 1;
                frequency = 1;
                float noiseHeight = 0;

                // Calcula o valor do ruído para cada oitava
                for (int i = 0; i < settings.octaves; i++)
                {
                    // Calcula as coordenadas de amostra para cada oitava
                    float sampleX = (x - halfWidth + octaveOffsets[i].x) / settings.scale * frequency;
                    float sampleY = (y - halfHeight + octaveOffsets[i].y) / settings.scale * frequency;

                    // Obtém o valor do Perlin Noise (entre -1 e 1) e ajusta para o intervalo de altura
                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
                    noiseHeight += perlinValue * amplitude;

                    amplitude *= settings.persistance; // Aplica a persistência na amplitude
                    frequency *= settings.lacunarity; // Aplica a lacunaridade na frequência
                }

                // Atualiza os valores mínimo e máximo do ruído
                if (noiseHeight > maxLocalNoiseHeight)
                {
                    maxLocalNoiseHeight = noiseHeight;
                }
                if (noiseHeight < minLocalNoiseHeight)
                {
                    minLocalNoiseHeight = noiseHeight;
                }

                // Armazena o valor de ruído no mapa
                noiseMap[x, y] = noiseHeight;

                // Normalização global (se ativada)
                if (settings.normalizeMode == NormalizeMode.Global)
                {
                    // Normaliza o valor do ruído para o intervalo entre 0 e 1
                    float normalizedHeight = (noiseMap[x, y] + 1) / (maxPossibleHeight / 0.9f);
                    noiseMap[x, y] = Mathf.Clamp(normalizedHeight, 0, int.MaxValue);
                }
            }
        }

        // Normalização local (se ativada)
        if (settings.normalizeMode == NormalizeMode.Local)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                for (int x = 0; x < mapWidth; x++)
                {
                    // Normaliza os valores para o intervalo entre 0 e 1 baseado nos valores mínimo e máximo locais
                    noiseMap[x, y] = Mathf.InverseLerp(minLocalNoiseHeight, maxLocalNoiseHeight, noiseMap[x, y]);
                }
            }
        }

        // Retorna o mapa de ruído gerado
        return noiseMap;
    }
}

// Classe serializável para armazenar as configurações do ruído
[System.Serializable]
public class NoiseSettings
{
    public Noise.NormalizeMode normalizeMode;  // Modo de normalização (Local ou Global)

    public float scale = 50;                   // Escala do ruído (afeta o "zoom" do ruído)
    public int octaves = 6;                    // Número de oitavas (camadas de ruído)
    [Range(0, 1)]
    public float persistance = .6f;            // Fator de persistência para controlar o decaimento da amplitude
    public float lacunarity = 2;               // Fator de lacunaridade para controlar o aumento da frequência

    public int seed;                           // Semente para a geração de números aleatórios do ruído
    public Vector2 offset;                     // Deslocamento para movimentar o mapa de ruído

    // Função para validar e ajustar os valores das configurações
    public void ValidateValues()
    {
        scale = Mathf.Max(scale, 0.01f);       // Garante que a escala não seja menor que 0.01
        octaves = Mathf.Max(octaves, 1);       // Garante que o número de oitavas seja no mínimo 1
        lacunarity = Mathf.Max(lacunarity, 1); // Garante que a lacunaridade seja no mínimo 1
        persistance = Mathf.Clamp01(persistance); // Clampa a persistência entre 0 e 1
    }
}
