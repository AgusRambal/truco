using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class CartaSaveManager
{
    public static void GuardarCartas(List<CartaSO> cartas)
    {
        SaveSystem.Datos.mazoPersonalizado = cartas.Select(c => c.id).ToList();
        SaveSystem.GuardarDatos();

        Debug.Log($"CartaSaveManager: Mazo personalizado guardado ({cartas.Count} cartas).");
    }

    public static List<CartaSO> CargarCartas(List<CartaSO> todasLasCartas)
    {
        var result = new List<CartaSO>();

        if (SaveSystem.Datos.mazoPersonalizado == null || SaveSystem.Datos.mazoPersonalizado.Count != 40)
        {
            Debug.LogWarning("CartaSaveManager: No hay mazo personalizado válido. Usando mazo default.");
            return todasLasCartas; // fallback al mazo original si no hay datos válidos
        }

        foreach (string id in SaveSystem.Datos.mazoPersonalizado)
        {
            var carta = todasLasCartas.FirstOrDefault(c => c.id == id);
            if (carta != null) result.Add(carta);
        }

        return result.Count == 40 ? result : todasLasCartas;
    }

    public static void ReemplazarCarta(CartaSO nueva, List<CartaSO> lista)
    {
        for (int i = 0; i < lista.Count; i++)
        {
            if (lista[i].valor == nueva.valor && lista[i].palo == nueva.palo)
            {
                lista[i] = nueva;
                Debug.Log($"Reemplazada: {nueva.valor} de {nueva.palo} → {nueva.id}");

                // Actualizar el mazo guardado también
                GuardarCartas(lista);
                return;
            }
        }

        Debug.LogWarning("CartaSaveManager: No se encontró la carta a reemplazar.");
    }
}
