using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EasyMapper
{
    public static class MapperJson
    {
        // TODO : JsonSerializer ve JsonConvert, generic kullanımlar için interfaceleri desteklemediğinden json => model veya model => json dönüştürücü yapı kurulacak.
        #region Json Parse Methods
        //public static IEnumerable<TDestination> ToMapDeseriliaze<TSource, TDestination>(this IEnumerable<TSource> sourceList)
        //{
        //    return sourceList.Select(source => source.ToMap<TSource, TDestination>());
        //}

        //public static TDestination ToMapDeseriliaze<TDestination>(this string jsonContent, bool isArray = false)
        //{
        //    if (JsonDetect(jsonContent, isArray))
        //        throw new Exception("text is not in json format");

        //    var destinationProperties = Mapper.GetInstanceByType<TDestination>().GetType().GetProperties();
        //    var destinationInstance = Mapper.GetInstanceByType<TDestination>();
        //    var sourceData = JsonParseByKeyValue(jsonContent, isArray);
        //    for (int i = 0; i < destinationProperties.Length; i++)
        //    {
        //        var currentPropertyName = destinationProperties[i].Name;
        //        var value = GetPropertyValue(jsonContent, currentPropertyName);
        //        if (value is null)
        //            continue;
        //        if (destinationInstance.GetType().GetProperty(currentPropertyName) == null)
        //            continue;
        //        var destProp = destinationInstance.GetType().GetProperty(currentPropertyName);
        //        if (destProp == null)
        //            continue;
        //        try { destProp.SetValue(destinationInstance, value); }
        //        catch (Exception ex) { /* Uyuşmayan veri tipleri */ }
        //    }
        //    return destinationInstance;
        //}

        //public static IEnumerable<TDestination> ToMapSeriliaze<TSource, TDestination>(this IEnumerable<TSource> sourceList)
        //{
        //    return sourceList.Select(source => source.ToMap<TSource, TDestination>());
        //}

        //public static string ToMapSeriliaze<TSource>(this TSource source, bool isArray = false) where TSource : class, new()
        //{
        //    if (JsonDetect(jsonContent, isArray))
        //        throw new Exception("text is not in json format");

        //    var destinationProperties = Mapper.GetInstanceByType<TDestination>().GetType().GetProperties();
        //    var destinationInstance = Mapper.GetInstanceByType<TDestination>();
        //    var sourceData = JsonParseByKeyValue(jsonContent, isArray);
        //    for (int i = 0; i < destinationProperties.Length; i++)
        //    {
        //        var currentPropertyName = destinationProperties[i].Name;
        //        var value = GetPropertyValue(jsonContent, currentPropertyName);
        //        if (value is null)
        //            continue;
        //        if (destinationInstance.GetType().GetProperty(currentPropertyName) == null)
        //            continue;
        //        var destProp = destinationInstance.GetType().GetProperty(currentPropertyName);
        //        if (destProp == null)
        //            continue;
        //        try { destProp.SetValue(destinationInstance, value); }
        //        catch (Exception ex) { /* Uyuşmayan veri tipleri */ }
        //    }
        //    return destinationInstance;
        //}
        #endregion

        private static bool JsonDetect(string jsonContent, bool isArray = false)
        {
            char[] JsonArrayChars = { '[', ']', '{', '}', ',', ':' };
            char[] JsonChars = { '{', '}', ',', ':' };
            if (isArray)
            {
                var complete = jsonContent.Any(a => JsonArrayChars.Contains(a));
                var correctStart = jsonContent.TrimStart().StartsWith(JsonArrayChars.First());
                var correctEnd = jsonContent.TrimEnd().EndsWith(JsonArrayChars.Skip(1).First());
                return complete && correctStart && correctEnd;
            }
            else
            {
                var complete = jsonContent.Any(a => JsonChars.Contains(a));
                var correctStart = jsonContent.TrimStart().StartsWith(JsonChars.First());
                var correctEnd = jsonContent.TrimEnd().EndsWith(JsonChars.Skip(1).First());
                return complete && correctStart && correctEnd;
            }
        }

        private static KeyValuePair<string, object> JsonParseByKeyValue(string jsonContentRaw)
        {
            var list = new Dictionary<string, object>();
            var clearWhiteSpaces = jsonContentRaw.ToList().Where(w => !char.IsWhiteSpace(w) && w != '\n' && w != '\r').ToString();
            var jsonContent = clearWhiteSpaces.TrimStart('{').TrimEnd('}');
            var jsonObjects = jsonContent.Split("\",").ToList();
            jsonObjects.ForEach(obj => obj += "\"");
            foreach (var jsonObject in jsonObjects)
                list.Add(jsonObject.Split(":")[0], jsonObject.Split(":")[1]);
            return list.FirstOrDefault();
        }

        //private static Dictionary<string, object> JsonParseByKeyValueList(string jsonContentRaw)
        //{
        //    var list = new Dictionary<string, object>();
        //    var clearWhiteSpaces = jsonContentRaw.ToList().Where(w => !char.IsWhiteSpace(w) && w != '\n' && w != '\r').ToString();
        //    var jsonContent = clearWhiteSpaces.TrimStart('[').TrimEnd(']');
        //}
    }
}
