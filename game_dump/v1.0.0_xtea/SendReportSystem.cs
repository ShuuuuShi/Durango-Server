using System;
using System.Collections;
using System.Collections.Generic;
using BestHTTP;
using UnityEngine;

public class SendReportSystem : GameSystem<SendReportSystem>
{
	public enum ReportType
	{
		Players,
		Scribbles,
		Nameables,
		Clans
	}

	public enum Response
	{
		Done,
		BadRequest,
		Conflict,
		Error
	}

	private ReportType _type;

	public ulong EntityId { get; private set; }

	public event Action<Response> ResponseReceived;

	public void SetTarget(ReportType type, ulong entityId)
	{
		_type = type;
		EntityId = entityId;
	}

	public void SendReport(string textReason)
	{
		((MonoBehaviour)this).StartCoroutine(CoSendReport(textReason));
	}

	private IEnumerator CoSendReport(string textReason)
	{
		Dictionary<string, string> fields = new Dictionary<string, string>
		{
			{
				"type",
				_type.ToString()
			},
			{
				"reporter_id",
				PlayerBehavior.LocalPlayer.EntityId.ToString()
			},
			{
				"reportee_id",
				EntityId.ToString()
			},
			{ "text", textReason }
		};
		Dictionary<string, string> header = new Dictionary<string, string> { 
		{
			"Authorization",
			KSingleton<GameManager>.Instance().SessionToken
		} };
		string url = KSingleton<GameManager>.Instance().MakeGatewayUrl("reports");
		HTTPRequest request = KUtility.RequestUrl(url, null, disableCache: true, header, fields);
		while (request.MoveNext())
		{
			yield return null;
		}
		Response response = Response.Done;
		HTTPRequestStates state = request.State;
		if (state == HTTPRequestStates.Finished)
		{
			if (!request.Response.IsSuccess)
			{
				response = request.Response.StatusCode switch
				{
					400 => Response.BadRequest, 
					409 => Response.Conflict, 
					_ => Response.Error, 
				};
			}
		}
		else
		{
			response = Response.Error;
		}
		if (this.ResponseReceived != null)
		{
			this.ResponseReceived(response);
		}
	}
}
