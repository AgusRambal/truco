using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Buttons")]
    public Button truco;
    public Button meVoy;
    public Button envido;

    public void SetBotonesInteractables(bool estado)
    {
        truco.interactable = estado;
        meVoy.interactable = estado;
        envido.interactable = estado;
    }

    public void ChangeTrucoState(int state)
    {
        if (state == 1)
        {
            truco.GetComponentInChildren<TMP_Text>().text = $"RETRUCO";
        }

        else if (state == 2) 
        {
            truco.GetComponentInChildren<TMP_Text>().text = $"QUIERO VALE 4";
        }
    }

    public void ResetTruco()
    {
        truco.GetComponentInChildren<TMP_Text>().text = $"TRUCO";
    }
}
