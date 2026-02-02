using Sources.Characters;
using Sources.Controllers;
using Sources.Code.Utils;
using Game.Gameplay.Characters;
using Sources.Code.Interfaces;
using Sources.Code.Gameplay.Interaction;
using Sources.Code.Gameplay.Grab;
using Sources.Code.Gameplay.Inventory;
using UnityEngine;
using UnityEngine.InputSystem;
using PurrNet;
using TriInspector;


namespace Sources.Code.Gameplay.Characters
{
    [DeclareBoxGroup("References")]
    [DeclareBoxGroup("Components", Title = "Runtime Components")]
    public class PlayerCharacter : Entity
    {
        [Group("References")]
        [SerializeField, Required] private Transform handSocket;


        [Group("Components"), ReadOnly, ShowInInspector]
        private GroundMover _mover;
        
        [Group("Components"), ReadOnly, ShowInInspector]
        private PlayerInteract _interact;
        
        [Group("Components"), ReadOnly, ShowInInspector]
        private CameraController _camera;
        
        [Group("Components"), ReadOnly, ShowInInspector]
        private InventorySystem _inventory;
        
        [Group("Components"), ReadOnly, ShowInInspector]
        private GrabInteractor _grabInteractor;


        [Group("Components"), ReadOnly, ShowInInspector]
        private NetworkIdentity _networkIdentity;


        private IInputManager _input;


        public PlayerInteract Interact => _interact;
        public InventorySystem Inventory => _inventory;
        public Transform HandSocket
        {
            get => handSocket;
            set => handSocket = value;
        }


        public bool IsLocalPlayer => _networkIdentity == null || _networkIdentity.isOwner;


        private void Awake()
        {
            _networkIdentity = GetComponent<NetworkIdentity>();


            _mover = GetComponent<GroundMover>();
            _interact = GetComponentInChildren<PlayerInteract>();
            _camera = GetComponentInChildren<CameraController>();
            _inventory = GetComponentInChildren<InventorySystem>();
            _grabInteractor = GetComponentInChildren<GrabInteractor>();


            if (_inventory != null && handSocket != null)
                _inventory.HandSocket = handSocket;
            Debug.Log($"[PlayerCharacter][Awake] {name}");

        }


        public void Construct(IInputManager input)
        {
            LoggerDebug.LogGameplay($"[PlayerCharacter] Construct START - input: {input != null}");
            
            var netIdentity = GetComponent<NetworkIdentity>();
            bool isLocal = netIdentity != null && netIdentity.isOwner;
            
            LoggerDebug.LogNetwork($"[PlayerCharacter] NetIdentity: {netIdentity != null}, isOwner: {netIdentity?.isOwner}, IsLocal: {isLocal}");


            if (!isLocal)
            {
                LoggerDebug.LogGameplay($"[PlayerCharacter] Skipping Construct - not local player");
                return;
            }


            _input = input;


            LoggerDebug.LogGameplay($"[PlayerCharacter] Constructing local player");


            if (_inventory != null)
            {
                _inventory.Construct(input);
                LoggerDebug.LogInventory($"[PlayerCharacter] Inventory constructed");
            }
            
            if (_mover != null)
            {
                _mover.Construct(input, _inventory);
                LoggerDebug.LogGameplay($"[PlayerCharacter] Mover constructed - input passed: {input != null}");
            }
            else
            {
                LoggerDebug.LogGameplayError("[PlayerCharacter] Mover is NULL!");
            }
            
            if (_camera != null)
                _camera.Construct(input);
                
            if (_interact != null)
                _interact.Construct(input);
                
            if (_grabInteractor != null)
                _grabInteractor.Construct(input);


            LockCursor();

            LoggerDebug.LogGameplay($"[PlayerCharacter] Construct COMPLETE");
        }


        private void Update()
        {

            if (!IsLocalPlayer)
                return;

            if (_input == null || _input.IsLocked)
                return;

            HandleCursorToggle();
            _interact?.UpdateInteract();
        }



        private void LockCursor()
        {
            if (!IsLocalPlayer)
                return;


            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            LoggerDebug.LogUI("[PlayerCharacter] Cursor locked");
        }


        private void HandleCursorToggle()
        {
            if (Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                if (Cursor.lockState == CursorLockMode.Locked)
                {
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                    LoggerDebug.LogUI("[PlayerCharacter] Cursor unlocked");
                }
                else
                {
                    LockCursor();
                }
            }


            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame && Cursor.lockState != CursorLockMode.Locked)
            {
                LockCursor();
            }
        }


        [Button("Debug: Show Components"), Group("Components")]
        private void DebugShowComponents()
        {
            LoggerDebug.LogGameplay($"[PlayerCharacter] Components:" +
                      $"\n- Mover: {_mover != null}" +
                      $"\n- Interact: {_interact != null}" +
                      $"\n- Camera: {_camera != null}" +
                      $"\n- Inventory: {_inventory != null}" +
                      $"\n- Grab: {_grabInteractor != null}" +
                      $"\n- IsLocal: {IsLocalPlayer}");
        }
    }
}
