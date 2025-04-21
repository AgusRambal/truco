using DG.Tweening;
using UnityEngine;

public class CardSelector : MonoBehaviour
{
    [SerializeField] private float hoverScaleAmount = 0.1f;
    [SerializeField] private float tiltAmount = 10f;
    [SerializeField] private float tiltSpeed = 5f;
    public bool isOpponent = false;
    public bool hasBeenPlayed = false;

    private Vector3 originalScale;
    private bool isHovered = false;
    private Tween hoverTween;
    private Tween tiltTween;

    private void Start()
    {
        originalScale = transform.localScale;
    }

    private void OnMouseEnter()
    {
        if (isHovered || hasBeenPlayed || isOpponent || GameManager.Instance.estadoRonda == EstadoRonda.Repartiendo || GameManager.Instance.isPaused) return;

        isHovered = true;
        hoverTween = transform.DOScale(originalScale + Vector3.one * hoverScaleAmount, 0.2f).SetEase(Ease.OutBack);
    }

    private void OnMouseExit()
    {
        if (hasBeenPlayed || isOpponent || GameManager.Instance.isPaused) return;

        isHovered = false;

        if (hoverTween != null && hoverTween.IsActive()) hoverTween.Kill();
        if (tiltTween != null && tiltTween.IsActive()) tiltTween.Kill();

        transform.DOScale(originalScale, 0.2f).SetEase(Ease.OutBack);

        if (GameManager.Instance.estadoRonda == EstadoRonda.Repartiendo)
            return;

        // Restaurar rotación solo para jugador
        Vector3 resetRot = new Vector3(-10f, 360f, 0f);
        transform.DORotate(resetRot, 0.3f).SetEase(Ease.OutCubic);
    }

    private void OnMouseDown()
    {
        if (hasBeenPlayed ||
            GameManager.Instance.target == null ||
            isOpponent ||
            GameManager.Instance.estadoRonda == EstadoRonda.Repartiendo ||
            GameManager.Instance.turnoActual != TurnoActual.Jugador || GameManager.Instance.isPaused)
            return;

        hasBeenPlayed = true;
        isHovered = false;

        if (hoverTween != null && hoverTween.IsActive()) hoverTween.Kill();
        if (tiltTween != null && tiltTween.IsActive()) tiltTween.Kill();

        Vector3 midPos = transform.position + Vector3.up * 0.5f;
        Vector3 finalPos = GameManager.Instance.target.position;
        finalPos.z += GameManager.Instance.GetZOffset();

        Vector3 finalRot = GameManager.Instance.target.rotation.eulerAngles;
        finalRot.z += Random.Range(-10f, 10f);

        Sequence s = DOTween.Sequence();
        s.Append(transform.DOScale(originalScale, 0.1f));
        s.Append(transform.DOMove(midPos, 0.15f).SetEase(Ease.OutSine));
        s.Append(transform.DOMove(finalPos, 0.2f).SetEase(Ease.InOutCubic));
        s.Join(transform.DORotate(finalRot, 0.2f).SetEase(Ease.InOutCubic));

        s.OnComplete(() =>
        {
            GameManager.Instance.CartaJugada(this);
        });
    }

    private void Update()
    {
        if (!isHovered || hasBeenPlayed || isOpponent) return;

        Vector3 mousePos = Input.mousePosition;
        Vector3 worldPos = Camera.main.WorldToScreenPoint(transform.position);
        Vector2 offset = (Vector2)(mousePos - worldPos);

        float xTilt = Mathf.Clamp(offset.y / 100f, -1f, 1f) * tiltAmount;
        float yTilt = Mathf.Clamp(-offset.x / 100f, -1f, 1f) * tiltAmount;

        Quaternion targetRotation = Quaternion.Euler(xTilt, yTilt, transform.rotation.eulerAngles.z);

        if (tiltTween != null && tiltTween.IsActive()) tiltTween.Kill();
        tiltTween = transform.DORotateQuaternion(targetRotation, 1f / tiltSpeed).SetEase(Ease.OutCubic);
    }
}
