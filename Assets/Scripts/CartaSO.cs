using UnityEngine;

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

    [Tooltip("Valor jerárquico para el Truco (de mayor a menor). Por ejemplo: 14 = 1 de Espada, 1 = 4 de Copas")]
    public int jerarquiaTruco;
}
