using Shared.Etc;

public class FourSideEnterable : EnterableArtifact
{
	public override bool HasWall => true;

	public override int Height => 2;

	public override void PostInit(string blueprintId, Point2 worldTile, Rotation rotation, Point2 size)
	{
		base.PostInit(blueprintId, worldTile, rotation, size);
		TimeGauge.IsSunUpChanged += RefreshNightLight;
	}

	protected override void UpdateVisibleState()
	{
		switch (base.Visible)
		{
		case VisibleState.Normal:
			ShowCovers(1f, 1f, 1f, 1f);
			break;
		case VisibleState.PlayerInside:
			switch (base.Artifact.Rotation)
			{
			case Rotation.Quarter:
				ShowCovers(0f, 1f, 1f, 0f);
				break;
			case Rotation.Half:
				ShowCovers(1f, 0f, 1f, 0f);
				break;
			case Rotation.ThreeQuarter:
				ShowCovers(1f, 0f, 0f, 1f);
				break;
			default:
				ShowCovers(0f, 1f, 0f, 1f);
				break;
			}
			break;
		case VisibleState.Obstruction:
			ShowCovers(0f, 0f, 0f, 0f);
			break;
		}
		bool active = base.Visible == VisibleState.PlayerInside;
		base.Models.GetModel("indoor_mask").SetActive(active);
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

	private void ShowCovers(float frontAlpha, float backAlpha, float leftAlpha, float rightAlpha)
	{
		ModelComponent.IModel model = base.Models.GetModel("front");
		AlphaTweenArtifactComponent(model, frontAlpha, 0.2f);
		ModelComponent.IModel model2 = base.Models.GetModel("back");
		AlphaTweenArtifactComponent(model2, backAlpha, 0.2f);
		ModelComponent.IModel model3 = base.Models.GetModel("left");
		AlphaTweenArtifactComponent(model3, leftAlpha, 0.2f);
		ModelComponent.IModel model4 = base.Models.GetModel("right");
		AlphaTweenArtifactComponent(model4, rightAlpha, 0.2f);
	}
}
