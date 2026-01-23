using DG.Tweening;
using Sources.Code.Gameplay.Characters;
using Sources.Code.Gameplay.Inventory;
using Sources.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Sources.Code.UI
{
    public class GameScreen : BaseScreen
    {
        [SerializeField] Image _image;
        [SerializeField] UIInteract _uiInteract;
        [SerializeField] ScreenInventory _inventoryUI;

        Tween _fadeTween;

        public void Init(PlayerCharacter player)
        {
            _fadeTween?.Kill();

            var c = _image.color;
            c.a = 1f;
            _image.color = c;

            if (_inventoryUI != null && player != null)
                _inventoryUI.SetInventory(player.Inventory);

            FadeOut(3f);
        }

        public void SetBlackInstant()
        {
            _fadeTween?.Kill();

            var c = _image.color;
            c.a = 1f;
            _image.color = c;
        }

        public Tween FadeOut(float duration, Ease ease = Ease.OutCubic)
        {
            _fadeTween?.Kill();
            return _fadeTween = _image.DOFade(0f, duration).SetEase(ease);
        }

        public Tween FadeIn(float duration, Ease ease = Ease.InCubic)
        {
            _fadeTween?.Kill();
            return _fadeTween = _image.DOFade(1f, duration).SetEase(ease);
        }

        public UIInteract GetUIInteract()
        {
            return _uiInteract;
        }
    }
}
