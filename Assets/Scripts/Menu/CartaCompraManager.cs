using System.Collections.Generic;
using UnityEngine;

public static class CartaCompraManager
{
    public static void ComprarCarta(CartaSO carta, int precio)
    {
        if (SaveSystem.Datos.monedas < precio)
        {
            Debug.LogWarning("No tenés suficientes créditos.");
            return;
        }

        if (YaEstaComprada(carta))
        {
            Debug.Log("Esta carta ya fue comprada.");
            return;
        }

        SaveSystem.Datos.monedas -= precio;
        SaveSystem.Datos.cartasCompradas.Add(carta.id);
        SaveSystem.GuardarDatos();

        Debug.Log($"Carta {carta.id} comprada. Créditos restantes: {SaveSystem.Datos.monedas}");
    }

    public static bool YaEstaComprada(CartaSO carta)
    {
        return SaveSystem.Datos.cartasCompradas.Contains(carta.id);
    }

    public static List<string> ObtenerIDsComprados()
    {
        return SaveSystem.Datos.cartasCompradas;
    }
}
