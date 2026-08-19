using System;
using System.Collections.Generic;
using System.Linq;
using Durango.UI.Control;
using L10N;
using Messages;
using UnityEngine;

namespace Durango.UI;

public class PetListNodeWidget : MonoBehaviour, IScreenResizeReceiver
{
	public Action<Pet> SelectedPet;

	[SerializeField]
	private UIWidget _titleWidget;

	[SerializeField]
	private UILabel _titleLabel;

	[SerializeField]
	private UILabel _countLabel;

	[SerializeField]
	private PetListInfoNode _baseInfoNode;

	[SerializeField]
	private UIWidget _addButton;

	private ListObjectPool<PetListInfoNode> _infoNodes;

	private bool _isInit;

	private void Init()
	{
		if (!_isInit)
		{
			_isInit = true;
			_infoNodes = new ListObjectPool<PetListInfoNode>();
			_infoNodes.BaseObject = _baseInfoNode;
			_infoNodes.Init(delegate(PetListInfoNode node)
			{
				node.Clicked = (Action)Delegate.Combine(node.Clicked, new Action(OnClickPetInfoNode));
			});
		}
	}

	public void BeginLoad(string title, Action<GameObject> addButtonClicked)
	{
		Init();
		_titleLabel.text = title;
		_infoNodes.BeginLoad();
		bool flag = addButtonClicked != null;
		_addButton.gameObject.SetActive(flag);
		if (flag)
		{
			UIEventListener.Get(_addButton.gameObject).onClick = delegate(GameObject go)
			{
				addButtonClicked(go);
			};
		}
	}

	public void AddPet(Pet pet)
	{
		PetListInfoNode next = _infoNodes.GetNext();
		next.Set(pet);
	}

	public void EndLoad()
	{
		_infoNodes.EndLoad();
		if (_countLabel != null)
		{
			_countLabel.text = T._("{0} 마리", _infoNodes.Count);
		}
		UpdateLayout();
	}

	private void UpdateLayout()
	{
		if (_isInit)
		{
			List<UIWidget> list2;
			if (_addButton.gameObject.activeSelf)
			{
				List<UIWidget> list = new List<UIWidget>();
				list.Add(_addButton);
				list2 = list;
			}
			else
			{
				list2 = new List<UIWidget>();
			}
			List<UIWidget> list3 = list2;
			list3.AddRange(_infoNodes.Select((PetListInfoNode node) => node.GetComponent<UIWidget>()));
			UIWidget component = GetComponent<UIWidget>();
			Vector3 localCenter = component.localCenter;
			localCenter.y -= (float)_titleWidget.height * 0.5f;
			if (UIManager.IsPortraitScreen)
			{
				float num = UIUtility.WidgetsReposition(list3, Vector3.right, localCenter, 0f, 0.5f);
				component.SetDimensions((int)num, _baseInfoNode.Widget.height + _titleWidget.height);
			}
			else
			{
				float num2 = UIUtility.WidgetsReposition(list3, Vector3.down, localCenter, 0f, 0.5f);
				component.SetDimensions(_baseInfoNode.Widget.width, (int)((float)_titleWidget.height + num2));
			}
		}
	}

	public string GetFirstPetId()
	{
		if (_infoNodes.Count > 0)
		{
			return _infoNodes[0].Pet.EntityId;
		}
		return string.Empty;
	}

	public bool Select(string id)
	{
		bool result = false;
		for (int i = 0; i < _infoNodes.Count; i++)
		{
			if (!string.IsNullOrEmpty(id) && _infoNodes[i].Pet.EntityId == id)
			{
				_infoNodes[i].Selected = true;
				result = true;
			}
			else
			{
				_infoNodes[i].Selected = false;
			}
		}
		return result;
	}

	private void OnClickPetInfoNode()
	{
		PetListInfoNode petListInfoNode = Selectable.Current as PetListInfoNode;
		if (!(petListInfoNode == null) && SelectedPet != null)
		{
			SelectedPet(petListInfoNode.Pet);
		}
	}

	public void OnChangeScreenSize()
	{
		UpdateLayout();
	}
}
