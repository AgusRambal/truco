using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CartaEnTienda : MonoBehaviour
{
    public CartaSO carta;                  // asignás el SO desde el inspector
    public int precio = 20;               // precio de esta carta
    public Button botonComprar;           // botón del prefab
    public GameObject marcoComprado;      // objeto visual para marcar compra
    public TMP_Text textoPrecio;          // si querés mostrar el precio
    public AudioClip buy;

    private MenuManager menuManager;

    private void Awake()
    {
        menuManager = FindFirstObjectByType<MenuManager>();
    }

    private void Start()
    {
        botonComprar.onClick.AddListener(Comprar);
        ActualizarVista();
    }

    private void ActualizarVista()
    {
        bool comprada = CartaCompraManager.YaEstaComprada(carta);

        // ⬇️ En vez de SetActive:
        var img = marcoComprado.GetComponent<Image>();
        if (img != null)
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

        int creditos = PlayerPrefs.GetInt("Creditos", 0);
        if (creditos < precio)
        {
            Debug.LogWarning("No tenés créditos suficientes.");
            return;
        }

        CartaCompraManager.ComprarCarta(carta, precio);
        ActualizarVista();
        menuManager.ActualizarCreditosUI();
        menuManager.AgregarCartaCompradaYSpawnear(carta);
        AudioManager.Instance.PlaySFX(buy);
    }
}
