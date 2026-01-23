using UnityEngine;
using Sources.Code.Interfaces;
using Sources.Code.Gameplay.Grab;

namespace Sources.Code.Gameplay.Interaction
{
    public class GrabInteractable : MonoBehaviour, IInteractable, IInteractableContext
    {
        private GrabInteractible grabTarget;

        private void Awake()
        {
            grabTarget = GetComponent<GrabInteractible>();
        }

        public bool CanInteract => grabTarget != null;

        public void Interact() { }

        public void Interact(PlayerInteract playerInteract)
        {
            var grabber = playerInteract.GetComponent<GrabInteractor>();
            if (grabber != null)
                grabber.Grab(grabTarget);
        }
    }
}
