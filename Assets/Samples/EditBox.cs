using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EditBox : MonoBehaviour
{
    public static int selectedBox = -1;

    [SerializeField]
    PixelImageData tempImage;

    [SerializeField]
    GameObject[] boxs;

    static Texture2D[] cacheTextures = null;
    static PixelImageData[] cacheImages = null;

    void Awake()
    {
        if (cacheTextures != null)
        {
            cacheTextures = new Texture2D[boxs.Length];
        }

        if (cacheImages != null)
        {
            cacheImages = new PixelImageData[boxs.Length];
            for (int i = 0; i < cacheImages.Length; i++)
            {
                cacheImages[i] = new PixelImageData();
            }
        }

        if (selectedBox >= 0)
        {
            boxs[selectedBox].GetComponent<PixelImageSetter>().Setup(tempImage.indexData, tempImage.paletteData);
        }

        for (int i = 0; i < boxs.Length; i++)
        {
            if (cacheTextures[i] != null)
            {
                boxs[i].GetComponent<Renderer>().material.mainTexture = cacheTextures[i];
            }
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out var hitInfo))
            {
                var go = hitInfo.collider.gameObject;

                for (int i = 0; i < boxs.Length; i++)
                {
                    if (boxs[i] == go)
                    {
                        selectedBox = i;
                        break;
                    }
                }

                SceneManager.LoadScene("PixelEditorScene");
            }
        }
    }
}
