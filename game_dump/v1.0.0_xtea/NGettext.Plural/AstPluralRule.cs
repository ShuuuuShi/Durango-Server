using System;
using NGettext.Plural.Ast;

namespace NGettext.Plural;

public class AstPluralRule : IPluralRule
{
	public int NumPlurals { get; protected set; }

	protected Token AstRoot { get; set; }

	public AstPluralRule(int numPlurals, Token astRoot)
	{
		if (numPlurals <= 0)
		{
			throw new ArgumentOutOfRangeException("numPlurals");
		}
		if (astRoot == null)
		{
			throw new ArgumentNullException("astRoot");
		}
		NumPlurals = numPlurals;
		AstRoot = astRoot;
	}

	public int Evaluate(long number)
	{
		return (int)Evaluate(AstRoot, number);
	}

	protected long Evaluate(Token node, long number)
	{
		return node.Type switch
		{
			TokenType.Number => node.Value, 
			TokenType.N => number, 
			TokenType.Plus => Evaluate(node.Children[0], number) + Evaluate(node.Children[1], number), 
			TokenType.Minus => Evaluate(node.Children[0], number) - Evaluate(node.Children[1], number), 
			TokenType.Divide => Evaluate(node.Children[0], number) / Evaluate(node.Children[1], number), 
			TokenType.Multiply => Evaluate(node.Children[0], number) * Evaluate(node.Children[1], number), 
			TokenType.Modulo => Evaluate(node.Children[0], number) % Evaluate(node.Children[1], number), 
			TokenType.GreaterThan => (Evaluate(node.Children[0], number) > Evaluate(node.Children[1], number)) ? 1 : 0, 
			TokenType.GreaterOrEquals => (Evaluate(node.Children[0], number) >= Evaluate(node.Children[1], number)) ? 1 : 0, 
			TokenType.LessThan => (Evaluate(node.Children[0], number) < Evaluate(node.Children[1], number)) ? 1 : 0, 
			TokenType.LessOrEquals => (Evaluate(node.Children[0], number) <= Evaluate(node.Children[1], number)) ? 1 : 0, 
			TokenType.Equals => (Evaluate(node.Children[0], number) == Evaluate(node.Children[1], number)) ? 1 : 0, 
			TokenType.NotEquals => (Evaluate(node.Children[0], number) != Evaluate(node.Children[1], number)) ? 1 : 0, 
			TokenType.And => (Evaluate(node.Children[0], number) != 0L && Evaluate(node.Children[1], number) != 0L) ? 1 : 0, 
			TokenType.Or => (Evaluate(node.Children[0], number) != 0L || Evaluate(node.Children[1], number) != 0L) ? 1 : 0, 
			TokenType.Not => (Evaluate(node.Children[0], number) == 0L) ? 1 : 0, 
			TokenType.TernaryIf => (Evaluate(node.Children[0], number) == 0L) ? Evaluate(node.Children[2], number) : Evaluate(node.Children[1], number), 
			_ => throw new InvalidOperationException($"Can not evaluate token: {node.Type}."), 
		};
	}
}
