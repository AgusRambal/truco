using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

public class DragCartaUI : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{

    [Header("Shake Settings")]
    public float shakeDuration = 1.5f;
    public float shakeStrength = 2f;
    public float shakeDelayMin = 1f;
    public float shakeDelayMax = 3f;

    private bool estaArrastrando = false;
    private Vector3 posicionInicial;
    private Tween shakeTween;
    private RectTransform rectTransform;
    private Canvas canvas;
    private Tween myTween;
    private Vector2 offset;

    void Awake()
    {
        DOTween.Init();
    }

    private void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        posicionInicial = rectTransform.anchoredPosition;

        StartCoroutine(LoopShake());
    }

    private System.Collections.IEnumerator LoopShake()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(shakeDelayMin, shakeDelayMax));

            if (!estaArrastrando)
            {
                shakeTween = rectTransform.DOShakeRotation(
                    shakeDuration,
                    new Vector3(0f, 0f, shakeStrength),
                    vibrato: 10,
                    randomness: 90,
                    fadeOut: true
                );
            }
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        estaArrastrando = true;

        if (shakeTween != null && shakeTween.IsActive()) shakeTween.Kill();

        // Calcular offset con respecto al mouse
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransform.parent as RectTransform,
            eventData.position,
            canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera,
            out Vector2 localMousePos);

        offset = rectTransform.anchoredPosition - localMousePos;

        // Escala con rebote
        myTween = rectTransform.DOScale(1.1f, 1f).SetEase(Ease.OutElastic);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!estaArrastrando) return;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransform.parent as RectTransform,
            eventData.position,
            canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera,
            out Vector2 localMousePos);

        rectTransform.anchoredPosition = localMousePos + offset;
    }


    public void OnPointerUp(PointerEventData eventData)
    {
        myTween.Kill();
        estaArrastrando = false;
        rectTransform.DOAnchorPos(posicionInicial, 0.3f).SetEase(Ease.OutBack);
        rectTransform.DOScale(1f, 0.3f).SetEase(Ease.OutBack);
    }

    private void OnDisable()
    {
        if (shakeTween != null && shakeTween.IsActive()) shakeTween.Kill();
    }

    private void OnDestroy()
    {
        shakeTween?.Kill();
        myTween?.Kill();
    }
}
