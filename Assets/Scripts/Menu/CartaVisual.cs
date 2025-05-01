using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CartaVisual : MonoBehaviour
{
    [SerializeField] private Image imagenCarta;
    [SerializeField] private Button boton;
    [SerializeField] private Image marcoSeleccionado;
    [SerializeField] private AudioClip buttonSound;
    [SerializeField] private TMP_Text botonTexto; 
    [SerializeField] private TMP_Text nombre; 

    private CartaSO cartaSO;              
    private CartaSO cartaOriginalSO;       
    private bool estaReemplazada = false;  
    private MenuManager menuManager;
    public string CartaId => cartaSO?.id;

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
        nombre.text = carta.ObtenerNombreCompleto();

        menuManager = FindFirstObjectByType<MenuManager>();

        estaReemplazada = menuManager.EstaCartaEnUso(cartaSO);
        cartaOriginalSO = menuManager.ObtenerCartaOriginal(cartaSO);

        SetMarcoSeleccion(estaReemplazada);
        ActualizarTextoBoton();

        boton.onClick.AddListener(() =>
        {
            AlternarCarta();
            AudioManager.Instance.PlaySFX(buttonSound);
        });
    }

    private void AlternarCarta()
    {
        if (menuManager == null) return;

        if (estaReemplazada)
        {
            if (cartaOriginalSO != null)
            {
                menuManager.ReemplazarCartaElegida(cartaOriginalSO);
                estaReemplazada = false;
                SetMarcoSeleccion(false);
                ActualizarTextoBoton();
            }
        }
        else
        {
            if (cartaOriginalSO != null)
            {
                menuManager.ReemplazarCartaElegida(cartaSO);
                estaReemplazada = true;
                SetMarcoSeleccion(true);
                ActualizarTextoBoton();
            }
        }
    }

    private void ActualizarTextoBoton()
    {
        if (botonTexto == null) return;

        botonTexto.text = estaReemplazada ? "SACAR" : "USAR";
    }

    public void SetMarcoSeleccion(bool activo)
    {
        if (marcoSeleccionado == null) return;
        marcoSeleccionado.DOFade(activo ? 1f : 0f, 0.25f).SetEase(Ease.OutQuad);
    }

    public bool EsMismaCarta(CartaSO otra)
    {
        return cartaSO.valor == otra.valor && cartaSO.palo == otra.palo;
    }

    public void ForzarDesmarcar()
    {
        estaReemplazada = false;
        SetMarcoSeleccion(false);
        ActualizarTextoBoton();
    }
}
