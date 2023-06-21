
using System.Collections.Generic;
using Newtonsoft.Json;

namespace RoRes
{
  public partial class Loadout
  {
    [JsonProperty("id")]
    public long Id { get; set; }

    [JsonProperty("seed")]
    public long Seed { get; set; }

    [JsonProperty("survivor")]
    public string Survivor { get; set; }

    [JsonProperty("skills")]
    public List<long> Skills { get; set; }

    [JsonProperty("stages")]
    public List<string> Stages { get; set; }
  }
}