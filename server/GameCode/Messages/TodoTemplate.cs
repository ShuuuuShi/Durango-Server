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
		else if (val.Goal is BuildGoal)
		{
			BuildGoal.Pack(packer, (BuildGoal)val.Goal, hint: true);
		}
		else if (val.Goal is CraftGoal)
		{
			CraftGoal.Pack(packer, (CraftGoal)val.Goal, hint: true);
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
		else if (val.CurrentTodo is LearnSkillTodo)
		{
			LearnSkillTodo.Pack(packer, (LearnSkillTodo)val.CurrentTodo, hint: true);
		}
		else if (val.CurrentTodo is BuildTodo)
		{
			BuildTodo.Pack(packer, (BuildTodo)val.CurrentTodo, hint: true);
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
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
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
		if (unpacker.ReadUInt32(out var result2))
		{
			switch (result2)
			{
			case 2513u:
				result.Goal = HuntGoal.Unpack(unpacker);
				break;
			case 3511u:
				result.Goal = BuildGoal.Unpack(unpacker);
				break;
			case 3510u:
				result.Goal = CraftGoal.Unpack(unpacker);
				break;
			case 3512u:
				result.Goal = SkillGoal.Unpack(unpacker);
				break;
			default:
				Debug.LogError("Unexpected type code: " + result2);
				break;
			}
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.CurrentTodo = null;
		}
		else
		{
			object currentTodo = null;
			if (unpacker.ReadUInt32(out var result3))
			{
				switch (result3)
				{
				case 3520u:
					currentTodo = CraftTodo.Unpack(unpacker);
					break;
				case 3522u:
					currentTodo = LearnSkillTodo.Unpack(unpacker);
					break;
				case 3521u:
					currentTodo = BuildTodo.Unpack(unpacker);
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
					Debug.LogError("Unexpected type code: " + result3);
					break;
				}
			}
			result.CurrentTodo = currentTodo;
		}
		unpacker.Read();
		result.Progress = unpacker.LastReadData.AsInt32();
		unpacker.Read();
		int num2 = unpacker.LastReadData.AsInt32();
		if (num2 < 0 || 2 < num2)
		{
			result.Difficulty = Difficulty.Invalid;
		}
		else
		{
			result.Difficulty = (Difficulty)num2;
		}
		unpacker.Read();
		result.Point = unpacker.LastReadData.AsInt32();
		unpacker.Read();
		result.Monitoring = unpacker.LastReadData.AsBoolean();
		return result;
	}

	public override string ToString()
	{
		return $"<TodoTemplate Type={Type} Goal={Goal} CurrentTodo={CurrentTodo} Progress={Progress} Difficulty={Difficulty} Point={Point} Monitoring={Monitoring}>";
	}
}
