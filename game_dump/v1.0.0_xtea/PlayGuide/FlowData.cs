using Newtonsoft.Json.Linq;

namespace PlayGuide;

public class FlowData
{
	public string Name;

	public string Cond;

	public FlowContainer TrueList;

	public FlowContainer FalseList;

	public string TrueFlow;

	public string FalseFlow;

	public static void ParseFlow(JArray list, FlowContainer dataList)
	{
		for (int i = 0; i < list.Count; i++)
		{
			JToken jToken = list[i];
			FlowData flowData = new FlowData();
			if (jToken.Type == JTokenType.String)
			{
				flowData.Name = (string)jToken;
			}
			else if (jToken.Type == JTokenType.Object)
			{
				flowData.Cond = (string)jToken["cond"];
				JToken jToken2 = jToken["true"];
				JToken jToken3 = jToken["false"];
				if (jToken2 != null)
				{
					if (jToken2.Type == JTokenType.String)
					{
						flowData.TrueFlow = (string)jToken2;
					}
					else if (jToken2.Type == JTokenType.Array)
					{
						flowData.TrueList = new FlowContainer();
						ParseFlow(jToken2 as JArray, flowData.TrueList);
					}
				}
				if (jToken3 != null)
				{
					if (jToken3.Type == JTokenType.String)
					{
						flowData.FalseFlow = (string)jToken3;
					}
					else if (jToken3.Type == JTokenType.Array)
					{
						flowData.FalseList = new FlowContainer();
						ParseFlow(jToken3 as JArray, flowData.FalseList);
					}
				}
			}
			dataList.List.Add(flowData);
		}
	}
}
