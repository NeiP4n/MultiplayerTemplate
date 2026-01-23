using System;
using Sources.Managers;
using UnityEngine;

namespace Sources.Code.UI
{
    public abstract class BasePopup : MonoBehaviour
    {
        public event Action<BasePopup> Closed;

        [SerializeField] protected CanvasGroup canvasGroup;
        [SerializeField] protected bool pauseGame = true;
        [SerializeField] protected bool closeOnEsc = true;

        private float cachedTimeScale;
        private bool isOpen;

        public virtual void Init()
        {
            if (canvasGroup == null)
                canvasGroup = GetComponentInChildren<CanvasGroup>(true);

            HideInstant();
        }

        public virtual void Open()
        {
            if (isOpen) return;

            isOpen = true;

            if (pauseGame)
            {
                cachedTimeScale = Time.timeScale;
                Time.timeScale = 0f;
            }

            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }

        public virtual void Close()
        {
            if (!isOpen) return;

            isOpen = false;

            if (pauseGame)
                Time.timeScale = cachedTimeScale;

            HideInstant();
            Closed?.Invoke(this);
        }

        protected virtual void Update()
        {
            if (!isOpen || !closeOnEsc)
                return;

            if (InputManager.Instance.ConsumeCancel())
                Close();
        }


        protected void HideInstant()
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
    }
}
