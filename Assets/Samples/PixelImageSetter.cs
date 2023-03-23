using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PixelImageSetter : MonoBehaviour
{
    byte[] indexData;
    Color32[] paletteData;
    Texture2D _tex;

    void Start()
    {
        if (indexData != null)
        {
            if (_tex != null)
            {
                Destroy(_tex);
            }
            _tex = UnityPixelEditor.CreateTexture(32, 32, indexData, paletteData);

            GetComponent<Renderer>().material.mainTexture = _tex;
        }
    }
}
