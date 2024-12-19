using UnityEngine;

public class TerrainChunk
{

    // Constante para o limiar de distância de geração do colisor
    const float colliderGenerationDistanceThreshold = 5;

    // Evento acionado quando a visibilidade do chunk muda
    public event System.Action<TerrainChunk, bool> onVisibilityChanged;
    public Vector2 coord; // Coordenada do chunk no terreno

    GameObject meshObject; // Objeto que contém a malha
    Vector2 sampleCentre; // Centro de amostragem para a geração de altura
    Bounds bounds; // Limites do chunk (área que ocupa)

    MeshRenderer meshRenderer; // Componente para renderizar a malha
    MeshFilter meshFilter; // Componente que armazena a malha
    MeshCollider meshCollider; // Colisor da malha para detecção de colisões

    LODInfo[] detailLevels; // Níveis de detalhe para o chunk
    LODMesh[] lodMeshes; // Malhas de níveis de detalhe
    int colliderLODIndex; // Índice do LOD usado para gerar o colisor

    HeightMap heightMap; // Mapa de alturas para o terreno
    bool heightMapReceived; // Flag indicando se o mapa de altura foi recebido
    int previousLODIndex = -1; // Índice do LOD anterior
    bool hasSetCollider; // Flag indicando se o colisor foi configurado
    float maxViewDst; // Distância máxima de visualização

    HeightMapSettings heightMapSettings; // Configurações para a geração do mapa de altura
    MeshSettings meshSettings; // Configurações para a malha do terreno
    Transform viewer; // Transformação da câmera ou visualizador

    // Variáveis para gerar a entrada da caverna
    private GameObject caveInstance;  // Referência da caverna gerada no chunk
    private Vector3 cavePosition;     // Posição onde a caverna foi gerada (salva)
    private bool isCaveGenerated = false; // Flag para verificar se a caverna foi gerada
    public GameObject cavePrefab; // Prefab da caverna
    public float caveSpawnHeightThreshold = 10f; // Limiar de altura para gerar a caverna
    public float caveSpawnChance = 0.05f; // Chance de spawn da caverna (0.0 a 1.0)

    // Construtor do TerrainChunk
    public TerrainChunk(Vector2 coord, HeightMapSettings heightMapSettings, MeshSettings meshSettings, LODInfo[] detailLevels, int colliderLODIndex, Transform parent, Transform viewer, Material material)
    {
        this.coord = coord;
        this.detailLevels = detailLevels;
        this.colliderLODIndex = colliderLODIndex;
        this.heightMapSettings = heightMapSettings;
        this.meshSettings = meshSettings;
        this.viewer = viewer;

        // Calcula o centro de amostragem baseado nas coordenadas do chunk
        sampleCentre = coord * meshSettings.meshWorldSize / meshSettings.meshScale;
        Vector2 position = coord * meshSettings.meshWorldSize;
        bounds = new Bounds(position, Vector2.one * meshSettings.meshWorldSize);

        // Criação do GameObject do chunk
        meshObject = new GameObject("Terrain Chunk");
        meshRenderer = meshObject.AddComponent<MeshRenderer>();
        meshFilter = meshObject.AddComponent<MeshFilter>();
        meshCollider = meshObject.AddComponent<MeshCollider>();
        meshRenderer.material = material;

        meshObject.transform.position = new Vector3(position.x, 0, position.y);
        meshObject.transform.parent = parent;
        SetVisible(false);

        // Inicialização dos LODMeshes
        lodMeshes = new LODMesh[detailLevels.Length];
        for (int i = 0; i < detailLevels.Length; i++)
        {
            lodMeshes[i] = new LODMesh(detailLevels[i].lod);
            lodMeshes[i].updateCallback += UpdateTerrainChunk;
            if (i == colliderLODIndex)
            {
                lodMeshes[i].updateCallback += UpdateCollisionMesh;
            }
        }

        maxViewDst = detailLevels[detailLevels.Length - 1].visibleDstThreshold;
    }

    // Método para carregar o mapa de altura
    public void Load()
    {
        ThreadedDataRequester.RequestData(() => HeightMapGenerator.GenerateHeightMap(meshSettings.numVertsPerLine, meshSettings.numVertsPerLine, heightMapSettings, sampleCentre), OnHeightMapReceived);
    }

    // Callback quando o mapa de altura é recebido
    void OnHeightMapReceived(object heightMapObject)
    {
        this.heightMap = (HeightMap)heightMapObject;
        heightMapReceived = true;

        // Gerar a entrada da caverna após o mapa de altura ser carregado
        TryGenerateCaveEntrance(cavePrefab);

        UpdateTerrainChunk();
    }


