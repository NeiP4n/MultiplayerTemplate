using UnityEngine;

namespace Sources.Code.UI
{
    public abstract class BaseScreen : MonoBehaviour
    {
        public virtual void Enable()
        {
            gameObject.SetActive(true);
        }
    
        public virtual void Disable()
        {
            gameObject.SetActive(false);
        }
    }
}