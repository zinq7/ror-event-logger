using System;
using Newtonsoft.Json;

namespace RoRes
{

  public partial class User
  {
    [JsonProperty("id")]
    public long Id { get; set; }

    [JsonProperty("username")]
    public string Username { get; set; }

    [JsonProperty("pfp")]
    public Uri Pfp { get; set; }
  }

  public partial class User
  {
    public static User FromJson(string json) => JsonConvert.DeserializeObject<User>(json, Converter.Settings);
  }


}