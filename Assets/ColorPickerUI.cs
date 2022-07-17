using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ColorPickerUI : MonoBehaviour
{
	Texture2D _hueTex;
	Texture2D _saturationTex;
	Texture2D _valueTex;

	[SerializeField]
	RawImage _hueImage;

	[SerializeField]
	RawImage _saturationImage;

	[SerializeField]
	RawImage _valueImage;

	[SerializeField]
	Slider _hueSlider;

	[SerializeField]
	Slider _sSlider;

	[SerializeField]
	Slider _vSlider;

	Color32 _color;
	float _h;
	float _s;
	float _v;

	Color32[] _buffer;

	public event System.Action<Color32> onChangeColor;

	private void Awake()
	{
		_buffer = new Color32[32];

		const int HQ = 32;

		_hueTex = new Texture2D(HQ, 1);
		_hueTex.filterMode = FilterMode.Point;

		Color32[] hueData = new Color32[32];
		for (int i = 0; i < hueData.Length; i++)
		{
			hueData[i] = Color.HSVToRGB(i / 32f, 1f, 1f);
		}

		_hueTex.SetPixels32(hueData);
		_hueTex.Apply();

		_hueImage.texture = _hueTex;

		const int SQ = 16;

		_saturationTex = new Texture2D(SQ, 1);
		_saturationTex.filterMode = FilterMode.Point;

		for (int i = 0; i < SQ; i++)
		{
			_buffer[i] = Color.HSVToRGB(0, (float)i / SQ, 1f);
		}

		_saturationTex.SetPixels32(0, 0, SQ, 1, _buffer);
		_saturationTex.Apply();

		_saturationImage.texture = _saturationTex;

		const int VQ = 16;

		_valueTex = new Texture2D(VQ, 1);
		_valueTex.filterMode = FilterMode.Point;

		for (int i = 0; i < VQ; i++)
		{
			_buffer[i] = Color.HSVToRGB(0, 0, (float)i / VQ);
		}

		_valueTex.SetPixels32(0, 0, VQ, 1, _buffer);
		_valueTex.Apply();

		_valueImage.texture = _valueTex;

		_hueSlider.onValueChanged.AddListener(value =>
		{
			_h = value / 32;

			for (int i = 0; i < SQ; i++)
			{
				_buffer[i] = Color.HSVToRGB(_h, (float)i / SQ, _v);
			}

			_saturationTex.SetPixels32(0, 0, VQ, 1, _buffer);
			_saturationTex.Apply();

			for (int i = 0; i < VQ; i++)
			{
				_buffer[i] = Color.HSVToRGB(_h, _s, (float)i / VQ);
			}

			_valueTex.SetPixels32(0, 0, VQ, 1, _buffer);
			_valueTex.Apply();

			_color = Color.HSVToRGB(_h, _s, _v);
			onChangeColor?.Invoke(_color);
		});

		_sSlider.onValueChanged.AddListener(value =>
		{
			_s = value / SQ;

			for (int i = 0; i < SQ; i++)
			{
				_buffer[i] = Color.HSVToRGB(_h, _s, (float)i / VQ);
			}

			_valueTex.SetPixels32(0, 0, VQ, 1, _buffer);
			_valueTex.Apply();

			_color = Color.HSVToRGB(_h, _s, _v);
			onChangeColor?.Invoke(_color);
		});

		_vSlider.onValueChanged.AddListener(value =>
		{
			_v = value / VQ;

			for (int i = 0; i < SQ; i++)
			{
				_buffer[i] = Color.HSVToRGB(_h, (float)i / SQ, _v);
			}

			_saturationTex.SetPixels32(0, 0, VQ, 1, _buffer);
			_saturationTex.Apply();

			_color = Color.HSVToRGB(_h, _s, _v);
			onChangeColor?.Invoke(_color);
		});

		_h = 0;
		_s = 1f;
		_v = 1f;
		_color = Color.HSVToRGB(_h, _s, _v);
	}

	public void SetColor(Color32 color)
	{
		Color.RGBToHSV(color, out _h, out _s, out _v);
		_color = color;

		_hueSlider.SetValueWithoutNotify(_h * 32);
		_sSlider.SetValueWithoutNotify(_s * 16);
		_vSlider.SetValueWithoutNotify(_v * 16);

		const int HQ = 32;

		for (int i = 0; i < HQ; i++)
		{
			_buffer[i] = Color.HSVToRGB((float)i / HQ, 1f, 1f);
		}

		_hueTex.SetPixels32(0, 0, HQ, 1, _buffer);
		_hueTex.Apply();

		_hueImage.texture = _hueTex;

		const int SQ = 16;

		for (int i = 0; i < SQ; i++)
		{
			_buffer[i] = Color.HSVToRGB(_h, (float)i / SQ, _v);
		}

		_saturationTex.SetPixels32(0, 0, SQ, 1, _buffer);
		_saturationTex.Apply();

		const int VQ = 16;

		for (int i = 0; i < VQ; i++)
		{
			_buffer[i] = Color.HSVToRGB(_h, _s, (float)i / VQ);
		}

		_valueTex.SetPixels32(0, 0, VQ, 1, _buffer);
		_valueTex.Apply();
	}
}
