using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PaletteUI : MonoBehaviour
{
	[SerializeField]
	UnityPixelEditor editor;

	Image[] images;

	int index = 0;
	List<Color32> colorSet = new List<Color32>();

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
		index = (index + dir + colorSet.Count) % colorSet.Count;
		var palette = editor.GetPalette();
		palette[0] = colorSet[index];

		for (int i = 0; i < palette.Length && i < images.Length; i++)
		{
			images[i] = transform.GetChild(i).GetComponent<Image>();
			images[i].color = palette[i];
		}
		editor.Refresh();
	}

	void CreatePalette()
	{

	}
}
