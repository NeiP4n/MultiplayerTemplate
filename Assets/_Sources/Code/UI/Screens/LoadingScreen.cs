using UnityEngine;
using UnityEngine.UI;

namespace Sources.Code.UI
{
    public class LoadingScreen : BaseScreen
    {
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Slider progressBar;
        [SerializeField] private TMPro.TextMeshProUGUI progressText;

        public void Show()
        {
            Enable();
            canvasGroup.alpha = 1f;
            SetProgress(0f);
        }

        public void Hide()
        {
            Disable();
        }

        public void SetProgress(float value)
        {
            value = Mathf.Clamp01(value);
            progressBar.value = value;

            if (progressText != null)
                progressText.text = $"{Mathf.RoundToInt(value * 100f)}%";
        }
    }
}
