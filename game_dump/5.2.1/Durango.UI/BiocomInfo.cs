using System.Collections.Generic;
using JetBrains.Annotations;
using L10N;
using UnityEngine;

namespace Durango.UI;

public class BiocomInfo : DiscoveryInfo
{
	private static readonly Dictionary<string, bool> UnknownBiocoms = new Dictionary<string, bool> { { "?", false } };

	public override void ShowUnknown()
	{
		Set(UnknownBiocoms);
	}

	public void Set([NotNull] Dictionary<string, bool> biocomNames)
	{
		int size = KUtility.GetSize(biocomNames);
		if (size == 0)
		{
			base.gameObject.SetActive(value: false);
			return;
		}
		int num = 0;
		_nodes.BeginLoad();
		foreach (KeyValuePair<string, bool> biocomName in biocomNames)
		{
			GameObject next = _nodes.GetNext();
			UILabel component = next.transform.Find("Label").GetComponent<UILabel>();
			Transform transform = next.transform.Find("Check");
			if (biocomName.Value)
			{
				component.text = biocomName.Key;
				transform.gameObject.SetActive(value: true);
				num++;
			}
			else
			{
				component.text = T._("?");
				transform.gameObject.SetActive(value: false);
			}
		}
		_nodes.EndLoad();
		string countLabel = $"<em>{num}</em>/{size}";
		SetCountLabel(countLabel);
		Vector3[] localCorners = _nodesWidget.localCorners;
		Vector3 basePos = Vector3.Lerp(localCorners[1], localCorners[2], 0.5f) + new Vector3(0f, -20f);
		float num2 = UIUtility.WidgetsReposition(_nodes, Vector3.down, basePos, 20f);
		_nodesWidget.height = (int)num2 + 40;
		UIWidget component2 = GetComponent<UIWidget>();
		_layout.UpdateLayout(component2.width, 0f);
		base.gameObject.SetActive(value: true);
	}
}
