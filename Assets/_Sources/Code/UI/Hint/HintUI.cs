using DG.Tweening;
using TMPro;
using UnityEngine;

public class HintUI : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TMP_Text text;

    [Header("Animation")]
    [SerializeField] private float fadeInTime = 0.4f;
    [SerializeField] private float fadeOutTime = 0.3f;
    [SerializeField] private Ease fadeInEase = Ease.OutCubic;
    [SerializeField] private Ease fadeOutEase = Ease.InCubic;

    private Tween tween;

    private void Awake()
    {
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
    }

    public void Show(string message)
    {
        text.text = message;

        tween?.Kill();
        canvasGroup.gameObject.SetActive(true);

        tween = canvasGroup
            .DOFade(1f, fadeInTime)
            .SetEase(fadeInEase)
            .SetUpdate(true);
    }

    public void Hide()
    {
        tween?.Kill();

        tween = canvasGroup
            .DOFade(0f, fadeOutTime)
            .SetEase(fadeOutEase)
            .SetUpdate(true)
            .OnComplete(() =>
            {
                canvasGroup.gameObject.SetActive(false);
            });
    }
}
