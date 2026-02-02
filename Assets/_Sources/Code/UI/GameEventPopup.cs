using DG.Tweening;
using TMPro;
using TriInspector;
using UnityEngine;
using UnityEngine.UI;
using Sources.Code.Configs;

namespace Sources.Code.UI
{
    [DeclareBoxGroup("Setup", Title = "Setup")]
    [DeclareBoxGroup("Runtime", Title = "Runtime (Debug)")]
    public sealed class GameEventPopup : BasePopup
    {
        private const float VisibleAlpha = 1f;

        // =============================
        // Setup
        // =============================

        [Group("Setup"), Required]
        [SerializeField] private Image image;

        [Group("Setup"), Required]
        [SerializeField] private TMP_Text text;

        // =============================
        // Runtime
        // =============================

        [Group("Runtime"), ShowInInspector, ReadOnly]
        private float duration;

        private Color imageColor;
        private Color victoryTextColor;
        private string victoryText;
        private Color defeatTextColor;
        private string defeatText;

        private Tween fadeTween;

        // =============================
        // Init
        // =============================

        public override void Init()
        {
            base.Init();

            var config = GameEventScreenConfig.Instance;
            if (config == null)
            {
                Debug.LogError("[GameEventPopup] Config not found", this);
                return;
            }

            duration = config.Duration;
            imageColor = config.ImageColor;

            victoryText = config.VictoryText;
            victoryTextColor = config.VictoryTextColor;

            defeatText = config.DefeatText;
            defeatTextColor = config.DefeatTextColor;
        }

        // =============================
        // API
        // =============================

        public void ShowVictory()
        {
            ShowInternal(victoryText, imageColor, victoryTextColor);
        }

        public void ShowDefeat()
        {
            ShowInternal(defeatText, imageColor, defeatTextColor);
        }

        private void ShowInternal(string message, Color imgColor, Color txtColor)
        {
            text.text = message;
            text.color = txtColor;
            image.color = imgColor;

            fadeTween?.Kill();

            canvasGroup.alpha = 0f;

            fadeTween = canvasGroup
                .DOFade(VisibleAlpha, duration)
                .SetUpdate(true)
                .SetLink(gameObject)
                .OnComplete(() =>
                {
                    // Автоматическое скрытие
                    canvasGroup
                        .DOFade(0f, duration)
                        .SetDelay(duration)
                        .SetUpdate(true)
                        .SetLink(gameObject);
                });
        }

    }
}
