using UnityEngine;

[System.Serializable]
public struct AcidPreset
{
    public Texture2D mainTexture;
    public Vector2   tiling;
    public Vector2   offset;
    
    public Color color1;
    public Color color2;
    public Color color3;
    public Color color4;
    
    public float gradientAngle;
    public float colorBands;
    public float gradientScale;
    
    public float pixelSize;
    public float ditherScale;
    public float ditherMix;
    
    public float edgeGlow;
    public float patternScale;
    
    public float colorCount;
    public int   colorMode;
    public int   pattern;
    public float useLocalCoordinates;
    public float useLighting;
    
    [System.NonSerialized] public int hash;
    
    public void CalculateHash()
    {
        unchecked
        {
            hash = 17;
            hash = hash * 31 + (mainTexture != null ? mainTexture.GetInstanceID() : 0);
            hash = hash * 31 + tiling.GetHashCode();
            hash = hash * 31 + offset.GetHashCode();
            hash = hash * 31 + color1.GetHashCode();
            hash = hash * 31 + color2.GetHashCode();
            hash = hash * 31 + color3.GetHashCode();
            hash = hash * 31 + color4.GetHashCode();
            hash = hash * 31 + (int)(gradientAngle   * 100f);
            hash = hash * 31 + (int)(colorBands      * 100f);
            hash = hash * 31 + (int)(gradientScale   * 100f);
            hash = hash * 31 + (int)(pixelSize       * 100f);
            hash = hash * 31 + (int)(ditherScale     * 100f);
            hash = hash * 31 + (int)(ditherMix       * 100f);
            hash = hash * 31 + (int)(edgeGlow        * 100f);
            hash = hash * 31 + (int)(patternScale    * 100f);
            hash = hash * 31 + (int)colorCount;
            hash = hash * 31 + colorMode;
            hash = hash * 31 + pattern;
            hash = hash * 31 + (int)useLocalCoordinates;
            hash = hash * 31 + (int)useLighting;
        }
    }
}
