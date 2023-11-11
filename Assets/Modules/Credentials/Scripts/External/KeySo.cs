using Sirenix.OdinInspector;
using UnityEngine;

namespace Modules.Credentials.External
{
    [CreateAssetMenu(fileName = "new _key", menuName = "Modules/Credentials/Key")]
    public class KeySo : ScriptableObject
    {
        public KeyVo Vo => keyVo;
        [HideLabel, SerializeField] KeyVo keyVo;
    }
}