using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UIButtonSFX : MonoBehaviour, IPointerEnterHandler
{
    [SerializeField] private bool playHoverSound = true;

    private Button button;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (playHoverSound)
            AudioManager.Instance.PlayUI(AudioManager.Instance.hoverSound);
    }
}
