using UnityEngine;

public class AcidObject : MonoBehaviour
{
    [Header("Preset (SO)")]
    public AcidPresetAsset presetAsset;

    [Header("Source")]
    public bool useMaterialOverride = false;   // true = берем все из материала, false = из SO

    [Header("Runtime Update")]
    public bool updateEveryFrame = false;
    
    [Header("Debug")]
    public bool logApply = false;
    
    Renderer _rend;
    MaterialPropertyBlock _propBlock;
    int _lastHash;
    
    void Awake()
    {
        _rend = GetComponent<Renderer>();
        _propBlock = new MaterialPropertyBlock();
    }
    
    void OnEnable()
    {
        ApplyPreset();
    }
    
    void Start()
    {
        ApplyPreset();
    }
    
    void Update()
    {
        if (updateEveryFrame)
            ApplyPreset();
    }
    
    public void ApplyPreset()
    {
        if (_rend == null) _rend = GetComponent<Renderer>();
        if (_rend == null) return;

        if (useMaterialOverride)
        {
            // чистим PropertyBlock, чтобы НИЧЕГО не переопределять – все значения идут из материала
            _rend.SetPropertyBlock(null);

            if (logApply)
            {
                Debug.Log("[AcidObject] Using MATERIAL values, SO ignored", this);
            }
            return;
        }

        if (presetAsset == null) return;
        if (_propBlock == null) _propBlock = new MaterialPropertyBlock();
        
        AcidPreset preset = presetAsset.ToPreset();
        preset.CalculateHash();
        
        if (_lastHash == preset.hash && !updateEveryFrame)
            return;
        
        _rend.GetPropertyBlock(_propBlock);
        AcidMaterialManager.ApplyPresetToBlock(ref preset, _propBlock);
        _rend.SetPropertyBlock(_propBlock);
        
        if (logApply)
        {
            Debug.Log("[AcidObject] Using SCRIPTABLE OBJECT values", this);
        }
        
#if UNITY_EDITOR
        if (!Application.isPlaying)
            UnityEditor.EditorUtility.SetDirty(_rend);
#endif
        
        _lastHash = preset.hash;
    }
    
    public void ForceApply()
    {
        _lastHash = 0;
        ApplyPreset();
    }
    
#if UNITY_EDITOR
    void OnValidate()
    {
        UnityEditor.EditorApplication.delayCall += () =>
        {
            if (this != null)
            {
                _lastHash = 0;
                ApplyPreset();
            }
        };
    }
#endif
}
