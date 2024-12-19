using UnityEngine;
using System.Collections;

// Este script define as configurações de um mesh como um ScriptableObject,
// permitindo customizar e reutilizar configurações de malha no Unity.
[CreateAssetMenu()]
public class MeshSettings : UpdatableData
{

    // Constantes que definem os valores suportados para níveis de detalhe (LODs) e tamanhos de chunks.
    public const int numSupportedLODs = 5;
    public const int numSupportedChunkSizes = 9;
    public const int numSupportedFlatshadedChunkSizes = 3;

    // Tamanhos de chunk suportados, definidos em uma matriz.
    public static readonly int[] supportedChunkSizes = { 48, 72, 96, 120, 144, 168, 192, 216, 240 };

    // Escala aplicada ao tamanho da malha no mundo.
    public float meshScale = 2.5f;

    // Define se a malha deve ser renderizada com sombreamento plano (flat shading).
    public bool useFlatShading;

    // Índices para selecionar o tamanho do chunk dependendo do tipo de sombreamento.
    [Range(0, numSupportedChunkSizes - 1)]
    public int chunkSizeIndex;

    [Range(0, numSupportedFlatshadedChunkSizes - 1)]
    public int flatshadedChunkSizeIndex;

    // Número de vértices por linha da malha no LOD 0.
    // Inclui 2 vértices extras para cálculo de normais, que são excluídos na malha final.
    public int numVertsPerLine
    {
        get
        {
            return supportedChunkSizes[(useFlatShading) ? flatshadedChunkSizeIndex : chunkSizeIndex] + 5;
        }
    }

    // Tamanho da malha no mundo, calculado com base no número de vértices por linha e na escala da malha.
    public float meshWorldSize
    {
        get
        {
            return (numVertsPerLine - 3) * meshScale;
        }
    }
}
