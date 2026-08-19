using Durango.Model;
using Messages;
using UnityEngine;

public class Mannequin : ArtifactComponent
{
	private CostumableModel _costumable;

	private MannequinDisplayInfo? _display;

	private static readonly Material[] SkinMaterials = new Material[2];

	private static Material GetSkinMaterial(bool isMale)
	{
		int num = ((!isMale) ? 1 : 0);
		if (SkinMaterials[num] == null)
		{
			SkinMaterials[num] = Resources.Load<Material>((!isMale) ? "Costume/f_skin_mannequin" : "Costume/m_skin_mannequin");
		}
		return SkinMaterials[num];
	}

	public override bool OnUpdateDisplay(ArtifactDisplay msg)
	{
		_display = msg.MannequinInfo;
		RefreshCostume();
		return false;
	}

	public override void ResourcesLoadCompleted()
	{
		_costumable = base.Artifact.GetComponentInChildren<CostumableModel>();
		if (!(_costumable == null))
		{
			_costumable.ChangeCostumeColor(CharacterCostume.CostumeType.Skin, new ItemColor(Color.gray));
		}
		RefreshCostume();
	}

	private void RefreshCostume()
	{
		if (!(_costumable == null))
		{
			MannequinDisplayInfo? display = _display;
			string text = ((!display.HasValue) ? null : _display.Value.Body);
			if (string.IsNullOrEmpty(text))
			{
				_costumable.SetSkinMaterial(null);
				_costumable.ChangeCostume(CharacterCostume.CostumeType.Body, (!_costumable.IsMale) ? "Models/PC/Female/Body/f_body_mannequin.FBX" : "Models/PC/Male/Body/m_body_mannequin.FBX");
			}
			else
			{
				_costumable.SetSkinMaterial(GetSkinMaterial(_costumable.IsMale));
				_costumable.ChangeCostume(CharacterCostume.CostumeType.Body, text);
			}
			MannequinDisplayInfo? display2 = _display;
			string fileName = ((!display2.HasValue) ? null : _display.Value.Head);
			_costumable.ChangeCostume(CharacterCostume.CostumeType.Head, fileName);
			MannequinDisplayInfo? display3 = _display;
			if (display3.HasValue)
			{
				MannequinDisplayInfo value = _display.Value;
				_costumable.ChangeCostumeColor(CharacterCostume.CostumeType.Body, new ItemColor(value.BodyColor));
				_costumable.ChangeCostumeColor(CharacterCostume.CostumeType.Head, new ItemColor(value.HeadColor));
			}
		}
	}
}
