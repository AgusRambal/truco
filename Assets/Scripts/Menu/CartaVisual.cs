using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class CartaVisual : MonoBehaviour
{
    [SerializeField] private Image imagenCarta;
    [SerializeField] private Button boton;
    [SerializeField] private Image marcoSeleccionado;
    [SerializeField] private AudioClip buttonSound;

    private CartaSO cartaSO;
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

        boton.onClick.AddListener(() =>
        {
            FindFirstObjectByType<MenuManager>().ReemplazarCartaElegida(cartaSO);
            SetMarcoSeleccion(true);
        });

        AudioManager.Instance.PlaySFX(buttonSound);
    }

    public void SetMarcoSeleccion(bool activo)
    {
        if (marcoSeleccionado == null) return;

        marcoSeleccionado.DOFade(activo ? 1f : 0f, 0.25f).SetEase(Ease.OutQuad);
    }

    public CartaSO GetCartaSO() => cartaSO;
}
