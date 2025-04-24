using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class CartaCompraManager
{
    private const string keyCompradas = "CartasCompradas";

    public static void ComprarCarta(CartaSO carta, int precio)
    {
        int creditos = PlayerPrefs.GetInt("Creditos", 0);
        if (creditos < precio)
        {
            Debug.LogWarning("No tenés suficientes créditos.");
            return;
        }

        if (YaEstaComprada(carta))
        {
            Debug.Log("Esta carta ya fue comprada.");
            return;
        }

        // Descontar monedas
        creditos -= precio;
        PlayerPrefs.SetInt("Creditos", creditos);

        // Guardar carta comprada
        var compradas = ObtenerIDsComprados();
        compradas.Add(carta.id);
        GuardarLista(compradas);

        Debug.Log($"Carta {carta.id} comprada. Créditos restantes: {creditos}");
    }

    public static bool YaEstaComprada(CartaSO carta)
    {
        return ObtenerIDsComprados().Contains(carta.id);
    }

    public static List<string> ObtenerIDsComprados()
    {
        string json = PlayerPrefs.GetString(keyCompradas, "{}");
        var wrapper = JsonUtility.FromJson<CartaIDWrapper>(json) ?? new CartaIDWrapper();
        return wrapper.ids?.ToList() ?? new List<string>();
    }

    public static void GuardarLista(List<string> lista)
    {
        var wrapper = new CartaIDWrapper { ids = lista.ToArray() };
        string json = JsonUtility.ToJson(wrapper);
        PlayerPrefs.SetString(keyCompradas, json);
        PlayerPrefs.Save();
    }
}
