using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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

    private void Start()
    {
        foreach (var carta in cartasMenu)
        {
            StartCoroutine(ShakeCartaLoop(carta));
        }
    }

    public void Jugar()
    {
        SceneManager.LoadScene("Player vs IA"); // Cambiá "GameScene" por el nombre exacto de tu escena de juego
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
            .SetEase(Ease.OutBack); // zoom rebote
    }

    public void CerrarOpciones()
    {
        popupOpciones.DOScale(Vector3.zero, animationDuration)
            .SetEase(Ease.InBack) // contrae con rebote inverso
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
}
