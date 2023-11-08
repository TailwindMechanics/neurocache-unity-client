using Unity.Plastic.Newtonsoft.Json;
using UnityEngine;
using System;

namespace Modules.Agents.External
{
	[Serializable]
	public class AgentVo
	{
		[JsonProperty("agent_id")]
		public string AgentId => agentId;
		[JsonProperty("payload")]
		public string Payload => payload;

		[SerializeField] string agentId;
		[TextArea(5,5), SerializeField] string payload;
	}
}