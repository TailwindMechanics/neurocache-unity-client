using Unity.Plastic.Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Sirenix.OdinInspector;
using System.Net.Http;
using System.Text;
using UnityEngine;
using System.IO;
using Zenject;
using System;

using Modules.Agents.External;


namespace Modules.Agents.Internal
{
    public class Agents : MonoInstaller, IAgents
    {
        [TextArea(2,2), SerializeField] string endpoint;
        [FoldoutGroup("Agent"), HideLabel, SerializeField]
        AgentVo agentVo;
        [DisableInEditorMode, Button(ButtonSizes.Large)]
        void SendRequest() => OnSendRequest(agentVo);


        public override void InstallBindings()
            => Container.Bind<IAgents>().FromInstance(this).AsSingle();

        void OnSendRequest(AgentVo agent)
        {
            var jsonPayload = JsonConvert.SerializeObject(agent);
            Debug.Log($"<color=yellow><b>>>> Sending request: {jsonPayload}</b></color>");

            Task.Run(() => SendAndProcessSseAsync(jsonPayload));
        }

        async Task SendAndProcessSseAsync(string jsonPayload)
        {
            using var httpClient = new HttpClient();
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/event-stream"));

            var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
            request.Content = content;
            // request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", "your_api_token");

            try
            {
                using var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
                if (response.IsSuccessStatusCode)
                {
                    await using (var stream = await response.Content.ReadAsStreamAsync())
                    using (var reader = new StreamReader(stream))
                    {
                        while (!reader.EndOfStream)
                        {
                            var line = await reader.ReadLineAsync();
                            if (!string.IsNullOrWhiteSpace(line) && line.StartsWith("data:"))
                            {
                                var data = line.Replace("data:", "").Trim();
                                Debug.Log($"<color=green><b>>>> {data}</b></color>");
                            }
                        }
                    }

                    Debug.Log("<color=orange><b>>>> Connection closed</b></color>");
                }

                else Debug.LogError($"Error: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error: {ex.Message}");
            }
        }
    }
}