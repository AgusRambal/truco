using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ScrollResetter : MonoBehaviour
{
    [SerializeField] private ScrollRect scrollRect;
    
    public void ResetScroll()
    {
        StartCoroutine(ResetScrollNextFrame());
    }

    private IEnumerator ResetScrollNextFrame()
    {
        yield return null; // Esperamos 1 frame

        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 1f;
    }
}
