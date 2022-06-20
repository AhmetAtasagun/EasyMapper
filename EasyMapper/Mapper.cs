using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace EasyMapper
{
    public static class Mapper
    {
        public static IEnumerable<TDestination> ToMap<TDestination>(this IEnumerable<object> source)
        {
            var mappedModel = source.Select(s => NewInstanceMap<TDestination>(s)).ToList().AsEnumerable();
            return mappedModel;
        }

        public static TDestination ToMap<TDestination>(this object source)
        {
            var mappedModel = NewInstanceMap<TDestination>(source);
            return mappedModel;
        }

        public static TDestination ToMap<TDestination>(this object source, TDestination destination)
        {
            var mappedModel = ValuesUpdateMap(source, destination);
            return mappedModel;
        }

        public static object ToMap(this object source, Type destionationType)
        {
            var mappedModel = NewInstanceMap(source, destionationType);
            return mappedModel;
        }

        public static IEnumerable<TDestination> ToMap<TDestination>(this IEnumerable<object> source, MapOptions options)
        {
            var mappedModel = source.Select(s => NewInstanceMap<TDestination>(s, options)).ToList().AsEnumerable();
            return mappedModel;
        }

        public static TDestination ToMap<TDestination>(this object source, MapOptions options)
        {
            var mappedModel = NewInstanceMap<TDestination>(source, options);
            return mappedModel;
        }

        public static TDestination ToMap<TDestination>(this object source, TDestination destination, MapOptions options)
        {
            var mappedModel = ValuesUpdateMap(source, destination, options);
            return mappedModel;
        }

        public static object ToMap(this object source, Type destionationType, MapOptions options)
        {
            var mappedModel = NewInstanceMap(source, destionationType, null, options);
            return mappedModel;
        }

        #region Private Process

        private static TEntity GetInstanceBy<TEntity>(string entityQualifiedName)
        {
            var destinationInstance = Activator.CreateInstance<TEntity>();
            if (destinationInstance.GetType() == typeof(object))
                destinationInstance = (TEntity)Activator.CreateInstance(Type.GetType(entityQualifiedName));
            return destinationInstance;
        }

        private static TDestination NewInstanceMap<TDestination>(object source, MapOptions options = null)
        {
            var destinationInstance = GetInstanceBy<TDestination>(typeof(TDestination).AssemblyQualifiedName);
            destinationInstance = (TDestination)MapEntityProperties(source, destinationInstance, null, false, options);
            return destinationInstance;
        }

        private static object NewInstanceMap(object source, Type destionationType, Type secondDestionationType = null, MapOptions options = null, GenerationLevel innerLevel = GenerationLevel.First)
        {
            var destinationInstance = TryCreateInstance(destionationType, secondDestionationType);
            destinationInstance = MapEntityProperties(source, destinationInstance, destionationType, false, options, innerLevel);
            return destinationInstance;
        }

        private static TDestination ValuesUpdateMap<TDestination>(object source, TDestination destinationInstance, MapOptions options = null)
        {
            //var newDestinationInstance = GetInstanceBy<TDestination>(destinationInstance.GetType().AssemblyQualifiedName);
            var newDestinationInstance = NewInstanceMap<TDestination>(destinationInstance, options);
            newDestinationInstance = (TDestination)MapEntityProperties(source, newDestinationInstance, isUpdateMap: true, options: options);
            return newDestinationInstance;
        }

        private static object MapEntityProperties(object source, object destionationEntity, Type destionationType = null, bool isUpdateMap = false,
            MapOptions options = null, GenerationLevel innerLevel = GenerationLevel.First)
        {
            if (options == null) options = new MapOptions().GetDefaultOptions();
            PropertyInfo[] sourceProperties = source.GetType().GetProperties();
            foreach (var sourceProperty in sourceProperties)
            {
                var currentPropertyName = sourceProperty.Name;
                if (options.IgnoreFields.Contains(currentPropertyName)) continue;
                var value = GetPropertyValue(source, currentPropertyName);
                if (value is null)
                    continue;
                var destinationProperty = destionationEntity.GetType().GetProperty(currentPropertyName); // GetDestinationProperty(destionationEntity, currentPropertyName);
                if (destinationProperty == null)
                    continue;
                #region For Update Method
                if (isUpdateMap && IsDefaultValue(value))
                    continue;
                #endregion
                if (!sourceProperty.PropertyType.AssemblyQualifiedName.StartsWith("System")) // Tek Model tip için
                {
                    if (sourceProperty.PropertyType.BaseType.Name == nameof(Enum))
                        if (destinationProperty.PropertyType.Name == sourceProperty.GetType().Name)
                        {
                            destinationProperty.SetValue(destionationEntity, sourceProperty); continue;
                        }
                    if (options.GenerationLevel < innerLevel) continue;
                    var innerDestination = Activator.CreateInstance(destinationProperty.PropertyType);
                    innerDestination = NewInstanceMap(value, destionationType, innerDestination.GetType(), options, ++innerLevel); innerLevel--;
                    if (destinationProperty.PropertyType.Name == innerDestination.GetType().Name)
                        destinationProperty.SetValue(destionationEntity, innerDestination);
                }
                else if (destinationProperty.PropertyType.FullName.Contains("Generic") && destinationProperty.PropertyType.FullName.Contains("List") &&
                    sourceProperty.PropertyType.FullName.Contains("Generic") && sourceProperty.PropertyType.FullName.Contains("List")) // Liste Model tip için
                {
                    if (options.GenerationLevel < innerLevel) continue;
                    var innerDestinationEnumerable = (IList)Activator.CreateInstance(destinationProperty.PropertyType);
                    var innerDestinationItemType = destinationProperty.PropertyType.GenericTypeArguments.FirstOrDefault();
                    var sourceValueEnumerable = (IList)value;
                    foreach (var sourceItem in sourceValueEnumerable)
                    {
                        var destinationItem = Activator.CreateInstance(destinationProperty.PropertyType.GenericTypeArguments.FirstOrDefault());
                        destinationItem = NewInstanceMap(sourceItem, destionationType, innerDestinationItemType, options, ++innerLevel); innerLevel--;
                        innerDestinationEnumerable.Add(destinationItem);
                    }
                    destinationProperty.SetValue(destionationEntity, innerDestinationEnumerable);
                }
                else if (destinationProperty.PropertyType.Name == sourceProperty.PropertyType.Name) // basit veri tipi için
                    destinationProperty.SetValue(destionationEntity, value);
                else if (destinationProperty.PropertyType.FullName.Contains(sourceProperty.PropertyType.Name)) // basit veri tipi için (nullable a setleme)
                    try { destinationProperty.SetValue(destionationEntity, value); }
                    catch (Exception) { }
                // else (daha sonra türden türe dönüşüm işlemlerine bakılacak.)
            }
            return destionationEntity;
        }

        private static object TryCreateInstance(Type destionationType, Type secondDestionationType = null)
        {
            object destinationInstance = null;
            try
            {
                if (destionationType != null)
                    destinationInstance = Activator.CreateInstance(destionationType);
            }
            catch { }
            finally
            {
                if (secondDestionationType != null)
                    try { destinationInstance = Activator.CreateInstance(secondDestionationType); } catch { }
            }
            return destinationInstance;
        }

        #region V2 preinitialized....
        //private static PropertyInfo GetDestinationProperty(object destionationEntity, string currentPropertyName)
        //{
        //    var destinationProperty = destionationEntity.GetType().GetProperty(currentPropertyName);
        //    if (destinationProperty == null)
        //        destinationProperty = destionationEntity.GetType().GetProperty(GetPluralName(currentPropertyName));
        //    if (destinationProperty == null)
        //        destinationProperty = destionationEntity.GetType().GetProperty(GetSingularName(currentPropertyName));
        //    return destinationProperty;
        //}

        //private static string GetPluralName(string propertyName)
        //{
        //    if (propertyName.EndsWith("s"))
        //        return propertyName + "es";
        //    else if (propertyName.EndsWith("y"))
        //        return propertyName.Substring(0, propertyName.Length - 1) + "ies";
        //    else if (propertyName.EndsWith("x"))
        //        return propertyName;
        //    else return propertyName + "s";
        //}

        //private static string GetSingularName(string propertyName)
        //{
        //    if (propertyName.EndsWith("ies"))
        //        return propertyName.Substring(0, propertyName.Length - 3) + "y";
        //    else if (propertyName.EndsWith("x"))
        //        return propertyName;
        //    else return propertyName.TrimEnd('s');
        //} 
        #endregion

        private static object GetPropertyValue(object source, string propName)
        {
            if (source == null) throw new ArgumentException("Value cannot be null.", nameof(source));
            if (propName == null) throw new ArgumentException("Value cannot be null.", nameof(propName));
            var prop = source.GetType().GetProperty(propName);
            return prop != null ? prop.GetValue(source, null) : null;
        }

        private static bool IsDefaultValue(object value)
        {
            if (value.GetType() == typeof(string))
                return value.ToString() == string.Empty;
            try // for : System.MissingMethodException: 'No parameterless constructor defined for type 'Microsoft.AspNetCore.Http.FormFile'.'
            {
                return Activator.CreateInstance(value.GetType(), true).Equals(value);
            }
            catch (MissingMethodException) { return true; }
        }

        #endregion
    }
}
