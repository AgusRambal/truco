using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

public class UIButtonHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private float moveAmount = 10f;
    [SerializeField] private float moveTime = 0.2f;
    [SerializeField] private bool isResponse = false;

    private Vector3 originalPosition;

    private void Start()
    {
        if (isResponse)
        {
            originalPosition = transform.localPosition + new Vector3(810,0,0);
        }

        else 
        { 
            originalPosition = transform.localPosition;
        }
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
