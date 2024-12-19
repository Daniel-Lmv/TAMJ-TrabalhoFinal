using UnityEngine;
using System.Collections;
using UnityEditor;

// Esta classe é um editor personalizado para o componente 'MapPreview'.
// Permite personalizar o Inspector no Unity.
[CustomEditor(typeof(MapPreview))]
public class MapPreviewEditor : Editor
{

    // Sobrescreve o método para customizar a interface gráfica do Inspector.
    public override void OnInspectorGUI()
    {
        MapPreview mapPreview = (MapPreview)target;

        // Renderiza os elementos padrão do Inspector.
        // Adiciona comportamento adicional quando o 'autoUpdate' está ativado.
        if (DrawDefaultInspector())
        {
            if (mapPreview.autoUpdate)
            {
                mapPreview.DrawMapInEditor();
            }
        }

        // Adiciona um botão "Generate" que, ao ser clicado, chama 'DrawMapInEditor'.
        if (GUILayout.Button("Generate"))
        {
            mapPreview.DrawMapInEditor();
        }
    }
}
