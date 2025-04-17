using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

public class UIButtonHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Vector3 originalPosition;
    [SerializeField] private float moveAmount = 10f;
    [SerializeField] private float moveTime = 0.2f;

    private void Start()
    {
        originalPosition = transform.localPosition;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        transform.DOLocalMoveX(originalPosition.x + moveAmount, moveTime).SetEase(Ease.OutCubic);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        transform.DOLocalMoveX(originalPosition.x, moveTime).SetEase(Ease.OutCubic);
    }
}
