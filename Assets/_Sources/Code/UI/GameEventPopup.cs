using System.Threading;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Sources.Code.Configs;

namespace Sources.Code.UI
{
    public class GameEventPopup : BasePopup
    {
        private const int EnabledEndValue = 1;
        
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private Image _image;
        [SerializeField] private TMP_Text _text;
        
        private CancellationToken _gameToken;
        private GameEventScreenConfig _eventScreenConfig;
        
        private Color _imageColor;
        private float _duration;
        
        private Color _victoryTextColor;
        private string _victoryText;
        
        private Color _defeatTextColor;
        private string _defeatText;
        
        public override void Init()
        {
            _eventScreenConfig = GameEventScreenConfig.Instance;
            
            _duration = _eventScreenConfig.Duration;
            _imageColor = _eventScreenConfig.ImageColor;
            
            _victoryText = _eventScreenConfig.VictoryText;
            _victoryTextColor = _eventScreenConfig.VictoryTextColor;
            
            _defeatText = _eventScreenConfig.DefeatText;
            _defeatTextColor = _eventScreenConfig.DefeatTextColor;
        }

        private Color SetColor(Color color, float fade)
        {
            return new Color(color.r, color.g, color.b, fade);
        }
        
        private void SetScreen(float duration, string text, Color imageColor, Color textColor)
        {
            _text.text = text;
            _text.color = SetColor(textColor, _text.color.a);
            _image.color = imageColor;

            _canvasGroup.DOFade(duration, EnabledEndValue).SetLink(gameObject);
        }

        public void ShowVictory()
        {
            SetScreen(_duration, _victoryText, _imageColor, _victoryTextColor);
        }
        
        public void ShowDefeat()
        { 
            SetScreen(_duration, _defeatText, SetColor(_imageColor, _image.color.a), _defeatTextColor);
        }
    }
}