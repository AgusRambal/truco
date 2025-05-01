using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class SaveManager
{
    private static string saveFileName = "jugador.save";
    private static string backupFileName1 = "jugador.bak1";
    private static string backupFileName2 = "jugador.bak2";
    private static string backupFileName3 = "jugador.bak3";

    private static string SavePath => Path.Combine(Application.persistentDataPath, saveFileName);
    private static string BackupPath1 => Path.Combine(Application.persistentDataPath, backupFileName1);
    private static string BackupPath2 => Path.Combine(Application.persistentDataPath, backupFileName2);
    private static string BackupPath3 => Path.Combine(Application.persistentDataPath, backupFileName3);

    private const int monedasAlArrancar = 5500;

    public static void Guardar(SaveData data)
    {
        if (data == null)
        {
            Debug.LogWarning("SaveManager: Intento de guardar datos nulos. Se cancela guardado.");
            return;
        }

        // Aseguramos que los campos estén inicializados antes de guardar
        if (data.estadisticas == null)
            data.estadisticas = new EstadisticasJugador();
        if (data.cartasCompradas == null)
            data.cartasCompradas = new List<string>();
        if (data.mazoPersonalizado == null)
            data.mazoPersonalizado = new List<string>();

        string json = JsonUtility.ToJson(data, prettyPrint: true);
        string encryptedJson = AESHelper.Encrypt(json);

        // Hacer los backups rotativos
        if (File.Exists(BackupPath1))
        {
            File.Copy(BackupPath1, BackupPath2, true); // Backup1 a Backup2
        }

        if (File.Exists(BackupPath2))
        {
            File.Copy(BackupPath2, BackupPath3, true); // Backup2 a Backup3
        }

        // Backup del archivo actual
        if (File.Exists(SavePath))
        {
            File.Copy(SavePath, BackupPath1, true); // Backup el save actual
        }

        // Finalmente, escribir el nuevo save
        File.WriteAllText(SavePath, encryptedJson);

        //Debug.Log($"SaveManager: Datos guardados correctamente en {SavePath}");
    }

    public static SaveData Cargar()
    {
        if (!ExisteSave())
        {
            //Debug.LogWarning("SaveManager: No existe archivo de guardado. Se crea uno nuevo.");
            SaveData nuevo = new SaveData();

            nuevo.monedas = monedasAlArrancar;

            Guardar(nuevo);
            return nuevo;
        }

        try
        {
            string encryptedJson = File.ReadAllText(SavePath);

            if (string.IsNullOrEmpty(encryptedJson) || encryptedJson.Length < 10)
                throw new IOException("Save vacío o inválido.");

            string decryptedJson = AESHelper.Decrypt(encryptedJson);

            SaveData data = JsonUtility.FromJson<SaveData>(decryptedJson);
            //Debug.Log($"SaveManager: Datos cargados de {SavePath} (desencriptado)");
            return data;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"SaveManager: Error al cargar jugador.save. Intentando cargar backup. {e.Message}");

            // Intentar cargar el primer backup
            if (File.Exists(BackupPath1))
            {
                try
                {
                    string backupEncryptedJson = File.ReadAllText(BackupPath1);

                    if (!string.IsNullOrEmpty(backupEncryptedJson) && backupEncryptedJson.Length >= 10)
                    {
                        string decryptedBackupJson = AESHelper.Decrypt(backupEncryptedJson);
                        SaveData backupData = JsonUtility.FromJson<SaveData>(decryptedBackupJson);
                        Debug.LogWarning("SaveManager: Backup1 cargado exitosamente.");
                        return backupData;
                    }
                }
                catch (System.Exception e2)
                {
                    Debug.LogError($"SaveManager: Error al cargar Backup1. {e2.Message}");
                }
            }

            // Intentar cargar el segundo backup
            if (File.Exists(BackupPath2))
            {
                try
                {
                    string backupEncryptedJson = File.ReadAllText(BackupPath2);

                    if (!string.IsNullOrEmpty(backupEncryptedJson) && backupEncryptedJson.Length >= 10)
                    {
                        string decryptedBackupJson = AESHelper.Decrypt(backupEncryptedJson);
                        SaveData backupData = JsonUtility.FromJson<SaveData>(decryptedBackupJson);
                        Debug.LogWarning("SaveManager: Backup2 cargado exitosamente.");
                        return backupData;
                    }
                }
                catch (System.Exception e2)
                {
                    Debug.LogError($"SaveManager: Error al cargar Backup2. {e2.Message}");
                }
            }

            // Intentar cargar el tercer backup
            if (File.Exists(BackupPath3))
            {
                try
                {
                    string backupEncryptedJson = File.ReadAllText(BackupPath3);

                    if (!string.IsNullOrEmpty(backupEncryptedJson) && backupEncryptedJson.Length >= 10)
                    {
                        string decryptedBackupJson = AESHelper.Decrypt(backupEncryptedJson);
                        SaveData backupData = JsonUtility.FromJson<SaveData>(decryptedBackupJson);
                        Debug.LogWarning("SaveManager: Backup3 cargado exitosamente.");
                        return backupData;
                    }
                }
                catch (System.Exception e2)
                {
                    Debug.LogError($"SaveManager: Error al cargar Backup3. {e2.Message}");
                }
            }

            // Si todo falla, crear un nuevo SaveData
            Debug.LogError("SaveManager: No se pudo cargar ni save ni backups. Creando save nuevo.");
            SaveData nuevo = new SaveData();
            Guardar(nuevo);
            return nuevo;
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

        if (File.Exists(BackupPath1))
        {
            File.Delete(BackupPath1);
        }

        if (File.Exists(BackupPath2))
        {
            File.Delete(BackupPath2);
        }

        if (File.Exists(BackupPath3))
        {
            File.Delete(BackupPath3);
        }
    }
}
