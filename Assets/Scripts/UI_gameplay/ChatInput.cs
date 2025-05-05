using TMPro;
using UnityEngine;

public class ChatInput : MonoBehaviour
{
    [SerializeField] private TMP_InputField inputField;

    private void Start()
    {
        inputField.onSubmit.AddListener(ProcesarTexto); // si usás onSubmit
        // o: inputField.onEndEdit.AddListener(ProcesarTexto); // si usás onEndEdit
    }

    private void ProcesarTexto(string texto)
    {
        if (string.IsNullOrWhiteSpace(texto)) return;

        string nombreJugador = GameManager.Instance.NombreJugador(true); // o el nombre que uses
        ChatManager.Instance.AgregarMensaje($"{texto}", TipoMensaje.Jugador);

        inputField.text = "";
        inputField.ActivateInputField(); // opcional: vuelve a enfocar
    }
}