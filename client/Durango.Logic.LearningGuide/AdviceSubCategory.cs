using Yaml;

namespace Durango.Logic.LearningGuide;

public class AdviceSubCategory
{
	private readonly Yaml.AdviceSubCategory _adviceSubCategory;

	public string Id => _adviceSubCategory.id;

	public Gettext Name => _adviceSubCategory.name;

	public AdviceSubCategory(Yaml.AdviceSubCategory sub)
	{
		_adviceSubCategory = sub;
	}
}
