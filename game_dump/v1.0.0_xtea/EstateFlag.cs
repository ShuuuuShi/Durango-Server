using ClanData;
using Messages;
using Shared.Estate;
using UnityEngine;

public class EstateFlag : ArtifactComponent
{
	public const string FlagContainerName = "drawing_board";

	private MeshRenderer _drawContainer;

	private bool _drawContainerFlag;

	private MeshRenderer DrawContainer
	{
		get
		{
			if (!_drawContainerFlag)
			{
				_drawContainerFlag = true;
				GameObject val = KUtility.FindObjectByName(((Component)base.Artifact).gameObject, "drawing_board", includeInactive: true);
				if ((Object)(object)val != (Object)null)
				{
					_drawContainer = val.GetComponent<MeshRenderer>();
				}
			}
			return _drawContainer;
		}
	}

	public ApngTexture FlagTexture { get; private set; }

	public override void ResourcesLoadCompleted()
	{
		MakeComponent();
		UpdateFlag();
	}

	private void MakeComponent()
	{
		MeshRenderer drawContainer = DrawContainer;
		if ((Object)(object)drawContainer != (Object)null)
		{
			FlagTexture = ((Component)drawContainer).gameObject.AddComponent<ApngTexture>();
		}
		if ((Object)(object)FlagTexture != (Object)null)
		{
			((Behaviour)FlagTexture).enabled = false;
		}
	}

	public override bool OnUpdateState(double eventTime)
	{
		UpdateFlag();
		return false;
	}

	public override bool OnSelectArtifact(bool isSelect)
	{
		if (isSelect)
		{
			UpdateFlag();
		}
		return false;
	}

	private void UpdateFlag()
	{
		if ((Object)(object)FlagTexture == (Object)null)
		{
			return;
		}
		ArtifactState artifactState = base.Artifact.ArtifactState;
		if (artifactState.Estate.HasValue)
		{
			EstateInfo value = artifactState.Estate.Value;
			if (value.Type == OwnerType.ClanEstate)
			{
				ClanSystem.GetClanInfo(value.OwnerId, OnOwnerClan);
			}
		}
	}

	private void OnOwnerClan(Clan clan)
	{
		clan.GetEmblem(SetFlagTexture);
	}

	private void SetFlagTexture(Texture2D texture)
	{
		if (!((Object)(object)FlagTexture == (Object)null))
		{
			if ((Object)(object)texture == (Object)null)
			{
				((Component)FlagTexture).gameObject.SetActive(false);
				return;
			}
			((Component)FlagTexture).gameObject.SetActive(true);
			FlagTexture.Set(texture);
		}
	}
}
