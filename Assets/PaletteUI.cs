using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PaletteUI : MonoBehaviour
{
	[SerializeField]
	UnityPixelEditor editor;

	Image[] images;

	[System.Serializable]
	struct ColorSet
	{
		public Color32 backColor;
		public Color32 mainColor;
		public Color32 secondColor;
	}

	int index = 0;
	List<Color32> colorSet = new List<Color32>();

	[SerializeField]
	ColorSet[] colorSetList;

	private void Start()
	{
		images = new Image[transform.childCount];
		var palette = editor.GetPalette();
		for (int i = 0; i < palette.Length && i < images.Length; i++)
		{
			images[i] = transform.GetChild(i).GetComponent<Image>();
			images[i].color = palette[i];
		}

		colorSet.Add(new Color32(0, 0xFF, 0, 0xFF));
		colorSet.Add(new Color32(0xFF, 0, 0xFF, 0xFF));
		colorSet.Add(new Color32(0, 0, 0xFF, 0xFF));
	}

	public void ChangePalette(int dir)
	{
		index = (index + dir + colorSetList.Length) % colorSetList.Length;
		var palette = editor.GetPalette();
		CreatePalette(colorSetList[index], palette);

		for (int i = 0; i < palette.Length && i < images.Length; i++)
		{
			images[i] = transform.GetChild(i).GetComponent<Image>();
			images[i].color = palette[i];
		}
		editor.Refresh();
	}

	void CreatePalette(in ColorSet colorSet, Color32[] palette)
	{
		palette[0] = colorSet.backColor;
		for (int i = 0; i < 4; i++)
		{
			palette[i + 1] = Color32.Lerp(colorSet.backColor, colorSet.mainColor, (i + 1) / 4.0f);
		}

		for (int i = 0; i < 3; i++)
		{
			palette[i + 5] = Color32.Lerp(colorSet.mainColor, colorSet.secondColor, (i + 1) / 3.0f);
		}
	}
}
