using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UnityPixelEditor : MonoBehaviour
{
	[SerializeField]
	int width = 16;

	[SerializeField]
	int height = 16;

	Texture2D _tex;
	int[] _indexData = null;
	Color32[] _paletteData = null;
	Color32[] _pixels = null;
	int _paletteIndex = 0;

	bool _down = false;
	Graphic _graphic = null;
	Vector2Int _point;
	
	public enum Tool
	{
		Pen,
		Line,
		FillRect,
		Paint
	}

	Tool _tool = Tool.Pen;

	public Texture2D GetTexture()
	{
		return _tex;
	}

	private void Awake()
	{
		_tex = new Texture2D(width, height);
		_tex.wrapMode = TextureWrapMode.Clamp;
		_tex.filterMode = FilterMode.Point;

		_indexData = new int[width * height];
		_paletteData = new Color32[8];
		_pixels = new Color32[_indexData.Length];

		_paletteData[0] = new Color32(255, 255, 255, 255);
		_paletteData[1] = new Color32(0, 0, 0, 255);
		_paletteData[2] = new Color32(123, 66, 0, 255);
		_paletteData[3] = new Color32(99, 33, 8, 255);

		for (int i = 0; i < _pixels.Length; i++)
		{
			_pixels[i] = _paletteData[0];
		}

		UpdateTexture();

		_graphic = GetComponent<Graphic>();
		GetComponent<RawImage>().texture = _tex;
	}

	private void OnDestroy()
	{
		Destroy(_tex);
		_tex = null;
	}

	void Start()
    {
        
    }

	void DrawDot(int x, int y, int index)
	{
		_indexData[y * width + x] = index;
		_pixels[y * width + x] = _paletteData[index];
	}

	void DrawLine(int x0, int y0, int x1, int y1, int index)
	{
		int dx = Mathf.Abs(x1 - x0);
		int dy = Mathf.Abs(y1 - y0);
		int dx2 = dx * 2;
		int dy2 = dy * 2;
		int sx = x1 > x0 ? 1 : -1;
		int sy = y1 > y0 ? 1 : -1;
		int x = x0;
		int y = y0;

		if (dx >= dy)
		{
			int e = -dx;
			for (int i = 0; i <= dx; i++)
			{
				if (x < 0 || x >= width || y < 0 || y >= height)
				{
					break;
				}

				_indexData[y * width + x] = index;
				_pixels[y * width + x] = _paletteData[index];
				x += sx;
				e += dy2;
				if (e >= 0)
				{
					y += sy;
					e -= dx2;
				}
			}
		}
		else
		{
			int e = -dy;
			for (int i = 0; i <= dy; i++)
			{
				if (x < 0 || x >= width || y < 0 || y >= height)
				{
					break;
				}
				_indexData[y * width + x] = index;
				_pixels[y * width + x] = _paletteData[index];
				y += sy;
				e += dx2;
				if (e >= 0)
				{
					x += sx;
					e -= dy2;
				}
			}
		}
	}

	void FillRect(int x0, int y0, int x1, int y1, int index)
	{
		int left = Mathf.Min(x0, x1);
		int right = Mathf.Max(x0, x1);
		int top = Mathf.Min(y0, y1);
		int bottom = Mathf.Max(y0, y1);

		if (left < 0) left = 0;
		if (right >= width) right = width - 1;
		if (top < 0) top = 0;
		if (bottom >= height) bottom = height - 1;

		for (int i = top; i <= bottom; i++)
		{
			int y = i * width;
			for (int j = left; j <= right; j++)
			{
				_indexData[y + j] = index;
				_pixels[y + j] = _paletteData[index];
			}
		}
	}

	void PaintImpl(int x, int y, int index, int c)
	{
		if (x >= width || x < 0) return;
		if (y >= height || y < 0) return;
		int k = y * width + x;
		if (_indexData[k] == c)
		{
			_indexData[k] = index;
			_pixels[k] = _paletteData[index];
			PaintImpl(x - 1, y, index, c);
			PaintImpl(x + 1, y, index, c);
			PaintImpl(x, y - 1, index, c);
			PaintImpl(x, y + 1, index, c);
		}
	}

	void Paint(int x, int y, int index)
	{
		int c = _indexData[y * width + x];

		if (c == index)
		{
			return;
		}

		PaintImpl(x, y, index, c);
	}

	void UpdateTexture()
	{
		_tex.SetPixels32(_pixels);
		_tex.Apply();
	}

	bool GetPoint(out Vector2Int result)
	{
		var pos = Input.mousePosition;
		
		var cam = _graphic.canvas.worldCamera;

		RectTransformUtility.ScreenPointToLocalPointInRectangle(_graphic.rectTransform, pos, cam, out var point);

		var rect = _graphic.rectTransform.rect;

		int x = (int)((point.x - rect.x) / rect.width * width);
		int y = (int)((point.y - rect.y) / rect.height * height);

        result = new Vector2Int(x, y);

        return x >= 0 && x < width && y >= 0 && y < height;
	}

	public void SelectColor(int index)
	{
		_paletteIndex = index;
	}

	public void SelectTool(int tool)
	{
		_tool = (Tool)tool;
	}

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            if (GetPoint(out var point))
            {
                _paletteIndex = _indexData[point.y * width + point.x];
            }
        }
        else if (Input.GetMouseButtonDown(0))
        {
            if (GetPoint(out var point))
            {
                if (_tool == Tool.Pen)
                {
                    DrawDot(point.x, point.y, _paletteIndex);
                    UpdateTexture();
                    _point = point;
                    _down = true;
                }
                else if (_tool == Tool.Paint)
                {
                    Paint(point.x, point.y, _paletteIndex);
                    UpdateTexture();
                }
            }
        }
        else if (Input.GetMouseButton(0))
        {
            if (_down)
            {
                if (GetPoint(out var point))
                {
                    if (_tool == Tool.Pen)
                    {
                        DrawLine(_point.x, _point.y, point.x, point.y, _paletteIndex);
                        //DrawDot(point.x, point.y, 1);
                        UpdateTexture();
                    }
                }
                _point = point;
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            _down = false;
        }
    }
}
