using System;
using System.Collections.Generic;
using Durango.Utils;
using Shared.Region;
using UnityEngine;
using Yaml;

namespace Durango.UI;

public class WorldRoutesUnstableArea : MonoBehaviour
{
	[SerializeField]
	private float _xMargin;

	private List<ExploreAreaNode>[] _exploreAreaNodes;

	public ExploreAreaNode Node;

	private int _minLevel;

	private int _maxLevel;

	private WorldRoutesViewer _parent;

	private float _shadowOffset;

	private bool _isInit;

	public void Init(int minLv, int maxLv)
	{
		if (_isInit)
		{
			return;
		}
		_isInit = true;
		_parent = UIUtility.FindComponentInParent<WorldRoutesViewer>(base.gameObject);
		_minLevel = minLv;
		_maxLevel = maxLv;
		_exploreAreaNodes = new List<ExploreAreaNode>[KUtility.GetSize(WorldRoutesViewer.BiomeGrid)];
		for (int i = 0; i < _exploreAreaNodes.Length; i++)
		{
			_exploreAreaNodes[i] = new List<ExploreAreaNode>();
		}
		List<RegionTemplate> unstableAreaList = GameSystem<MapSystem>.Instance().UnstableAreaList;
		HashSet<Pair<Biome, int>> hashSet = new HashSet<Pair<Biome, int>>();
		for (int j = 0; j < unstableAreaList.Count; j++)
		{
			RegionTemplate regionTemplate = unstableAreaList[j];
			int level = regionTemplate.Level;
			if ((_minLevel >= 0 && level < _minLevel) || (_maxLevel >= 0 && level > _maxLevel))
			{
				continue;
			}
			if (regionTemplate.Role == Role.Risky)
			{
				Pair<Biome, int> item = new Pair<Biome, int>(regionTemplate.MajorBiome(), regionTemplate.Level);
				if (hashSet.Contains(item))
				{
					continue;
				}
				hashSet.Add(item);
			}
			int biomeGridIndex = GetBiomeGridIndex(regionTemplate.MajorBiome());
			if (biomeGridIndex != -1)
			{
				ExploreAreaNode component = base.gameObject.AddChild(_parent.AreaNodeBase.gameObject).GetComponent<ExploreAreaNode>();
				component.gameObject.SetActive(value: true);
				component.Clicked = OnClickNode;
				component.Template = regionTemplate;
				component.gameObject.AddComponent<UIDragScrollView>().scrollView = _parent.ScrollView;
				_exploreAreaNodes[biomeGridIndex].Add(component);
			}
		}
	}

	private static void OnClickNode(ExploreAreaNode node)
	{
		UISound.PlayClick(UISound.ClickType.ButtonDefault);
		UIManager.FindScript<ExploreGroup>().SelectUnstableArea(node.Template);
	}

	private int GetBiomeGridIndex(Biome biome)
	{
		int i = 0;
		for (int size = KUtility.GetSize(WorldRoutesViewer.BiomeGrid); i < size; i++)
		{
			Biome[] array = WorldRoutesViewer.BiomeGrid[i];
			int j = 0;
			for (int size2 = KUtility.GetSize(array); j < size2; j++)
			{
				if (array[j] == biome)
				{
					return i;
				}
			}
		}
		return -1;
	}

	public void Set(bool riskyOnly = false)
	{
		for (int i = 0; i < _exploreAreaNodes.Length; i++)
		{
			List<ExploreAreaNode> list = _exploreAreaNodes[i];
			for (int j = 0; j < _exploreAreaNodes[i].Count; j++)
			{
				ExploreAreaNode exploreAreaNode = list[j];
				if (exploreAreaNode.Template.Role == Role.Risky)
				{
					exploreAreaNode.Set();
					continue;
				}
				exploreAreaNode.gameObject.SetActive(!riskyOnly);
				if (!riskyOnly)
				{
					exploreAreaNode.Set();
				}
			}
		}
	}

	public void UpdateLayout(global::System.Random rand)
	{
		if (!_isInit)
		{
			return;
		}
		int num = 0;
		int num2 = 0;
		UIWidget component = GetComponent<UIWidget>();
		Vector3 vector = component.localCorners[0];
		float num3 = (float)component.height / (float)(_exploreAreaNodes.Length + 1);
		vector += new Vector3(_xMargin, num3);
		_shadowOffset = 0f;
		bool flag = false;
		using (Reusable<List<bool>> reusable = ReusableList<bool>.Pop())
		{
			while (true)
			{
				int num4 = 0;
				for (int i = 0; i < _exploreAreaNodes.Length; i++)
				{
					for (int j = 0; j < _exploreAreaNodes[i].Count; j++)
					{
						int level = _exploreAreaNodes[i][j].Template.Level;
						if (level > num)
						{
							num4 = ((num4 != 0) ? Mathf.Min(level, num4) : level);
							break;
						}
					}
				}
				if (num4 == 0)
				{
					break;
				}
				int num5 = 0;
				reusable.Value.Clear();
				for (int k = 0; k < _exploreAreaNodes.Length; k++)
				{
					int num6 = 0;
					for (int l = 0; l < _exploreAreaNodes[k].Count; l++)
					{
						ExploreAreaNode exploreAreaNode = _exploreAreaNodes[k][l];
						int level2 = exploreAreaNode.Template.Level;
						if (level2 > num)
						{
							if (level2 > num4)
							{
								break;
							}
							Vector3 vector2 = new Vector3((float)(num2 + num6) * _xMargin, (float)k * num3);
							if (num6 < reusable.Value.Count)
							{
								reusable.Value[num6] = !reusable.Value[num6];
							}
							else
							{
								reusable.Value.Add(rand.NextDouble() > 0.5);
							}
							if (reusable.Value[num6])
							{
								vector2.x += _xMargin * 0.5f;
							}
							Vector3 vector3 = WorldRoutesViewer.GetRandomPositionOffset(rand);
							Vector3 localPosition = vector + vector2 + vector3;
							exploreAreaNode.transform.localPosition = localPosition;
							num6++;
							if (GameSystem<StatisticsSystem>.Instance().Level < exploreAreaNode.Template.AvailableLevel)
							{
								_shadowOffset = ((!(_shadowOffset > 0f)) ? vector2.x : Mathf.Min(_shadowOffset, vector2.x));
							}
							else
							{
								flag = true;
							}
						}
					}
					num5 = Mathf.Max(num5, num6);
					reusable.Value.RemoveRange(num6, reusable.Value.Count - num6);
				}
				num = num4;
				num2 += num5;
			}
		}
		component.width = (int)(_xMargin * ((float)num2 + 1.5f));
		if (flag)
		{
			if (_shadowOffset == 0f)
			{
				_shadowOffset = component.width;
			}
		}
		else
		{
			_shadowOffset = 0f;
		}
	}

	public float GetShadowOffset()
	{
		return _shadowOffset;
	}

	public ExploreAreaNode FindRoutesArea(Role role, Biome biome, int level)
	{
		int i = 0;
		for (int size = KUtility.GetSize(_exploreAreaNodes); i < size; i++)
		{
			int j = 0;
			for (int size2 = KUtility.GetSize(_exploreAreaNodes[i]); j < size2; j++)
			{
				ExploreAreaNode exploreAreaNode = _exploreAreaNodes[i][j];
				if (exploreAreaNode.Template.Role == role && exploreAreaNode.Template.Level == level && (biome == Biome.Invalid || biome == exploreAreaNode.Template.MajorBiome()))
				{
					return exploreAreaNode;
				}
			}
		}
		return null;
	}
}
