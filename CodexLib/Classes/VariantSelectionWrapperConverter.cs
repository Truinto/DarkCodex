using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodexLib
{
    [UsedImplicitly]
    internal class VariantSelectionWrapperConverter : JsonConverter //BlueprintConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(VariantSelectionWrapper);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) // TODO: also allow simpleblueprint (parse GUID)
        {
            var wrapper = existingValue as VariantSelectionWrapper ?? new();

            if (reader.Value is not string text || text == "" || text == "null")
            {
                Helper.PrintDebug("JsonConverter read KineticistTree.Element is null");
                return wrapper;
            }

            var element = KineticistTree.Instance.GetAll(true, true, archetype: true).FirstOrDefault(f => f.BlastFeature?.Get()?.name == text);
            wrapper.Selected = element;
            Helper.PrintDebug("JsonConverter read KineticistTree.Element " + element);
            return wrapper;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value is not VariantSelectionWrapper wrapper 
                || wrapper.Selected is not KineticistTree.Element element 
                || element.BlastFeature?.Get()?.name is not string name)
            {
                writer.WriteNull();
                return;
            }

            writer.WriteValue(name);
            Helper.PrintDebug("JsonConverter write KineticistTree.Element " + element);
        }
    }
}
