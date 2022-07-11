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
	List<byte[]> _redoStack;
	int _redoTop = 0;

	public enum Tool
	{
		Pen,
		Paint,
		Line,
		FillRect,
		FillEllipse
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
		_redoStack = new List<byte[]>();

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

		Destroy(_workTex);
		_workTex = null;
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

	float Eps(float x0, float y0, float x1, float y1, float x, float y)
	{
		float dx = x1 - x0;
		float dy = y1 - y0;
		float dx2 = dx * dx;
		float dy2 = dy * dy;
		float ex = 2 * x - x0 - x1;
		float ey = 2 * y - y0 - y1;
		return dx2 * dy2 - dy * dy * ex * ex - dx * dx * ey * ey;

	}

	void FillEllipse(byte[] indexData, Color32[] pixels, Color32[] paletteData, int x0, int y0, int x1, int y1, byte index)
	{
		int left = Mathf.Min(x0, x1),
			right = Mathf.Max(x0, x1),
			top = Mathf.Min(y0, y1),
			bottom = Mathf.Max(y0, y1);

		x0 = left;
		x1 = right;
		y0 = top;
		y1 = bottom;

		int dx = x1 - x0;
		int dy = y1 - y0;
		int dx2 = dx * dx;
		int dy2 = dy * dy;
		int a = dx >> 1;
		int b = dy >> 1;

		int x0c = x0 < 0 ? 0 : x0;
		int x1c = x1 >= width ? width - 1 : x1;
		int y0c = y0 < 0 ? 0 : y0;
		int y1c = y1 >= height ? height - 1 : y1;

		int ix = (x0 + x1) / 2;
		if (0 <= ix && ix < width)
		{
			for (int j = y0c; j <= y1c; j++)
			{
				int k = j * width + ix;
				indexData[k] = index;
				pixels[k] = paletteData[index];
			}
		}

		if ((dx & 1) == 1)
		{
			ix = (x0 + x1) / 2 + 1;
			if (0 <= ix && ix < width)
			{
				for (int j = y0; j <= y1; j++)
				{
					if (0 <= j && j < height)
					{
						int k = j * width + ix;
						indexData[k] = index;
						pixels[k] = paletteData[index];
					}
				}
			}
		}

		int y = (y0 + y1) / 2;
		int iy = (y0 + y1) / 2 * width;
		if (0 <= iy && iy < width * height)
		{
			for (int j = x0c; j <= x1c; j++)
			{
				int k = iy + j;
				indexData[k] = index;
				pixels[k] = paletteData[index];
			}
		}
		if ((dy & 1) == 1)
		{
			iy = ((y0 + y1) / 2 + 1) * width;
			if(0 <= iy && iy < width * height)
			{
				for(int j = x0c; j <= x1c; j++)
				{
					int k = iy + j;
					indexData[k] = index;
					pixels[k] = paletteData[index];
				}
			}
		}

		int a2 = a * a;
		int b2 = b * b;
		int f = b2 * (-2 * a + 1) + 2 * a2;
		int cx = x0 + a;
		int cy = y0 + b;
		float n = (a / Mathf.Sqrt((float)b2 / a2 + 1) - 0.5f);

		y = y1;
		int x = (x0 + x1) / 2;
		for(int i = 0; i < n; i++)
		{
			float e0 = Eps(x0 + 0.5f, y0 + 0.5f, x1 + 0.5f, y1 + 0.5f, x - 0.5f, y + 0.5f);
			float e1 = Eps(x0 + 0.5f, y0 + 0.5f, x1 + 0.5f, y1 + 0.5f, x - 0.5f, y - 0.5f);
			if (Mathf.Abs(e0) >= Mathf.Abs(e1))
			{
				y = y - 1;
			}
			x--;
			for (int j = x; j <= x1 - x + x0; j++)
			{
				if (0 <= j && j < width)
				{
					int k = y * width + j;
					indexData[k] = index;
					pixels[k] = paletteData[index];
					k = (y1 - y + y0) * width + j;
					indexData[k] = index;
					pixels[k] = paletteData[index];
				}
			}
		}

		if (y - 1 <= cy)
		{
			return;
		}

		y = (y0 + y1) / 2;
		x = x1;
		n = (b / Mathf.Sqrt((float)a2 / b2 + 1));
		for (int i = 0; i < n; i++)
		{
			float e0 = Eps(x0 + 0.5f, y0 + 0.5f, x1 + 0.5f, y1 + 0.5f, x + 0.5f, y - 1 + 0.5f);
			float e1 = Eps(x0 + 0.5f, y0 + 0.5f, x1 + 0.5f, y1 + 0.5f, x - 1 + 0.5f, y - 1 + 0.5f);

			if(Mathf.Abs(e0) < Mathf.Abs(e1))
			{
				y = y - 1;
			}
			else
			{
				x = x - 1;
				y = y - 1;
			}

			for (int j = x1 - x + x0; j <= x; j++)
			{
				if (0 <= j && j < width)
				{
					int k = y * width + j;
					indexData[k] = index;
					pixels[k] = paletteData[index];
					k = (y1 - y + y0) * width + j;
					indexData[k] = index;
					pixels[k] = paletteData[index];
				}
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

	void PushUndo()
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
			if (_undoTop == _undoStack.Count)
			{
				_undoStack.Add(indexData);
			}
			else
			{
				_undoStack[_undoTop] = indexData;
			}
		}
		System.Array.Copy(_indexData, indexData, _indexData.Length);
		_undoTop++;
	}

	byte[] PopUndo()
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

	void PushRedo()
	{
		const int RedoCapacity = 1024;
		byte[] indexData = null;
		if (_redoStack.Count >= RedoCapacity)
		{
			indexData = _redoStack[_undoTop % RedoCapacity];
		}
		else
		{
			indexData = new byte[_indexData.Length];
			if (_redoTop == _redoStack.Count)
			{
				_redoStack.Add(indexData);
			}
			else
			{
				_redoStack[_redoTop] = indexData;
			}
		}
		System.Array.Copy(_indexData, indexData, _indexData.Length);
		_redoTop++;
	}

	byte[] PopRedo()
	{
		if (_redoTop == 0)
		{
			return null;
		}
		const int UndoCapacity = 1024;
		_redoTop--;
		var indexData = _redoStack[_redoTop % UndoCapacity];
		if (indexData == null)
		{
			return null;
		}
		_redoStack[_redoTop % UndoCapacity] = null;
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
		var indexData = PopUndo();
		if (indexData == null)
		{
			return;
		}
		PushRedo();
		System.Array.Copy(indexData, _indexData, _indexData.Length);
		Refresh();
	}

	public void Redo()
	{
		var indexData = PopRedo();
		if (indexData == null)
		{
			return;
		}
		PushUndo();
		System.Array.Copy(indexData, _indexData, _indexData.Length);
		Refresh();
	}

	public void Refresh()
	{
		for (int i = 0; i < _indexData.Length; i++)
		{
			_pixels[i] = _paletteData[_indexData[i]];
		}
		UpdateTexture(_tex, _pixels);
	}


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
				PushUndo();
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
				else if (_tool == Tool.FillEllipse)
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
					else if (_tool == Tool.FillEllipse)
					{
						ClearWork();
						FillEllipse(_workIndexData, _workPixels, _paletteData, _beginPoint.x, _beginPoint.y, point.x, point.y, (byte)_paletteIndex);
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
				else if (_tool == Tool.FillEllipse)
				{
					ClearWork();
					UpdateTexture(_workTex, _workPixels);
					FillEllipse(_indexData, _pixels, _paletteData, _beginPoint.x, _beginPoint.y, _point.x, _point.y, (byte)_paletteIndex);
					UpdateTexture(_tex, _pixels);
				}
			}
            _down = false;
        }
    }
}
