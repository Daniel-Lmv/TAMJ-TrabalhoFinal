using UnityEngine;
using System.Collections;

public class UpdatableData : ScriptableObject
{

    // Evento que é disparado quando os valores são atualizados
    public event System.Action OnValuesUpdated;

    // Flag que define se a atualização automática é habilitada
    public bool autoUpdate;

#if UNITY_EDITOR
    // Método chamado sempre que o script é validado no editor do Unity
    protected virtual void OnValidate()
    {
        // Se a atualização automática estiver habilitada, registra o método NotifyOfUpdatedValues para ser chamado no próximo frame de edição
        if (autoUpdate)
        {
            UnityEditor.EditorApplication.update += NotifyOfUpdatedValues;
        }
    }

    // Método que notifica que os valores foram atualizados e dispara o evento OnValuesUpdated
    public void NotifyOfUpdatedValues()
    {
        // Remove o método de atualização para não ser chamado novamente
        UnityEditor.EditorApplication.update -= NotifyOfUpdatedValues;

        // Dispara o evento OnValuesUpdated, se houver algum ouvinte registrado
        if (OnValuesUpdated != null)
        {
            OnValuesUpdated();
        }
    }
#endif
}
