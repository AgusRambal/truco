// SaveSystem.cs
using System.Collections.Generic;
using UnityEngine;

public static class SaveSystem
{
    public static SaveData Datos { get; private set; }

    public static void CargarDatos()
    {
        Datos = SaveManager.Cargar();

        if (Datos == null)
        {
            Datos = new SaveData();
        }

        if (Datos.estadisticas == null)
            Datos.estadisticas = new EstadisticasJugador();
        if (Datos.cartasCompradas == null)
            Datos.cartasCompradas = new List<string>();
        if (Datos.mazoPersonalizado == null)
            Datos.mazoPersonalizado = new List<string>();
    }

    public static void GuardarDatos()
    {
        if (Datos == null)
        {
            Debug.LogWarning("SaveSystem: Datos está null. Se crea uno nuevo antes de guardar.");
            Datos = new SaveData();
        }

        // Revalidar campos críticos
        if (Datos.estadisticas == null)
            Datos.estadisticas = new EstadisticasJugador();
        if (Datos.cartasCompradas == null)
            Datos.cartasCompradas = new List<string>();
        if (Datos.mazoPersonalizado == null)
            Datos.mazoPersonalizado = new List<string>();

        // Serializar para validar antes de encriptar
        string json = JsonUtility.ToJson(Datos, prettyPrint: true);
        if (string.IsNullOrEmpty(json) || json.Length < 10)
        {
            Debug.LogError("SaveSystem: JSON generado inválido. No se guarda.");
            return;
        }

        SaveManager.Guardar(Datos);
    }

}
