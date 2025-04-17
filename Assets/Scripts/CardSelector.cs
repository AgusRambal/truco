using DG.Tweening;
using UnityEngine;

public class CardSelector : MonoBehaviour
{
    [SerializeField] private float hoverScaleAmount = 0.1f;
    public bool isOpponent = false;

    private Vector3 originalScale;
    private bool isHovered = false;
    public bool hasBeenPlayed = false;

    private Tween hoverTween;

    private void Start()
    {
        originalScale = transform.localScale;
    }

    private void OnMouseEnter()
    {
        if (isHovered || hasBeenPlayed || isOpponent || GameManager.Instance.estadoRonda == EstadoRonda.Repartiendo) return;

        isHovered = true;
        hoverTween = transform.DOScale(originalScale + Vector3.one * hoverScaleAmount, 0.2f).SetEase(Ease.OutBack);
    }

    private void OnMouseExit()
    {
        if (hasBeenPlayed) return;

        isHovered = false;
        if (hoverTween != null && hoverTween.IsActive()) hoverTween.Kill();
        transform.DOScale(originalScale, 0.2f).SetEase(Ease.OutBack);
    }

    private void OnMouseDown()
    {
        if (hasBeenPlayed ||
            GameManager.Instance.target == null ||
            isOpponent ||
            GameManager.Instance.estadoRonda == EstadoRonda.Repartiendo ||
            GameManager.Instance.turnoActual != TurnoActual.Jugador)
            return;

        hasBeenPlayed = true;
        isHovered = false;

        // 🔥 Cancelar hover si estaba activo
        if (hoverTween != null && hoverTween.IsActive()) hoverTween.Kill();

        // 🔄 Animación jugada
        Vector3 midPos = transform.position + Vector3.up * 0.5f;
        Vector3 finalPos = GameManager.Instance.target.position;
        finalPos.z += GameManager.Instance.GetZOffset();

        Vector3 finalRot = GameManager.Instance.target.rotation.eulerAngles;
        finalRot.z += Random.Range(-10f, 10f);

        Sequence s = DOTween.Sequence();
        s.Append(transform.DOScale(originalScale, 0.1f)); // asegurar escala correcta
        s.Append(transform.DOMove(midPos, 0.15f).SetEase(Ease.OutSine));
        s.Append(transform.DOMove(finalPos, 0.2f).SetEase(Ease.InOutCubic));
        s.Join(transform.DORotate(finalRot, 0.2f).SetEase(Ease.InOutCubic));

        s.OnComplete(() =>
        {
            GameManager.Instance.CartaJugada(this);
        });
    }
}
