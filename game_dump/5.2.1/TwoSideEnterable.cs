using Shared.Etc;

public class TwoSideEnterable : EnterableArtifact
{
	public override bool HasWall => true;

	public override int Height => 3;

	public override void PostInit(string blueprintId, Point2 worldTile, Rotation rotation, Point2 size)
	{
		base.PostInit(blueprintId, worldTile, rotation, size);
		TimeGauge.IsSunUpChanged += RefreshNightLight;
	}

	protected override void UpdateVisibleState()
	{
		ModelComponent.IModel model = base.Models.GetModel("indoor_mask");
		switch (base.Visible)
		{
		case VisibleState.Normal:
			ShowCovers(1f, 1f);
			break;
		case VisibleState.PlayerInside:
			if (GameManager.Region.IsPvpIsland())
			{
				ShowCovers(0.3f, 0.3f);
			}
			else
			{
				ShowCovers(0f, 1f);
			}
			break;
		case VisibleState.Obstruction:
			ShowCovers(0f, 0f);
			break;
		}
		bool active = base.Visible == VisibleState.PlayerInside;
		model.SetActive(active);
		RefreshNightLight();
	}

	public override void OnRemoved()
	{
		base.OnRemoved();
		TimeGauge.IsSunUpChanged -= RefreshNightLight;
	}

	private void RefreshNightLight()
	{
		bool active = !TimeGauge.IsSunUp && base.Visible == VisibleState.Normal;
		base.Models.GetModel("night_light").SetActive(active);
	}

	private void ShowCovers(float frontAlpha, float backAlpha)
	{
		ModelComponent.IModel model = base.Models.GetModel("front");
		AlphaTweenArtifactComponent(model, frontAlpha, 0.2f);
		ModelComponent.IModel model2 = base.Models.GetModel("back");
		AlphaTweenArtifactComponent(model2, backAlpha, 0.2f);
	}
}
