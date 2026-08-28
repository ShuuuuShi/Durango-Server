using System.Diagnostics;
using System.Linq;
using Durango.System;
using UnityEngine;

[ResourcePath("ui_prefab_map")]
public class UIPrefabMap : ResourceSingleton<UIPrefabMap>
{
	public enum Type
	{
		Mobile,
		PC
	}

	public enum Category
	{
		Main,
		Title,
		Prologue,
		PrologueAdditional
	}

	[SortedUnityObjectList]
	[SerializeField]
	private GameObject[] _mainMobile;

	[SortedUnityObjectList]
	[SerializeField]
	private GameObject[] _prologueMobile;

	[SortedUnityObjectList]
	[SerializeField]
	private GameObject[] _titleMobile;

	[SortedUnityObjectList]
	[SerializeField]
	private GameObject[] _mainPC;

	[SortedUnityObjectList]
	[SerializeField]
	private GameObject[] _prologuePC;

	[SortedUnityObjectList]
	[SerializeField]
	private GameObject[] _titlePC;

	[SortedUnityObjectList]
	[SerializeField]
	private GameObject[] _prologueAdditionalMobile;

	[SortedUnityObjectList]
	[SerializeField]
	private GameObject[] _prologueAdditionalPC;

	public GameObject[] GetUIList(Type uiType, Category uiCategory)
	{
		switch (uiType)
		{
		case Type.Mobile:
			switch (uiCategory)
			{
			case Category.Main:
				return _mainMobile;
			case Category.Title:
				return _titleMobile;
			case Category.Prologue:
				return _prologueMobile;
			case Category.PrologueAdditional:
				return _prologueAdditionalMobile;
			}
			break;
		case Type.PC:
			switch (uiCategory)
			{
			case Category.Main:
				return _mainPC;
			case Category.Title:
				return _titlePC;
			case Category.Prologue:
				return _prologuePC;
			case Category.PrologueAdditional:
				return _prologueAdditionalPC;
			}
			break;
		}
		return null;
	}

	public GameObject[] GetMain()
	{
		return GetUIList(Platform.Instance.UIType, Category.Main);
	}

	public GameObject[] GetPrologue()
	{
		GameObject[] uIList = GetUIList(Platform.Instance.UIType, Category.Prologue);
		GameObject[] uIList2 = GetUIList(Platform.Instance.UIType, Category.PrologueAdditional);
		return uIList.Concat(uIList2).ToArray();
	}

	public GameObject[] GetTitle()
	{
		// The title/Main screen keeps the PC button layout. In-game UI remains mobile.
		return _titlePC;
	}

	[Conditional("UNITY_EDITOR")]
	public void SetList(Type uiType, Category uiCategory, GameObject[] uiList)
	{
		switch (uiType)
		{
		case Type.Mobile:
			switch (uiCategory)
			{
			case Category.Main:
				_mainMobile = uiList;
				break;
			case Category.Title:
				_titleMobile = uiList;
				break;
			case Category.Prologue:
				_prologueMobile = uiList;
				break;
			case Category.PrologueAdditional:
				_prologueAdditionalMobile = uiList;
				break;
			}
			break;
		case Type.PC:
			switch (uiCategory)
			{
			case Category.Main:
				_mainPC = uiList;
				break;
			case Category.Title:
				_titlePC = uiList;
				break;
			case Category.Prologue:
				_prologuePC = uiList;
				break;
			case Category.PrologueAdditional:
				_prologueAdditionalPC = uiList;
				break;
			}
			break;
		}
	}
}
