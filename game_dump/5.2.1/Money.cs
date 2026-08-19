using System;
using Shared.Economy;

public struct Money : IComparable<Money>, IEquatable<Money>
{
	private class DifferentCurrencyException : Exception
	{
		public DifferentCurrencyException(string message)
			: base(message)
		{
		}
	}

	public readonly int Amount;

	public readonly Currency Currency;

	public static readonly Money ForFree = new Money(0, Currency.TStone);

	public Money(int amount, Currency currency)
	{
		Amount = amount;
		Currency = currency;
	}

	public Money(long amount, Currency currency)
	{
		Amount = (int)amount;
		Currency = currency;
	}

	public override string ToString()
	{
		return $"<Money Amount={Amount} Currency={Currency}>";
	}

	private static void RequireSameCurrencies(Money m1, Money m2)
	{
		if (m1.Currency != m2.Currency)
		{
			throw new DifferentCurrencyException($"Different currencies: {m1.Currency} != {m2.Currency}");
		}
	}

	public static Money operator +(Money m1, Money m2)
	{
		RequireSameCurrencies(m1, m2);
		return new Money(m1.Amount + m2.Amount, m1.Currency);
	}

	public static Money operator -(Money m1, Money m2)
	{
		RequireSameCurrencies(m1, m2);
		return new Money(m1.Amount - m2.Amount, m1.Currency);
	}

	public static Money operator +(Money m, int amount)
	{
		return new Money(m.Amount + amount, m.Currency);
	}

	public static Money operator -(Money m, int amount)
	{
		return new Money(m.Amount - amount, m.Currency);
	}

	public static Money operator *(Money m, int times)
	{
		return new Money(m.Amount * times, m.Currency);
	}

	public static Money operator /(Money m, int times)
	{
		return new Money(m.Amount * times, m.Currency);
	}

	public bool Equals(Money other)
	{
		if (Amount == other.Amount)
		{
			return Currency == other.Currency;
		}
		return false;
	}

	public override bool Equals(object other)
	{
		if (other is Money)
		{
			return Equals((Money)other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return (Amount.GetHashCode() * 397) ^ (int)Currency;
	}

	public int CompareTo(Money other)
	{
		RequireSameCurrencies(this, other);
		return Amount.CompareTo(other.Amount);
	}

	public static bool operator ==(Money m1, Money m2)
	{
		RequireSameCurrencies(m1, m2);
		return m1.Amount == m2.Amount;
	}

	public static bool operator !=(Money m1, Money m2)
	{
		RequireSameCurrencies(m1, m2);
		return m1.Amount != m2.Amount;
	}

	public static bool operator <(Money m1, Money m2)
	{
		RequireSameCurrencies(m1, m2);
		return m1.Amount < m2.Amount;
	}

	public static bool operator <=(Money m1, Money m2)
	{
		RequireSameCurrencies(m1, m2);
		return m1.Amount <= m2.Amount;
	}

	public static bool operator >(Money m1, Money m2)
	{
		RequireSameCurrencies(m1, m2);
		return m1.Amount > m2.Amount;
	}

	public static bool operator >=(Money m1, Money m2)
	{
		RequireSameCurrencies(m1, m2);
		return m1.Amount >= m2.Amount;
	}
}
