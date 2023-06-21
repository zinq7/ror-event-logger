using System.Collections.Generic;
using Newtonsoft.Json;

namespace RoRes
{


  public partial class Race
  {
    [JsonProperty("id")]
    public long Id { get; set; }

    [JsonProperty("lobbyCode")]
    public string LobbyCode { get; set; }

    [JsonProperty("password")]
    public string Password { get; set; }

    [JsonProperty("active")]
    public bool Active { get; set; }

    [JsonProperty("loadouts")]
    public List<Loadout> Loadouts { get; set; }

    [JsonProperty("teams")]
    public List<Team> Teams { get; set; }


    [JsonProperty("createTimestamp")]
    public long CreateTimestamp { get; set; }

    [JsonProperty("createUser")]
    public long CreateUser { get; set; }
  }

  public partial class Race
  {
    public static Race FromJson(string json) => JsonConvert.DeserializeObject<Race>(json, Converter.Settings);
  }




}