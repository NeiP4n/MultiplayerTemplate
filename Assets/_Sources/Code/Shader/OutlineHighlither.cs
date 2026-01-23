using UnityEngine;

public class OutlineObject : MonoBehaviour
{
    [SerializeField] private Material outlineMaterial;

    private Renderer[] renderers;
    private Material[][] originalMaterials;

    private void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>();
        originalMaterials = new Material[renderers.Length][];

        for (int i = 0; i < renderers.Length; i++)
            originalMaterials[i] = renderers[i].materials;
    }

    public void EnableOutline()
    {
        for (int i = 0; i < renderers.Length; i++)
        {
            var mats = renderers[i].materials;
            var newMats = new Material[mats.Length + 1];
            mats.CopyTo(newMats, 0);
            newMats[newMats.Length - 1] = outlineMaterial;
            renderers[i].materials = newMats;
        }
    }

    public void DisableOutline()
    {
        for (int i = 0; i < renderers.Length; i++)
            renderers[i].materials = originalMaterials[i];
    }
}
