using System.Collections.Generic;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
    // Constante para o limite de movimento do visualizador para atualização de chunks
    const float viewerMoveThresholdForChunkUpdate = 25f;
    const float sqrViewerMoveThresholdForChunkUpdate = viewerMoveThresholdForChunkUpdate * viewerMoveThresholdForChunkUpdate;

    public int colliderLODIndex; // Índice do LOD para geração do colisor
    public LODInfo[] detailLevels; // Níveis de detalhe para o terreno

    public MeshSettings meshSettings; // Configurações para a malha do terreno
    public HeightMapSettings heightMapSettings; // Configurações para o mapa de altura
    public TextureData textureSettings; // Configurações para o mapeamento de texturas

    public Transform viewer; // Transformação do visualizador (câmera)
    public Material mapMaterial; // Material utilizado para o terreno

    // Prefab da caverna que será instanciado
    public GameObject cavePrefab;

    Vector2 viewerPosition; // Posição atual do visualizador
    Vector2 viewerPositionOld; // Posição anterior do visualizador

    float meshWorldSize; // Tamanho do mundo da malha
    int chunksVisibleInViewDst; // Número de chunks visíveis dentro da distância de visualização

    // Dicionário que armazena os chunks do terreno
    Dictionary<Vector2, TerrainChunk> terrainChunkDictionary = new Dictionary<Vector2, TerrainChunk>();

    // Lista de chunks visíveis
    List<TerrainChunk> visibleTerrainChunks = new List<TerrainChunk>();

    // Método que é chamado ao iniciar o jogo
    void Start()
    {
        // Aplica as configurações de textura no material do mapa
        textureSettings.ApplyToMaterial(mapMaterial);

        // Atualiza as alturas das malhas com base nas configurações de altura
        textureSettings.UpdateMeshHeights(mapMaterial, heightMapSettings.minHeight, heightMapSettings.maxHeight);

        // Calcula a distância máxima de visualização
        float maxViewDst = detailLevels[detailLevels.Length - 1].visibleDstThreshold;
        meshWorldSize = meshSettings.meshWorldSize;

        // Calcula o número de chunks visíveis dentro da distância de visualização
        chunksVisibleInViewDst = Mathf.RoundToInt(maxViewDst / meshWorldSize);

        // Atualiza os chunks visíveis
        UpdateVisibleChunks();
    }

    // Método chamado a cada quadro
    void Update()
    {
        viewerPosition = new Vector2(viewer.position.x, viewer.position.z);

        // Atualiza a malha de colisão dos chunks visíveis
        if (viewerPosition != viewerPositionOld)
        {
            foreach (TerrainChunk chunk in visibleTerrainChunks)
            {
                chunk.UpdateCollisionMesh();
            }
        }

        // Atualiza os chunks visíveis se o visualizador se moveu além do limite
        if ((viewerPositionOld - viewerPosition).sqrMagnitude > sqrViewerMoveThresholdForChunkUpdate)
        {
            viewerPositionOld = viewerPosition;
            UpdateVisibleChunks();
        }
    }

    // Atualiza os chunks visíveis com base na posição do visualizador
    void UpdateVisibleChunks()
    {
        HashSet<Vector2> alreadyUpdatedChunkCoords = new HashSet<Vector2>();

        // Atualiza os chunks visíveis
        for (int i = visibleTerrainChunks.Count - 1; i >= 0; i--)
        {
            alreadyUpdatedChunkCoords.Add(visibleTerrainChunks[i].coord);
            visibleTerrainChunks[i].UpdateTerrainChunk();
            //visibleTerrainChunks[i].TryGenerateCaveEntrance(cavePrefab); // Gera/Destrói as cavernas
        }

        // Calcula as coordenadas do chunk atual
        int currentChunkCoordX = Mathf.RoundToInt(viewerPosition.x / meshWorldSize);
        int currentChunkCoordY = Mathf.RoundToInt(viewerPosition.y / meshWorldSize);

        // Atualiza os chunks ao redor do visualizador
        for (int yOffset = -chunksVisibleInViewDst; yOffset <= chunksVisibleInViewDst; yOffset++)
        {
            for (int xOffset = -chunksVisibleInViewDst; xOffset <= chunksVisibleInViewDst; xOffset++)
            {
                Vector2 viewedChunkCoord = new Vector2(currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);

                // Se o chunk ainda não foi atualizado, cria ou atualiza o chunk
                if (!alreadyUpdatedChunkCoords.Contains(viewedChunkCoord))
                {
                    if (terrainChunkDictionary.ContainsKey(viewedChunkCoord))
                    {
                        terrainChunkDictionary[viewedChunkCoord].UpdateTerrainChunk();
                    }
                    else
                    {
                        TerrainChunk newChunk = new TerrainChunk(viewedChunkCoord, heightMapSettings, meshSettings, detailLevels, colliderLODIndex, transform, viewer, mapMaterial);
                        terrainChunkDictionary.Add(viewedChunkCoord, newChunk);
                        newChunk.onVisibilityChanged += OnTerrainChunkVisibilityChanged;
                        newChunk.Load();

                        // Chama o método TryGenerateCaveEntrance para tentar gerar a entrada de caverna
                        newChunk.TryGenerateCaveEntrance(cavePrefab);
                    }
                }
            }
        }
    }

    // Método chamado quando a visibilidade de um chunk muda
    void OnTerrainChunkVisibilityChanged(TerrainChunk chunk, bool isVisible)
    {
        if (isVisible)
        {
            visibleTerrainChunks.Add(chunk);
        }
        else
        {
            visibleTerrainChunks.Remove(chunk);
        }
    }
}

[System.Serializable]
public struct LODInfo
{
    [Range(0, MeshSettings.numSupportedLODs - 1)]
    public int lod; // Nível de detalhe
    public float visibleDstThreshold; // Limite de distância de visibilidade

    // Propriedade para calcular o quadrado da distância de visibilidade
    public float sqrVisibleDstThreshold
    {
        get
        {
            return visibleDstThreshold * visibleDstThreshold;
        }
    }
}
