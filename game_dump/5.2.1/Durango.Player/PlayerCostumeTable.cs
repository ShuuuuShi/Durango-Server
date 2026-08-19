using System;
using System.Collections.Generic;
using System.Linq;
using Durango.Utils.Extensions;
using UnityEngine;

namespace Durango.Player;

[ResourcePath("player_costume_table")]
public class PlayerCostumeTable : ResourceSingleton<PlayerCostumeTable>
{
	public enum Category
	{
		Body,
		Hair,
		Head,
		Beard
	}

	public enum ClothState
	{
		Normal,
		Torn,
		Nothing
	}

	[Serializable]
	public class PreviewableDatum
	{
		[SerializeField]
		public Texture PreviewTexture;

		[SerializeField]
		public string BaseName;

		[SerializeField]
		public string AssetBundlePathBase;

		[SerializeField]
		public string FileDirectory;
	}

	[Serializable]
	public class SimpleDatum
	{
		[SerializeField]
		public string BaseName;

		[SerializeField]
		public string AssetBundlePathBase;

		[SerializeField]
		public string FileDirectory;
	}

	public const string DeaultMaleBody = "Models/PC/Male/Body/m_body_nothing.FBX";

	public const string DeaultFemaleBody = "Models/PC/Female/Body/f_body_nothing.FBX";

	public const string DeaultInnerMaleBody = "Models/PC/Male/Inner/m_inner_basic.FBX";

	public const string DeaultInnerFemaleBody = "Models/PC/Female/Inner/f_inner_basic.FBX";

	public const string NullCostumeKeyword = "none";

	public static string BeardPreviewDirectory = "Texture/BeardPreview";

	public static string MaleHairPreviewDirectory = "Texture/MaleHairPreview";

	public static string FemaleHairPreviewDirectory = "Texture/FemaleHairPreview";

	public static readonly string[] NormalBodyModels = new string[8] { "body_engineer", "body_officelook", "body_school", "body_farmer", "body_waiter", "body_soldier", "body_house", "body_hoody" };

	[SerializeField]
	public List<PreviewableDatum> MaleHairs;

	[SerializeField]
	public List<PreviewableDatum> MaleBeards;

	[SerializeField]
	public List<PreviewableDatum> FemaleHairs;

	[SerializeField]
	public List<SimpleDatum> MaleBodies;

	[SerializeField]
	public List<SimpleDatum> MaleHeads;

	[SerializeField]
	public List<SimpleDatum> FemaleBodies;

	[SerializeField]
	public List<SimpleDatum> FemaleHeads;

	public PreviewableDatum GetRandom(Category type, bool isMale)
	{
		List<PreviewableDatum> list = null;
		switch (type)
		{
		case Category.Hair:
			list = ((!isMale) ? FemaleHairs : MaleHairs);
			break;
		case Category.Beard:
			list = ((!isMale) ? null : MaleBeards);
			break;
		}
		if (list == null)
		{
			return new PreviewableDatum();
		}
		return list.Random();
	}

	public List<PreviewableDatum> GetDataArray(Category type, bool dataIsMale)
	{
		switch (type)
		{
		case Category.Hair:
			if (dataIsMale)
			{
				return MaleHairs;
			}
			return FemaleHairs;
		case Category.Beard:
			return MaleBeards;
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	public string GetPlayerDefaultBodyModelAssetBundlePath(bool isMale, int dataJob, ClothState clothState)
	{
		string bodyFileName = string.Empty;
		switch (clothState)
		{
		case ClothState.Normal:
			bodyFileName = string.Format("{0}_{1}.fbx", (!isMale) ? "f" : "m", NormalBodyModels[dataJob]);
			break;
		case ClothState.Torn:
			bodyFileName = string.Format("{0}_{1}_torn.fbx", (!isMale) ? "f" : "m", NormalBodyModels[dataJob]);
			break;
		case ClothState.Nothing:
			bodyFileName = string.Format("{0}_inner_basic.fbx", (!isMale) ? "f" : "m");
			break;
		default:
			throw new ArgumentOutOfRangeException("clothState", clothState, null);
		}
		SimpleDatum simpleDatum = ((!isMale) ? FemaleBodies.FirstOrDefault((SimpleDatum elem) => elem.BaseName.EndsWith(bodyFileName)) : MaleBodies.FirstOrDefault((SimpleDatum elem) => elem.BaseName.EndsWith(bodyFileName)));
		if (simpleDatum != null)
		{
			return simpleDatum.AssetBundlePathBase;
		}
		return string.Empty;
	}
}
