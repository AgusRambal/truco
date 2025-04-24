using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class CartaSaveManager
{
    private const string keyCartas = "CartasPersonalizadas";

    public static void GuardarCartas(List<CartaSO> cartas)
    {
        var ids = cartas.Select(c => c.id).ToArray();
        var wrapper = new CartaIDWrapper { ids = ids };
        string json = JsonUtility.ToJson(wrapper);

        PlayerPrefs.SetString(keyCartas, json);
        PlayerPrefs.Save();

        Debug.Log($"Cartas guardadas ({cartas.Count})");
    }

    public static List<CartaSO> CargarCartas(List<CartaSO> todasLasCartas)
    {
        if (!PlayerPrefs.HasKey(keyCartas)) return todasLasCartas;

        string json = PlayerPrefs.GetString(keyCartas);
        var wrapper = JsonUtility.FromJson<CartaIDWrapper>(json);

        var result = new List<CartaSO>();

        foreach (string id in wrapper.ids)
        {
            var carta = todasLasCartas.FirstOrDefault(c => c.id == id);
            if (carta != null) result.Add(carta);
        }

        return result.Count == 40 ? result : todasLasCartas; // fallback
    }

    public static void ReemplazarCarta(CartaSO nueva, List<CartaSO> lista)
    {
        for (int i = 0; i < lista.Count; i++)
        {
            if (lista[i].valor == nueva.valor && lista[i].palo == nueva.palo)
            {
                lista[i] = nueva;
                Debug.Log($"Reemplazada: {nueva.valor} de {nueva.palo} → {nueva.id}");
                return;
            }
        }

        Debug.LogWarning("No se encontró la carta a reemplazar");
    }
}
