using UnityEngine;
using System;

namespace Modules.Agents.External.Schema
{
	[Serializable]
	public class OperationReportVo
	{
		public string Author => author;
		public string Recipient => recipient;
		public string Payload => payload;
		public string ReportId => reportId;

		[SerializeField] string author;
		[SerializeField] string recipient;
		[SerializeField] string reportId;
		[TextArea(3,3), SerializeField] string payload;
	}
}