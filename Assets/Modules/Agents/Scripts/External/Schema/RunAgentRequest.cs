using Unity.Plastic.Newtonsoft.Json;

namespace Modules.Agents.External.Schema
{
	public class RunAgentRequest
	{
		public RunAgentRequest(string agentId, string prompt)
		{
			AgentId = agentId;
			Prompt = prompt;
		}

		[JsonProperty("agentid")]
		public string AgentId {get;}
		[JsonProperty("prompt")]
		public string Prompt {get;}
	}
}