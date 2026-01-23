using DG.Tweening;
using UnityEngine;

public class FadeController : MonoBehaviour
{
    public static FadeController Instance;

    [SerializeField] private CanvasGroup fadeGroup;

    private void Awake()
    {
        Instance = this;
        fadeGroup.alpha = 0f;
    }

    public void SetBlackInstant()
    {
        fadeGroup.alpha = 1f;
    }

    public Tween FadeOut(float time)
    {
        return fadeGroup.DOFade(0f, time).SetEase(Ease.OutCubic);
    }

    public Tween FadeIn(float time)
    {
        return fadeGroup.DOFade(1f, time).SetEase(Ease.InCubic);
    }
}
