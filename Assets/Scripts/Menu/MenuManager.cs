using DG.Tweening;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using static Utils;

public class MenuManager : MonoBehaviour
{
    [Header("Cartas en pantalla")]
    [SerializeField] private List<Transform> cartasMenu;

    [Header("Configuración del movimiento")]
    [SerializeField] private float shakeDuration = 1.5f;
    [SerializeField] private float shakeStrength = 2f;
    [SerializeField] private float shakeInterval = 2f;

    [Header("Referencias")]
    [SerializeField] private RectTransform popupOpciones;
    [SerializeField] private float animationDuration = 0.4f;
    [SerializeField] private TMP_Text creditosTexto;

    [Header("Botones principales")]
    [SerializeField] private List<Transform> botonesMenuPrincipal;

    [Header("Botones del menú de juego")]
    [SerializeField] private List<Transform> botonesPuntos;

    [Header("Configuración de animación")]
    [SerializeField] private float delayEntreBotones = 0.05f;
    [SerializeField] private float duracionAnimacion = 0.25f;

    void Awake()
    {
        DOTween.Init();
    }

    private void Start()
    {
        foreach (var carta in cartasMenu)
        {
            StartCoroutine(ShakeCartaLoop(carta));
        }

        int creditos = PlayerPrefs.GetInt("Creditos", 0);
        creditosTexto.text = $"{creditos}";
    }

    public void Jugar(int puntos)
    {
        ParametrosDePartida.puntosParaGanar = puntos;
        SceneManager.LoadScene("Player vs IA");
    }

    public void MostrarMenuJuego()
    {
        for (int i = 0; i < botonesMenuPrincipal.Count; i++)
        {
            Transform btn = botonesMenuPrincipal[i];
            btn.DOScale(Vector3.zero, duracionAnimacion)
               .SetEase(Ease.InBack)
               .SetDelay(i * delayEntreBotones)
               .OnComplete(() => btn.gameObject.SetActive(false));
        }

        for (int i = 0; i < botonesPuntos.Count; i++)
        {
            Transform btn = botonesPuntos[i];
            btn.localScale = Vector3.zero;
            btn.gameObject.SetActive(true);

            btn.DOScale(Vector3.one, duracionAnimacion)
               .SetEase(Ease.OutBack)
               .SetDelay(botonesMenuPrincipal.Count * delayEntreBotones + i * delayEntreBotones);
        }
    }

    public void VolverAlMenuPrincipal()
    {
        for (int i = 0; i < botonesPuntos.Count; i++)
        {
            Transform btn = botonesPuntos[i];
            btn.DOScale(Vector3.zero, duracionAnimacion)
               .SetEase(Ease.InBack)
               .SetDelay(i * delayEntreBotones)
               .OnComplete(() => btn.gameObject.SetActive(false));
        }

        for (int i = 0; i < botonesMenuPrincipal.Count; i++)
        {
            Transform btn = botonesMenuPrincipal[i];
            btn.localScale = Vector3.zero;
            btn.gameObject.SetActive(true);

            btn.DOScale(Vector3.one, duracionAnimacion)
               .SetEase(Ease.OutBack)
               .SetDelay(botonesPuntos.Count * delayEntreBotones + i * delayEntreBotones);
        }
    }

    private IEnumerator<WaitForSeconds> ShakeCartaLoop(Transform carta)
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(shakeInterval * 0.5f, shakeInterval * 1.5f));

            if (carta != null)
            {
                carta.DOShakeRotation(
                    shakeDuration,
                    new Vector3(0f, 0f, shakeStrength),
                    vibrato: 10,
                    randomness: 90,
                    fadeOut: true
                );
            }
        }
    }

    public void AbrirOpciones()
    {
        popupOpciones.gameObject.SetActive(true);
        popupOpciones.localScale = Vector3.zero;

        popupOpciones.DOScale(Vector3.one, animationDuration)
            .SetEase(Ease.OutBack);
    }

    public void CerrarOpciones()
    {
        popupOpciones.DOScale(Vector3.zero, animationDuration - 0.2f)
            .SetEase(Ease.InBack)
            .OnComplete(() =>
            {
                popupOpciones.gameObject.SetActive(false);
            });
    }

    public void QuitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    [ContextMenu("Resetear Créditos")]
    public void ResetearCreditosDesdeEditor()
    {
        PlayerPrefs.SetInt("Creditos", 0);
        PlayerPrefs.Save();
        Debug.Log("Créditos reseteados desde el editor.");
    }
}
