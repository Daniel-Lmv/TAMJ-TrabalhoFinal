using UnityEngine;
using System.Collections;

public class HideOnPlay : MonoBehaviour
{

    // Método chamado ao iniciar o jogo, desativa o GameObject
    void Start()
    {
        gameObject.SetActive(false);
    }
}
