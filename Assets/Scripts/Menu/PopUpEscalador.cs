using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

public class PopUpEscalador : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private GameObject popUp;
    [SerializeField] private float duracionAnimacion = 0.3f;
    private RectTransform popUpRect;

    private void Awake()
    {
        popUpRect = popUp.GetComponent<RectTransform>();
        popUpRect.localScale = Vector3.zero;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        popUp.SetActive(true);
        popUpRect.DOKill(); // cancelamos animaciones anteriores
        popUpRect.DOScale(Vector3.one, duracionAnimacion).SetEase(Ease.OutBack);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        popUpRect.DOKill();
        popUpRect.DOScale(Vector3.zero, duracionAnimacion)
            .SetEase(Ease.InBack)
            .OnComplete(() => popUp.SetActive(false));
    }
}
