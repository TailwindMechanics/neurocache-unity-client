using Unity.Plastic.Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Sirenix.OdinInspector;
using System.Threading;
using System.Net.Http;
using System.Text;
using UnityEngine;
using System.IO;
using Zenject;
using System;

using Modules.Agents.External.Schema;
using Modules.Credentials.External;
using Modules.Agents.External;


namespace Modules.Agents.Internal
{
    public class Agents : MonoInstaller, IAgents
    {
        [TextArea(2,2), SerializeField] string endpoint;
        [InlineEditor, SerializeField] KeySo apiKeySo;
        [InlineEditor, SerializeField] KeySo agentIdSo;
        [TextArea(3,3), SerializeField] string promptText;


        [UsedImplicitly]
        string ButtonText => executing ? "Running..." : "Send Request";
        [PropertyOrder(3), SerializeField]
        bool executing;
        CancellationTokenSource cancellationTokenSource;

        [HideIf("$executing"), Button("$ButtonText", ButtonSizes.Large)]
        void Send()
            => OnSendRequest(agentIdSo.Vo, promptText, apiKeySo.Vo);

        [ShowIf("executing"), PropertyOrder(2), Button("Cancel Request", ButtonSizes.Large)]
        void Cancel()
            => cancellationTokenSource?.Cancel();

        public override void InstallBindings()
            => Container.Bind<IAgents>().FromInstance(this).AsSingle();

        void OnSendRequest(KeyVo agentId, string prompt, KeyVo apiKey)
        {
            executing = true;
            cancellationTokenSource = new CancellationTokenSource();
            Task.Run(() => SendAndProcessSseAsync(new RunAgentRequest(agentId.Key, prompt), apiKey, cancellationTokenSource.Token));
        }

        async Task SendAndProcessSseAsync(RunAgentRequest runAgentRequest, KeyVo apiKey, CancellationToken cancelToken)
        {
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/event-stream"));
            var jsonPayload = JsonConvert.SerializeObject(runAgentRequest);
            Debug.Log("<color=white><b>Subscribing...</b></color>");

            var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
            request.Headers.Add("apikey", apiKey.Key);
            request.Content = content;
            var emissionCount = 0;

            try
            {
                using var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancelToken);
                if (!response.IsSuccessStatusCode)
                {
                    Debug.LogError($"Error: {response.StatusCode}, {response.ReasonPhrase}");
                    Debug.LogError($"Error: {JsonConvert.SerializeObject(response)}");
                }
                else
                {
                    await using (var stream = await response.Content.ReadAsStreamAsync())
                    using (var reader = new StreamReader(stream))
                    {
                        while (!reader.EndOfStream && executing)
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

                            if (!data.Contains("{"))
                            {
                                Debug.Log($"<color=yellow><b>       {data}</b></color>");
                                continue;
                            }

                            var node = JsonConvert.DeserializeObject<Node>(data);
                            Debug.Log(
                                $"<color=yellow><b>       {emissionCount}. Executing node: {node.Data.NodeName}, {JsonConvert.SerializeObject(node)}</b></color>");

                            emissionCount++;
                        }
                    }

                    Debug.Log("<color=white><b>Subscription closed.</b></color>");
                }
            }
            catch (OperationCanceledException)
            {
                Debug.Log("<color=red><b>Operation cancelled by user.</b></color>");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error: {JsonConvert.SerializeObject(ex)}");
            }
            finally
            {
                executing = false;
                cancellationTokenSource = null;
            }
        }
    }
}