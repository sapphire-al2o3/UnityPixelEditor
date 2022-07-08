using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PaletteUI : MonoBehaviour
{
	[SerializeField]
	UnityPixelEditor editor;

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
	List<Color32> colorSet = new List<Color32>();

	[SerializeField]
	ColorSet[] colorSetList;

	private void Start()
	{
		images = new List<Image>();
		var palette = editor.GetPalette();
		for (int i = 0; i < palette.Length && i < transform.childCount; i++)
		{
			var t = transform.GetChild(i);
			images.Add(t.GetComponent<Image>());
			images[i].color = palette[i];

			int k = i;
			t.GetComponent<Button>().onClick.AddListener(() => {
				Select(k);
			});
		}

		colorSet.Add(new Color32(0, 0xFF, 0, 0xFF));
		colorSet.Add(new Color32(0xFF, 0, 0xFF, 0xFF));
		colorSet.Add(new Color32(0, 0, 0xFF, 0xFF));
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
	}

	public void ChangePalette(int dir)
	{
		index = (index + dir + colorSetList.Length) % colorSetList.Length;
		var palette = editor.GetPalette();
		CreatePalette(colorSetList[index], palette);

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
