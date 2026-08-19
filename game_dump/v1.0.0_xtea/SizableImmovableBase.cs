using Shared.Etc;
using UnityEngine;

public abstract class SizableImmovableBase : ImmovableBase
{
	public Point2 Size { get; protected set; }

	public virtual int Height { get; protected set; }

	public Rotation Rotation { get; protected set; }

	public Vector2 CenterTile
	{
		get
		{
			//IL_0009: Unknown result type (might be due to invalid IL or missing references)
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_0023: Unknown result type (might be due to invalid IL or missing references)
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			//IL_002d: Unknown result type (might be due to invalid IL or missing references)
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			Vector2 val = base.WorldTile.ToVector2();
			return val + Size.ToVector2() * 0.5f;
		}
	}

	public override Vector3 Center => TerrainA6.TilePositionToClientPosition(CenterTile);

	public ulong GetEstateId()
	{
		Point2 worldTile = base.WorldTile;
		Point2 size = Size;
		ulong num = 0uL;
		for (int i = 0; i < size.x; i++)
		{
			for (int j = 0; j < size.y; j++)
			{
				ulong num2 = TerrainA6.GetTileObject(worldTile + new Point2(i, j))?.EstateId ?? 0;
				if (num != 0L && num != num2)
				{
					return 0uL;
				}
				num = num2;
			}
		}
		return num;
	}
}
