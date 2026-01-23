using Game.Interfaces;
using UnityEngine;

namespace Sources.Controllers
{
    public class CameraFollow
    {
        private Transform headBone;
        private Vector3 offset;
        private float amplitude;
        private float frequency;

        private float walkTime;
        private float currentY;
        private float yVelocity;

        private ICameraInputProvider inputProvider;

        public CameraFollow(
            Transform headBone,
            Vector3 offset,
            float amplitude,
            float frequency)
        {
            this.headBone  = headBone;
            this.offset    = offset;
            this.amplitude = amplitude;
            this.frequency = frequency;
        }

        public void SetInputProvider(ICameraInputProvider provider)
        {
            inputProvider = provider;
        }

        public void UpdateCameraPosition(Transform camTransform)
        {
            if (headBone == null || inputProvider == null)
                return;

            Vector3 basePosition = headBone.position + offset;

            bool isMoving = inputProvider.GetMoveInput().sqrMagnitude > 0.01f;

            float targetY = 0f;

            if (isMoving)
            {
                walkTime += Time.deltaTime;
                targetY = Mathf.Sin(walkTime * frequency) * amplitude;
            }
            else
            {
                walkTime = 0f;
            }

            currentY = Mathf.SmoothDamp(
                currentY,
                targetY,
                ref yVelocity,
                0.08f
            );

            camTransform.position = basePosition + Vector3.up * currentY;
        }
    }
}
