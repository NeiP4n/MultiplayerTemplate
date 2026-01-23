using UnityEngine;
using Sources.Code.Audio;

public class WorldSoundEvents : MonoBehaviour
{
    public void PlayDoorOpen()
    {
        AudioManager.Play(AudioManager.Cat.doorOpen);
    }

    public void PlayDoorClose()
    {
        AudioManager.Play(AudioManager.Cat.doorClose);
    }

    public void PlayLeverPull()
    {
        AudioManager.Play(AudioManager.Cat.leverPull);
    }
}
