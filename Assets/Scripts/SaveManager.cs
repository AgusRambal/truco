using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class SaveManager
{
    private static string saveFileName = "jugador.save";

    private static string SavePath => Path.Combine(Application.persistentDataPath, saveFileName);

    public static void Guardar(SaveData data)
    {
        if (data == null)
        {
            Debug.LogWarning("SaveManager: Intento de guardar datos nulos. Se cancela guardado.");
            return;
        }

        if (data.estadisticas == null)
            data.estadisticas = new EstadisticasJugador();
        if (data.cartasCompradas == null)
            data.cartasCompradas = new List<string>();
        if (data.mazoPersonalizado == null)
            data.mazoPersonalizado = new List<string>();

        string json = JsonUtility.ToJson(data, prettyPrint: true);
        string encryptedJson = AESHelper.Encrypt(json);

        // BORRAMOS el archivo viejo antes de escribir
        if (File.Exists(SavePath))
        {
            File.Delete(SavePath);
        }

        File.WriteAllText(SavePath, encryptedJson);

        Debug.Log($"SaveManager: Datos guardados correctamente en {SavePath}");
    }

    public static SaveData Cargar()
    {
        if (!ExisteSave())
        {
            Debug.LogWarning("SaveManager: No existe archivo de guardado. Se crea uno nuevo.");
            SaveData nuevo = new SaveData();
            Guardar(nuevo);
            return nuevo;
        }

        try
        {
            string encryptedJson = File.ReadAllText(SavePath);

            if (string.IsNullOrEmpty(encryptedJson) || encryptedJson.Length < 10)
            {
                Debug.LogWarning("SaveManager: Archivo de guardado vacío o inválido. Se crea uno nuevo.");
                SaveData nuevo = new SaveData();
                Guardar(nuevo);
                return nuevo;
            }

            string decryptedJson = AESHelper.Decrypt(encryptedJson);

            SaveData data = JsonUtility.FromJson<SaveData>(decryptedJson);
            Debug.Log($"SaveManager: Datos cargados de {SavePath} (desencriptado)");
            return data;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"SaveManager: Error al cargar o desencriptar el save. Se crea uno nuevo. {e.Message}");
            return new SaveData();
        }
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
