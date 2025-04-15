using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardSelector : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("Outline de esta carta")]
    [SerializeField] private Image outlineImage;

    [Header("Otras cartas")]
    [SerializeField] private CardSelector otherCard1;
    [SerializeField] private CardSelector otherCard2;

    public int cardID;

    public void OnPointerEnter(PointerEventData eventData)
    {
        Highlight();
        if (otherCard1 != null) otherCard1.Unhighlight();
        if (otherCard2 != null) otherCard2.Unhighlight();
    }

    public void OnPointerExit(PointerEventData eventData) 
    {
        Unhighlight();
    }

    private void Highlight()
    {
        GameManager.Instance.cardIdSelected = cardID;
        outlineImage.DOFade(1f, 0.5f);
    }

    private void Unhighlight()
    {
        // Solo desmarcamos si esta carta era la seleccionada
        if (GameManager.Instance.cardIdSelected == cardID)
        {
            GameManager.Instance.cardIdSelected = -1;
        }

        outlineImage.DOFade(0f, 0.5f);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        MoveToTarget();
    }

    private void MoveToTarget()
    {
        Transform target = GameManager.Instance.target;

        if (target != null)
        {
            transform.DOMove(target.position, 0.5f).SetEase(Ease.OutBack);
            transform.DORotate(Vector3.zero, 0.5f).SetEase(Ease.OutBack);
        }

        else
        {
            Debug.LogWarning("No hay target asignado en GameManager.");
        }
    }

}
