using MsgPack;
using Shared.Etc;
using Shared.Guide;

namespace Messages;

public struct TodoTemplate
{
	public TemplateType Type;

	public object Goal;

	public object CurrentTodo;

	public int Progress;

	public Difficulty Difficulty;

	public int Point;

	public bool Monitoring;

	public static void Pack(Packer packer, TodoTemplate val, bool hint = false)
	{
		packer.PackArrayHeader(7);
		packer.Pack((int)val.Type);
		if (val.Goal == null)
		{
			packer.PackNull();
		}
		else if (val.Goal is HuntGoal)
		{
			HuntGoal.Pack(packer, (HuntGoal)val.Goal, hint: true);
		}
		else if (val.Goal is CraftGoal)
		{
			CraftGoal.Pack(packer, (CraftGoal)val.Goal, hint: true);
		}
		else if (val.Goal is BuildGoal)
		{
			BuildGoal.Pack(packer, (BuildGoal)val.Goal, hint: true);
		}
		else if (val.Goal is SkillGoal)
		{
			SkillGoal.Pack(packer, (SkillGoal)val.Goal, hint: true);
		}
		if (val.CurrentTodo == null)
		{
			packer.PackNull();
		}
		else if (val.CurrentTodo is CraftTodo)
		{
			CraftTodo.Pack(packer, (CraftTodo)val.CurrentTodo, hint: true);
		}
		else if (val.CurrentTodo is BuildTodo)
		{
			BuildTodo.Pack(packer, (BuildTodo)val.CurrentTodo, hint: true);
		}
		else if (val.CurrentTodo is LearnSkillTodo)
		{
			LearnSkillTodo.Pack(packer, (LearnSkillTodo)val.CurrentTodo, hint: true);
		}
		else if (val.CurrentTodo is GetSlotItemTodo)
		{
			GetSlotItemTodo.Pack(packer, (GetSlotItemTodo)val.CurrentTodo, hint: true);
		}
		else if (val.CurrentTodo is GetToolTodo)
		{
			GetToolTodo.Pack(packer, (GetToolTodo)val.CurrentTodo, hint: true);
		}
		else if (val.CurrentTodo is UseActionTodo)
		{
			UseActionTodo.Pack(packer, (UseActionTodo)val.CurrentTodo, hint: true);
		}
		packer.Pack(val.Progress);
		packer.Pack((int)val.Difficulty);
		packer.Pack(val.Point);
		packer.Pack(val.Monitoring);
	}

	public static TodoTemplate Unpack(Unpacker unpacker)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_0200: Unknown result type (might be due to invalid IL or missing references)
		//IL_0205: Unknown result type (might be due to invalid IL or missing references)
		//IL_021b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0220: Unknown result type (might be due to invalid IL or missing references)
		//IL_025b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0260: Unknown result type (might be due to invalid IL or missing references)
		//IL_0278: Unknown result type (might be due to invalid IL or missing references)
		//IL_027d: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		int num = ((MessagePackObject)(ref lastReadData)).AsInt32();
		TodoTemplate result = default(TodoTemplate);
		if (num < 1 || 4 < num)
		{
			result.Type = TemplateType.Invalid;
		}
		else
		{
			result.Type = (TemplateType)num;
		}
		unpacker.Read();
		result.Goal = null;
		uint num2 = default(uint);
		if (unpacker.ReadUInt32(ref num2))
		{
			switch (num2)
			{
			case 2513u:
				result.Goal = HuntGoal.Unpack(unpacker);
				break;
			case 3510u:
				result.Goal = CraftGoal.Unpack(unpacker);
				break;
			case 3511u:
				result.Goal = BuildGoal.Unpack(unpacker);
				break;
			case 3512u:
				result.Goal = SkillGoal.Unpack(unpacker);
				break;
			default:
				Debug.LogError((object)("Unexpected type code: " + num2));
				break;
			}
		}
		unpacker.Read();
		MessagePackObject lastReadData2 = unpacker.LastReadData;
		if (((MessagePackObject)(ref lastReadData2)).IsNil)
		{
			result.CurrentTodo = null;
		}
		else
		{
			object currentTodo = null;
			uint num3 = default(uint);
			if (unpacker.ReadUInt32(ref num3))
			{
				switch (num3)
				{
				case 3520u:
					currentTodo = CraftTodo.Unpack(unpacker);
					break;
				case 3521u:
					currentTodo = BuildTodo.Unpack(unpacker);
					break;
				case 3522u:
					currentTodo = LearnSkillTodo.Unpack(unpacker);
					break;
				case 3523u:
					currentTodo = GetSlotItemTodo.Unpack(unpacker);
					break;
				case 3524u:
					currentTodo = GetToolTodo.Unpack(unpacker);
					break;
				case 3526u:
					currentTodo = UseActionTodo.Unpack(unpacker);
					break;
				default:
					Debug.LogError((object)("Unexpected type code: " + num3));
					break;
				}
			}
			result.CurrentTodo = currentTodo;
		}
		unpacker.Read();
		MessagePackObject lastReadData3 = unpacker.LastReadData;
		result.Progress = ((MessagePackObject)(ref lastReadData3)).AsInt32();
		unpacker.Read();
		MessagePackObject lastReadData4 = unpacker.LastReadData;
		int num4 = ((MessagePackObject)(ref lastReadData4)).AsInt32();
		if (num4 < 0 || 2 < num4)
		{
			result.Difficulty = Difficulty.Invalid;
		}
		else
		{
			result.Difficulty = (Difficulty)num4;
		}
		unpacker.Read();
		MessagePackObject lastReadData5 = unpacker.LastReadData;
		result.Point = ((MessagePackObject)(ref lastReadData5)).AsInt32();
		unpacker.Read();
		MessagePackObject lastReadData6 = unpacker.LastReadData;
		result.Monitoring = ((MessagePackObject)(ref lastReadData6)).AsBoolean();
		return result;
	}

	public override string ToString()
	{
		return $"<TodoTemplate Type={Type} Goal={Goal} CurrentTodo={CurrentTodo} Progress={Progress} Difficulty={Difficulty} Point={Point} Monitoring={Monitoring}>";
	}
}
