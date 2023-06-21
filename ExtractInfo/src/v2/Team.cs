
namespace RoRes
{

  using System.Collections.Generic;
  using Newtonsoft.Json;


  public partial class Team
  {
    [JsonProperty("id")]
    public long Id { get; set; }

    [JsonProperty("userIds")]
    public List<long> UserIds { get; set; }

    [JsonProperty("runSubmissions")]
    public List<RunSubmission> RunSubmissions { get; set; } = new List<RunSubmission>();
  }
}