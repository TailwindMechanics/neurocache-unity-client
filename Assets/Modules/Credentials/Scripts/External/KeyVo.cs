using UnityEngine;
using System;

namespace Modules.Credentials.External
{
	[Serializable]
	public class KeyVo
	{
		public string Key => key;
		[TextArea(5,5), SerializeField] string key;
	}
}