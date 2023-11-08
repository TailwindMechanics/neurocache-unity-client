using UnityEngine;

namespace Modules.Credentials.External
{
    [CreateAssetMenu(fileName = "new _apiKey", menuName = "Modules/Credentials/Api Key")]
    public class ApiKeySo : ScriptableObject
    {
        public string ApiKey => apiKey;
        [TextArea(5,5), SerializeField] string apiKey;
    }
}