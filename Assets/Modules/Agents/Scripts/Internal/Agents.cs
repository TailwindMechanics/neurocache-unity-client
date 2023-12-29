using System.Threading.Tasks;
using JetBrains.Annotations;
using Sirenix.OdinInspector;
using System.Net.WebSockets;
using System.Threading;
using System.Text;
using UnityEngine;
using Zenject;
using System;

using Modules.Agents.External.Schema;
using Modules.Agents.External;

namespace Modules.Agents.Internal
{
    public class Agents : MonoInstaller, IAgents
    {
        [TextArea(2, 2), ReadOnly, SerializeField]
        string operationToken;

        [InlineEditor, SerializeField]
        ConnectionSettingsSo settings;

        [FoldoutGroup("Report"), HideLabel, SerializeField]
        OperationReportVo reportVo;

        [UsedImplicitly]
        bool Connected => webSocket is { State: WebSocketState.Open } or { State: WebSocketState.Connecting };
        [UsedImplicitly]
        bool AllowSend => !string.IsNullOrWhiteSpace(operationToken);

        [EnableIf("$Connected"), ButtonGroup("OpenClose"), Button(ButtonSizes.Large)]
        void Disconnect() => CloseWebSocketConnection();
        [DisableIf("$Connected"), ButtonGroup("OpenClose"), Button(ButtonSizes.Large)]
        void Connect()
        {
            operationToken = "";
            StartWebSocketConnection();
        }

        [EnableIf("$AllowSend"), Button(ButtonSizes.Large)]
        void Send()
        {
            var report = new OperationReport(
                Guid.Parse(operationToken),
                reportVo.Author,
                reportVo.Recipient,
                reportVo.Payload,
                Guid.Parse(settings.AgentId),
                true,
                reportVo.ReportId
            );
            SendWebSocketMessage(OperationReport.ToJson(report));
        }

        CancellationTokenSource cancellationTokenSource;
        ClientWebSocket webSocket;

        public override void InstallBindings()
            => Container.Bind<IAgents>().FromInstance(this).AsSingle();

        void OnReportReceived(OperationReport report)
        {
            if (OnReceivedDispatchStopReport(report)) return;

            Debug.Log($"<color=yellow><b>Received report: {report}</b></color>");
            OnReceivedVanguardStartedReport(report);
        }

        bool OnReceivedDispatchStopReport(OperationReport report)
        {
            if (report.Author == "Vanguard" && report.ReportId == "vanguard_stop")
            {
                Debug.Log($"Dispatching stop signal for operation: {operationToken}");
                CloseWebSocketConnection();
                return true;
            }

            return false;
        }

        void OnReceivedVanguardStartedReport(OperationReport report)
        {
            if (report.ReportId == "vanguard_started")
            {
                operationToken = report.Token.ToString();
            }
        }

        async void StartWebSocketConnection()
        {
            cancellationTokenSource = new CancellationTokenSource();
            webSocket = new ClientWebSocket();

            try
            {
                var wsUri = new Uri($"{settings.Endpoint}/agent/run?agentId={settings.AgentId}&apiKey={settings.ApiKey}");
                Debug.Log($"<color=white><b>Connecting to uri:{wsUri}</b></color>");
                await webSocket.ConnectAsync(wsUri, cancellationTokenSource.Token);
                Debug.Log("<color=#47fcfc><b>>>> Connected via WebSocket</b></color>");

                await ReceiveMessages(cancellationTokenSource.Token);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error with WebSocket connection: {ex}");
                webSocket?.Dispose();
                webSocket = null;
            }
        }

        async void CloseWebSocketConnection()
        {
            if (webSocket is not { State: WebSocketState.Open })
            {
                Debug.Log("<color=red><b>WebSocket connection is not open or already closed.</b></color>");
                return;
            }

            try
            {
                await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
                cancellationTokenSource?.Cancel();
            }
            catch (WebSocketException ex)
            {
                Debug.LogError($"WebSocketException during close: {ex.Message}");
            }
            finally
            {
                webSocket.Dispose();
                webSocket = null;
                cancellationTokenSource?.Dispose();
                cancellationTokenSource = null;
                Debug.Log("<color=orange><b>WebSocket connection closed.</b></color>");
            }
        }

        async Task ReceiveMessages(CancellationToken cancelToken)
        {
            var buffer = new byte[1024 * 4];
            while (webSocket is { State: WebSocketState.Open } && !cancelToken.IsCancellationRequested)
            {
                var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), cancelToken);
                if (result.MessageType == WebSocketMessageType.Text)
                {
                    var receivedMessage = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    var report = OperationReport.FromJson(receivedMessage);
                    OnReportReceived(report);
                }
                else if (result.MessageType == WebSocketMessageType.Close)
                {
                    break;
                }
            }
        }

        async void SendWebSocketMessage(string message)
        {
            if (webSocket is { State: WebSocketState.Open })
            {
                var messageBuffer = Encoding.UTF8.GetBytes(message);
                await webSocket.SendAsync(new ArraySegment<byte>(messageBuffer), WebSocketMessageType.Text, true, CancellationToken.None);
                Debug.Log($"<color=green><b>Message sent: {message}</b></color>");
            }
            else
            {
                Debug.LogError("<color=red><b>WebSocket connection is not open.</b></color>");
            }
        }
    }
}