    // Método para tentar gerar a entrada da caverna
    public void TryGenerateCaveEntrance(GameObject cavePrefab)
    {
        this.cavePrefab = cavePrefab;
        if (heightMapReceived)
        {
            if (heightMap.values == null)
            {
                Debug.LogError("HeightMap is null in TryGenerateCaveEntrance.");
                return;
            }

            // Verifica a distância do visualizador (jogador) até o chunk
            float viewerDstFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(new Vector2(viewer.position.x, viewer.position.z)));

            // Se o jogador está perto o suficiente, gera a caverna
            if (viewerDstFromNearestEdge <= maxViewDst)
            {
                // Se a caverna não foi gerada ou foi destruída, tenta gerar
                if (!isCaveGenerated)
                {
                    for (int x = 0; x < heightMap.values.GetLength(0); x++)
                    {
                        for (int y = 0; y < heightMap.values.GetLength(1); y++)
                        {
                            float height = heightMap.values[x, y];
                            if (height < caveSpawnHeightThreshold && Random.value < caveSpawnChance)
                            {
                                // Calcular a posição mundial para a entrada da caverna
                                Vector3 worldPosition = GetWorldPositionFromHeightMapCoords(x, y, height);

                                // Salva a posição da caverna
                                cavePosition = worldPosition;

                                // Instanciar a caverna no local calculado
                                caveInstance = GameObject.Instantiate(cavePrefab, worldPosition, Quaternion.identity);
                                caveInstance.transform.parent = meshObject.transform; // Faz a caverna ser filha do chunk

                                // Marca que a caverna foi gerada
                                isCaveGenerated = true;
                                return;
                            }
                        }
                    }
                }
            }
            else
            {
                // Se o jogador se afastou, destrói a caverna (se existir)
                if (caveInstance != null)
                {
                    GameObject.Destroy(caveInstance);
                    caveInstance = null;

                    // Marca que a caverna foi destruída
                    isCaveGenerated = false;
                }
            }
        }
        else
        {
            // Verifica se a caverna precisa ser recriada quando o chunk voltar
            if (isCaveGenerated && caveInstance == null)
            {
                // Recria a caverna na posição salva
                caveInstance = GameObject.Instantiate(cavePrefab, cavePosition, Quaternion.identity);
                caveInstance.transform.parent = meshObject.transform; // Faz a caverna ser filha do chunk
            }
        }
    }

    Vector3 GetWorldPositionFromHeightMapCoords(int x, int y, float height)
    {
        // Lógica para converter as coordenadas locais do mapa de altura para as coordenadas do mundo
        float worldX = coord.x * meshSettings.meshWorldSize + x * meshSettings.meshScale;
        float worldY = height; // A altura do terreno
        float worldZ = coord.y * meshSettings.meshWorldSize + y * meshSettings.meshScale;

        return new Vector3(worldX, worldY, worldZ);
    }

    // Propriedade para obter a posição do visualizador (câmera)
    Vector2 viewerPosition
    {
        get
        {
            return new Vector2(viewer.position.x, viewer.position.z);
        }
    }

    // Atualiza o chunk do terreno com base na visibilidade e LOD
    public void UpdateTerrainChunk()
    {
        if (heightMapReceived)
        {
            float viewerDstFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(viewerPosition));

            bool wasVisible = IsVisible();
            bool visible = viewerDstFromNearestEdge <= maxViewDst;

            // Atualiza o índice de LOD com base na distância do visualizador
            if (visible)
            {
                int lodIndex = 0;

                for (int i = 0; i < detailLevels.Length - 1; i++)
                {
                    if (viewerDstFromNearestEdge > detailLevels[i].visibleDstThreshold)
                    {
                        lodIndex = i + 1;
                    }
                    else
                    {
                        break;
                    }
                }

                // Se o LOD mudou, atualiza a malha
                if (lodIndex != previousLODIndex)
                {
                    LODMesh lodMesh = lodMeshes[lodIndex];
                    if (lodMesh.hasMesh)
                    {
                        previousLODIndex = lodIndex;
                        meshFilter.mesh = lodMesh.mesh;
                    }
                    else if (!lodMesh.hasRequestedMesh)
                    {
                        lodMesh.RequestMesh(heightMap, meshSettings);
                    }
                }
            }

            // Atualiza a visibilidade do chunk
            if (wasVisible != visible)
            {
                SetVisible(visible);
                onVisibilityChanged?.Invoke(this, visible);
            }
        }
    }

    // Atualiza a malha do colisor
    public void UpdateCollisionMesh()
    {
        if (!hasSetCollider)
        {
            float sqrDstFromViewerToEdge = bounds.SqrDistance(viewerPosition);

            // Solicita a malha do colisor se necessário
            if (sqrDstFromViewerToEdge < detailLevels[colliderLODIndex].sqrVisibleDstThreshold)
            {
                if (!lodMeshes[colliderLODIndex].hasRequestedMesh)
                {
                    lodMeshes[colliderLODIndex].RequestMesh(heightMap, meshSettings);
                }
            }

            // Se a distância do visualizador estiver abaixo do limiar, define o colisor
            if (sqrDstFromViewerToEdge < colliderGenerationDistanceThreshold * colliderGenerationDistanceThreshold)
            {
                if (lodMeshes[colliderLODIndex].hasMesh)
                {
                    meshCollider.sharedMesh = lodMeshes[colliderLODIndex].mesh;
                    hasSetCollider = true;
                }
            }
        }
    }

    // Define a visibilidade do chunk
    public void SetVisible(bool visible)
    {
        meshObject.SetActive(visible);
    }

    // Retorna se o chunk está visível ou não
    public bool IsVisible()
    {
        return meshObject.activeSelf;
    }
}

// Classe LODMesh para gerenciamento das malhas de diferentes níveis de detalhe
class LODMesh
{

    public Mesh mesh; // A malha gerada para o nível de detalhe
    public bool hasRequestedMesh; // Flag indicando se a malha foi solicitada
    public bool hasMesh; // Flag indicando se a malha foi gerada
    int lod; // Nível de detalhe
    public event System.Action updateCallback; // Callback para atualização do chunk

    // Construtor do LODMesh
    public LODMesh(int lod)
    {
        this.lod = lod;
    }

    // Callback quando a malha é recebida
    void OnMeshDataReceived(object meshDataObject)
    {
        mesh = ((MeshData)meshDataObject).CreateMesh();
        hasMesh = true;
        updateCallback();
    }

    // Solicita a malha para o LOD especificado
    public void RequestMesh(HeightMap heightMap, MeshSettings meshSettings)
    {
        hasRequestedMesh = true;
        ThreadedDataRequester.RequestData(() => MeshGenerator.GenerateTerrainMesh(heightMap.values, meshSettings, lod), OnMeshDataReceived);
    }
}