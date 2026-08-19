using Building_;
using Crafting;
using ItemSystem;
using L10N;
using Messages;
using PlayGuide;
using Shared.Guide;
using SkillData;
using Yaml;

namespace AutoGuide;

public static class TemplateFactory
{
	public static Template Create(OfferType key, TodoTemplate todoTemplate)
	{
		Template template = new Template(key, todoTemplate);
		UpdateTemplate(template, todoTemplate.Type, todoTemplate.Goal);
		UpdateToDo(template, todoTemplate);
		template.SetGuided(todoTemplate.Monitoring);
		return template;
	}

	private static void UpdateTemplate(Template template, TemplateType type, object goal)
	{
		switch (type)
		{
		case TemplateType.Build:
			CreateBuildTemplate(template, (BuildGoal)goal);
			break;
		case TemplateType.Craft:
			CreateCraftTemplate(template, (CraftGoal)goal);
			break;
		case TemplateType.Skill:
			CreateSkillTemplate(template, (SkillGoal)goal);
			break;
		case TemplateType.Hunt:
			CreateHuntTemplate(template, (HuntGoal)goal);
			break;
		}
	}

	private static void CreateBuildTemplate(Template template, BuildGoal buildGoal)
	{
		Building_.Blueprint blueprint = GameSystem<RecipeSystem>.Instance().GetBlueprint(buildGoal.BlueprintId);
		if (blueprint != null)
		{
			template.TitleText = T._("{0} 건설", blueprint.LocalizedName);
		}
	}

	private static void CreateCraftTemplate(Template template, CraftGoal craftGoal)
	{
		Crafting.Recipe recipe = GameSystem<RecipeSystem>.Instance().GetRecipe(craftGoal.RecipeId);
		if (recipe != null)
		{
			template.TitleText = T._("{0} 제작", recipe.LocalizedName);
		}
	}

	private static void CreateSkillTemplate(Template template, SkillGoal skillGoal)
	{
		Messages.Skill skill = skillGoal.Skill;
		SkillNode skillNode = GameSystem<SkillSystem>.Instance().FindSkill(skill.SkillId, skill.SubId, skill.Level);
		template.TitleText = T._("{0} 배우기", skillNode.Name);
	}

	private static void CreateHuntTemplate(Template template, HuntGoal huntGoal)
	{
		template.TitleText = T._("{0:을} 상대로 {1} 액션 사용", AnimalYaml.GetName(huntGoal.TargetEntityType), huntGoal.ActionName);
	}

	private static void UpdateToDo(Template template, TodoTemplate todoTemplate)
	{
		string phaseName = string.Empty;
		ToDoBase toDoBase = null;
		object currentTodo = todoTemplate.CurrentTodo;
		if (currentTodo is BuildTodo)
		{
			toDoBase = CreateBuildToDo(todoTemplate);
			phaseName = T._("건설");
		}
		else if (currentTodo is CraftTodo)
		{
			toDoBase = CreateCraftToDo(todoTemplate);
			phaseName = T._("제작");
		}
		else if (currentTodo is GetSlotItemTodo)
		{
			toDoBase = CreateGetSlotItemToDo(todoTemplate);
			phaseName = T._("재료 구하기");
		}
		else if (currentTodo is LearnSkillTodo)
		{
			toDoBase = CreateLearnSkillToDo(todoTemplate);
			phaseName = T._("스킬 배우기");
		}
		else if (currentTodo is GetToolTodo)
		{
			toDoBase = CreateGetToolToDo(todoTemplate);
			phaseName = T._("도구 구하기");
		}
		else if (currentTodo is UseActionTodo)
		{
			toDoBase = CreateUseActionToDo(todoTemplate);
			phaseName = T._("액션 사용하기");
		}
		else if (currentTodo == null)
		{
		}
		if (toDoBase != null)
		{
			toDoBase.Key = string.Concat(template.Key, ".", toDoBase.Key);
			toDoBase.FromAutoGuide = true;
			template.PhaseName = phaseName;
			template.SetToDo(toDoBase);
		}
	}

	private static BuildToDo CreateBuildToDo(TodoTemplate msg)
	{
		BuildTodo buildTodo = (BuildTodo)msg.CurrentTodo;
		BuildToDo buildToDo = new BuildToDo(buildTodo.BlueprintId);
		Building_.Blueprint blueprint = GameSystem<RecipeSystem>.Instance().GetBlueprint(buildTodo.BlueprintId);
		buildToDo.Tooltip = T._("UI의 건설 메뉴에서 <em>{0}</em>{0:-을} 건설 할 수 있습니다.", (blueprint == null) ? string.Empty : blueprint.LocalizedName);
		buildToDo.Key = "build." + buildTodo.BlueprintId;
		return buildToDo;
	}

	private static CraftToDo CreateCraftToDo(TodoTemplate msg)
	{
		CraftTodo craftTodo = (CraftTodo)msg.CurrentTodo;
		CraftToDo craftToDo = new CraftToDo(string.Empty, craftTodo.RecipeId);
		Crafting.Recipe recipe = GameSystem<RecipeSystem>.Instance().GetRecipe(craftTodo.RecipeId);
		craftToDo.Tooltip = T._("UI의 제작 메뉴에서 <em>{0}</em>{0:-을} 제작 할 수 있습니다.", (recipe == null) ? string.Empty : recipe.LocalizedName);
		craftToDo.Key = "craft." + craftTodo.RecipeId;
		return craftToDo;
	}

