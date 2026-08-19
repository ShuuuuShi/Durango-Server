using System;
using System.Collections.Generic;
using InteractionData;
using L10N;
using Shared.System;
using UnityEngine;

public static class PrologueInteractionExtension
{
	public static void MakePrologueMode()
	{
		UIManager.FindScript<InteractionButtonGroup>().InteractionSearchOverride = Prologue_Interaction_Search_Function;
		GameSystem<InteractionSystem>.Instance().PreTouchTarget += InteractionSystem_PreTouchTarget;
		InteractionButtonGroup.RefreshInteractions();
		InteractionButtonGroup.IgnoreAnimals = true;
		((Component)UIManager.FindScript<CombatGroup>().TargetSelectContainer).gameObject.SetActive(false);
	}

	public static void AdjustActionButtonPosition()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		ActionButtonGroup actionButtonGroup = UIManager.FindScript<ActionButtonGroup>();
		ActionButtonContainer actionButtons = actionButtonGroup.ActionButtons;
		Vector3 localPosition = ((Component)actionButtons).transform.localPosition;
		localPosition.y += 100f;
		((Component)actionButtons).transform.localPosition = localPosition;
	}

	public static void UndoPrologueMode()
	{
		UIManager.FindScript<InteractionButtonGroup>().InteractionSearchOverride = null;
		GameSystem<InteractionSystem>.Instance().PreTouchTarget -= InteractionSystem_PreTouchTarget;
		InteractionButtonGroup.IgnoreAnimals = false;
	}

	private static void InteractionSystem_PreTouchTarget(InteractionObject obj, ref bool result)
	{
		if (obj.ObjectType == InteractionObject.Type.Animal)
		{
			OnTouchedPrologueAnimal();
			result = true;
		}
	}

	private static void OnTouchedPrologueAnimal()
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		string name = T._("미지의 생물");
		InteractionMenuData data = new InteractionMenuData(Shared.System.Interaction.Attack);
		data.Icon = "icon_purpose_weapon";
		data.SetColor(UIManager.UIRed);
		InteractionMenuList menuList = GameSystem<InteractionSystem>.Instance().MenuList;
		menuList.Reset();
		menuList.Add(data);
		menuList.Name = name;
		menuList.Apply();
	}

	private static KeyValuePair<IList<GameObject>, float> Prologue_Interaction_Search_Function()
	{
		List<GameObject> list = new List<GameObject>();
		float value = 0f;
		if (KSingleton<PrologueManager>.Instance().BeginIntreaction)
		{
			SearchInteractionsNearbyPrologue(list, SelectableObject.FindSelectable);
			value = 1000f;
		}
		else if (KSingleton<PrologueManager>.Instance().CurrentState == PrologueManager.State.CharacterSelect)
		{
			SearchInteractionsNearbyPrologue(list, (GameObject o) => (!((Object)(object)o.GetComponent<TriggerPrologueSelectCharacter>() != (Object)null)) ? null : o);
			value = 1000f;
		}
		return new KeyValuePair<IList<GameObject>, float>(list, value);
	}

	private static void SearchInteractionsNearbyPrologue(List<GameObject> list, Func<GameObject, GameObject> filter)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		InteractionSystem.GetNearObjectsInternal(list, LayerMask.op_Implicit(LayerHelper.DefaultMask), 800f, filter);
	}
}
