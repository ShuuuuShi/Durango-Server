using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using Durango.Logic.Clusters;
using Durango.Utils;

namespace Durango.Offline;

public static class Servers
{
	[CompilerGenerated]
	private sealed class _003CGetServers_003Ed__0 : IEnumerable<Server>, IEnumerable, IEnumerator<Server>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private Server _003C_003E2__current;

		private int _003C_003El__initialThreadId;

		private Dictionary<string, Cluster> clusters;

		public Dictionary<string, Cluster> _003C_003E3__clusters;

		private string[] _003Carray_003E5__2;

		private int _003Ci_003E5__3;

		Server IEnumerator<Server>.Current
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
		public _003CGetServers_003Ed__0(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
			_003C_003El__initialThreadId = Thread.CurrentThread.ManagedThreadId;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			_003Carray_003E5__2 = null;
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			switch (_003C_003E1__state)
			{
			default:
				return false;
			case 0:
			{
				_003C_003E1__state = -1;
				string[] directories = AppData.GetDirectories("offline", "*", SearchOption.TopDirectoryOnly);
				_003Carray_003E5__2 = directories;
				_003Ci_003E5__3 = 0;
				goto IL_0136;
			}
			case 1:
				_003C_003E1__state = -1;
				goto IL_0124;
			case 2:
				_003C_003E1__state = -1;
				_003C_003E2__current = new Server("solo", new Dictionary<string, string>
				{
					{ "en_US", "Single Play Mode" },
					{ "ko_KR", "싱글플레이 모드" }
				});
				_003C_003E1__state = 3;
				return true;
			case 3:
				_003C_003E1__state = -1;
				_003C_003E2__current = new Server("multi", new Dictionary<string, string>
				{
					{ "en_US", "Multi Play Mode" },
					{ "ko_KR", "멀티플레이 모드" }
				});
				_003C_003E1__state = 4;
				return true;
			case 4:
				_003C_003E1__state = -1;
				_003C_003E2__current = new Server("online", new Dictionary<string, string>
				{
					{ "en_US", "Online Server (For Test)" },
					{ "ko_KR", "온라인 서버 (테스트)" }
				});
				_003C_003E1__state = 5;
				return true;
			case 5:
				{
					_003C_003E1__state = -1;
					return false;
				}
				IL_0136:
				if (_003Ci_003E5__3 < _003Carray_003E5__2.Length)
				{
					string name = new DirectoryInfo(_003Carray_003E5__2[_003Ci_003E5__3]).Name;
					Cluster cluster = clusters.Get(name);
					if (cluster != null)
					{
						Dictionary<string, string> dictionary = new Dictionary<string, string>();
						foreach (KeyValuePair<string, string> name2 in cluster.Names)
						{
							string text = ((!(name2.Key == "en_US")) ? "[기록] " : "[Saved] ");
							dictionary.Add(name2.Key, text + name2.Value);
						}
						Server server = new Server(name, dictionary);
						if (server.Contexts.Count > 0)
						{
							_003C_003E2__current = server;
							_003C_003E1__state = 1;
							return true;
						}
					}
					goto IL_0124;
				}
				_003C_003E2__current = new Server("free", new Dictionary<string, string>
				{
					{ "en_US", "Creative Island" },
					{ "ko_KR", "창작섬" }
				});
				_003C_003E1__state = 2;
				return true;
				IL_0124:
				_003Ci_003E5__3++;
				goto IL_0136;
			}
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}

		[DebuggerHidden]
		IEnumerator<Server> IEnumerable<Server>.GetEnumerator()
		{
			_003CGetServers_003Ed__0 _003CGetServers_003Ed__;
			if (_003C_003E1__state == -2 && _003C_003El__initialThreadId == Thread.CurrentThread.ManagedThreadId)
			{
				_003C_003E1__state = 0;
				_003CGetServers_003Ed__ = this;
			}
			else
			{
				_003CGetServers_003Ed__ = new _003CGetServers_003Ed__0(0);
			}
			_003CGetServers_003Ed__.clusters = _003C_003E3__clusters;
			return _003CGetServers_003Ed__;
		}

		[DebuggerHidden]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<Server>)this).GetEnumerator();
		}
	}

	public static IEnumerable<Server> GetServers(Dictionary<string, Cluster> clusters)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CGetServers_003Ed__0(-2)
		{
			_003C_003E3__clusters = clusters
		};
	}
}
