using System.Collections.Generic;
using Durango.Logic.Item;
using Shared.Item;
using UnityEngine;
using Yaml;
using Yaml.Util;

namespace Durango.UI.Control;

public class ItemGradeViewer : UITexture
{
	private const float Margin = 3f;

	private readonly List<Color> _grades = new List<Color>();

	private float _alignPivot;

	private bool _isUpward;

	private int _countPerRow;

	protected override void Awake()
	{
		base.Awake();
		if (mainTexture == null)
		{
			mainTexture = Texture2D.whiteTexture;
		}
	}

	public ItemGradeViewer SetOptions(float alignPivot, bool upward, int countPerRow)
	{
		_alignPivot = alignPivot;
		_isUpward = upward;
		_countPerRow = countPerRow;
		return this;
	}

	public ItemGradeViewer Set(IEnumerable<TagData> tags)
	{
		SettingBegin();
		foreach (TagData tag in tags)
		{
			AddTagData(tag.Id, tag.Level);
		}
		SettingEnd();
		return this;
	}

	public void SettingBegin()
	{
		_grades.Clear();
	}

	public void SettingEnd()
	{
		mChanged = true;
	}

	public void AddTagData(string id, int level)
	{
		if (SingletonDict<string, Tag>.TryGetValue(id, out var value) && value.Visible && value.Grade != TagGrade.Neutral)
		{
			_grades.Add(TagData.GetGradeColor(value.Grade));
		}
	}

	public void Set(List<TagData> tags, float alignPivot, bool upward, int countPerRow)
	{
		SetOptions(alignPivot, upward, countPerRow);
		Set(tags);
	}

	public void Set(ItemData item, float alignPivot = 0.5f, bool upward = true, int countPerRow = 5)
	{
		List<TagData> tags = null;
		if (item != null)
		{
			tags = item.Tags;
		}
		Set(tags, alignPivot, upward, countPerRow);
	}

	public override void OnFill(UIGeometry.Arguments arguments)
	{
		if (!Application.isPlaying)
		{
			base.OnFill(arguments);
			return;
		}
		int size = arguments.verts.size;
		int num = ((_countPerRow <= 0) ? ((_grades.Count > 0) ? 1 : 0) : Mathf.CeilToInt((float)_grades.Count / (float)_countPerRow));
		int num2 = ((_countPerRow <= 0) ? _grades.Count : Mathf.Min(_grades.Count, _countPerRow));
		float num3 = (float)(num2 * base.width) + (float)(num2 - 1) * 3f;
		Vector3 vector = base.localCenter - new Vector3(num3 * _alignPivot, (float)base.height * 0.5f);
		for (int i = 0; i < num; i++)
		{
			int num4 = ((_countPerRow <= 0) ? _grades.Count : (Mathf.Min((i + 1) * _countPerRow, _grades.Count) - i * _countPerRow));
			float num5 = (float)(num4 * base.width) + (float)(num4 - 1) * 3f;
			Vector3 vector2 = vector + new Vector3((num3 - num5) * _alignPivot, (float)i * ((float)base.height + 3f) * ((!_isUpward) ? (-1f) : 1f));
			for (int j = 0; j < num4; j++)
			{
				Rect vert = new Rect(vector2 + new Vector3(((float)base.width + 3f) * (float)j, 0f), new Vector2(base.width, base.height));
				Rect uv = new Rect(0f, 0f, 1f, 1f);
				Color col = _grades[i * _countPerRow + j];
				DrawQuad(arguments.verts, arguments.uvs, arguments.cols, vert, uv, col);
			}
		}
		if (onPostFill != null)
		{
			onPostFill(this, size, arguments);
		}
	}

	private static void DrawQuad(BetterList<Vector3> verts, BetterList<Vector2> uvs, BetterList<Color> cols, Rect vert, Rect uv, Color col)
	{
		verts.Add(new Vector2(vert.xMin, vert.yMin));
		verts.Add(new Vector2(vert.xMin, vert.yMax));
		verts.Add(new Vector2(vert.xMax, vert.yMax));
		verts.Add(new Vector2(vert.xMax, vert.yMin));
		uvs.Add(new Vector2(uv.xMin, uv.yMin));
		uvs.Add(new Vector2(uv.xMin, uv.yMax));
		uvs.Add(new Vector2(uv.xMax, uv.yMax));
		uvs.Add(new Vector2(uv.xMax, uv.yMin));
		cols.Add(col);
		cols.Add(col);
		cols.Add(col);
		cols.Add(col);
	}
}
