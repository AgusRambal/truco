using UnityEngine;
using Steamworks;

public class SteamUserManager : MonoBehaviour
{
    public static SteamUserManager Instance { get; private set; }

    public string PlayerName { get; private set; }
    public Sprite PlayerAvatar { get; private set; }
    public CSteamID PlayerSteamID { get; private set; }  // 🔥 Nueva línea

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        if (SteamManager.Initialized)
        {
            PlayerName = SteamFriends.GetPersonaName();
            PlayerSteamID = SteamUser.GetSteamID();  // Guardamos el SteamID
            LoadAvatar();
        }
        else
        {
            Debug.LogWarning("Steam no está inicializado. No se pudo cargar nombre ni avatar.");
        }
    }

    private void LoadAvatar()
    {
        int avatarInt = SteamFriends.GetLargeFriendAvatar(SteamUser.GetSteamID());
        if (avatarInt == -1)
        {
            Debug.LogWarning("No se pudo obtener el avatar del jugador.");
            return;
        }

        uint width, height;
        if (SteamUtils.GetImageSize(avatarInt, out width, out height))
        {
            byte[] image = new byte[width * height * 4];
            if (SteamUtils.GetImageRGBA(avatarInt, image, (int)(width * height * 4)))
            {
                Texture2D avatarTexture = new Texture2D((int)width, (int)height, TextureFormat.RGBA32, false);
                avatarTexture.LoadRawTextureData(image);
                avatarTexture.Apply();

                Texture2D flippedTexture = FlipTextureVertically(avatarTexture);
                PlayerAvatar = Sprite.Create(flippedTexture, new Rect(0, 0, flippedTexture.width, flippedTexture.height), new Vector2(0.5f, 0.5f));
            }
            else
            {
                Debug.LogWarning("No se pudo cargar la imagen del avatar.");
            }
        }
    }

    private Texture2D FlipTextureVertically(Texture2D original)
    {
        Texture2D flipped = new Texture2D(original.width, original.height);

        for (int y = 0; y < original.height; y++)
        {
            flipped.SetPixels(0, y, original.width, 1, original.GetPixels(0, original.height - y - 1, original.width, 1));
        }

        flipped.Apply();
        return flipped;
    }
}
