using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Messages;

public static class MusicsExtension
{
	[CompilerGenerated]
	private sealed class _003CGetMyMusics_003Ed__2 : IEnumerable<KeyValuePair<MusicId, Music>>, IEnumerable, IEnumerator<KeyValuePair<MusicId, Music>>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private KeyValuePair<MusicId, Music> _003C_003E2__current;

		private int _003C_003El__initialThreadId;

		private Musics msg;

		public Musics _003C_003E3__msg;

		private Dictionary<int, Music>.Enumerator _003C_003E7__wrap1;

		KeyValuePair<MusicId, Music> IEnumerator<KeyValuePair<MusicId, Music>>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CGetMyMusics_003Ed__2(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
			_003C_003El__initialThreadId = Thread.CurrentThread.ManagedThreadId;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			int num = _003C_003E1__state;
			if (num == -3 || num == 1)
			{
				try
				{
				}
				finally
				{
					_003C_003Em__Finally1();
				}
			}
			_003C_003E7__wrap1 = default(Dictionary<int, Music>.Enumerator);
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			try
			{
				switch (_003C_003E1__state)
				{
				default:
					return false;
				case 0:
					_003C_003E1__state = -1;
					if (msg._Musics == null)
					{
						return false;
					}
					_003C_003E7__wrap1 = msg._Musics.GetEnumerator();
					_003C_003E1__state = -3;
					break;
				case 1:
					_003C_003E1__state = -3;
					break;
				}
				if (_003C_003E7__wrap1.MoveNext())
				{
					KeyValuePair<int, Music> current = _003C_003E7__wrap1.Current;
					_003C_003E2__current = new KeyValuePair<MusicId, Music>(current.Key, current.Value);
					_003C_003E1__state = 1;
					return true;
				}
				_003C_003Em__Finally1();
				_003C_003E7__wrap1 = default(Dictionary<int, Music>.Enumerator);
				return false;
			}
			catch
			{
				//try-fault
				((IDisposable)this).Dispose();
				throw;
			}
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		private void _003C_003Em__Finally1()
		{
			_003C_003E1__state = -1;
			((IDisposable)_003C_003E7__wrap1).Dispose();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}

		[DebuggerHidden]
		IEnumerator<KeyValuePair<MusicId, Music>> IEnumerable<KeyValuePair<MusicId, Music>>.GetEnumerator()
		{
			_003CGetMyMusics_003Ed__2 _003CGetMyMusics_003Ed__;
			if (_003C_003E1__state == -2 && _003C_003El__initialThreadId == Thread.CurrentThread.ManagedThreadId)
			{
				_003C_003E1__state = 0;
				_003CGetMyMusics_003Ed__ = this;
			}
			else
			{
				_003CGetMyMusics_003Ed__ = new _003CGetMyMusics_003Ed__2(0);
			}
			_003CGetMyMusics_003Ed__.msg = _003C_003E3__msg;
			return _003CGetMyMusics_003Ed__;
		}

		[DebuggerHidden]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<KeyValuePair<MusicId, Music>>)this).GetEnumerator();
		}
	}

	[CompilerGenerated]
	private sealed class _003CGetSharedMusics_003Ed__3 : IEnumerable<KeyValuePair<MusicId, Music>>, IEnumerable, IEnumerator<KeyValuePair<MusicId, Music>>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private KeyValuePair<MusicId, Music> _003C_003E2__current;

		private int _003C_003El__initialThreadId;

		private Musics msg;

		public Musics _003C_003E3__msg;

		private Dictionary<string, Music>.Enumerator _003C_003E7__wrap1;

		KeyValuePair<MusicId, Music> IEnumerator<KeyValuePair<MusicId, Music>>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CGetSharedMusics_003Ed__3(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
			_003C_003El__initialThreadId = Thread.CurrentThread.ManagedThreadId;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			int num = _003C_003E1__state;
			if (num == -3 || num == 1)
			{
				try
				{
				}
				finally
				{
					_003C_003Em__Finally1();
				}
			}
			_003C_003E7__wrap1 = default(Dictionary<string, Music>.Enumerator);
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			try
			{
				switch (_003C_003E1__state)
				{
				default:
					return false;
				case 0:
					_003C_003E1__state = -1;
					if (msg.SharedMusics == null)
					{
						return false;
					}
					_003C_003E7__wrap1 = msg.SharedMusics.GetEnumerator();
					_003C_003E1__state = -3;
					break;
				case 1:
					_003C_003E1__state = -3;
					break;
				}
				if (_003C_003E7__wrap1.MoveNext())
				{
					KeyValuePair<string, Music> current = _003C_003E7__wrap1.Current;
					_003C_003E2__current = new KeyValuePair<MusicId, Music>(current.Key, current.Value);
					_003C_003E1__state = 1;
					return true;
				}
				_003C_003Em__Finally1();
				_003C_003E7__wrap1 = default(Dictionary<string, Music>.Enumerator);
				return false;
			}
			catch
			{
				//try-fault
				((IDisposable)this).Dispose();
				throw;
			}
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		private void _003C_003Em__Finally1()
		{
			_003C_003E1__state = -1;
			((IDisposable)_003C_003E7__wrap1).Dispose();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}

		[DebuggerHidden]
		IEnumerator<KeyValuePair<MusicId, Music>> IEnumerable<KeyValuePair<MusicId, Music>>.GetEnumerator()
		{
			_003CGetSharedMusics_003Ed__3 _003CGetSharedMusics_003Ed__;
			if (_003C_003E1__state == -2 && _003C_003El__initialThreadId == Thread.CurrentThread.ManagedThreadId)
			{
				_003C_003E1__state = 0;
				_003CGetSharedMusics_003Ed__ = this;
			}
			else
			{
				_003CGetSharedMusics_003Ed__ = new _003CGetSharedMusics_003Ed__3(0);
			}
			_003CGetSharedMusics_003Ed__.msg = _003C_003E3__msg;
			return _003CGetSharedMusics_003Ed__;
		}

		[DebuggerHidden]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<KeyValuePair<MusicId, Music>>)this).GetEnumerator();
		}
	}

	public static int GetTotalMusicCount(this Musics msg)
	{
		return KUtility.GetSize(msg._Musics) + KUtility.GetSize(msg.SharedMusics);
	}

	public static IEnumerable<KeyValuePair<MusicId, Music>> GetAllMusics(this Musics msg)
	{
		return msg.GetSharedMusics().Concat(msg.GetMyMusics());
	}

	public static IEnumerable<KeyValuePair<MusicId, Music>> GetMyMusics(this Musics msg)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CGetMyMusics_003Ed__2(-2)
		{
			_003C_003E3__msg = msg
		};
	}

	public static IEnumerable<KeyValuePair<MusicId, Music>> GetSharedMusics(this Musics msg)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CGetSharedMusics_003Ed__3(-2)
		{
			_003C_003E3__msg = msg
		};
	}
}
