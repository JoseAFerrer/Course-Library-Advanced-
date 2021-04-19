using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace CourseLibrary.API.Services
{
    public class PropertyCheckerService : IPropertyCheckerService
    {
        public bool TypeHasProperties<T>(string fields)
        {
            if (string.IsNullOrWhiteSpace(fields))
            { return true; }

            //The fields are separated by ",", so we split them
            var fieldsAfterSplit = fields.Split(',');

            //Check if the requested fields exist on source
            foreach (var field in fieldsAfterSplit)
            {
                //Trim each field, as it might contain spaces. Use another var:
                var propertyName = field.Trim();

                //Use reflection to check if the property can be found on T.
                var propertyInfo = typeof(T)
                    .GetProperty(propertyName,
                    BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

                //It can't be found, return false
                if (propertyInfo == null)
                {
                    return false;
                }
            }
            //All checks out, return true
            return true;
        }

    }
}
