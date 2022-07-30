using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestPaint : MonoBehaviour
{
    [SerializeField]
    UnityPixelEditor pixelEditor;


    void Start()
    {
        GetComponent<Renderer>().material.mainTexture = pixelEditor.GetTexture();
    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(0, Time.deltaTime * 10.0f, 0.0f);
    }
}
