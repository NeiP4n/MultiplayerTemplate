using System;
using Sources.Code.Interfaces;
using TriInspector;
using UnityEngine;

namespace Sources.Code.UI
{
    [DeclareBoxGroup("Settings", Title = "Popup Settings")]
    [DeclareBoxGroup("Runtime", Title = "Runtime (Debug)")]
    public abstract class BasePopup : MonoBehaviour
    {
        public event Action<BasePopup> Closed;

        // =============================
        // Settings
        // =============================

        [Group("Settings"), Required]
        [SerializeField] protected CanvasGroup canvasGroup;

        [Group("Settings")]
        [SerializeField] private bool pauseGame = true;

        [Group("Settings")]
        [SerializeField] private bool closeOnEsc = true;

        // =============================
        // Runtime
        // =============================

        [Group("Runtime"), ShowInInspector, ReadOnly]
        protected bool isOpen;

        protected IInputManager input;
        private float cachedTimeScale;

        // =============================
        // Init
        // =============================

        public virtual void Construct(IInputManager inputManager)
        {
            input = inputManager;
        }

        public virtual void Init()
        {
            if (canvasGroup == null)
                canvasGroup = GetComponentInChildren<CanvasGroup>(true);

            HideInstant();
        }

        // =============================
        // Open / Close
        // =============================

        public virtual bool CanOpen => true;

        public virtual void Open()
        {
            if (isOpen || !CanOpen)
                return;

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
            if (!isOpen)
                return;

            isOpen = false;

            if (pauseGame)
                Time.timeScale = cachedTimeScale;

            HideInstant();
            Closed?.Invoke(this);
        }

        // =============================
        // Update
        // =============================

        protected virtual void Update()
        {
            if (!isOpen || !closeOnEsc)
                return;

            if (input != null && input.ConsumeCancel())
                Close();
        }

        // =============================
        // Helpers
        // =============================

        protected void HideInstant()
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
    }
}
