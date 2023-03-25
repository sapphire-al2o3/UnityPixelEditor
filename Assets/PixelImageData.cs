using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class PixelImageData : ScriptableObject
{
    public int width;
    public int height;
    public byte[] indexData;
    public Color32[] paletteData;
}
