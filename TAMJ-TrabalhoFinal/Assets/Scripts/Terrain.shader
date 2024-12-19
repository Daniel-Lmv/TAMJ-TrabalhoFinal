Shader "Custom/Terrain" {
    Properties {
        testTexture("Texture", 2D) = "white"{} // Definindo uma textura base para o shader
        testScale("Scale", Float) = 1 // Definindo o fator de escala da textura
    }

    SubShader {
        Tags { "RenderType"="Opaque" }
        LOD 200
        
        CGPROGRAM
        // Modelo de iluminação fisicamente baseado, com sombras habilitadas para todos os tipos de luz
        #pragma surface surf Standard fullforwardshadows

        // Usando o modelo de shader 3.0 para obter uma iluminação mais realista
        #pragma target 3.0

        // Definindo parâmetros máximos de camadas
        const static int maxLayerCount = 8;
        const static float epsilon = 1E-4;

        // Definindo variáveis para as camadas
        int layerCount;
        float3 baseColours[maxLayerCount];
        float baseStartHeights[maxLayerCount];
        float baseBlends[maxLayerCount];
        float baseColourStrength[maxLayerCount];
        float baseTextureScales[maxLayerCount];

        // Definindo variáveis para altura mínima e máxima
        float minHeight;
        float maxHeight;

        // Definindo a textura base e a escala
        sampler2D testTexture;
        float testScale;

        // Declarando a textura de camadas do terreno
        UNITY_DECLARE_TEX2DARRAY(baseTextures);

        // Estrutura de entrada para o cálculo do shader
        struct Input {
            float3 worldPos; // Posição no mundo
            float3 worldNormal; // Normal no mundo
        };

        // Função para normalizar um valor entre dois limites
        float inverseLerp(float a, float b, float value) {
            return saturate((value - a) / (b - a));
        }

        // Função Triplanar para amostrar a textura em 3 eixos
        float3 triplanar(float3 worldPos, float scale, float3 blendAxes, int textureIndex) {
            // Projeções nos três eixos (X, Y, Z)
            float3 scaledWorldPos = worldPos / scale;
            float3 xProjection = UNITY_SAMPLE_TEX2DARRAY(baseTextures, float3(scaledWorldPos.y, scaledWorldPos.z, textureIndex)) * blendAxes.x;
            float3 yProjection = UNITY_SAMPLE_TEX2DARRAY(baseTextures, float3(scaledWorldPos.x, scaledWorldPos.z, textureIndex)) * blendAxes.y;
            float3 zProjection = UNITY_SAMPLE_TEX2DARRAY(baseTextures, float3(scaledWorldPos.x, scaledWorldPos.y, textureIndex)) * blendAxes.z;
            return xProjection + yProjection + zProjection;
        }

        // Função principal do shader (surf)
        void surf (Input IN, inout SurfaceOutputStandard o) {
            // Calculando a porcentagem da altura
            float heightPercent = inverseLerp(minHeight, maxHeight, IN.worldPos.y);
            // Calculando a normal do terreno
            float3 blendAxes = abs(IN.worldNormal);
            blendAxes /= blendAxes.x + blendAxes.y + blendAxes.z;

            // Loop pelas camadas de textura
            for (int i = 0; i < layerCount; i++) {
                // Calculando a força de aplicação da camada
                float drawStrength = inverseLerp(-baseBlends[i] / 2 - epsilon, baseBlends[i] / 2, heightPercent - baseStartHeights[i]);

                // Calculando as cores e as texturas
                float3 baseColour = baseColours[i] * baseColourStrength[i];
                float3 textureColour = triplanar(IN.worldPos, baseTextureScales[i], blendAxes, i) * (1 - baseColourStrength[i]);

                // Combinando a cor base e a textura com base na força de aplicação
                o.Albedo = o.Albedo * (1 - drawStrength) + (baseColour + textureColour) * drawStrength;
            }
        }

        ENDCG
    }

    FallBack "Diffuse" // Usando o fallback de shader difuso
}
