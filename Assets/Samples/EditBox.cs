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
        if (cacheTextures == null)
        {
            cacheTextures = new Texture2D[boxs.Length];
        }

        if (cacheImages == null)
        {
            cacheImages = new PixelImageData[boxs.Length];
            for (int i = 0; i < cacheImages.Length; i++)
            {
                cacheImages[i] = ScriptableObject.CreateInstance<PixelImageData>();
            }
        }

        if (selectedBox >= 0)
        {
            cacheImages[selectedBox] = tempImage;
            if (cacheTextures[selectedBox] == null)
            {
                cacheTextures[selectedBox] = UnityPixelEditor.CreateTexture(tempImage.width, tempImage.height, tempImage.indexData, tempImage.paletteData);
            }
            else
            {
                UnityPixelEditor.UpdateTexture(tempImage.width, tempImage.height, tempImage.indexData, tempImage.paletteData, cacheTextures[selectedBox]);
            }
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

                var image = cacheImages[selectedBox];
                //System.Array.Copy(image.indexData, tempImage.indexData, image.indexData.Length);
                SceneManager.LoadScene("PixelEditorScene");
            }
        }
    }
}
