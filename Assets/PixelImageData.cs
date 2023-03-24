using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class PixelImageData : ScriptableObject
{
    int width;
    int height;
    public byte[] indexData;
    public Color32[] paletteData;
}
