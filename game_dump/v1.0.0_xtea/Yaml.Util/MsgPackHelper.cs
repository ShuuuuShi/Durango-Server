using MsgPack.Serialization;

namespace Yaml.Util;

public static class MsgPackHelper
{
	private static SerializationContext _context;

	public static SerializationContext Context
	{
		get
		{
			//IL_000a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0014: Expected O, but got Unknown
			if (_context == null)
			{
				_context = new SerializationContext();
				_context.Serializers.Register<Gettext>((MessagePackSerializer<Gettext>)new GettextSerializer(_context));
			}
			return _context;
		}
	}
}
