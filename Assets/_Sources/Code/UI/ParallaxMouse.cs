using UnityEngine;

public class ParallaxMouse : MonoBehaviour
{
    [System.Serializable]
    public class ParallaxTarget
    {
        public Transform transform;
        public float strength = 0.5f;
        public float smooth = 5f;
    }

    [SerializeField] private ParallaxTarget[] targets;

    private Vector3[] startPositions;

    private void Awake()
    {
        startPositions = new Vector3[targets.Length];
        for (int i = 0; i < targets.Length; i++)
            startPositions[i] = targets[i].transform.localPosition;
    }

    private void Update()
    {
        Vector2 mousePos = Input.mousePosition;

        float x = (mousePos.x / Screen.width) * 2f - 1f;
        float y = (mousePos.y / Screen.height) * 2f - 1f;

        Vector3 mouseDir = new Vector3(x, y, 0f);

        for (int i = 0; i < targets.Length; i++)
        {
            var t = targets[i];
            Vector3 targetPos = startPositions[i] + mouseDir * t.strength;
            t.transform.localPosition = Vector3.Lerp(
                t.transform.localPosition,
                targetPos,
                Time.deltaTime * t.smooth
            );
        }
    }
}
