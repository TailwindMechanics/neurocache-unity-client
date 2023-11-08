using Unity.Plastic.Newtonsoft.Json;
using System.Threading.Tasks;
using Sirenix.OdinInspector;
using System.Net.Http;
using UnityEngine;
using System.IO;
using Zenject;
using System;
using UniRx;

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

            var httpClient = new HttpClient();
            var queryParams = $"?payload={Uri.EscapeDataString(agent.Payload)}&agent_id={Uri.EscapeDataString(agent.AgentId)}";
            var sseEndpoint = endpoint + queryParams;

            httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("text/event-stream"));
            httpClient.GetStreamAsync(sseEndpoint).ToObservable()
                .SelectMany(stream => ReadStream(stream).ToObservable())
                .SelectMany(stream => stream.Select(line => line))
                .TakeUntilDestroy(this)
                .Finally(() => Debug.Log("<color=orange><b>>>> Connection closed</b></color>"))
                .Subscribe(
                    line => Debug.Log($"<color=green><b>>>> {line}</b></color>"),
                    ex => Debug.LogError($"Error: {ex.Message}")
                );
        }

        Task<IObservable<string>> ReadStream(Stream stream)
        {
            return Task.FromResult(Observable.Create<string>(observer =>
            {
                var reader = new StreamReader(stream);
                var cancellationToken = new System.Threading.CancellationToken();

                Action readLines = ReadLines;
                readLines.Invoke();

                return Disposable.Create(() =>
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    cancellationToken = new System.Threading.CancellationToken(true);
                });

                async void ReadLines()
                {
                    try
                    {
                        while (!cancellationToken.IsCancellationRequested)
                        {
                            var line = await reader.ReadLineAsync();
                            if (!string.IsNullOrWhiteSpace(line) && line.StartsWith("data:"))
                            {
                                observer.OnNext(line.Replace("data:", "").Trim());
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        observer.OnError(ex);
                    }
                    finally
                    {
                        reader.Dispose();
                        observer.OnCompleted();
                    }
                }
            }));
        }
    }
}