using UnityEngine;
using System.Collections;

// Este script define as configurações de um mapa de altura como um ScriptableObject,
// permitindo fácil personalização e reutilização dentro do Unity.
[CreateAssetMenu()]
public class HeightMapSettings : UpdatableData
{

    // Configurações relacionadas ao ruído para gerar o mapa de altura.
    public NoiseSettings noiseSettings;

    // Determina se o mapa de altura deve usar um decaimento (falloff) para suavizar as bordas.
    public bool useFalloff;

    // Multiplicador de altura para ajustar a escala vertical do terreno.
    public float heightMultiplier;

    // Curva de animação que define como a altura varia com base em valores normalizados (0 a 1).
    public AnimationCurve heightCurve;

    // Calcula a altura mínima com base na curva e no multiplicador de altura.
    public float minHeight
    {
        get
        {
            return heightMultiplier * heightCurve.Evaluate(0);
        }
    }

    // Calcula a altura máxima com base na curva e no multiplicador de altura.
    public float maxHeight
    {
        get
        {
            return heightMultiplier * heightCurve.Evaluate(1);
        }
    }

#if UNITY_EDITOR

    // Método chamado no editor quando as propriedades são alteradas.
    // Garante que os valores do 'noiseSettings' sejam validados.
    protected override void OnValidate()
    {
        noiseSettings.ValidateValues();
        base.OnValidate();
    }
#endif
}
