using UnityEngine;
using Sources.Code.Interfaces;
using Sources.Code.Audio;

namespace Gameplay.Interaction
{
    public class Door : MonoBehaviour, IInteractable
    {
        [SerializeField] private Animator _animator;
        private bool _isOpen;

        public bool CanInteract => true;

        private void Awake()
        {
            if (_animator == null)
                _animator = GetComponentInChildren<Animator>();
        }

        public void Interact()
        {
            _isOpen = !_isOpen;

            if (_animator != null)
                _animator.SetBool("IsOpen", _isOpen);

            if (_isOpen)
                AudioManager.Play(AudioManager.Cat.doorOpen);
            else
                AudioManager.Play(AudioManager.Cat.doorClose);
        }
        public void Open()
        {
            if (_isOpen) return;
            _isOpen = true;
            _animator.SetBool("IsOpen", true);
        }

        public void Close()
        {
            if (!_isOpen) return;
            _isOpen = false;
            _animator.SetBool("IsOpen", false);
        }
    }
}
