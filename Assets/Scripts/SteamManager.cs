using UnityEngine;
using Steamworks;

public class SteamManager : MonoBehaviour
{
    private static SteamManager instance;
    public static bool Initialized { get; private set; } = false;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        try
        {
            if (SteamAPI.RestartAppIfNecessary((AppId_t)480))
            {
                Application.Quit();
                return;
            }
        }
        catch (System.DllNotFoundException)
        {
            Debug.LogError("Steamworks DLL not found. Steam must be running for the game to launch.");
            Application.Quit();
            return;
        }

        Initialized = SteamAPI.Init();
        if (!Initialized)
        {
            Debug.LogError("SteamAPI.Init() failed.");
            Application.Quit();
        }
    }

    private void Update()
    {
        if (Initialized)
        {
            SteamAPI.RunCallbacks();
        }
    }

    private void OnApplicationQuit()
    {
        if (Initialized)
        {
            SteamAPI.Shutdown();
        }
    }
}
