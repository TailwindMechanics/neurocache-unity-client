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
			bool final,
			string reportId,
			List<string> errors = null,
			int status = 200
			)
		{
			Token = token;
			Author = author;
			Payload = payload;
			Final = final;
			ReportId = reportId;
			Errors = errors;
			Status = status;
		}

		public Guid Token { get; }
		public string Author { get; private set; }
		public string Payload { get; }
		public bool Final { get; }
		public string ReportId { get; }

		public List<string> Errors { get; }
		public int Status { get; }

		public void SetClientAuthor()
			=> Author = "Client";
		public override string ToString()
			=> $"OperationReport({Author}, {Payload}, {Token}, {Final}, {ReportId}, {Errors}, {Status})";
		public static OperationReport FromJson(string json)
			=> JsonConvert.DeserializeObject<OperationReport>(json);
		public static string ToJson(OperationReport report)
			=> JsonConvert.SerializeObject(report);
	}
}