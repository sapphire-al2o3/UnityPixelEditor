using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransition : MonoBehaviour
{
    [SerializeField]
    string scene;

    [SerializeField]
    PixelImageData image;

    [SerializeField]
    UnityPixelEditor editor;

    void Awake()
    {
        if (image.width == 16 && image.height == 16)
        {
            editor.SetImage(image.indexData, image.paletteData);
        }
    }
    public void Run()
    {
        image.width = 16;
        image.height = 16;
        image.indexData = editor.GetIndex();
        image.paletteData = editor.GetPalette();
        SceneManager.LoadScene(scene);
    }
}
