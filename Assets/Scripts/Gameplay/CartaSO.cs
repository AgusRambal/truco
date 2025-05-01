using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public enum Palo
{
    Espada,
    Basto,
    Oro,
    Copa
}

[CreateAssetMenu(fileName = "NuevaCarta", menuName = "Truco/Carta")]

public class CartaSO : ScriptableObject
{
    public Sprite imagen;
    public Palo palo;
    public int valor;
    public string id; 
    public int jerarquiaTruco;
    public int precio => jerarquiaTruco * 200 + 200;

    [ContextMenu("Generar ID automáticamente")]
    public void GenerarID()
    {
        id = $"{valor}_{palo}".ToLower();
#if UNITY_EDITOR
        EditorUtility.SetDirty(this);
        AssetDatabase.SaveAssets();
#endif
        Debug.Log($"ID generado: {id}");
    }

    public string ObtenerNombreCompleto()
    {
        return $"{valor} DE {palo.ToString().ToUpper()}";
    }

}


