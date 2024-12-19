using UnityEngine;
using System.Collections;
using UnityEditor;

// Esta classe é um editor personalizado para 'UpdatableData' e seus tipos derivados.
// Permite customizar o comportamento do Inspector no Unity.
[CustomEditor(typeof(UpdatableData), true)]
public class UpdatableDataEditor : Editor
{

    // Sobrescreve o método padrão do Inspector para adicionar controles personalizados.
    public override void OnInspectorGUI()
    {
        // Chama o Inspector padrão.
        base.OnInspectorGUI();

        // Obtém a instância do objeto 'UpdatableData' sendo editado.
        UpdatableData data = (UpdatableData)target;

        // Adiciona um botão "Update" ao Inspector.
        // Quando clicado, chama 'NotifyOfUpdatedValues' para aplicar alterações.
        if (GUILayout.Button("Update"))
        {
            data.NotifyOfUpdatedValues();

            // Marca o objeto como modificado para que o Unity registre as mudanças.
            EditorUtility.SetDirty(target);
        }
    }
}
