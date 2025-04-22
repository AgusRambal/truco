using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

public class DragCartaUI : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    private bool estaArrastrando = false;
    private Vector3 posicionInicial;
    private Tween shakeTween;

    private RectTransform rectTransform;
    private Canvas canvas;

    [Header("Shake Settings")]
    public float shakeDuration = 1.5f;
    public float shakeStrength = 2f;
    public float shakeDelayMin = 1f;
    public float shakeDelayMax = 3f;

    private Tween myTween;

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

        // Escala con rebote
        myTween = rectTransform.DOScale(1.1f, 1f).SetEase(Ease.OutElastic);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!estaArrastrando) return;

        Vector2 localMousePos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransform.parent as RectTransform,
            eventData.position,
            canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera,
            out localMousePos);

        rectTransform.anchoredPosition = localMousePos;
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
}
