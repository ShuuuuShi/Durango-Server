namespace NGettext.Plural.Ast;

public enum TokenType
{
	None,
	TernaryIf,
	TernaryElse,
	Or,
	And,
	Equals,
	NotEquals,
	GreaterThan,
	LessThan,
	GreaterOrEquals,
	LessOrEquals,
	Minus,
	Plus,
	Multiply,
	Divide,
	Modulo,
	Not,
	N,
	Number,
	LeftParenthesis,
	RightParenthesis,
	EOF
}
