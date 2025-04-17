using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class IAOponente : MonoBehaviour
{
    public void JugarCarta()
    {
        StartCoroutine(JugarCartaCoroutine());
    }

    private IEnumerator JugarCartaCoroutine()
    {
        yield return new WaitForSeconds(1f); // Simula tiempo de respuesta

        var disponibles = new List<CardSelector>();
        foreach (var carta in GameManager.Instance.allCards)
        {
            if (carta.isOpponent && !carta.hasBeenPlayed)
                disponibles.Add(carta);
        }

        if (disponibles.Count == 0)
        {
            Debug.Log("Oponente no tiene más cartas.");
            yield break;
        }

        var elegida = disponibles[Random.Range(0, disponibles.Count)];
        elegida.hasBeenPlayed = true;

        // Animación estilo jugador
        Vector3 midPos = elegida.transform.position + Vector3.up * 0.5f;

        Vector3 finalPos = GameManager.Instance.target.position;
        finalPos.z += GameManager.Instance.GetZOffset(); // ✅

        Vector3 finalRot = GameManager.Instance.target.rotation.eulerAngles;
        finalRot.z += Random.Range(-10f, 10f);

        // Visual: poner la carta boca arriba
        elegida.transform.GetChild(0).localRotation = Quaternion.Euler(0f, 0f, 0f);
        elegida.transform.GetChild(0).localPosition = new Vector3(0f, 0f, 0.1f);

        Sequence s = DOTween.Sequence();
        s.Append(elegida.transform.DOMove(midPos, 0.15f).SetEase(Ease.OutSine));
        s.Append(elegida.transform.DOMove(finalPos, 0.2f).SetEase(Ease.InOutCubic));
        s.Join(elegida.transform.DORotate(finalRot, 0.2f).SetEase(Ease.InOutCubic));

        yield return s.WaitForCompletion();

        GameManager.Instance.CartaJugada(elegida);
    }

    public void ResponderTruco()
    {
        StartCoroutine(ResponderTrucoCoroutine());
    }

    private IEnumerator ResponderTrucoCoroutine()
    {
        yield return new WaitForSeconds(1f); // Dramita

        bool acepta = Random.value > 0.4f; // 60% chance de aceptar

        if (acepta)
        {
            Debug.Log("Oponente: ¡Quiero!");
            GameManager.Instance.estadoRonda = EstadoRonda.Jugando;
        }
        else
        {
            Debug.Log("Oponente: ¡No quiero!");
            GameManager.Instance.SumarPuntosJugador();
            GameManager.Instance.FinalizarRonda();
        }
    }
}
