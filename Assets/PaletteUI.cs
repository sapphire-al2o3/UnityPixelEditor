using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PaletteUI : MonoBehaviour
{
	[SerializeField]
	UnityPixelEditor editor;

	[SerializeField]
	ColorPickerUI picker;

	[SerializeField]
	Sprite normalSprite;

	[SerializeField]
	Sprite selectedSprite;

	List<Image> images;

	[System.Serializable]
	struct ColorSet
	{
		public Color32 backColor;
		public Color32 mainColor;
		public Color32 secondColor;
	}

	int index = 0;
	List<Color32[]> paletteList = new List<Color32[]>();

	[SerializeField]
	ColorSet[] colorSetList;

	private void Start()
	{
		images = new List<Image>();

		foreach (var e in colorSetList)
		{
			var p = new Color32[8];
			CreatePalette(e, p);
			paletteList.Add(p);
		}

		var palette = editor.GetPalette();
		for (int i = 0; i < palette.Length && i < transform.childCount; i++)
		{
			var t = transform.GetChild(i);
			images.Add(t.GetComponent<Image>());
			images[i].color = palette[i];
			paletteList[0][i] = palette[i];

			int k = i;
			t.GetComponent<Button>().onClick.AddListener(() => {
				Select(k);
			});
		}

		Select(1);
	}

	public void Select(int index)
	{
		for (int i = 0; i < images.Count; i++)
		{
			images[i].sprite = normalSprite;
			images[i].rectTransform.sizeDelta = new Vector2(32, 32);
		}

		images[index].sprite = selectedSprite;
		images[index].rectTransform.sizeDelta = new Vector2(32, 40);
		editor.SelectColor(index);

		picker.SetColor(paletteList[this.index][index]);
	}

	public void ChangePalette(int dir)
	{
		index = (index + dir + colorSetList.Length) % colorSetList.Length;
		var palette = editor.GetPalette();
		var currentPalette = paletteList[index];

		for (int i = 0; i < palette.Length; i++)
		{
			palette[i] = currentPalette[i];
		}

		for (int i = 0; i < images.Count; i++)
		{
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
