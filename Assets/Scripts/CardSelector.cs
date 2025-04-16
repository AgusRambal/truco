using DG.Tweening;
using UnityEngine;

public class CardSelector : MonoBehaviour
{
    [SerializeField] private float hoverScaleAmount = 0.1f;

    private Vector3 originalScale;
    private bool isHovered = false;
    private bool hasBeenPlayed = false;

    private static float yOffset = 0f;

    private void Start()
    {
        originalScale = transform.localScale;
    }

    private void OnMouseEnter()
    {
        if (isHovered || hasBeenPlayed) return;

        isHovered = true;
        transform.DOScale(originalScale + Vector3.one * hoverScaleAmount, 0.2f).SetEase(Ease.OutBack);
    }

    private void OnMouseExit()
    {
        if (hasBeenPlayed) return;

        isHovered = false;
        transform.DOScale(originalScale, 0.2f).SetEase(Ease.OutBack);
    }

    private void OnMouseDown()
    {
        if (hasBeenPlayed || GameManager.Instance.target == null)
            return;

        hasBeenPlayed = true;
        transform.DOScale(originalScale, 0.1f);

        // Paso 1: levantar en Y
        Vector3 midPos = transform.position + Vector3.up * 0.5f;

        // Paso 2: ir al target con offset en Y
        Vector3 finalPos = GameManager.Instance.target.position;
        finalPos.y += yOffset;
        yOffset += 0.1f;

        // Rotación final con variación
        Vector3 finalRot = GameManager.Instance.target.rotation.eulerAngles;
        finalRot.z += Random.Range(-10f, 10f);

        // Animaciones
        transform.DOMove(midPos, 0.15f).SetEase(Ease.OutSine).OnComplete(() =>
        {
            transform.DOMove(finalPos, 0.2f).SetEase(Ease.InOutCubic);
            transform.DORotate(finalRot, 0.2f).SetEase(Ease.InOutCubic);
        });
    }

    public static void ResetYOffset()
    {
        yOffset = 0f;
    }
}
