using System;
using Durango.Logic.Clusters;
using Durango.UI.Control;
using L10N;
using UnityEngine;

namespace Durango.UI;

public class TitleClusterSelection : MonoBehaviour
{
	[SerializeField]
	private KScrollView _clusterListScroll;

	[SerializeField]
	private SelectableWidget _okButton;

	[SerializeField]
	private UILabel _okButtonLabel;

	[SerializeField]
	private UILabel _titleLabel;

	private string _selectedClusterKey;

	private Action<string> _confirmCluster;

	private void Awake()
	{
		_clusterListScroll.UpdateLayout();
		_titleLabel.text = ManualTranslator.ServerSelection;
		_okButtonLabel.text = ManualTranslator.Confirm;
		_okButton.Clicked = OkButton_Clicked;
		ListObjectPool nodes = _clusterListScroll.Nodes;
		nodes.Init(delegate(GameObject obj)
		{
			Selectable component = obj.GetComponent<Selectable>();
			component.Clicked = (Action)Delegate.Combine(component.Clicked, new Action(OnClickCluster));
		});
	}

	public void ShowClusters(Clusters clusters, Action<string> confirmCluster, string currentClusterKey)
	{
		_confirmCluster = confirmCluster;
		string[] clusterKeys = clusters.GetClusterKeys();
		ListObjectPool nodes = _clusterListScroll.Nodes;
		int index = -1;
		nodes.BeginLoad();
		int i = 0;
		for (int size = KUtility.GetSize(clusterKeys); i < size; i++)
		{
			string text = clusterKeys[i];
			TitleClusterInfo node = nodes.GetNext().GetComponent<TitleClusterInfo>();
			node.Init(text, clusters);
			node.SetPlayerInfo(clusters.GetPlayerCount(text));
			clusters.GetOrRequestAccounts(text, delegate(Account account)
			{
				node.SetPlayerInfo((account == null) ? (-1) : KUtility.GetSize(account.Players));
			});
			node.DoubleClicked = ConfirmCluster;
			if (text == currentClusterKey)
			{
				index = i;
			}
		}
		nodes.EndLoad();
		SelectCluster(index);
		_clusterListScroll.ResetPosition();
	}

	public void OnClickCluster()
	{
		int index = _clusterListScroll.Nodes.IndexOf(Selectable.Current.gameObject);
		SelectCluster(index);
	}

	private void SelectCluster(int index)
	{
		int count = _clusterListScroll.Nodes.Count;
		if (index < 0 || index >= count)
		{
			return;
		}
		for (int i = 0; i < _clusterListScroll.Nodes.Count; i++)
		{
			TitleClusterInfo component = _clusterListScroll.Nodes[i].GetComponent<TitleClusterInfo>();
			if (!(component == null))
			{
				component.Selected = i == index;
				if (component.Selected)
				{
					_selectedClusterKey = component.ClusterKey;
				}
			}
		}
	}

	private void OkButton_Clicked()
	{
		ConfirmCluster();
	}

	public void ConfirmCluster()
	{
		if (_confirmCluster != null)
		{
			_confirmCluster(_selectedClusterKey);
		}
	}
}
