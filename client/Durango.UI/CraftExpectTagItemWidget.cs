using Durango.Logic.Item;
using Shared.Item;
using UnityEngine;
using Yaml;
using Yaml.Util;

namespace Durango.UI;

public class CraftExpectTagItemWidget : UIWidget
{
	[SerializeField]
	private UILabel _nameLabel;

	public void Set(string id, int level)
	{
		Tag tag = SingletonDict<string, Tag>.Get(id);
		_nameLabel.text = ((tag != null) ? GetTagName(tag, level) : id);
		_nameLabel.color = TagData.GetGradeColor(tag?.Grade ?? TagGrade.Neutral);
		base.width = _nameLabel.width + 20;
	}

	public void SetUnrevealTag(TagGrade grade)
	{
		_nameLabel.text = "???";
		_nameLabel.color = TagData.GetGradeColor(grade);
	}

	public string GetTagName(Tag data, int level)
	{
		return (!data.VisibleLevel) ? data.Name.ToString() : TagData.GetNameWithLevel(data.Name, level);
	}
}
