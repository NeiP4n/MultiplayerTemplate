using UnityEngine;
using Game.Interfaces;
using Sources.Code.Interfaces;
using Sources.Code.Utils;
using PurrNet;
using TriInspector;

namespace Sources.Controllers
{
    [DeclareBoxGroup("References")]
    [DeclareBoxGroup("Follow")]
    [DeclareBoxGroup("Rotation")]
    public class CameraController : MonoBehaviour
    {
        [Group("References")]
        [SerializeField, Required] private Transform headBone;
        
        [Group("References")]
        [SerializeField, Required] private Transform bodyTransform;
        
        [Group("References")]
        [SerializeField, Required] private Camera cam;
        
        [Group("References")]
        [SerializeField, Required] private SineMotion sineMotion;

        [Group("Follow")]
        [SerializeField] private Vector3 offset = new(0f, 0.2f, 0f);
        
        [Group("Follow")]
        [SerializeField, Range(0f, 0.2f)] private float amplitude = 0.05f;
        
        [Group("Follow")]
        [SerializeField, Range(1f, 20f)] private float frequency = 7f;

        [Group("Rotation")]
        [SerializeField, Range(0.1f, 10f)] private float mouseSensitivity = 2f;
        
        [Group("Rotation")]
        [SerializeField, Range(0f, 90f)] private float maxLookUp = 80f;
        
        [Group("Rotation")]
        [SerializeField, Range(-90f, 0f)] private float minLookDown = -80f;
        
        [Group("Rotation")]
        [SerializeField, Range(0.01f, 0.5f)] private float rotationSmoothTime = 0.05f;

        private IInputManager input;
        private ICameraInputProvider inputProvider;
        private CameraFollow follow;
        private CameraRotation rotation;
        private NetworkIdentity networkIdentity;
        private AudioListener audioListener;

        private bool shakeEnabled;
        private float shakeIntensity;
        private float shakeTime;
        private float baseFov;
        private Vector3 baseLocalPos;

        public Camera Camera => cam;
        public Transform BodyTransform => bodyTransform;

        public float GetYaw() => rotation?.CurrentYaw ?? bodyTransform.eulerAngles.y;
        public float GetPitch() => rotation?.CurrentPitch ?? cam.transform.localEulerAngles.x;

        private void Awake()
        {
            networkIdentity = GetComponentInParent<NetworkIdentity>();
            
            if (cam != null)
                audioListener = cam.GetComponent<AudioListener>();

            if (networkIdentity == null)
            {
                LoggerDebug.LogGameplayError("[CameraController] NetworkIdentity not found");
                enabled = false;
                return;
            }
            
            if (cam != null)
            {
                baseFov = cam.fieldOfView;
                baseLocalPos = cam.transform.localPosition;
            }
            
            if (cam != null)
                cam.enabled = false;
            
            if (audioListener != null)
                audioListener.enabled = false;
        }

        public void Construct(IInputManager inputManager)
        {
            if (inputManager == null)
            {
                LoggerDebug.LogGameplayError("[CameraController] Cannot construct without InputManager");
                return;
            }

            if (networkIdentity != null && !networkIdentity.isOwner)
            {
                LoggerDebug.LogGameplay("[CameraController] Not owner, staying disabled");
                enabled = false;
                return;
            }

            enabled = true;
            
            this.input = inputManager;
            inputProvider = new MouseInputProvider(inputManager);

            follow = new CameraFollow(headBone, offset, amplitude, frequency);
            follow.SetInputProvider(inputProvider);

            rotation = new CameraRotation(mouseSensitivity, maxLookUp, minLookDown, rotationSmoothTime);
            rotation.Init(cam.transform, bodyTransform);
            rotation.SetInputProvider(inputProvider);

            if (cam != null)
                cam.enabled = true;
            
            if (audioListener != null)
                audioListener.enabled = true;

            LoggerDebug.LogGameplay("[CameraController] Constructed - Camera and AudioListener enabled");
        }

        public void Apply(CameraSettings settings)
        {
            if (!settings.overrideCamera)
                return;

            rotation?.SetRotationBlocked(settings.blockRotation);
            rotation?.SetSensitivityMultiplier(settings.sensitivityMultiplier);

            if (settings.overrideFov)
                cam.fieldOfView = settings.fov;

            shakeEnabled = settings.cameraShake;
            shakeIntensity = settings.shakeIntensity;
        }

        public void Restore()
        {
            rotation?.SetRotationBlocked(false);
            rotation?.ResetSensitivity();

            cam.fieldOfView = baseFov;
            cam.transform.localPosition = baseLocalPos;

            shakeEnabled = false;
            shakeIntensity = 0f;
            shakeTime = 0f;
        }

        private void LateUpdate()
        {
            if (!IsOwner)
            {
                LoggerDebug.LogGameplayWarning("[CameraController] LateUpdate called but not owner!");
                return;
            }

            follow?.UpdateCameraPosition(cam.transform);
            
            if (rotation != null && inputProvider != null)
            {
                Vector2 lookDelta = inputProvider.GetLookDelta();
                LoggerDebug.LogGameplay($"[CameraController] LookDelta: {lookDelta}, CursorLock: {Cursor.lockState}");
                
                rotation.UpdateRotation(cam.transform, bodyTransform);
            }

            ApplyShake();
        }

        private void ApplyShake()
        {
            if (!shakeEnabled || sineMotion == null)
                return;

            shakeTime += Time.deltaTime;

            float shake = sineMotion.GetSine(shakeTime, 25f) * shakeIntensity * 0.02f;
            cam.transform.localPosition = baseLocalPos + Vector3.up * shake;
        }

        public void SetRotationFromSave(float yaw, float pitch)
        {
            bodyTransform.rotation = Quaternion.Euler(0f, yaw, 0f);
            cam.transform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
            rotation?.ForceSetAngles(yaw, pitch);
        }
        
        private bool IsOwner => networkIdentity != null && networkIdentity.isOwner;
    }
}
