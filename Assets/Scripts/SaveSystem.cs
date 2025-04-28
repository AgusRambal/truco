// SaveSystem.cs
using UnityEngine;

public static class SaveSystem
{
    public static SaveData Datos { get; private set; }

    public static void CargarDatos()
    {
        Datos = SaveManager.Cargar();
    }

    public static void GuardarDatos()
    {
        SaveManager.Guardar(Datos);
    }
}
