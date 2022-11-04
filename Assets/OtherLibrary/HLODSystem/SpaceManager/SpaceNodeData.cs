using System;
using Newtonsoft.Json;
using UnityEngine;

[Serializable]
public struct SpaceNodeData
{
    public int index;
    public int parent;
    public int[] children;
    public string[] nodes;
    [JsonConverter(typeof(Bounds))]
    public Bounds bounds;
}