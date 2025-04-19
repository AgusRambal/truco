using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
    [Header("Cartas en pantalla")]
    [SerializeField] private List<Transform> cartasMenu;

    [Header("Configuración del movimiento")]
    [SerializeField] private float shakeDuration = 1.5f;
    [SerializeField] private float shakeStrength = 2f;
    [SerializeField] private float shakeInterval = 2f;

    private void Start()
    {
        foreach (var carta in cartasMenu)
        {
            StartCoroutine(ShakeCartaLoop(carta));
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

    public void QuitGame()
    { 
        Application.Quit();
    }
}
