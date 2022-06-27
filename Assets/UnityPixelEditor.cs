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

	[SerializeField]
	Color32[] _paletteData = null;


	Texture2D _tex;
	byte[] _indexData = null;
	Color32[] _pixels = null;
	int _paletteIndex = 1;

	Texture2D _workTex;
	byte[] _workIndexData = null;
	Color32[] _workPixels = null;

	bool _down = false;
	Graphic _graphic = null;
	Vector2Int _point;
	Vector2Int _beginPoint;

	List<byte[]> _undoStack;
	int _undoTop = 0;
	Queue<byte[]> _redoStack;

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

	public Color32[] GetPalette()
	{
		return _paletteData;
	}

	private void Awake()
	{
		_tex = new Texture2D(width, height);
		_tex.wrapMode = TextureWrapMode.Clamp;
		_tex.filterMode = FilterMode.Point;

		int length = width * height;
		_indexData = new byte[length];
		_pixels = new Color32[length];

		_workTex = new Texture2D(width, height);
		_workTex.wrapMode = TextureWrapMode.Clamp;
		_workTex.filterMode = FilterMode.Point;
		_workIndexData = new byte[length];
		_workPixels = new Color32[length];

		_undoStack = new List<byte[]>();
		_redoStack = new Queue<byte[]>();

		if (_paletteData == null || _paletteData.Length == 0)
		{
			_paletteData = new Color32[8];
			_paletteData[0] = new Color32(255, 255, 255, 255);
			_paletteData[1] = new Color32(0, 0, 0, 255);
			_paletteData[2] = new Color32(123, 66, 0, 255);
			_paletteData[3] = new Color32(99, 33, 8, 255);
		}

		for (int i = 0; i < _pixels.Length; i++)
		{
			_pixels[i] = _paletteData[0];
		}

		UpdateTexture(_tex, _pixels);

		ClearWork();
		UpdateTexture(_workTex, _workPixels);

		_graphic = GetComponent<Graphic>();
		GetComponent<RawImage>().texture = _tex;
		GetComponent<RawImage>().material.SetTexture("_WorkTex", _workTex);
	}

	private void OnDestroy()
	{
		Destroy(_tex);
		_tex = null;
	}

	void DrawDot(byte[] indexData, Color32[] pixels, Color32[] paletteData, int x, int y, byte index)
	{
		indexData[y * width + x] = index;
		pixels[y * width + x] = paletteData[index];
	}

	void DrawLine(byte[] indexData, Color32[] pixels, Color32[] paletteData, int x0, int y0, int x1, int y1, byte index)
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
				int k = y * width + x;
				indexData[k] = index;
				pixels[k] = paletteData[index];
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
				int k = y * width + x;
				indexData[k] = index;
				pixels[k] = paletteData[index];
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

	void FillRect(byte[] indexData, Color32[] pixels, Color32[] paletteData, int x0, int y0, int x1, int y1, byte index)
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
				indexData[y + j] = index;
				pixels[y + j] = paletteData[index];
			}
		}
	}

	void PaintImpl(byte[] indexData, Color32[] pixels, Color32[] paletteData, int x, int y, byte index, int c)
	{
		if (x >= width || x < 0) return;
		if (y >= height || y < 0) return;
		int k = y * width + x;
		if (indexData[k] == c)
		{
			indexData[k] = index;
			pixels[k] = paletteData[index];
			PaintImpl(indexData, pixels, paletteData, x - 1, y, index, c);
			PaintImpl(indexData, pixels, paletteData, x + 1, y, index, c);
			PaintImpl(indexData, pixels, paletteData, x, y - 1, index, c);
			PaintImpl(indexData, pixels, paletteData, x, y + 1, index, c);
		}
	}

	void Paint(byte[] indexData, Color32[] pixels, Color32[] paletteData, int x, int y, byte index)
	{
		int c = indexData[y * width + x];

		if (c == index)
		{
			return;
		}

		PaintImpl(indexData, pixels, paletteData, x, y, index, c);
	}

	void ClearWork()
	{
		for (int i = 0; i < _workIndexData.Length; i++)
		{
			_workIndexData[i] = 0;
		}
		for (int i = 0; i < _workPixels.Length; i++)
		{
			_workPixels[i].r = 0;
			_workPixels[i].g = 0;
			_workPixels[i].b = 0;
			_workPixels[i].a = 0;
		}
	}

	void pushUndo()
	{
		const int UndoCapacity = 1024;
		byte[] indexData = null;
		if (_undoStack.Count >= UndoCapacity)
		{
			indexData = _undoStack[_undoTop % UndoCapacity];
		}
		else
		{
			indexData = new byte[_indexData.Length];
			_undoStack.Add(indexData);
		}
		System.Array.Copy(_indexData, indexData, _indexData.Length);
		_undoTop++;
	}

	byte[] popUndo()
	{
		if (_undoTop == 0)
		{
			return null;
		}
		const int UndoCapacity = 1024;
		_undoTop--;
		var indexData = _undoStack[_undoTop % UndoCapacity];
		if (indexData == null)
		{
			return null;
		}
		_undoStack[_undoTop % UndoCapacity] = null;
		return indexData;
	}

	void UpdateTexture(Texture2D tex, Color32[] pixels)
	{
		tex.SetPixels32(pixels);
		tex.Apply();
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

	public void Undo()
	{
		var indexData = popUndo();
		if (indexData == null)
		{
			return;
		}
		System.Array.Copy(indexData, _indexData, _indexData.Length);
		for (int i = 0; i < _indexData.Length; i++)
		{
			_pixels[i] = _paletteData[_indexData[i]];
		}
		UpdateTexture(_tex, _pixels);
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
				pushUndo();
                if (_tool == Tool.Pen)
                {
                    DrawDot(_indexData, _pixels, _paletteData, point.x, point.y, (byte)_paletteIndex);
                    UpdateTexture(_tex, _pixels);
                    _point = point;
                    _down = true;
                }
				else if (_tool == Tool.Line)
				{
					_point = point;
					_beginPoint = point;
					_down = true;
				}
                else if (_tool == Tool.Paint)
                {
                    Paint(_indexData, _pixels, _paletteData, point.x, point.y, (byte)_paletteIndex);
                    UpdateTexture(_tex, _pixels);
                }
				else if (_tool == Tool.FillRect)
				{
					_point = point;
					_beginPoint = point;
					_down = true;
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
                        DrawLine(_indexData, _pixels, _paletteData, _point.x, _point.y, point.x, point.y, (byte)_paletteIndex);
                        //DrawDot(point.x, point.y, 1);
                        UpdateTexture(_tex, _pixels);
                    }
					else if (_tool == Tool.Line)
					{
						ClearWork();
						DrawLine(_workIndexData, _workPixels, _paletteData, _beginPoint.x, _beginPoint.y, point.x, point.y, (byte)_paletteIndex);
						_point = point;
						UpdateTexture(_workTex, _workPixels);
					}
					else if (_tool == Tool.FillRect)
					{
						ClearWork();
						FillRect(_workIndexData, _workPixels, _paletteData, _beginPoint.x, _beginPoint.y, point.x, point.y, (byte)_paletteIndex);
						_point = point;
						UpdateTexture(_workTex, _workPixels);
					}
                }
                _point = point;
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
			if (_down)
			{
				if (_tool == Tool.Line)
				{
					ClearWork();
					UpdateTexture(_workTex, _workPixels);
					DrawLine(_indexData, _pixels, _paletteData, _beginPoint.x, _beginPoint.y, _point.x, _point.y, (byte)_paletteIndex);
					UpdateTexture(_tex, _pixels);
				}
				else if (_tool == Tool.FillRect)
				{
					ClearWork();
					UpdateTexture(_workTex, _workPixels);
					FillRect(_indexData, _pixels, _paletteData, _beginPoint.x, _beginPoint.y, _point.x, _point.y, (byte)_paletteIndex);
					UpdateTexture(_tex, _pixels);
				}
			}
            _down = false;
        }
    }
}
