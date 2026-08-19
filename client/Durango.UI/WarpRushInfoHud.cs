using System.Collections.Generic;
using Durango.Logic;
using Durango.UI.Control;
using Shared.Season2;
using UnityEngine;

namespace Durango.UI;

public class WarpRushInfoHud : MonoBehaviour, IUIInitializable
{
	[SerializeField]
	private UIWidget _widget;

	[SerializeField]
	private ListObjectPool _resourceInfo;

	[SerializeField]
	private UIWidget _resourceBgWidget;

	private readonly ResourceType[] _resourceTypes = new ResourceType[2]
	{
		ResourceType.AlphaStone,
		ResourceType.BravoStone
	};

	private readonly Dictionary<ResourceType, KeyValueLabel> _kvLabels = new Dictionary<ResourceType, KeyValueLabel>();

	void IUIInitializable.Init()
	{
		bool flag = GameManager.Region.IsWarpRush();
		base.gameObject.SetActive(flag);
		if (flag)
		{
			GameSystem<WarpRushSystem>.Instance().RegionResourceUpdated += WarpRushSystem_RegionResourceUpdated;
			_resourceInfo.BeginLoad();
			int num = 0;
			ResourceType[] resourceTypes = _resourceTypes;
			foreach (ResourceType resourceType in resourceTypes)
			{
				GameObject next = _resourceInfo.GetNext();
				Transform transform = next.transform.Find("kvLabel");
				KeyValueLabel component = transform.GetComponent<KeyValueLabel>();
				component.Set($"[icon={WarpRushSystem.GetResourceIcon(resourceType, small: true)}]", 0.ToString());
				_kvLabels.Add(resourceType, component);
				num++;
			}
			_resourceInfo.EndLoad();
			_widget.width = _resourceInfo.BaseObject.GetComponent<UIWidget>().width * _resourceInfo.Count;
			UIUtility.WidgetsReposition(_resourceInfo, _widget, Vector3.right);
		}
	}

	private void WarpRushSystem_RegionResourceUpdated()
	{
		WarpRushSystem warpRushSystem = GameSystem<WarpRushSystem>.Instance();
		ResourceType[] resourceTypes = _resourceTypes;
		foreach (ResourceType resourceType in resourceTypes)
		{
			int warpRushRegionResource = warpRushSystem.GetWarpRushRegionResource(resourceType);
			_kvLabels[resourceType].SetValue(warpRushRegionResource.ToString());
		}
	}
}
