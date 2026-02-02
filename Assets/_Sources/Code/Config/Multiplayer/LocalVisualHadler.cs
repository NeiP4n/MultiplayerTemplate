using PurrNet;
using UnityEngine;

[RequireComponent(typeof(NetworkIdentity))]
public sealed class LocalVisualHider : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField] private GameObject[] objectsToHide;

    [Header("Settings")]
    [SerializeField] private bool hideForOwner = true;

    private NetworkIdentity identity;
    private bool applied;

    private void Awake()
    {
        identity = GetComponent<NetworkIdentity>();
    }

    private void Start()
    {
        TryApply();
    }

    private void Update()
    {
        if (!applied && identity != null && identity.isSpawned)
        {
            TryApply();
        }
    }

    public void SetHidden(bool value)
    {
        hideForOwner = value;
        applied = false;
        TryApply();
    }

    public bool IsHidden => hideForOwner;

    private void TryApply()
    {
        if (identity == null)
            return;

        if (!identity.isOwner)
            return;

        applied = true;
        ApplyState(hideForOwner);
    }

    private void ApplyState(bool hideState)
    {
        foreach (var obj in objectsToHide)
        {
            if (obj == null)
                continue;

            var renderers = obj.GetComponentsInChildren<Renderer>(true);

            foreach (var r in renderers)
                r.enabled = !hideState;
        }
    }
}
