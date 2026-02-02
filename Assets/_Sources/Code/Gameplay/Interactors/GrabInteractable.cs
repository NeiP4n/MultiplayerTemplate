
using UnityEngine;
using Sources.Code.Interfaces;
using Sources.Code.Gameplay.Grab;
using Sources.Code.Gameplay.Characters;
using Sources.Code.Multiplayer;
using PurrNet;

namespace Sources.Code.Gameplay.Interaction
{
    public class GrabInteractable : MonoBehaviour, IInteractable, IInteractableContext
    {
        private GrabInteractible grabTarget;

        private void Awake()
        {
            grabTarget = GetComponent<GrabInteractible>();
        }

        public bool CanInteract
        {
            get
            {
                if (grabTarget == null)
                    return false;

                if (!grabTarget.IsLocked)
                    return true;

                var settings = ServerSettings.Instance;
                if (settings == null)
                    return false;

                return settings.allowStealingFromHands.value || grabTarget.CanStealFromHands;
            }
        }


        public void Interact() { }

        public void Interact(PlayerInteract playerInteract)
        {
            if (grabTarget == null)
                return;

            var character = playerInteract?.GetComponentInParent<PlayerCharacter>();
            if (character == null || !character.IsLocalPlayer)
                return;

            var grabber = character.GetComponentInChildren<GrabInteractor>();
            if (grabber == null)
                return;

            var playerIdentity = character.GetComponent<NetworkIdentity>();
            if (playerIdentity == null)
                return;

            if (grabber.HasItem &&
                grabTarget.holderGuid.value != playerIdentity.owner.ToString())
                return;

            grabber.Grab(grabTarget);
        }

    }
}

