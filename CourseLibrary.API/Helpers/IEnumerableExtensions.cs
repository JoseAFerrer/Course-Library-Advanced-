using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace CourseLibrary.API.Helpers
{
    public static class IEnumerableExtensions
    {
        public static IEnumerable<ExpandoObject> ShapeData<TSource>(
            this IEnumerable<TSource> source,
            string fields)
        {
            if (source ==null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            //Create a list to hold our ExpandoObjects
            var expandoObjectList = new List<ExpandoObject>();

            //Create a list with PropertyInfo objects on TSource.
            //Reflection is expensive, so rather than doing it for each object in the list, we do it once and reuse the results
            // After all, part of the reflection is on the type of object, not on the instance
            var propertyInfoList = new List<PropertyInfo>();

            if (string.IsNullOrWhiteSpace(fields))
            {
                // All public properties should be in the ExpandObject
                var propertyInfos = typeof(TSource)
                    .GetProperties(BindingFlags.Public | BindingFlags.Instance);

                propertyInfoList.AddRange(propertyInfos);
            }
            else
            {
                // Split the fields
                var fieldsAfterSplit = fields.Split(',');

                foreach (var field in fieldsAfterSplit)
                {
                    //Trim each field, use a different var
                    var propertyName = field.Trim();

                    //Use reflection to get the property on the source object. We need to include public and instance, b/c specifying a binding flag overwrites the already existing binding flags
                    var propertyInfo = typeof(TSource)
                        .GetProperty(propertyName,
                        BindingFlags.IgnoreCase |
                        BindingFlags.Public |
                        BindingFlags.Instance);

                    if (propertyInfo == null)
                    {
                        throw new Exception($"Property {propertyName} wasn't found on" +
                            $" {typeof(TSource)}");
                    }
                    //Add propertyInfo to list
                    propertyInfoList.Add(propertyInfo);
                }
            }
            //Run through the source objects
            foreach (TSource sourceObject in source)
            {
                //Create ExpandoObject that will hold the selected properties and values
                var dataShapedObject = new ExpandoObject();

                //Get the value of each property. We run through the list
                foreach (var propertyInfo in propertyInfoList)
                {
                    //Get value returns the value of the property on the source object
                    var propertyValue = propertyInfo.GetValue(sourceObject);

                    //Add the field to the ExpandoObject
                    ((IDictionary<string, object>)dataShapedObject)
                        .Add(propertyInfo.Name, propertyValue);
                }

                //Add the ExpandoObject to the list
                expandoObjectList.Add(dataShapedObject);
            }

            //Return the list
            return expandoObjectList;
        }
    }
}
