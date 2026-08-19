namespace Durango.UI.Control;

public class KeyValueLabel : KeyLabelBase
{
	public KeyLabelBase Set(SyncString key, SyncString value)
	{
		return SetKey(key).SetValue(value);
	}

	public KeyLabelBase SetValue(SyncString value)
	{
		return SetValue((IContent)value);
	}

	public KeyLabelBase SetValue(string data)
	{
		return SetValue(new SyncString(data));
	}

	public override KeyLabelBase SetValue(IContent value)
	{
		SyncString text = (SyncString)(object)value;
		Init();
		if (_valueLabel == null)
		{
			return this;
		}
		if (_valueLabel.overflowMethod == UILabel.Overflow.ResizeFreely)
		{
			_valueLabel.overflowWidth = 0;
		}
		_valueLabel.SetText(text);
		return this;
	}
}
