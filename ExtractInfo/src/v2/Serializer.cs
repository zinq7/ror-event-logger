
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
namespace RoRes
{

  public static class Serializer
  {
    public static string ToJson(this Race self) => JsonConvert.SerializeObject(self, Converter.Settings);
    public static string ToJson(this User self) => JsonConvert.SerializeObject(self, Converter.Settings);
    public static string ToJson(this RunSubmission self) => JsonConvert.SerializeObject(self, Converter.Settings);
  }

  internal static class Converter
  {
    public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
    {
      MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
      DateParseHandling = DateParseHandling.None,
      Converters =
            {
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
    };
  }
}