using System;
using System.Collections;
using System.Collections.Generic;
using ExploreData;
using L10N;
using Shared.Region;
using TerrainData;
using UnityEngine;

public class RoutesViewer : MonoBehaviour
{
	[Serializable]
	private class OptionStruct
	{
		public int MinimumCount = 6;

		public int InnerRadius = 150;

		public int NodeDistance = 60;

		public float AngleRandomRange = 0.1f;

		public float DistanceRandomRange = 20f;

		public float DrawLineSpeed = 0.02f;
	}

	[Serializable]
	private class Decoration
	{
		public SpriteData[] WaveSprites;

		public SpriteData ReefSprite;

		public SpriteData DecoIslandSprite;

		public float ReefRatio;

		public float DecoAmount;

		public float DecoColliderSize;
	}

	[Serializable]
	private struct TooltipStruct
	{
		[TextArea(1, 1)]
		[LocalizableString]
		public string StablRegionsTitle;

		[TextArea]
		[LocalizableString]
		public string StablRegionsComment;

		[TextArea(1, 1)]
		[LocalizableString]
		public string UnstablRegionsTitle;

		[LocalizableString]
		[TextArea]
		public string UnstablRegionsComment;
	}

	[Serializable]
	public struct RegionInfo
	{
		public SpriteData Icon;

		public Color Color;

		public Color FontColor;
	}

	private class Position
	{
		public float Angle;

		public float Distance;

		public TerrainData.Biome Biome;

		public bool Used;

		public Vector3 GetPosition(float aspectRatio)
		{
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			//IL_0023: Unknown result type (might be due to invalid IL or missing references)
			return new Vector3(Mathf.Cos(Angle) * aspectRatio, Mathf.Sin(Angle)) * Distance;
		}
	}

	private struct PageStruct
	{
		public ListObjectPool Regions;

		public ListObjectPool Waves;

		public ListObjectPool Reefs;

		public ListObjectPool DecoIsalnd;
	}

	private class Path
	{
		public Vector2 Position;

		public Vector2 Vector;

