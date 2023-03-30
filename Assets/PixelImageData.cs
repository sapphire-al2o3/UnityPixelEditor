using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

[CreateAssetMenu()]
public class PixelImageData : ScriptableObject
{
    public int width;
    public int height;
    public byte[] indexData;
    public Color32[] paletteData;

    public void Copy(PixelImageData src)
    {
        width = src.width;
        height = src.height;
        if (indexData == null || indexData.Length != src.indexData.Length)
        {
            indexData = new byte[src.indexData.Length];
        }

        Array.Copy(src.indexData, indexData, indexData.Length);

        if (paletteData == null || paletteData.Length != src.paletteData.Length)
        {
            paletteData = new Color32[src.paletteData.Length];
        }

        Array.Copy(src.paletteData, paletteData, paletteData.Length);
    }
}
