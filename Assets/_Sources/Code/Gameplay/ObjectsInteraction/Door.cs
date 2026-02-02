using UnityEngine;

namespace Sources.Code.Gameplay.ObjectsInteraction
{
    public sealed class Door : MonoBehaviour
    {
        [SerializeField] private Animator animator;

        public bool IsOpen { get; private set; }

        public void ApplyState(bool open)
        {
            if (IsOpen == open)
                return;

            IsOpen = open;

            if (animator != null)
                animator.SetBool("IsOpen", open);
        }
    }
}
