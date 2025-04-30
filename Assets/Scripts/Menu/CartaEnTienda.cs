using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CartaEnTienda : MonoBehaviour
{
    [SerializeField] private CartaSO carta;
    [SerializeField] private int precio = 20;
    [SerializeField] private Button botonComprar;
    [SerializeField] private GameObject marcoComprado;
    [SerializeField] private TMP_Text textoPrecio;
    [SerializeField] private TMP_Text nombre;
    [SerializeField] private Image imagen;
    [SerializeField] private AudioClip buy;
    [SerializeField] private Vector2 advertenciaOffset = new Vector2(0f, 100f);

    private MenuManager menuManager;

    private void Awake()
    {
        menuManager = FindFirstObjectByType<MenuManager>();
    }

    private void Start()
    {
        botonComprar.onClick.AddListener(Comprar);
        nombre.text = carta.ObtenerNombreCompleto();
        imagen.sprite = carta.imagen;
        ActualizarVista();
    }

    private void ActualizarVista()
    {
        bool comprada = CartaCompraManager.YaEstaComprada(carta);

        if (marcoComprado.TryGetComponent<Image>(out var img))
        {
            float targetAlpha = comprada ? 1f : 0f;
            img.DOFade(targetAlpha, 0.25f).SetEase(Ease.OutQuad);
        }

        botonComprar.interactable = !comprada;
        textoPrecio.text = $"{precio}";
    }

    private void Comprar()
    {
        if (CartaCompraManager.YaEstaComprada(carta))
        {
            Debug.Log("Ya tenés esta carta.");
            return;
        }

        if (SaveSystem.Datos.monedas < precio)
        {
            menuManager.MostrarAdvertencia("No tenes creditos suficientes", transform, advertenciaOffset, 20);
            return;
        }

        CartaCompraManager.ComprarCarta(carta, precio);
        ActualizarVista();
        menuManager.ActualizarCreditosUI();
        menuManager.AgregarCartaCompradaYSpawnear(carta);
        menuManager.ShowLessCredits($"-{precio}");
        AudioManager.Instance.PlaySFX(buy);
    }
}
