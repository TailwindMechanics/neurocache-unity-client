using Unity.Plastic.Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Sirenix.OdinInspector;
using System.Net.Http;
using System.Text;
using UnityEngine;
using System.IO;
using Zenject;
using System;

using Modules.Credentials.External;
using Modules.Agents.External;
using Modules.Agents.External.Schema;


namespace Modules.Agents.Internal
{
    public class Agents : MonoInstaller, IAgents
    {
        [TextArea(2,2), SerializeField] string endpoint;
        [InlineEditor, SerializeField] KeySo apiKeySo;
        [InlineEditor, SerializeField] KeySo agentIdSo;
        [TextArea(3,3), SerializeField] string promptText;
        [DisableIf("$executing"), Button("$ButtonText", ButtonSizes.Large)]
        void SendRequest()
        {
            executing = true;
            OnSendRequest(agentIdSo.Vo, promptText, apiKeySo.Vo);
        }

        [HideInInspector, SerializeField]
        bool executing;
        [UsedImplicitly]
        string ButtonText => executing ? "Running..." : "Send Request";

        public override void InstallBindings()
            => Container.Bind<IAgents>().FromInstance(this).AsSingle();

        void OnSendRequest(KeyVo agentId, string prompt, KeyVo apiKey)
            => Task.Run(()
                => SendAndProcessSseAsync(
                    new RunAgentRequest(agentId.Key, prompt),
                    apiKey
                ));

        async Task SendAndProcessSseAsync(RunAgentRequest runAgentRequest, KeyVo apiKey)
        {
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/event-stream"));
            var jsonPayload = JsonConvert.SerializeObject(runAgentRequest);
            Debug.Log($"<color=white><b>Subscribing...</b></color>");

            var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
            request.Headers.Add("apikey", apiKey.Key);
            request.Content = content;
            var emissionCount = 0;

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
                            if (string.IsNullOrWhiteSpace(line) || !line.StartsWith("data:")) continue;

                            var data = line.Replace("data:", "").Trim();
                            if (data.Contains("<start"))
                            {
                                Debug.Log($"<color=green><b>    {data}</b></color>");
                                continue;
                            }
                            if (data.Contains("</end"))
                            {
                                Debug.Log($"<color=orange><b>   {data}</b></color>");
                                break;
                            }

                            var node = JsonConvert.DeserializeObject<NodeData>(data);
                            Debug.Log($"<color=yellow><b>       {emissionCount}. Executing node: {node.NodeName}</b></color>");
                            emissionCount++;
                        }
                    }

                    Debug.Log("<color=white><b>Subscription closed.</b></color>");
                }
                else
                {
                    Debug.LogError($"Error: {response.StatusCode}, {response.ReasonPhrase}");
                    Debug.LogError($"Error: {JsonConvert.SerializeObject(response)}");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error: {JsonConvert.SerializeObject(ex)}");
            }
            finally
            {
                executing = false;
            }
        }
    }
}