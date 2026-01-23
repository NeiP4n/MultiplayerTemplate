using UnityEngine;
using UnityEngine.EventSystems;
using Sources.Code.Audio;

public class UISoundEvents : MonoBehaviour,
    IPointerEnterHandler,
    IPointerClickHandler
{
    public void OnPointerEnter(PointerEventData eventData)
    {
        AudioManager.Play(AudioManager.Cat.uiHover);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        AudioManager.Play(AudioManager.Cat.uiClick);
    }
}
