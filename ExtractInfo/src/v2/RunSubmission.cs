

namespace RoRes
{
  using System.Collections.Generic;
  using Newtonsoft.Json;


  public partial class RunSubmission
  {
    [JsonProperty("loadoutId")]
    public long LoadoutId { get; set; }

    [JsonProperty("userId")]
    public long UserId { get; set; }

    [JsonProperty("runTime")]
    public long RunTime { get; set; }

    [JsonProperty("lazarusCount")]
    public long LazarusCount { get; set; }

    [JsonProperty("didConcede")]
    public bool DidConcede { get; set; }

    [JsonProperty("splitTimes")]
    public List<long> SplitTimes { get; set; }
  }

  public partial class RunSubmission
  {
    public static RunSubmission FromJson(string json) => JsonConvert.DeserializeObject<RunSubmission>(json, Converter.Settings);
  }



}
