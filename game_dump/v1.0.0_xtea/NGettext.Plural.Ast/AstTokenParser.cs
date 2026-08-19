using System;
using System.Collections.Generic;
using System.Text;

namespace NGettext.Plural.Ast;

public class AstTokenParser
{
	protected readonly Dictionary<TokenType, TokenDefinition> TokenDefinitions = new Dictionary<TokenType, TokenDefinition>();

	protected string Input;

	protected int Position;

	protected Token CurrentToken;

	public AstTokenParser()
	{
		RegisterTokenDefinition(TokenType.TernaryIf, 20).SetLeftDenotationGetter(delegate(Token self, Token left)
		{
			self.Children[0] = left;
			self.Children[1] = ParseNextExpression();
			AdvancePosition(TokenType.TernaryElse);
			self.Children[2] = ParseNextExpression();
			return self;
		});
		RegisterTokenDefinition(TokenType.TernaryElse);
		RegisterLeftInfixTokenDefinition(TokenType.Or, 30);
		RegisterLeftInfixTokenDefinition(TokenType.And, 40);
		RegisterLeftInfixTokenDefinition(TokenType.Equals, 50);
		RegisterLeftInfixTokenDefinition(TokenType.NotEquals, 50);
		RegisterLeftInfixTokenDefinition(TokenType.GreaterThan, 50);
		RegisterLeftInfixTokenDefinition(TokenType.LessThan, 50);
		RegisterLeftInfixTokenDefinition(TokenType.GreaterOrEquals, 50);
		RegisterLeftInfixTokenDefinition(TokenType.LessOrEquals, 50);
		RegisterLeftInfixTokenDefinition(TokenType.Minus, 60);
		RegisterLeftInfixTokenDefinition(TokenType.Plus, 60);
		RegisterLeftInfixTokenDefinition(TokenType.Multiply, 70);
		RegisterLeftInfixTokenDefinition(TokenType.Divide, 70);
		RegisterLeftInfixTokenDefinition(TokenType.Modulo, 70);
		RegisterPrefixTokenDefinition(TokenType.Not, 80);
		RegisterTokenDefinition(TokenType.N).SetNullDenotationGetter((Token self) => self);
		RegisterTokenDefinition(TokenType.Number).SetNullDenotationGetter((Token self) => self);
		RegisterTokenDefinition(TokenType.LeftParenthesis).SetNullDenotationGetter(delegate
		{
			Token result = ParseNextExpression();
			AdvancePosition(TokenType.RightParenthesis);
			return result;
		});
		RegisterTokenDefinition(TokenType.RightParenthesis);
		RegisterTokenDefinition(TokenType.EOF);
	}

	protected TokenDefinition RegisterTokenDefinition(TokenType tokenType, int leftBindingPower = 0)
	{
		if (TokenDefinitions.TryGetValue(tokenType, out var value))
		{
			value.LeftBindingPower = Math.Max(value.LeftBindingPower, leftBindingPower);
		}
		else
		{
			value = new TokenDefinition(tokenType, leftBindingPower);
			TokenDefinitions[tokenType] = value;
		}
		return value;
	}

	protected TokenDefinition RegisterLeftInfixTokenDefinition(TokenType tokenType, int leftBindingPower)
	{
		return RegisterTokenDefinition(tokenType, leftBindingPower).SetLeftDenotationGetter(delegate(Token self, Token left)
		{
			self.Children[0] = left;
			self.Children[1] = ParseNextExpression(leftBindingPower);
			return self;
		});
	}

	protected TokenDefinition RegisterRightInfixTokenDefinition(TokenType tokenType, int leftBindingPower)
	{
		return RegisterTokenDefinition(tokenType, leftBindingPower).SetLeftDenotationGetter(delegate(Token self, Token left)
		{
			self.Children[0] = left;
			self.Children[1] = ParseNextExpression(leftBindingPower - 1);
			return self;
		});
	}

