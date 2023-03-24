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

    void Awake()
    {
        if (selectedBox >= 0)
        {
            boxs[selectedBox].GetComponent<PixelImageSetter>();
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
