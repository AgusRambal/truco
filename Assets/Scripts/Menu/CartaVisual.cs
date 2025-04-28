using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class CartaVisual : MonoBehaviour
{
    [SerializeField] private Image imagenCarta;
    [SerializeField] private Button boton;
    [SerializeField] private Image marcoSeleccionado;
    [SerializeField] private AudioClip buttonSound;

    private CartaSO cartaSO;              // La carta personalizada (nueva)
    private CartaSO cartaOriginalSO;       // La carta original (guardada antes de reemplazar)
    private bool estaReemplazada = false;  // Estado actual
    private MenuManager menuManager;

    private void Awake()
    {
        if (marcoSeleccionado != null)
        {
            var color = marcoSeleccionado.color;
            color.a = 0f;
            marcoSeleccionado.color = color;
        }
    }

    public void Configurar(CartaSO carta)
    {
        cartaSO = carta;
        imagenCarta.sprite = carta.imagen;

        menuManager = FindFirstObjectByType<MenuManager>();

        estaReemplazada = menuManager.EstaCartaEnUso(cartaSO);
        cartaOriginalSO = menuManager.ObtenerCartaOriginal(cartaSO);

        SetMarcoSeleccion(estaReemplazada);

        boton.onClick.AddListener(() =>
        {
            AlternarCarta();
        });

        AudioManager.Instance.PlaySFX(buttonSound);
    }

    private void AlternarCarta()
    {
        if (menuManager == null) return;

        if (estaReemplazada)
        {
            // Restaurar la original
            if (cartaOriginalSO != null)
            {
                menuManager.ReemplazarCartaElegida(cartaOriginalSO);
                estaReemplazada = false;
                SetMarcoSeleccion(false);
            }
        }
        else
        {
            // Guardar la carta que está ahora antes de reemplazar
            cartaOriginalSO = menuManager.ObtenerCartaOriginal(cartaSO);
            if (cartaOriginalSO != null)
            {
                menuManager.ReemplazarCartaElegida(cartaSO);
                estaReemplazada = true;
                SetMarcoSeleccion(true);
            }
        }
    }

    public void SetMarcoSeleccion(bool activo)
    {
        if (marcoSeleccionado == null) return;
        marcoSeleccionado.DOFade(activo ? 1f : 0f, 0.25f).SetEase(Ease.OutQuad);
    }
}
