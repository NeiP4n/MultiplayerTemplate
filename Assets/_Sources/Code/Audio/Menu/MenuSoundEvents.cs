using UnityEngine;
using Sources.Code.Audio;

public class MenuSoundEvents : MonoBehaviour
{
    public void PlayMenuOpen()
    {
        AudioManager.Play(AudioManager.Cat.menuOpen);
    }

    public void PlayMenuClose()
    {
        AudioManager.Play(AudioManager.Cat.menuClose);
    }

    public void PlayMenuStart()
    {
        AudioManager.Play(AudioManager.Cat.menuStart);
    }
}
