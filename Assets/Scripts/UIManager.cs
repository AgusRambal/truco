using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Botones de juego")]
    public Button truco;
    public Button meVoy;
    public Button envido;

    private void Start()
    {
        //truco.onClick.AddListener(() => GameManager.Instance.CantarTruco());
        //meVoy.onClick.AddListener(() => GameManager.Instance.MeVoy(true)); // true = se va el jugador
        //envido.onClick.AddListener(() => GameManager.Instance.CantarEnvido());
    }
}
