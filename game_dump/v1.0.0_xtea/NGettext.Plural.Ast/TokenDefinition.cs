using System;

namespace NGettext.Plural.Ast;

public class TokenDefinition
{
	public delegate Token NullDenotationGetterDelegate(Token self);

	public delegate Token LeftDenotationGetterDelegate(Token self, Token left);

	protected NullDenotationGetterDelegate NullDenotationGetter;

	protected LeftDenotationGetterDelegate LeftDenotationGetter;

	public TokenType TokenType { get; protected set; }

	public int LeftBindingPower { get; set; }

	public TokenDefinition(TokenType tokenType, int leftBindingPower)
	{
		TokenType = tokenType;
		LeftBindingPower = leftBindingPower;
	}

	public TokenDefinition SetNullDenotationGetter(NullDenotationGetterDelegate nullDenotationGetter)
	{
		NullDenotationGetter = nullDenotationGetter;
		return this;
	}

	public TokenDefinition SetLeftDenotationGetter(LeftDenotationGetterDelegate leftDenotationGetter)
	{
		LeftDenotationGetter = leftDenotationGetter;
		return this;
	}

	public Token GetNullDenotation(Token self)
	{
		if (NullDenotationGetter == null)
		{
			throw new InvalidOperationException("Unable to invoke null denotation getter: getter is not set.");
		}
		if (self.Type != TokenType)
		{
			throw new ArgumentException("Unable to invoke null denotation getter: invalid self type.", "self");
		}
		return NullDenotationGetter(self);
	}

	public Token GetLeftDenotation(Token self, Token left)
	{
		if (LeftDenotationGetter == null)
		{
			throw new InvalidOperationException("Unable to invoke left denotation getter: getter is not set.");
		}
		if (self.Type != TokenType)
		{
			throw new ArgumentException("Unable to invoke null denotation getter: invalid self type.", "self");
		}
		return LeftDenotationGetter(self, left);
	}
}
