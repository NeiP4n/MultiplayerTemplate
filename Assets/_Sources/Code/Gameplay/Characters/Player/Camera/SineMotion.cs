using UnityEngine;

namespace Sources.Controllers
{
    [System.Serializable]
    public class SineMotion
    {
        public float GetSine(float time, float frequency)
        {
            return Mathf.Sin(time * frequency);
        }
    }
}
