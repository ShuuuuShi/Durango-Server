using System;
using System.Linq;
using Durango.UI.Control;
using Durango.Utils;
using L10N;
using Messages;
using UnityEngine;

namespace Durango.UI;

public class PetListWidget : MonoBehaviour, IUIInitializable
{
	public enum PetType
	{
		[T.EnumName("동반")]
		Spawned,
		[T.EnumName("소환 가능")]
		Have,
		[T.EnumName("축사에 보관중")]
		InCage
	}

	[SerializeField]
	private KScrollView _scrollView;

	public event Action<Pet> PetSelected;

	void IUIInitializable.Init()
	{
		_scrollView.Nodes.Init(delegate(GameObject obj)
		{
			PetListNodeWidget component = obj.GetComponent<PetListNodeWidget>();
			component.SelectedPet = (Action<Pet>)Delegate.Combine(component.SelectedPet, new Action<Pet>(OnPetSelected));
		});
		UIManager.AddOnPreScreenResize(delegate
		{
			_scrollView.ScrollView.movement = ((!UIManager.IsPortraitWidget(base.gameObject)) ? UIScrollView.Movement.Vertical : UIScrollView.Movement.Horizontal);
		});
	}

	public void Set(PetGroup.PetOwnType petOwnType, PetsInfo info, Action<GameObject> addButtonClicked)
	{
		ListObjectPool nodes = _scrollView.Nodes;
		nodes.BeginLoad();
		if (petOwnType == PetGroup.PetOwnType.Holding)
		{
			PetType[] array = Enums<PetType>.All();
			int i = 0;
			for (int size = KUtility.GetSize(array); i < size; i++)
			{
				PetListNodeWidget petListNodeWidget = null;
				PetType petType = array[i];
				Pet[] data = info.Pets.Data;
				if (data != null)
				{
					foreach (Pet item in data.OrderBy((Pet x) => x.Name))
					{
						if (IsValidType(item, petType))
						{
							if (petListNodeWidget == null)
							{
								petListNodeWidget = nodes.GetNext().GetComponent<PetListNodeWidget>();
								petListNodeWidget.BeginLoad(petType.GetName(), addButtonClicked);
							}
							petListNodeWidget.AddPet(item);
						}
					}
				}
				if (petListNodeWidget != null)
				{
					petListNodeWidget.EndLoad();
				}
			}
		}
		if (petOwnType == PetGroup.PetOwnType.Grazing)
		{
			PetListNodeWidget component = nodes.GetNext().GetComponent<PetListNodeWidget>();
			component.BeginLoad(T._("방목 중"), addButtonClicked);
			if (info.GrazedPets.Data != null)
			{
				foreach (Pet item2 in info.GrazedPets.Data.OrderBy((Pet x) => x.Name))
				{
					component.AddPet(item2);
				}
			}
			component.EndLoad();
		}
		nodes.EndLoad();
		UIUtility.UpdateAnchors(base.transform);
		_scrollView.Reposition();
	}

	public string GetFirstPetId()
	{
		ListObjectPool nodes = _scrollView.Nodes;
		int i = 0;
		for (int count = nodes.Count; i < count; i++)
		{
			string firstPetId = nodes[i].GetComponent<PetListNodeWidget>().GetFirstPetId();
			if (!string.IsNullOrEmpty(firstPetId))
			{
				return firstPetId;
			}
		}
		return string.Empty;
	}

	private void OnPetSelected(Pet pet)
	{
		if (this.PetSelected != null)
		{
			this.PetSelected(pet);
		}
	}

	public void Select(Pet pet)
	{
		string entityId = pet.EntityId;
		ListObjectPool nodes = _scrollView.Nodes;
		for (int i = 0; i < nodes.Count; i++)
		{
			nodes[i].GetComponent<PetListNodeWidget>().Select(entityId);
		}
	}

	private static bool IsValidType(Pet pet, PetType type)
	{
		if (Singleton<PetManager>.Instance().GetPet(pet.EntityId).HasValue)
		{
			return type == PetType.Spawned;
		}
		CageInfo? cageInfo = pet.CageInfo;
		if (cageInfo.HasValue && !string.IsNullOrEmpty(pet.CageInfo.Value.RegionId))
		{
			return type == PetType.InCage;
		}
		return type == PetType.Have;
	}
}
