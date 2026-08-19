namespace NGettext.Plural.Ast;

public class Token
{
	public const int MAX_CHILDREN_COUNT = 3;

	public readonly Token[] Children = new Token[3];

	public TokenType Type { get; protected set; }

	public long Value { get; set; }

	public Token(TokenType type, long value = 0)
	{
		Type = type;
		Value = value;
	}
}
