namespace Sources.Code.Interfaces
{
    public interface IInputManager
    {
        float Horizontal { get; }
        float Vertical { get; }

        bool SprintPressed { get; }
        bool RagdollPressed { get; }

        bool IsLocked { get; }
        void Lock();
        void Unlock();

        bool ConsumeJump();
        bool ConsumeCrouch();
        bool ConsumeInteract();
        bool ConsumeThrow();
        bool ConsumeLeftClick();
        bool ConsumeRightClick();
        bool ConsumeCancel();
        bool ConsumeDrop();
        bool ConsumeActivate();

        int SlotIndexPressed { get; }
        float ScrollDelta { get; }
    }
}
