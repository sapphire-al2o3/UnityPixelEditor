using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PaletteUI : MonoBehaviour
{
	[SerializeField]
	UnityPixelEditor editor;

	Image[] images;

	private void Start()
	{
		images = new Image[transform.childCount];
		var palette = editor.GetPalette();
		for (int i = 0; i < palette.Length && i < images.Length; i++)
		{
			images[i] = transform.GetChild(i).GetComponent<Image>();
			images[i].color = palette[i];
		}
	}

	public void ChangePalette(int dir)
	{
		var palette = editor.GetPalette();
	}

	void CreatePalette()
	{

	}
}
