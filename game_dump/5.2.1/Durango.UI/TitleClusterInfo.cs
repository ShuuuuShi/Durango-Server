using Durango.Logic.Clusters;
using Durango.UI.Control;
using L10N;
using UnityEngine;

namespace Durango.UI;

public class TitleClusterInfo : SelectableWidget
{
	[SerializeField]
	private UILabel _clusterName;

	[SerializeField]
	private UILabel _characterInfo;

	[SerializeField]
	private UILabel _statusDescription;

	[SerializeField]
	private UILabel _recommended;

	public string ClusterKey { get; private set; }

	private void Awake()
	{
		TitleUIRootResizer.AddOnScreenResized(OnScreenResize);
	}

	public void Init(string key, Clusters clusters)
	{
		Cluster cluster = clusters.GetCluster(key);
		_clusterName.text = cluster.GetName(LocalizeSystem.Locale);
		ClusterKey = key;
		_statusDescription.gameObject.SetActive(cluster.IsInMaintenance());
		_statusDescription.text = cluster.GetMaintenanceText(LocalizeSystem.Locale, em: false);
		_recommended.gameObject.SetActive(cluster.IsRecommendable);
		_recommended.text = ManualTranslator.Recommended;
	}

	public void SetPlayerInfo(int userCount)
	{
		bool flag = userCount < 0;
		string text = ((!flag) ? string.Format("{0} {1}", "icon_mainhud_social".ToEncodedIcon(), userCount) : ManualTranslator.Loading);
		_characterInfo.text = text;
		_characterInfo.color = ((!flag) ? Color.white : new Color(1f, 1f, 1f, 0.6f));
	}

	private void OnScreenResize()
	{
		GetComponent<UIWidget>().width = ((!TitleUIRootResizer.IsPortrait) ? 762 : 640);
		UIUtility.ResetAndUpdateAnchors(base.transform);
	}
}