	protected TokenDefinition RegisterPrefixTokenDefinition(TokenType tokenType, int leftBindingPower)
	{
		return RegisterTokenDefinition(tokenType, leftBindingPower).SetNullDenotationGetter(delegate(Token self)
		{
			self.Children[0] = ParseNextExpression(leftBindingPower);
			self.Children[1] = null;
			return self;
		});
	}

	protected TokenDefinition GetDefinition(TokenType tokenType)
	{
		if (!TokenDefinitions.TryGetValue(tokenType, out var value))
		{
			throw new ParserException(string.Format("Can not find token definition for \"\" token type.", tokenType));
		}
		return value;
	}

	public Token Parse(string input)
	{
		Input = input + "\0";
		Position = 0;
		CurrentToken = GetNextToken();
		return ParseNextExpression();
	}

	protected Token ParseNextExpression(int rightBindingPower = 0)
	{
		Token currentToken = CurrentToken;
		CurrentToken = GetNextToken();
		Token token = GetDefinition(currentToken.Type).GetNullDenotation(currentToken);
		while (rightBindingPower < GetDefinition(CurrentToken.Type).LeftBindingPower)
		{
			currentToken = CurrentToken;
			CurrentToken = GetNextToken();
			token = GetDefinition(currentToken.Type).GetLeftDenotation(currentToken, token);
		}
		return token;
	}

	protected void AdvancePosition()
	{
		CurrentToken = GetNextToken();
	}

	protected void AdvancePosition(TokenType expectedTokenType)
	{
		if (CurrentToken.Type != expectedTokenType)
		{
			throw new ParserException($"Expected token \"{expectedTokenType}\" but received \"{CurrentToken.Type}\"");
		}
		AdvancePosition();
	}

	protected Token GetNextToken()
	{
		while (Input[Position] == ' ' || Input[Position] == '\t')
		{
			Position++;
		}
		char c = Input[Position++];
		TokenType type = TokenType.None;
		long value = 0L;
		switch (c)
		{
		case '0':
		case '1':
		case '2':
		case '3':
		case '4':
		case '5':
		case '6':
		case '7':
		case '8':
		case '9':
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(c);
			while (char.IsNumber(Input[Position]))
			{
				stringBuilder.Append(Input[Position++]);
			}
			type = TokenType.Number;
			value = long.Parse(stringBuilder.ToString());
			break;
		}
		case '&':
		case '=':
		case '|':
			if (Input[Position] == c)
			{
				Position++;
				switch (c)
				{
				case '=':
					type = TokenType.Equals;
					break;
				case '&':
					type = TokenType.And;
					break;
				case '|':
					type = TokenType.Or;
					break;
				}
				break;
			}
			throw new ParserException($"Found invalid character \"{Input[Position]}\" after character \"{c}\" in input stream.");
		case '!':
			if (Input[Position] == '=')
			{
				Position++;
				type = TokenType.NotEquals;
			}
			else
			{
				type = TokenType.Not;
			}
			break;
		case '<':
			if (Input[Position] == '=')
			{
				Position++;
				type = TokenType.LessOrEquals;
			}
			else
			{
				type = TokenType.LessThan;
			}
			break;
		case '>':
			if (Input[Position] == '=')
			{
				Position++;
				type = TokenType.GreaterOrEquals;
			}
			else
			{
				type = TokenType.GreaterThan;
			}
			break;
		case '*':
			type = TokenType.Multiply;
			break;
		case '/':
			type = TokenType.Divide;
			break;
		case '%':
			type = TokenType.Modulo;
			break;
		case '+':
			type = TokenType.Plus;
			break;
		case '-':
			type = TokenType.Minus;
			break;
		case 'n':
			type = TokenType.N;
			break;
		case '?':
			type = TokenType.TernaryIf;
			break;
		case ':':
			type = TokenType.TernaryElse;
			break;
		case '(':
			type = TokenType.LeftParenthesis;
			break;
		case ')':
			type = TokenType.RightParenthesis;
			break;
		case '\0':
		case '\n':
		case ';':
			type = TokenType.EOF;
			Position--;
			break;
		default:
			throw new ParserException($"Found invalid character \"{c}\" in input stream at position {Position}.");
		}
		return new Token(type, value);
	}
}
