using MsgPack;

namespace Messages;

public struct AppearAnimal
{
	public const uint TypeCode = 91u;

	public string EntityId;

	public ushort EntityType;

	public bool IsAlive;

	public AnimalDisplay Display;

	public int Level;

	public Move Move;

	public Survival Survival;

	public string Role;

	public string EnemyId;

	public static void Pack(Packer packer, AppearAnimal val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(10);
			packer.Pack(91u);
		}
		else
		{
			packer.PackArrayHeader(9);
		}
		if (val.EntityId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.EntityId);
		}
		packer.Pack(val.EntityType);
		packer.Pack(val.IsAlive);
		AnimalDisplay.Pack(packer, val.Display);
		packer.Pack(val.Level);
		Move.Pack(packer, val.Move);
		Survival.Pack(packer, val.Survival);
		if (val.Role == null)
		{
			packer.PackNull();
		}
		else if (val.Role == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.Role);
		}
		if (val.EnemyId == null)
		{
			packer.PackNull();
		}
		else if (val.EnemyId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.EnemyId);
		}
	}

	public static AppearAnimal Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		AppearAnimal result = default(AppearAnimal);
		result.EntityId = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.EntityType = unpacker.LastReadData.AsUInt16();
		unpacker.Read();
		result.IsAlive = unpacker.LastReadData.AsBoolean();
		unpacker.Read();
		result.Display = AnimalDisplay.Unpack(unpacker);
		unpacker.Read();
		result.Level = unpacker.LastReadData.AsInt32();
		unpacker.Read();
		result.Move = Move.Unpack(unpacker);
		unpacker.Read();
		result.Survival = Survival.Unpack(unpacker);
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.Role = null;
		}
		else
		{
			string role = unpacker.LastReadData.AsString();
			result.Role = role;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.EnemyId = null;
		}
		else
		{
			string enemyId = unpacker.LastReadData.AsString();
			result.EnemyId = enemyId;
		}
		return result;
	}

	public override string ToString()
	{
		return $"<AppearAnimal EntityId={EntityId} EntityType={EntityType} IsAlive={IsAlive} Display={Display} Level={Level} Move={Move} Survival={Survival} Role={Role} EnemyId={EnemyId}>";
	}
}
