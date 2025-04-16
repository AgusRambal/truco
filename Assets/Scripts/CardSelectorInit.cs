using UnityEngine;

public static class CardSelectorInit
{
    [RuntimeInitializeOnLoadMethod]
    private static void ResetCardOffsets()
    {
        CardSelector.ResetYOffset();
    }
}
