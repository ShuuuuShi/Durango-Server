using System.Collections.Generic;
using Yaml;

namespace Durango.Logic.LearningGuide;

public class AdviceCategory
{
	private readonly Yaml.AdviceCategory _adviceCategory;

	public string Id => _adviceCategory.id;

	public Gettext Name => _adviceCategory.name;

	public string Icon => _adviceCategory.icon;

	public List<AdviceSubCategory> SubCategories { get; private set; }

	public AdviceCategory(Yaml.AdviceCategory category)
	{
		_adviceCategory = category;
		SubCategories = new List<AdviceSubCategory>();
		Yaml.AdviceSubCategory[] subcategories = _adviceCategory.subcategories;
		foreach (Yaml.AdviceSubCategory sub in subcategories)
		{
			SubCategories.Add(new AdviceSubCategory(sub));
		}
	}
}
