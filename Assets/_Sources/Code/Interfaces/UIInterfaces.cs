using System;
using Sources.Code.Interfaces;
namespace Game.Interfaces
{
    public interface IUIInteract
    {
        void Init(IInteractable interactable);
        void ShowTextMessage();
        void HideTextMessage();
    }
    public interface IUIHealth
    {
        event Action<float> OnHealthChanged;
        void TakeDamage(float amount);
    }
    public interface IPlayerStaminaUI
    {
        void Init(IPlayerStamina playerStamina);
    }
}