		public Path(Vector2 pos)
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			Position = pos;
			Vector = Vector2.zero;
		}
	}

	private struct AreaStruct
	{
		public int Start;

		public int End;

		public int Max;

		public int Goal;

		public int Count => (End < Start) ? (Max - Start + End) : (End - Start);

		public int Diff => Count - Goal;

		public void PlusEnd(int val)
		{
			End += val;
			if (End < 0)
			{
				End += Max;
			}
			else if (End >= Max)
			{
				End -= Max;
			}
		}

		public void PlusStart(int val)
		{
			Start += val;
			if (Start < 0)
			{
				Start += Max;
			}
			else if (Start >= Max)
			{
				Start -= Max;
			}
		}
	}

	[SerializeField]
	private UISpriteLabel _label;

	[SerializeField]
	private ExploreRegionNode _myRegion;

	[SerializeField]
	private GameObject _stableRegionsIcon;

	[SerializeField]
	private GameObject _regionNode;

	[SerializeField]
	private GameObject _innerContainer;

	[SerializeField]
	private GameObject _outterContainer;

	[SerializeField]
	private ListObjectPool _dots;

	[SerializeField]
	private Decoration _decoration;

	[SerializeField]
	private OptionStruct _option;

	[Tooltip("Unspecified\nTemperateForest\nTropicalForest\nDesert\nTundra\nSnowField\nGrassland")]
	[SerializeField]
	private RegionInfo[] _reigonOptions;

	[SerializeField]
	private TooltipStruct _tooltip;

	public static RegionInfo[] RegionOptions;

	private PageStruct _inners;

	private PageStruct _outters;

	private string _titleLabelFormat;

	private readonly List<Path> _paths = new List<Path>();

	private readonly List<Position> _positions = new List<Position>();

	private float _resizeRatio;

	private float _aspectRatio;

	private UIWidget _widget;

	private bool _isInit;

	public bool IsOpen { get; private set; }

	public bool IsInner { get; private set; }

	public UIWidget Widget
	{
		get
		{
			if ((Object)(object)_widget == (Object)null)
			{
				_widget = ((Component)this).GetComponent<UIWidget>();
			}
			return _widget;
		}
	}

	public event Action<Route> RouteClicked;

	private void Init()
	{
		if (!_isInit)
		{
			_isInit = true;
			_dots.Init(null);
			_inners = MakePageStruct(_innerContainer);
			_outters = MakePageStruct(_outterContainer);
			Object.Destroy((Object)(object)_regionNode.gameObject);
			UIEventListener uIEventListener = UIEventListener.Get(_stableRegionsIcon);
			uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, (UIEventListener.VoidDelegate)delegate
			{
				ShowInner(instant: false);
			});
			UIEventListener uIEventListener2 = UIEventListener.Get(((Component)_myRegion).gameObject);
			uIEventListener2.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener2.onClick, (UIEventListener.VoidDelegate)delegate
			{
				ShowOutter(instant: false);
			});
			UIEventListener uIEventListener3 = UIEventListener.Get(((Component)_label).gameObject);
			uIEventListener3.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener3.onClick, (UIEventListener.VoidDelegate)delegate
			{
				//IL_0082: Unknown result type (might be due to invalid IL or missing references)
				//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
				WidgetTooltipControl widgetTooltipControl = UIManager.Popup.Tooltip<WidgetTooltipControl>();
				string text = ((!IsInner) ? _tooltip.UnstablRegionsTitle : _tooltip.StablRegionsTitle);
				string text2 = ((!IsInner) ? _tooltip.UnstablRegionsComment : _tooltip.StablRegionsComment);
				widgetTooltipControl.Set(T._(text), T._(text2));
				widgetTooltipControl.Direction = TooltipBase.TooltipDirection.Vertical;
				UILabel label = _label.Label;
				widgetTooltipControl.Show((UIWidget)label, Vector2.right * ((float)label.width * 0.5f - (float)label.height * 0.5f), 60f);
			});
			RegionOptions = _reigonOptions;
		}
	}

	private PageStruct MakePageStruct(GameObject container)
	{
		PageStruct pageStruct = default(PageStruct);
		pageStruct.Regions = new ListObjectPool();
		pageStruct.Reefs = new ListObjectPool();
		pageStruct.Waves = new ListObjectPool();
		pageStruct.DecoIsalnd = new ListObjectPool();
		PageStruct result = pageStruct;
		UIWidget component = _regionNode.GetComponent<UIWidget>();
		result.Regions.BaseObject = container.AddChild(_regionNode);
		UISprite uISprite = container.AddChild<UISprite>();
		uISprite.depth = component.depth - 1;
		result.Waves.BaseObject = ((Component)uISprite).gameObject;
		UISprite uISprite2 = container.AddChild<UISprite>();
		uISprite2.depth = component.depth - 2;
		_decoration.ReefSprite.Set(uISprite2);
		uISprite2.MakePixelPerfect();
		result.Reefs.BaseObject = ((Component)uISprite2).gameObject;
		UISprite uISprite3 = container.AddChild<UISprite>();
		uISprite3.depth = component.depth - 3;
		_decoration.DecoIslandSprite.Set(uISprite3);
		uISprite3.MakePixelPerfect();
		result.DecoIsalnd.BaseObject = ((Component)uISprite3).gameObject;
		result.Regions.Init(OnInitRegionNode);
		result.Waves.Init(null);
		result.Reefs.Init(null);
		result.DecoIsalnd.Init(null);
		return result;
	}

	private void Start()
	{
		Init();
	}

	private void OnDisable()
	{
		IsOpen = false;
		IsInner = false;
	}

	private void OnInitRegionNode(GameObject obj)
	{
		UIEventListener uIEventListener = UIEventListener.Get(obj);
		uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, new UIEventListener.VoidDelegate(OnClickRegionNode));
	}

	public void Active(Region myRegion)
	{
		((Component)this).gameObject.SetActive(true);
		Init();
		_myRegion.Set(myRegion);
		ShowInner(instant: true);
	}

	public void SetInnerOutter(Region myRegion, IList<Route> inner, IList<Route> outter, int randomSeed)
	{
		Random random = new Random(randomSeed);
		int randomSeed2 = random.Next();
		int randomSeed3 = random.Next();
		Set(_inners, inner, randomSeed2, biomeGrouping: false, myRegion);
		Set(_outters, outter, randomSeed3, biomeGrouping: true, myRegion);
		if (outter.Count == 0)
		{
			ShowInner(instant: true);
			return;
		}
		if (!IsOpen)
		{
			IsOpen = true;
		}
		((MonoBehaviour)this).StartCoroutine(ShowAnimation());
	}

	private void Set(PageStruct strct, IList<Route> routes, int randomSeed, bool biomeGrouping, Region myReion)
	{
		Random rand = new Random(randomSeed);
		MakePosition(routes.Count, rand);
		if (biomeGrouping)
		{
			CalcAreas(routes);
		}
		AllocationPosition(strct.Regions, routes, myReion);
		MakeDecorations(strct, rand);
	}

	private IEnumerator ShowAnimation()
	{
		ShowInner(instant: true);
		yield return (object)new WaitForSeconds(0.5f);
		ShowOutter(instant: false);
	}

	public void ShowInner(bool instant)
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		IsInner = true;
		AnimationWidget component = _innerContainer.GetComponent<AnimationWidget>();
		AnimationWidget component2 = _outterContainer.GetComponent<AnimationWidget>();
		((Component)component).gameObject.SetActive(true);
		component.SetAlpha(1f, !instant);
		component.SetScale(Vector3.one, !instant);
		component2.SetAlpha(0f, !instant);
		component2.SetScale(Vector3.one * 1.5f, !instant);
		component.Delay = 0f;
		component2.Delay = 0.2f;
		LabelUpdate(instant);
	}

	public void ShowOutter(bool instant)
	{
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		if (_outters.Regions.Count != 0)
		{
			IsInner = false;
			AnimationWidget component = _innerContainer.GetComponent<AnimationWidget>();
			AnimationWidget component2 = _outterContainer.GetComponent<AnimationWidget>();
			component.SetAlpha(0f, !instant);
			component.SetScale(Vector3.one * 0.5f, !instant);
			((Component)component2).gameObject.SetActive(true);
			component2.SetAlpha(1f, !instant);
			component2.SetScale(Vector3.one, !instant);
			component.Delay = 0.2f;
			component2.Delay = 0f;
			LabelUpdate(instant);
		}
	}

	private void LabelUpdate(bool instant)
	{
		TweenAlpha component = ((Component)_label).GetComponent<TweenAlpha>();
		if (instant)
		{
			_label.alpha = 1f;
			((Behaviour)component).enabled = false;
			LabelTextUpdate();
		}
		else
		{
			component.ResetToBeginning();
			component.PlayForward();
			KUtility.DelayedCall((MonoBehaviour)(object)this, LabelTextUpdate, component.duration * 0.5f);
		}
	}

	private void LabelTextUpdate()
	{
		if (_titleLabelFormat == null)
		{
			_titleLabelFormat = _label.text;
		}
		_label.text = string.Format(_titleLabelFormat, (!IsInner) ? T._("불안정 해역") : T._("안정 해역"));
	}

	private static int SortByAngle(Position p1, Position p2)
	{
		return (int)Mathf.Sign(p1.Angle - p2.Angle);
	}

	private static int SortByDepth(Position p1, Position p2)
	{
		return (int)Mathf.Sign(p1.Distance - p2.Distance);
	}

	private void MakePosition(int count, Random rand)
	{
		int num = 0;
		int num2 = 0;
		int num3 = Mathf.Min(Widget.width, Widget.height);
		float num4 = Mathf.Ceil(((float)num3 * 0.5f - (float)_option.InnerRadius) / (float)_option.NodeDistance);
		int num5 = Mathf.Max(Mathf.CeilToInt((float)count / num4), _option.MinimumCount);
		int num6 = (Mathf.CeilToInt((float)count / (float)num5) + 1) * num5;
		_positions.Clear();
		float num7 = rand.Next(360);
		for (int i = 0; i < num6; i++)
		{
			float num8 = num7 + 360f * ((float)num2 + ((num % 2 != 1) ? 0f : 0.5f)) / (float)num5;
			num8 += ((float)rand.NextDouble() - 0.5f) * _option.AngleRandomRange;
			num8 = KMathUtil.PositiveAngDeg(num8) * ((float)Math.PI / 180f);
			float distance = (float)(_option.InnerRadius + num * _option.NodeDistance) + ((float)rand.NextDouble() - 0.5f) * _option.DistanceRandomRange;
			_positions.Add(new Position
			{
				Angle = num8,
				Distance = distance
			});
			num2++;
			if (num2 >= num5)
			{
				num2 = 0;
				num++;
			}
		}
	}

	private void CalcAreas(IList<Route> routes)
	{
		_positions.Sort(SortByAngle);
		AreaStruct[] array = new AreaStruct[6];
		for (int i = 0; i < routes.Count; i++)
		{
			TerrainData.Biome biome = routes[i].Region.MajorBiome();
			int num = (int)biome;
			if (num >= 0 && num < array.Length)
			{
				array[num].Goal++;
			}
		}
		for (int j = 0; j < array.Length; j++)
		{
			array[j].Max = _positions.Count;
		}
		int num2 = 0;
		for (int k = 0; k < _positions.Count; k++)
		{
			int num3 = (num2 + 1) % array.Length;
			float num4 = (float)Math.PI * 2f * (float)num3 / (float)array.Length;
			if (num4 <= 0f)
			{
				num4 += (float)Math.PI * 2f;
			}
			if (_positions[k].Angle >= num4)
			{
				array[num2].End = k;
				num2++;
				array[num2].Start = k;
			}
		}
		int num5 = 0;
		int num6 = 0;
		do
		{
			bool flag = true;
			int num7 = -1;
			int num8 = 0;
			for (int l = 0; l < array.Length; l++)
			{
				int num9 = (num5 + l) % array.Length;
				int num10 = (num5 + l + 1) % array.Length;
				int diff = array[num9].Diff;
				int diff2 = array[num10].Diff;
				if (diff < 0)
				{
					flag = false;
				}
				int num11 = diff2 - diff;
				if (Mathf.Abs(num8) < Mathf.Abs(num11))
				{
					num8 = num11;
					num7 = num9;
				}
			}
			if (flag)
			{
				break;
			}
			int num12 = (num7 + 1) % array.Length;
			if (num8 > 0)
			{
				array[num12].PlusStart(1);
				array[num7].PlusEnd(1);
			}
			else
			{
				array[num12].PlusStart(-1);
				array[num7].PlusEnd(-1);
			}
			num5 = (num5 + 1) % array.Length;
			num6++;
		}
		while (num6 <= 1000);
		for (int m = 0; m < array.Length; m++)
		{
			int count = array[m].Count;
			int start = array[m].Start;
			for (int n = 0; n < count; n++)
			{
				int index = (start + n) % _positions.Count;
				_positions[index].Biome = (TerrainData.Biome)m;
			}
		}
	}

	private void AllocationPosition(ListObjectPool nodes, IList<Route> routes, Region myReion)
	{
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0154: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02dc: Unknown result type (might be due to invalid IL or missing references)
		_positions.Sort(SortByDepth);
		Route[] array = new Route[routes.Count];
		routes.CopyTo(array, 0);
		Array.Sort(array, SortByLevel);
		_aspectRatio = Widget.aspectRatio;
		nodes.Clear();
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].Region.Id == myReion.Id)
			{
				array[i] = null;
				continue;
			}
			TerrainData.Biome biome = array[i].Region.MajorBiome();
			if (biome != TerrainData.Biome.Unspecified)
			{
				int num = FindEmptyPosition(biome);
				if (num != -1)
				{
					SetRegion(nodes, num, array[i]);
					array[i] = null;
				}
			}
		}
		for (int j = 0; j < array.Length; j++)
		{
			if (array[j] != null)
			{
				int index = FindEmptyPosition();
				SetRegion(nodes, index, array[j]);
				array[j] = null;
			}
		}
		Vector2 localSize = nodes.BaseObject.GetComponent<UIWidget>().localSize;
		float num2 = float.MaxValue;
		float num3 = float.MinValue;
		float num4 = float.MaxValue;
		float num5 = float.MinValue;
		for (int k = 0; k < nodes.Count; k++)
		{
			Vector2 val = Vector2.op_Implicit(nodes[k].transform.localPosition);
			num2 = Mathf.Min(num2, val.x);
			num3 = Mathf.Max(num3, val.x);
			num4 = Mathf.Min(num4, val.y);
			num5 = Mathf.Max(num5, val.y);
		}
		float num6 = 1f;
		Rect val2 = default(Rect);
		((Rect)(ref val2))._002Ector(Vector2.op_Implicit(Widget.localCorners[0]), Widget.localSize);
		((Rect)(ref val2)).xMin = ((Rect)(ref val2)).xMin + localSize.x * 0.5f;
		((Rect)(ref val2)).xMax = ((Rect)(ref val2)).xMax - localSize.x * 0.5f;
		((Rect)(ref val2)).yMin = ((Rect)(ref val2)).yMin + localSize.y * 0.5f;
		((Rect)(ref val2)).yMax = ((Rect)(ref val2)).yMax - localSize.y * 0.5f;
		num6 = Mathf.Max(num6, num3 / ((Rect)(ref val2)).xMax);
		num6 = Mathf.Max(num6, num2 / ((Rect)(ref val2)).xMin);
		num6 = Mathf.Max(num6, num5 / ((Rect)(ref val2)).yMax);
		num6 = Mathf.Max(num6, num4 / ((Rect)(ref val2)).yMin);
		if (!(num6 > 1f))
		{
			_resizeRatio = 1f;
			return;
		}
		_resizeRatio = 1f / num6;
		for (int l = 0; l < nodes.Count; l++)
		{
			Transform transform = nodes[l].transform;
			transform.localPosition *= _resizeRatio;
		}
	}

	private void MakeDecorations(PageStruct strct, Random rand)
	{
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_0166: Unknown result type (might be due to invalid IL or missing references)
		//IL_0210: Unknown result type (might be due to invalid IL or missing references)
		//IL_0221: Unknown result type (might be due to invalid IL or missing references)
		//IL_0232: Unknown result type (might be due to invalid IL or missing references)
		//IL_0234: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e5: Unknown result type (might be due to invalid IL or missing references)
		UIWidget component = strct.Regions.BaseObject.GetComponent<UIWidget>();
		int num = Mathf.Max(component.width, component.height);
		strct.DecoIsalnd.Clear();
		for (int i = 0; i < _positions.Count; i++)
		{
			if (!_positions[i].Used)
			{
				strct.DecoIsalnd.Add().transform.localPosition = _positions[i].GetPosition(Widget.aspectRatio) * _resizeRatio;
			}
		}
		UIWidget widget = Widget;
		Rect val = default(Rect);
		((Rect)(ref val))._002Ector(Vector2.op_Implicit(widget.localCorners[0]), widget.localSize);
		strct.Waves.Clear();
		strct.Reefs.Clear();
		int j = 0;
		Vector2 val2 = default(Vector2);
		for (int num2 = (int)((float)widget.width * _decoration.DecoAmount * ((float)widget.height * _decoration.DecoAmount)); j < num2; j++)
		{
			val2.x = ((Rect)(ref val)).xMin + ((Rect)(ref val)).width * (float)rand.NextDouble();
			val2.y = ((Rect)(ref val)).yMin + ((Rect)(ref val)).height * (float)rand.NextDouble();
			if (!(((Vector2)(ref val2)).sqrMagnitude < (float)(num * num)) && !IsConflictPos(strct, val2, num, _decoration.DecoColliderSize))
			{
				if (rand.NextDouble() > (double)_decoration.ReefRatio)
				{
					UISprite uISprite = ((ListObjectPoolBase<GameObject>)strct.Waves).Add<UISprite>();
					SpriteData spriteData = _decoration.WaveSprites[rand.Next(_decoration.WaveSprites.Length)];
					spriteData.Set(uISprite);
					((Component)uISprite).transform.localPosition = Vector2.op_Implicit(val2);
					uISprite.MakePixelPerfect();
				}
				else
				{
					GameObject val3 = strct.Reefs.Add();
					val3.transform.localEulerAngles = Vector3.forward * (float)rand.Next(360);
					val3.transform.localPosition = Vector2.op_Implicit(val2);
				}
			}
		}
	}

	private bool IsConflictPos(PageStruct strct, Vector2 pos, float len, float decoLen)
	{
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_015c: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0162: Unknown result type (might be due to invalid IL or missing references)
		float num = len * len;
		float num2 = decoLen * decoLen;
		int i = 0;
		for (int num3 = strct.Regions.Count + strct.Waves.Count + strct.Reefs.Count + strct.DecoIsalnd.Count; i < num3; i++)
		{
			GameObject val;
			float num4;
			if (i < strct.Regions.Count)
			{
				val = strct.Regions[i];
				num4 = num;
			}
			else if (i < strct.Waves.Count + strct.Regions.Count)
			{
				val = strct.Waves[i - strct.Regions.Count];
				num4 = num2;
			}
			else if (i < strct.Waves.Count + strct.Regions.Count + strct.Reefs.Count)
			{
				val = strct.Reefs[i - strct.Waves.Count - strct.Regions.Count];
				num4 = num2;
			}
			else
			{
				val = strct.DecoIsalnd[i - strct.Regions.Count - strct.Waves.Count - strct.Reefs.Count];
				num4 = num2;
			}
			Vector2 val2 = Vector2.op_Implicit(val.transform.localPosition);
			Vector2 val3 = val2 - pos;
			if (((Vector2)(ref val3)).sqrMagnitude < num4)
			{
				return true;
			}
		}
		return false;
	}

	private int FindEmptyPosition(TerrainData.Biome biome = TerrainData.Biome.Unspecified)
	{
		int result = -1;
		for (int i = 0; i < _positions.Count; i++)
		{
			if (!_positions[i].Used && (biome == TerrainData.Biome.Unspecified || _positions[i].Biome == biome))
			{
				result = i;
				break;
			}
		}
		return result;
	}

	private void SetRegion(ListObjectPool nodes, int index, Route region)
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		_positions[index].Used = true;
		ExploreRegionNode exploreRegionNode = ((ListObjectPoolBase<GameObject>)nodes).Add<ExploreRegionNode>();
		((Component)exploreRegionNode).transform.localPosition = _positions[index].GetPosition(_aspectRatio);
		exploreRegionNode.Set(region);
	}

	private static int SortByLevel(Route r1, Route r2)
	{
		return r1.Region.Level - r2.Region.Level;
	}

	private void MakePath(ListObjectPool nodes, Vector2 start, Vector2 end)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		//IL_016d: Unknown result type (might be due to invalid IL or missing references)
		//IL_016e: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0183: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01de: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0201: Unknown result type (might be due to invalid IL or missing references)
		//IL_025a: Unknown result type (might be due to invalid IL or missing references)
		//IL_025f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0217: Unknown result type (might be due to invalid IL or missing references)
		//IL_0228: Unknown result type (might be due to invalid IL or missing references)
		//IL_023a: Unknown result type (might be due to invalid IL or missing references)
		//IL_023f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0244: Unknown result type (might be due to invalid IL or missing references)
		//IL_0249: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0308: Unknown result type (might be due to invalid IL or missing references)
		//IL_030d: Unknown result type (might be due to invalid IL or missing references)
		UIWidget component = nodes.BaseObject.GetComponent<UIWidget>();
		int num = Mathf.Max(component.width, component.height);
		Rect startRect = default(Rect);
		((Rect)(ref startRect))._002Ector(start - Vector2.one * (float)num * 0.5f, Vector2.one * (float)num);
		Rect endRect = default(Rect);
		((Rect)(ref endRect))._002Ector(end - Vector2.one * (float)num * 0.5f, Vector2.one * (float)num);
		Vector2 val = end - start;
		float magnitude = ((Vector2)(ref val)).magnitude;
		start = Vector2.Lerp(start, end, (float)((Component)_myRegion).GetComponent<UIWidget>().width * 0.4f / magnitude);
		end = Vector2.Lerp(end, start, (float)num * 0.4f / magnitude);
		_paths.Clear();
		_paths.Add(new Path(start));
		_paths.Add(new Path(end));
		int num2 = 0;
		int num3 = 0;
		while (true)
		{
			num3++;
			if (num3 > 1000)
			{
				_paths.Clear();
				_paths.Add(new Path(start));
				_paths.Add(new Path(end));
				break;
			}
			Vector2 position = _paths[num2].Position;
			Vector2 end2 = _paths[num2 + 1].Position;
			if (!MakePath(nodes, position, ref end2, num, startRect, endRect))
			{
				_paths.Insert(num2 + 1, new Path(end2));
				continue;
			}
			num2++;
			if (num2 != _paths.Count - 1)
			{
				continue;
			}
			break;
		}
		for (int i = 0; i < _paths.Count; i++)
		{
			Vector2 val2 = Vector2.zero;
			if (i > 0)
			{
				val2 += _paths[i].Position - _paths[i - 1].Position;
			}
			if (i < _paths.Count - 1)
			{
				val2 += _paths[i + 1].Position - _paths[i].Position;
			}
			_paths[i].Vector = ((Vector2)(ref val2)).normalized;
		}
		for (int j = 0; j < 2; j++)
		{
			int index = ((j != 0) ? (_paths.Count - 1) : 0);
			Vector2 vector = _paths[index].Vector;
			float num4 = Mathf.Atan2(vector.y, vector.x);
			float num5 = Random.value * 2f - 1f;
			num5 *= (float)Math.PI / 10f;
			num4 += num5;
			_paths[index].Vector = new Vector2(Mathf.Cos(num4), Mathf.Sin(num4));
		}
	}

	private bool MakePath(ListObjectPool nodes, Vector2 start, ref Vector2 end, int len, Rect startRect, Rect endRect)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		float num = (float)len * 0.5f;
		Rect val2 = default(Rect);
		for (int i = 0; i < nodes.Count; i++)
		{
			Vector2 val = Vector2.op_Implicit(nodes[i].transform.localPosition);
			if (!((Rect)(ref startRect)).Contains(val) && !((Rect)(ref endRect)).Contains(val))
			{
				float ratio;
				float angle;
				float num2 = CheckLineCross(start, end, val, out angle, out ratio);
				((Rect)(ref val2))._002Ector(val - Vector2.one * (float)len * 0.5f, Vector2.one * (float)len);
				if (Mathf.Abs(num2) < num && (((Rect)(ref val2)).Contains(end) || (ratio > 0f && ratio < 1f)))
				{
					angle = KMathUtil.NormalizeAngDeg(angle * 57.29578f);
					Vector2 val3 = end - start;
					Vector2 normalized = ((Vector2)(ref val3)).normalized;
					float num3 = Mathf.Sign(angle);
					normalized = ((!(num3 > 0f)) ? new Vector2(0f - normalized.y, normalized.x) : new Vector2(normalized.y, 0f - normalized.x));
					end = val + normalized * (num * 1.1f);
					return false;
				}
			}
		}
		return true;
	}

	public void DrawLine(Route route, Action onFinish = null)
	{
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		Role role = route.Region.Role();
		ListObjectPool listObjectPool = ((role != Role.Risky && role != Role.Troubled) ? _inners.Regions : _outters.Regions);
		if (listObjectPool == null)
		{
			return;
		}
		int num = -1;
		for (int i = 0; i < listObjectPool.Count; i++)
		{
			if (listObjectPool[i].GetComponent<ExploreRegionNode>().Route == route)
			{
				num = i;
				break;
			}
		}
		if (num != -1)
		{
			MakePath(listObjectPool, Vector2.zero, Vector2.op_Implicit(listObjectPool[num].transform.localPosition));
			((MonoBehaviour)this).StartCoroutine(CoDrawLine(onFinish));
		}
	}

	public void ClearLine()
	{
		_dots.Clear();
	}

	private IEnumerator CoDrawLine(Action onFinish)
	{
		float ratio = 0f;
		int index = 0;
		bool isValidCurve = false;
		KMathUtil.BezierCurve4 curve = default(KMathUtil.BezierCurve4);
		float remainDelta = 0f;
		_dots.Clear();
		while (index < _paths.Count - 1)
		{
			if (!isValidCurve)
			{
				Path s = _paths[index];
				Path e = _paths[index + 1];
				curve = KMathUtil.MakeBezierCurve4(s.Position, e.Position, s.Vector, e.Vector);
				isValidCurve = true;
			}
			float i;
			if (remainDelta > 0f)
			{
				i = remainDelta;
			}
			else
			{
				_dots.Add().transform.localPosition = Vector2.op_Implicit(curve.Get(ratio));
				i = 10f;
			}
			if (!curve.Next(i, ref ratio))
			{
				remainDelta = 10f * ratio;
				index++;
				ratio = 0f;
				isValidCurve = false;
			}
			else
			{
				remainDelta = 0f;
			}
			yield return (object)new WaitForSeconds(_option.DrawLineSpeed);
		}
		onFinish?.Invoke();
	}

	private float CheckLineCross(Vector2 p1, Vector2 p2, Vector2 point, out float angle, out float ratio)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		Vector2 val = p2 - p1;
		Vector2 val2 = point - p1;
		angle = Mathf.Atan2(val2.y, val2.x) - Mathf.Atan2(val.y, val.x);
		float magnitude = ((Vector2)(ref val2)).magnitude;
		ratio = magnitude * Mathf.Cos(angle) / ((Vector2)(ref val)).magnitude;
		return magnitude * Mathf.Sin(angle);
	}

	private void OnClickRegionNode(GameObject obj)
	{
		ExploreRegionNode component = obj.GetComponent<ExploreRegionNode>();
		if (!((Object)(object)component == (Object)null) && this.RouteClicked != null)
		{
			this.RouteClicked(component.Route);
		}
	}
}
