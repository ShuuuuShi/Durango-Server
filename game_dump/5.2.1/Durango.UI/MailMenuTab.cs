using Durango.Logic.Mail;
using L10N;
using UnityEngine;

namespace Durango.UI;

public class MailMenuTab : MonoBehaviour
{
	[SerializeField]
	private UILabel _categoryLabel;

	[SerializeField]
	private UILabel _countLabel;

	[SerializeField]
	private UIWidget _subCategorySymbol;

	public void Init(CategoryType categoryType)
	{
		_categoryLabel.text = categoryType.GetName();
		bool flag = false;
		if (categoryType == CategoryType.Shop || categoryType == CategoryType.System || categoryType == CategoryType.GM)
		{
			flag = true;
		}
		_subCategorySymbol.gameObject.SetActive(flag);
		Vector3 localPosition = _subCategorySymbol.transform.localPosition;
		localPosition.x += ((!flag) ? 0f : ((float)_subCategorySymbol.width + 3f));
		_categoryLabel.transform.localPosition = localPosition;
		SetCount(0);
	}

	public void UpdateLayout()
	{
		Vector3 localPosition = _subCategorySymbol.transform.localPosition;
		localPosition.x += ((!_subCategorySymbol.gameObject.activeSelf) ? 0f : ((float)_subCategorySymbol.width + 3f));
		_categoryLabel.transform.localPosition = localPosition;
	}

	public void SetCount(int count)
	{
		_countLabel.text = count.ToString();
	}
}
