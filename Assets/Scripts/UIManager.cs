using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Botones de juego")]
    public Button truco;
    public Button meVoy;
    public Button envido;

    public void SetBotonesInteractables(bool estado)
    {
        truco.interactable = estado;
        meVoy.interactable = estado;
        envido.interactable = estado;
    }
}
