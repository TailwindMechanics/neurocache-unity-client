using Modules.Credentials.External;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Modules.Agents.External.Schema
{
	[CreateAssetMenu(fileName = "new _connectionSettings", menuName = "Modules/Agents/Settings/Connection")]
	public class ConnectionSettingsSo : ScriptableObject
	{
		public string Endpoint => endpoint;
		public string ApiKey => apiKeySo.Vo.Key;
		public string AgentId => agentIdSo.Vo.Key;

		[TextArea(2, 2), SerializeField] string endpoint;
		[InlineEditor, SerializeField] KeySo apiKeySo;
		[InlineEditor, SerializeField] KeySo agentIdSo;
	}
}