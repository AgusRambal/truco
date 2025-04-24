using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

public class MenuHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private float scaleAmount = 1.1f;
    [SerializeField] private float scaleTime = 0.2f;

    private Vector3 originalScale;
    private Tween scaleTween;

    private void Start()
    {
        originalScale = Vector3.one;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        scaleTween?.Kill();

        scaleTween = transform.DOScale(originalScale * scaleAmount, scaleTime).SetEase(Ease.OutBack);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        scaleTween?.Kill();

        scaleTween = transform.DOScale(originalScale, scaleTime).SetEase(Ease.OutBack);
    }

    private void OnDisable()
    {
        scaleTween?.Kill();
    }
}
