using System;
using UnityEngine;
using DG.Tweening;
using Random = UnityEngine.Random;

public class MezclarCartas : MonoBehaviour
{
    private Transform[] cartas;
    private Vector3[] posicionesOriginales;
    private bool estaMezclando = false;

    private void Awake()
    {
        Application.targetFrameRate = 60;
    }

    void Start()
    {
        // Obtiene automáticamente los hijos como cartas
        cartas = new Transform[transform.childCount];
        posicionesOriginales = new Vector3[cartas.Length];

        for (int i = 0; i < cartas.Length; i++)
        {
            cartas[i] = transform.GetChild(i);
            posicionesOriginales[i] = cartas[i].localPosition; // Guardamos posiciones locales
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && !estaMezclando)
        {
            StartCoroutine(Mezclar());
        }
    }

    private System.Collections.IEnumerator Mezclar()
    {
        estaMezclando = true;

        // Mueve las cartas a posiciones aleatorias locales
        foreach (var carta in cartas)
        {
            Vector3 posRandom = posicionesOriginales[Random.Range(0, cartas.Length)] + 
                                new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f), 0);

            carta.DOLocalMove(posRandom, 0.3f).SetEase(Ease.OutQuad);
        }

        yield return new WaitForSeconds(0.5f);

        // Regresa las cartas a su posición original
        for (int i = 0; i < cartas.Length; i++)
        {
            cartas[i].DOLocalMove(posicionesOriginales[i], 0.4f).SetEase(Ease.InOutQuad);
        }

        yield return new WaitForSeconds(0.5f);
        estaMezclando = false;
    }
}