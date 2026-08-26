using System;
using System.Collections.Generic;
using BestHTTP;
using Durango.Utils;
using Newtonsoft.Json;

public class SendReportSystem : GameSystem<SendReportSystem>
{
	public enum ReportType
	{
		Players,
		Scribbles,
		Nameables,
		Clans,
		ServerStatus,
		Suggestion
	}

	public enum PlayerReportCategory
	{
		None,
		ImproperName,
		Insult,
		Cheating,
		Spam,
		Etc
	}

	public enum Response
	{
		Done,
		BadRequest,
		NotFound,
		Conflict,
		Error
	}

	private struct Payload
	{
		[JsonProperty(PropertyName = "text")]
		public string Text;
	}

	public void SendReport(ReportType type, string entityId, PlayerReportCategory category, string content, Action<Response> callback)
	{
		string value = "none";
		switch (category)
		{
		case PlayerReportCategory.ImproperName:
			value = "improper_name";
			break;
		case PlayerReportCategory.Insult:
			value = "insult";
			break;
		case PlayerReportCategory.Cheating:
			value = "cheating";
			break;
		case PlayerReportCategory.Spam:
			value = "spam";
			break;
		case PlayerReportCategory.Etc:
			value = "etc";
			break;
		}
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		dictionary.Add("type", type.ToString());
		dictionary.Add("reporter_id", PlayerBehavior.LocalPlayer.EntityId);
		dictionary.Add("reportee_id", entityId);
		dictionary.Add("category", value);
		dictionary.Add("text", content);
		string url = GameManager.GatewayUrl + "/reports";
		Http.Request(url, delegate(byte[] bytes, HTTPResponse response)
		{
			Response obj = Response.Error;
			if (response != null)
			{
				if (response.IsSuccess)
				{
					obj = Response.Done;
				}
				else
				{
					switch (response.StatusCode)
					{
					case 400:
						obj = Response.BadRequest;
						break;
					case 404:
						obj = Response.NotFound;
						break;
					case 409:
						obj = Response.Conflict;
						break;
					}
				}
			}
			if (callback != null)
			{
				callback(obj);
			}
		}, disableCache: true, addSession: true, dictionary, HTTPMethods.Post);
	}

	public void SendServerStatus(string text, string title, Action<bool> callback)
	{
		// เดิมส่งเข้า Slack webhook ของ Nexon (ตายแล้ว) — ส่งเข้ารายงานของเซิร์ฟเราแทน
		// server เก็บเป็นไฟล์ใน data/reports/ แล้วตอบ 200 = รับเรื่องแล้ว
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		dictionary.Add("type", "ServerStatus");
		dictionary.Add("reporter_id", PlayerBehavior.LocalPlayer.EntityId);
		dictionary.Add("reportee_id", string.Empty);
		dictionary.Add("category", title);
		dictionary.Add("text", text);
		string url = GameManager.GatewayUrl + "/reports";
		Http.Request(url, delegate(byte[] bytes, HTTPResponse response)
		{
			if (callback != null)
			{
				callback(response?.IsSuccess ?? false);
			}
		}, disableCache: true, addSession: true, dictionary, HTTPMethods.Post);
	}
}
