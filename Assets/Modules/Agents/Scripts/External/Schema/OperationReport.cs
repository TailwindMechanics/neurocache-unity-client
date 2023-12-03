using Unity.Plastic.Newtonsoft.Json;
using System.Collections.Generic;
using System;

namespace Modules.Agents.External.Schema
{
	public class OperationReport
	{
		public OperationReport(Guid token,
			string author,
			string payload,
			Guid agentId,
			bool final,
			string reportId,
			int status = 200,
			List<string> errors = null
			)
		{
			Token = token;
			Author = author;
			Payload = payload;
			AgentId = agentId;
			Final = final;
			ReportId = reportId;
			Status = status;
			Errors = errors;
		}

		public Guid Token { get; }
		public string Author { get; private set; }
		public string Payload { get; }
		public Guid AgentId { get; }
		public bool Final { get; }
		public string ReportId { get; }

		public List<string> Errors { get; }
		public int Status { get; }

		public void SetClientAuthor()
			=> Author = "Client";
		public void SetVanguardAuthor()
			=> Author = "Client";
		public override string ToString()
			=> $"OperationReport({Author}, {Payload}, {Token}, {Final}, {ReportId}, {Errors}, {Status})";
		public static OperationReport? FromJson(string json)
			=> JsonConvert.DeserializeObject<OperationReport>(json);
		public static string ToJson(OperationReport report)
			=> JsonConvert.SerializeObject(report);
	}
}