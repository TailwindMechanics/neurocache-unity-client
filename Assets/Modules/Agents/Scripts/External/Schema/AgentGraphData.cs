using Unity.Plastic.Newtonsoft.Json;
using System.Collections.Generic;


namespace Modules.Agents.External.Schema
{
    public class Node
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("data")]
        public NodeData Data { get; set; }

        [JsonProperty("type")]
        public string NodeType { get; set; }

        [JsonProperty("width")]
        public float Width { get; set; }

        [JsonProperty("height")]
        public float Height { get; set; }

        [JsonProperty("position")]
        public PositionXy Position { get; set; }

        [JsonProperty("selected")]
        public bool Selected { get; set; }

        [JsonProperty("positionAbsolute")]
        public PositionXy PositionAbsolute { get; set; }

        [JsonProperty("dragging")]
        public bool? Dragging { get; set; }
    }

    public class NodeData
    {
        [JsonProperty("body")]
        public string Body { get; set; }

        [JsonProperty("nodeId")]
        public string NodeId { get; set; }

        [JsonProperty("handles")]
        public List<Handle> Handles { get; set; }

        [JsonProperty("nodeName")]
        public string NodeName { get; set; }

        [JsonProperty("nodeType")]
        public string NodeType { get; set; }

        [JsonProperty("nodePosition")]
        public PositionXy NodePosition { get; set; }
    }

    public class Handle
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("type")]
        public string HandleType { get; set; }

        [JsonProperty("angle")]
        public float Angle { get; set; }

        [JsonProperty("offset")]
        public PositionXy Offset { get; set; }
    }

    public class PositionXy
    {
        [JsonProperty("x")]
        public float X { get; set; }

        [JsonProperty("y")]
        public float Y { get; set; }
    }
}