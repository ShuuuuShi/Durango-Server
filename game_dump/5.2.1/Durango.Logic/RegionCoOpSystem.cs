using Durango.Network;
using Durango.UI;
using Durango.Utils;
using L10N;
using Messages;
using Yaml;

namespace Durango.Logic;

public class RegionCoOpSystem : GameSystem<RegionCoOpSystem>
{
	private const string NavigationKeyPrefix = "RegionCoOp.";

	private void Awake()
	{
		Connections.Frontend.On<CurrentRegionCoOpTodos>(OnCurrentRegionCoOpTodos);
		Connections.Radiotower.On<RegionCoOpTodoSpawned>(OnRegionCoOpTodoSpawned);
		Connections.Radiotower.On<NotifyRegionCoOpTodoProceed>(OnNotifyRegionCoOpTodoProceed);
		Connections.Radiotower.On<RegionCoOpTodoCompleted>(OnRegionCoOpTodoCompleted);
	}

	private static string GenerateKey(string id)
	{
		return "RegionCoOp." + id;
	}

	private void OnCurrentRegionCoOpTodos(CurrentRegionCoOpTodos packet, PacketHeader header)
	{
		RegionCoOpTodo[] todos = packet.Todos;
		for (int i = 0; i < todos.Length; i++)
		{
			RegionCoOpTodo regionCoOpTodo = todos[i];
			if (regionCoOpTodo.Notice.HasValue)
			{
				AddToMapIndicator(regionCoOpTodo.CoOpId, regionCoOpTodo.Notice.Value.Item2);
			}
		}
	}

	private void OnRegionCoOpTodoSpawned(RegionCoOpTodoSpawned packet, PacketHeader header)
	{
		GameSystem<MapSystem>.Instance().GetRegion(packet.RegionId, delegate(Region region)
		{
			UIManager.Alarm.ShowNotify(T._("{0}에서 강력한 신호가 감지되었습니다. 개척자들의 힘을 모아주세요.", region.Name), "act_warpback", major: true);
		});
		if (packet.CoOp.Notice.HasValue && packet.RegionId == GameManager.Region.Id)
		{
			AddToMapIndicator(packet.CoOp.CoOpId, packet.CoOp.Notice.Value.Item2);
		}
	}

	private void OnNotifyRegionCoOpTodoProceed(NotifyRegionCoOpTodoProceed packet, PacketHeader header)
	{
	}

	private void OnRegionCoOpTodoCompleted(RegionCoOpTodoCompleted packet, PacketHeader header)
	{
		GameSystem<MapSystem>.Instance().GetRegion(packet.RegionId, delegate(Region region)
		{
			RegionCoOp regionCoOp = RegionCoOpDict.GetRegionCoOp(region.TemplateId, packet.CoOpId);
			if (regionCoOp != null)
			{
				UIManager.SystemMsg(T._("{0}의 <em>{1}</em> 공동 임무가 종료되었습니다.", region.Name, regionCoOp.Subject));
			}
		});
		if (packet.RegionId == GameManager.Region.Id)
		{
			RemoveFromMapIndicator(packet.CoOpId);
		}
	}

	private void AddToMapIndicator(string coOpId, Point2 tile)
	{
		RegionCoOp regionCoOp = RegionCoOpDict.GetRegionCoOp(GameManager.Region.TemplateId, coOpId);
		if (regionCoOp != null)
		{
			string titleName = regionCoOp.Subject;
			string noticeIcon = regionCoOp.NoticeIcon;
			Singleton<MapIndicators>.Instance().AddAnnounceBalloon(AnnounceType.GeneralPoint, tile.ToVector2(), GenerateKey(coOpId), noticeIcon, titleName, 49);
		}
	}

	private void RemoveFromMapIndicator(string coOpId)
	{
		Singleton<MapIndicators>.Instance().RemoveAnnounceBalloon(AnnounceType.GeneralPoint, GenerateKey(coOpId));
	}
}