	private static GetSlotItemToDo CreateGetSlotItemToDo(TodoTemplate msg)
	{
		GetSlotItemTodo getSlotItemTodo = (GetSlotItemTodo)msg.CurrentTodo;
		TagFilter[] tags = TagFilter.CreateTagFilters(getSlotItemTodo.RequiredTags);
		TagFilter[] materials = TagFilter.CreateTagFilters(getSlotItemTodo.RequiredMaterials);
		string slotName = getSlotItemTodo.SlotName;
		GetSlotItemToDo getSlotItemToDo = new GetSlotItemToDo(tags, materials, slotName);
		getSlotItemToDo.TargetProgress = getSlotItemTodo.Count;
		getSlotItemToDo.Key = "get_slot_item." + getSlotItemTodo.SlotId;
		getSlotItemToDo.Tooltip = CreateGetSlotTooltip(tags, materials, slotName, getSlotItemTodo.Count);
		return getSlotItemToDo;
	}

	private static string CreateGetSlotTooltip(TagFilter[] tags, TagFilter[] materials, string slotLocalized, int count)
	{
		string text = ItemSystem.Util.LocalizedTagRequiredMsg(tags, showLevel: false);
		string text2 = ItemSystem.Util.LocalizedTagRequiredMsg(materials, showLevel: false);
		if (tags.Length > 0)
		{
			int requiredLevel = tags[0].RequiredLevel;
			if (materials.Length > 0)
			{
				return T._("<em>{0}</em>에는 <em>{1:lv:}</em> 이상 <em>{2}</em> 속성을 가진 <em>{3}</em> 재료가 {4}개 필요합니다.", slotLocalized, requiredLevel, text, text2, count);
			}
			return T._("<em>{0}</em>에는 <em>{1:lv:}</em> 이상 <em>{2}</em> 속성을 가진 재료가 {3}개 필요합니다.", slotLocalized, requiredLevel, text, count);
		}
		int num = ((materials.Length <= 0) ? 1 : materials[0].RequiredLevel);
		return T._("<em>{0}</em>에는 <em>{1:lv:}</em> 이상 <em>{2}</em> 재료가 {3}개 필요합니다.", slotLocalized, num, text2, count);
	}

	private static LearnSkillToDo CreateLearnSkillToDo(TodoTemplate msg)
	{
		LearnSkillTodo learnSkillTodo = (LearnSkillTodo)msg.CurrentTodo;
		LearnSkillToDo learnSkillToDo = new LearnSkillToDo(learnSkillTodo.Skill.SkillId, learnSkillTodo.Skill.SubId, learnSkillTodo.Skill.Level);
		learnSkillToDo.Key = "learn_skill." + learnSkillTodo.Skill.SkillId;
		SkillNode skillNode = GameSystem<SkillSystem>.Instance().FindSkill(learnSkillTodo.Skill);
		if (skillNode == null)
		{
			return learnSkillToDo;
		}
		learnSkillToDo.Tooltip = T._("<em>{0}</em> 계열의 <em>{1}</em> 스킬을 <em>{2} 랭크</em>로 올리세요", LocalizeUtil.Get(skillNode.Category), skillNode.Name, skillNode.Level);
		return learnSkillToDo;
	}

	private static GetItemToDo CreateGetToolToDo(TodoTemplate msg)
	{
		TagFilter[] array = TagFilter.CreateTagFilters(((GetToolTodo)msg.CurrentTodo).RequiredTags);
		string text = ItemSystem.Util.LocalizedTagRequiredMsg(array, showLevel: false);
		int num = ((array.Length <= 0) ? 1 : array[0].RequiredLevel);
		GetItemToDo getItemToDo = new GetItemToDo(array);
		getItemToDo.Key = "get_tool." + ((array.Length <= 0) ? "None" : array[0].TagId);
		getItemToDo.LocalText = T._("도구 구하기");
		getItemToDo.Tooltip = T._("<em>{0:lv:}</em> 이상의 <em>{1}</em> 속성을 가진 도구가 필요합니다.", num, text);
		return getItemToDo;
	}

	private static ManualToDo CreateUseActionToDo(TodoTemplate msg)
	{
		UseActionTodo useActionTodo = (UseActionTodo)msg.CurrentTodo;
		ManualToDo manualToDo = new ManualToDo();
		manualToDo.Key = "use_action." + useActionTodo.SkillRewardId;
		manualToDo.LocalText = T._("대상에게 <em>{0}</em> 액션을 {1}/{2}회 사용하기", useActionTodo.ActionName, useActionTodo.Count, useActionTodo.RequiredCount);
		manualToDo.Tooltip = T._("<em>{0}</em>{0:-에게} <em>{1}</em> 액션을 <em>{2}</em>회 사용하여 사냥해 봅니다.", AnimalYaml.GetName(useActionTodo.TargetEntityType), useActionTodo.ActionName, useActionTodo.RequiredCount);
		return manualToDo;
	}

	private static string GetLocalizedGoalName(TodoTemplate template)
	{
		switch (template.Type)
		{
		case TemplateType.Build:
		{
			BuildGoal buildGoal = (BuildGoal)template.Goal;
			Building_.Blueprint blueprint = GameSystem<RecipeSystem>.Instance().GetBlueprint(buildGoal.BlueprintId);
			return (blueprint == null) ? string.Empty : blueprint.LocalizedName;
		}
		case TemplateType.Craft:
		{
			CraftGoal craftGoal = (CraftGoal)template.Goal;
			Crafting.Recipe recipe = GameSystem<RecipeSystem>.Instance().GetRecipe(craftGoal.RecipeId);
			return (recipe == null) ? string.Empty : recipe.LocalizedName;
		}
		default:
			return string.Empty;
		}
	}
}
