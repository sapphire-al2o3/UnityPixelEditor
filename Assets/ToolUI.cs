using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToolUI : MonoBehaviour
{
	[SerializeField]
	UnityPixelEditor editor;

	[SerializeField]
	Button[] toolButtons;

	[SerializeField]
	Color normalColor;

	[SerializeField]
	Color selectedColor;

	private void Awake()
	{
		foreach(var button in toolButtons)
		{
			var colors = button.colors;
			colors.normalColor = normalColor;
		}

		var selectedButton = toolButtons[0].colors;
		selectedButton.normalColor = selectedColor;
		selectedButton.selectedColor = selectedColor;
		toolButtons[0].colors = selectedButton;
	}

	public void Select(int index)
	{
		for (int i = 0; i < toolButtons.Length; i++)
		{
			var colors = toolButtons[i].colors;
			colors.normalColor = i == index ? selectedColor : normalColor;
			colors.selectedColor = i == index ? selectedColor : normalColor;
			toolButtons[i].colors = colors;
		}

		editor.SelectTool(index);
	}
}
