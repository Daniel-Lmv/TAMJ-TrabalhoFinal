using UnityEngine;
using System.Collections;

public static class FalloffGenerator
{

    // Método público estático que gera um mapa de queda (falloff map) baseado no tamanho fornecido
    public static float[,] GenerateFalloffMap(int size)
    {
        // Cria um array bidimensional para armazenar os valores do mapa de queda
        float[,] map = new float[size, size];

        // Itera por cada ponto do mapa (i, j) para calcular o valor de queda
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                // Calcula as coordenadas normalizadas no intervalo de -1 a 1 para os eixos x e y
                float x = i / (float)size * 2 - 1;
                float y = j / (float)size * 2 - 1;

                // Calcula o valor máximo entre a distância de x e y em relação à origem (0, 0)
                float value = Mathf.Max(Mathf.Abs(x), Mathf.Abs(y));

                // Aplica a função de avaliação para obter o valor final do mapa de queda
                map[i, j] = Evaluate(value);
            }
        }

        // Retorna o mapa de queda gerado
        return map;
    }

    // Função privada que avalia o valor do mapa de queda com base em um valor dado
    static float Evaluate(float value)
    {
        // Constantes para o cálculo da função de queda
        float a = 3;
        float b = 2.2f;

        // Aplica a fórmula de avaliação que cria um comportamento de queda suave
        return Mathf.Pow(value, a) / (Mathf.Pow(value, a) + Mathf.Pow(b - b * value, a));
    }
}
