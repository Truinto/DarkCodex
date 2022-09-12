using Kingmaker.UI;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodexLib
{
    [UsedImplicitly]
    internal class VariantSelectionDataConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(VariantSelectionData);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var result = existingValue as VariantSelectionData ?? new VariantSelectionData();

            var jt = JToken.Load(reader);
            if (jt.Type != JTokenType.Object)
                return result;

            var type = jt["type"]?.ToString();
            var guid = BlueprintGuid.Parse(jt["guid"]?.ToString());
            if (guid == null)
            {
                Helper.PrintDebug("VariantSelectionDataConverter no guid");
            }
            else if (type == "SimpleBlueprint")
            {
                var bp = ResourcesLibrary.TryGetBlueprint(guid);
                if (bp is IUIDataProvider uibp)
                    result.Selected = uibp;
            }
            else if (type == "KineticistTree+Element")
            {
                result.Selected = KineticistTree.Instance.GetAll(true, true, archetype: true).FirstOrDefault(f => f.BlastFeature.deserializedGuid == guid);
            }

            return result;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value is VariantSelectionData data)
                value = data.Selected;

            if (value is null)
            {
                writer.WriteNull();
            }
            else if (value is SimpleBlueprint sb)
            {
                writer.WriteStartObject();
                writer.WritePropertyName("type");
                writer.WriteValue("SimpleBlueprint");
                writer.WritePropertyName("guid");
                writer.WriteValue(sb.AssetGuid.ToString());
                writer.WriteEndObject();
            }
            else if (value is KineticistTree.Element element)
            {
                writer.WriteStartObject();
                writer.WritePropertyName("type");
                writer.WriteValue("KineticistTree+Element");
                writer.WritePropertyName("guid");
                writer.WriteValue(element.BlastFeature.deserializedGuid.ToString());
                writer.WriteEndObject();
            }

            //if (value is not VariantSelectionWrapper wrapper 
            //    || wrapper.Selected is not KineticistTree.Element element 
            //    || element.BlastFeature?.Get()?.name is not string name)
            //{
            //    writer.WriteNull();
            //    return;
            //}

            //writer.WriteValue(name);
            //Helper.PrintDebug("JsonConverter write KineticistTree.Element " + element);
        }
    }
}
