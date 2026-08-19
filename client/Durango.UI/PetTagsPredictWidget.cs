using System.Collections.Generic;
using Durango.Logic.Item;
using Durango.UI.Control;
using Durango.Utils;
using L10N;
using Messages;
using UnityEngine;
using Yaml;
using Yaml.Util;

namespace Durango.UI;

public class PetTagsPredictWidget : MonoBehaviour
{
	[SerializeField]
	private UILabel _emptyLabel;

	[SerializeField]
	private KScrollView _tagsList;

	private bool _reset = true;

	private void Start()
	{
		_emptyLabel.text = T._("추가 효과 없음");
	}

	private void OnDisable()
	{
		_reset = true;
	}

	public void Set(IList<ItemData> items)
	{
		_tagsList.Nodes.BeginLoad();
		using (Reusable<Dictionary<string, float>> reusable = ReusableDictionary<string, float>.Pop())
		{
			int i = 0;
			for (int size = KUtility.GetSize(items); i < size; i++)
			{
				ItemData itemData = items[i];
				foreach (Performance performance in itemData.Performances)
				{
					foreach (KeyValuePair<string, float> num in performance.Nums)
					{
						if (TagYaml.IsPetTag(num.Key))
						{
							reusable.Value[num.Key] = reusable.Value.Get(num.Key, 0f) + num.Value;
						}
					}
				}
			}
			if (reusable.Value.Count > 0)
			{
				foreach (KeyValuePair<string, float> item in reusable.Value)
				{
					if (SingletonDict<string, Yaml.Tag>.Instance.TryGetValue(item.Key, out var value))
					{
						PetTagPredictItemWidget component = _tagsList.Nodes.GetNext().GetComponent<PetTagPredictItemWidget>();
						int petMilestoneDiffLevel = PetUtil.GetPetMilestoneDiffLevel(item.Value);
						component.Set(value, petMilestoneDiffLevel);
					}
				}
			}
		}
		_tagsList.Nodes.EndLoad();
		_tagsList.Reposition(_reset, !_reset);
		_emptyLabel.gameObject.SetActive(_tagsList.Nodes.Count == 0);
		_reset = false;
	}
}
