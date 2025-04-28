using System.IO;
using UnityEngine;

public static class SaveManager
{
    private static string saveFileName = "jugador.save"; // Puedes cambiar el nombre si querés

    private static string SavePath => Path.Combine(Application.persistentDataPath, saveFileName);

    public static void Guardar(SaveData data)
    {
        string json = JsonUtility.ToJson(data, prettyPrint: true);

        File.WriteAllText(SavePath, json);

        Debug.Log($"SaveManager: Datos guardados en {SavePath}");
    }

    public static SaveData Cargar()
    {
        if (!ExisteSave())
        {
            Debug.LogWarning("SaveManager: No existe archivo de guardado. Se crea uno nuevo.");
            return new SaveData(); // Devuelve un SaveData vacío
        }

        string json = File.ReadAllText(SavePath);
        SaveData data = JsonUtility.FromJson<SaveData>(json);

        Debug.Log($"SaveManager: Datos cargados de {SavePath}");
        return data;
    }

    public static bool ExisteSave()
    {
        return File.Exists(SavePath);
    }

    public static void BorrarSave()
    {
        if (ExisteSave())
        {
            File.Delete(SavePath);
            Debug.Log("SaveManager: Archivo de guardado eliminado.");
        }
    }
}
