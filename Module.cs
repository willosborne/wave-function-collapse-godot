using Godot;
using Newtonsoft.Json;
using System.Collections.Generic;

public class Module
{
    [JsonProperty("mesh")]
    public string MeshName {get; set;} = "";
    public int Rotation {get; set;}
    public Dictionary<string, string> Sockets{get; set;}
    public Dictionary<string, List<string>> Adjacency{get; set;}
    [JsonIgnore]
    public PackedScene Scene {get; set; }
}
