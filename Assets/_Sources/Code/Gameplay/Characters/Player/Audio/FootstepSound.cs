using UnityEngine;
using Sources.Code.Audio;
using Sources.Code.Config.Audio;

public class FootstepSound : MonoBehaviour
{
    [Header("Step Settings")]
    [SerializeField] private float stepInterval = 0.45f;
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private float rayDistance = 2f;

    private float timer;

    private void Update()
    {
        if (!IsMoving())
            return;

        timer -= Time.deltaTime;
        if (timer > 0f)
            return;

        timer = stepInterval;
        PlayStep();
    }

    private void PlayStep()
    {
        if (!Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, rayDistance, groundMask))
        {
            AudioManager.Play(AudioManager.Cat.stepDefault);
            return;
        }

        var surface = hit.collider.GetComponent<SurfaceIdentifier>();
        if (surface == null)
        {
            AudioManager.Play(AudioManager.Cat.stepDefault);
            return;
        }

        SoundData sound = surface.surfaceType switch
        {
            SurfaceType.Wood => AudioManager.Cat.stepWood,
            SurfaceType.Metal => AudioManager.Cat.stepMetal,
            SurfaceType.Snow => AudioManager.Cat.stepSnow,
            _ => AudioManager.Cat.stepDefault
        };


        AudioManager.Play(sound);
    }

    private bool IsMoving()
    {
        return Input.GetAxisRaw("Horizontal") != 0f ||
               Input.GetAxisRaw("Vertical") != 0f;
    }
}
