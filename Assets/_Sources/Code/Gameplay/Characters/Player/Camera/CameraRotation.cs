using Game.Interfaces;
using UnityEngine;

namespace Sources.Controllers
{
    public class CameraRotation
    {
        private float baseSensitivity;
        private float sensitivity;
        private bool rotationBlocked;
        private float maxLookUp;
        private float minLookDown;
        private float smoothTime;

        private ICameraInputProvider inputProvider;

        private float targetX;
        private float targetY;
        private float currentX;
        private float currentY;

        private float velX;
        private float velY;

        public float CurrentYaw => currentX;
        public float CurrentPitch => currentY;
        public bool IsRotationBlocked => rotationBlocked;

        public CameraRotation(float sensitivity, float maxLookUp, float minLookDown, float smoothTime)
        {
            this.baseSensitivity = sensitivity;
            this.sensitivity = sensitivity;
            this.maxLookUp = maxLookUp;
            this.minLookDown = minLookDown;
            this.smoothTime = smoothTime;
        }

        public void SetSensitivityMultiplier(float multiplier)
        {
            sensitivity = baseSensitivity * multiplier;
        }

        public void SetRotationBlocked(bool blocked)
        {
            rotationBlocked = blocked;
        }

        public void ResetSensitivity()
        {
            sensitivity = baseSensitivity;
        }

        public void Init(Transform cam, Transform body)
        {
            currentX = targetX = body.eulerAngles.y;
            currentY = targetY = cam.localEulerAngles.x;
        }

        public void SetInputProvider(ICameraInputProvider provider) =>
            inputProvider = provider;

        public void UpdateRotation(Transform cam, Transform body)
        {
            if (inputProvider == null || rotationBlocked)
                return;

            Vector2 look = inputProvider.GetLookDelta() * sensitivity;

            targetX += look.x;
            targetY = Mathf.Clamp(targetY - look.y, minLookDown, maxLookUp);

            currentX = Mathf.SmoothDamp(currentX, targetX, ref velX, smoothTime);
            currentY = Mathf.SmoothDamp(currentY, targetY, ref velY, smoothTime);

            cam.localRotation = Quaternion.Euler(currentY, 0f, 0f);
            body.rotation = Quaternion.Euler(0f, currentX, 0f);
        }

        public void ForceSetAngles(float yaw, float pitch)
        {
            targetX = currentX = yaw;
            targetY = currentY = Mathf.Clamp(pitch, minLookDown, maxLookUp);
        }
    }
}
