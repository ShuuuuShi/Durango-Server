using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace SmartFormat.Core.Parsing;

public class Format : FormatItem
{
	private class SplitList : IEnumerable, ICollection<Format>, IList<Format>, IEnumerable<Format>
	{
		private readonly Format format;

		private readonly IList<int> splits;

		public Format this[int index]
		{
			get
			{
				if (index > splits.Count)
				{
					throw new ArgumentOutOfRangeException("index");
				}
				if (splits.Count == 0)
				{
					return format;
				}
				if (index == 0)
				{
					return format.Substring(0, splits[0]);
				}
				if (index == splits.Count)
				{
					return format.Substring(splits[index - 1] + 1);
				}
				int num = splits[index - 1] + 1;
				return format.Substring(num, splits[index] - num);
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		public int Count => splits.Count + 1;

		public bool IsReadOnly => true;

		public SplitList(Format format, IList<int> splits)
		{
			this.format = format;
			this.splits = splits;
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			throw new NotSupportedException();
		}

		public void CopyTo(Format[] array, int arrayIndex)
		{
			int num = splits.Count + 1;
			for (int i = 0; i < num; i++)
			{
				array[arrayIndex + i] = this[i];
			}
		}

		public int IndexOf(Format item)
		{
			throw new NotSupportedException();
		}

		public void Insert(int index, Format item)
		{
			throw new NotSupportedException();
		}

		public void RemoveAt(int index)
		{
			throw new NotSupportedException();
		}

		public void Add(Format item)
		{
			throw new NotSupportedException();
		}

		public void Clear()
		{
			throw new NotSupportedException();
		}

		public bool Contains(Format item)
		{
			throw new NotSupportedException();
		}

		public bool Remove(Format item)
		{
			throw new NotSupportedException();
		}

		public IEnumerator<Format> GetEnumerator()
		{
			throw new NotSupportedException();
		}
	}

	public readonly Placeholder parent;

	private char splitCacheChar;

	private IList<Format> splitCache;

	public List<FormatItem> Items { get; private set; }

	public bool HasNested { get; set; }

	public Format(string baseString)
		: base(baseString, 0, baseString.Length)
	{
		parent = null;
		Items = new List<FormatItem>();
	}

	public Format(Placeholder parent, int startIndex)
		: base(parent, startIndex)
	{
		this.parent = parent;
		Items = new List<FormatItem>();
	}

	public Format Substring(int startIndex)
	{
		return Substring(startIndex, endIndex - base.startIndex - startIndex);
	}

	public Format Substring(int startIndex, int length)
	{
		startIndex = base.startIndex + startIndex;
		int num = startIndex + length;
		if (startIndex < base.startIndex || startIndex > endIndex)
		{
			throw new ArgumentOutOfRangeException("startIndex");
		}
		if (num > endIndex)
		{
			throw new ArgumentOutOfRangeException("length");
		}
		if (startIndex == base.startIndex && num == endIndex)
		{
			return this;
		}
		Format format = new Format(baseString);
		format.startIndex = startIndex;
		format.endIndex = num;
		Format format2 = format;
		foreach (FormatItem item2 in Items)
		{
			if (item2.endIndex <= startIndex)
			{
				continue;
			}
			if (num <= item2.startIndex)
			{
				break;
			}
			FormatItem item = item2;
			if (item2 is LiteralText)
			{
				if (startIndex > item2.startIndex || item2.endIndex > num)
				{
					LiteralText literalText = new LiteralText(format2);
					literalText.startIndex = Math.Max(startIndex, item2.startIndex);
					literalText.endIndex = Math.Min(num, item2.endIndex);
					item = literalText;
				}
			}
			else
			{
				format2.HasNested = true;
			}
			format2.Items.Add(item);
		}
		return format2;
	}

	public int IndexOf(char search)
	{
		return IndexOf(search, 0);
	}

	public int IndexOf(char search, int startIndex)
	{
		startIndex = base.startIndex + startIndex;
		foreach (FormatItem item in Items)
		{
			if (item.endIndex >= startIndex && item is LiteralText literalText)
			{
				if (startIndex < literalText.startIndex)
				{
					startIndex = literalText.startIndex;
				}
				int num = literalText.baseString.IndexOf(search, startIndex, literalText.endIndex - startIndex);
				if (num != -1)
				{
					return num - base.startIndex;
				}
			}
		}
		return -1;
	}

	private IList<int> FindAll(char search)
	{
		return FindAll(search, -1);
	}

	private IList<int> FindAll(char search, int maxCount)
	{
		List<int> list = new List<int>();
		int num = 0;
		while (maxCount != 0)
		{
			num = IndexOf(search, num);
			if (num == -1)
			{
				break;
			}
			list.Add(num);
			num++;
			maxCount--;
		}
		return list;
	}

	public IList<Format> Split(char search)
	{
		if (splitCache == null || splitCacheChar != search)
		{
			splitCacheChar = search;
			splitCache = Split(search, -1);
		}
		return splitCache;
	}

	public IList<Format> Split(char search, int maxCount)
	{
		IList<int> splits = FindAll(search, maxCount);
		return new SplitList(this, splits);
	}

	public string GetLiteralText()
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (FormatItem item in Items)
		{
			if (item is LiteralText literalText)
			{
				stringBuilder.Append(literalText.baseString, literalText.startIndex, literalText.endIndex - literalText.startIndex);
			}
		}
		return stringBuilder.ToString();
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder(endIndex - startIndex);
		foreach (FormatItem item in Items)
		{
			stringBuilder.Append(item.ToString());
		}
		return stringBuilder.ToString();
	}
}
