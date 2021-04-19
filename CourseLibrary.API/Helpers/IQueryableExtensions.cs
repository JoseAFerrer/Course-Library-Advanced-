using CourseLibrary.API.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;

namespace CourseLibrary.API.Helpers
{
    public static class IQueryableExtensions
    {
        public static IQueryable<T> ApplySort<T>(
            this IQueryable<T> source,
            string orderBy,
            Dictionary<string, PropertyMappingValue> mappingDictionary)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            if (mappingDictionary == null)
            {
                throw new ArgumentNullException(nameof(mappingDictionary));
            }

            if (string.IsNullOrWhiteSpace(orderBy))
            {
                return source;
            }

            // the orderBy string is separated by ",", so we split it.
            var orderByAfterSplit = orderBy.Split(',');


            // This is the OrderBy string to be applied to the IQueryable. It is built at the end of the inner foreach loop below.
            var orderByString = "";

            // apply each orderby clause
            foreach (var orderByClause in orderByAfterSplit)
            {
                //Trim the orderby clause, as it might contain spaces. Can't trim the var in foreach, so use another var
                var trimmedOrderByClause = orderByClause.Trim();

                //If the sort option ends with "desc", we order in descending
                var orderDescending = trimmedOrderByClause.EndsWith("desc");

                //Remove "asc" or "desc" from the orderByClause, so we get the property name to look for in the mapping dictionary
                var indexOfFirstSpace = trimmedOrderByClause.IndexOf(" ");
                var propertyName = indexOfFirstSpace == -1 ?
                    trimmedOrderByClause : trimmedOrderByClause.Remove(indexOfFirstSpace); //Removes everything after the specified position

                //Find the matching property
                if (!mappingDictionary.ContainsKey(propertyName))
                {
                    throw new ArgumentException($"Key mapping for {propertyName} is missing");
                }

                //Get the PropertyMappingValue
                var propertyMappingValue = mappingDictionary[propertyName];

                if (propertyMappingValue ==null)
                {
                    throw new ArgumentNullException("propertyMappingValue");
                }

                //Run through the property names so the orderby clauses are applied in the correct order
                foreach (var destinationProperty in 
                    propertyMappingValue.DestinationProperties)
                {
                    //Revert sort order if necessary
                    if (propertyMappingValue.Revert)
                    {
                        orderDescending = !orderDescending;
                    }

                    orderByString = orderByString +
                        (string.IsNullOrWhiteSpace(orderByString) ? string.Empty : ",")
                        + destinationProperty
                        + " " //Extremely important to add the whitespace before the words
                        + (orderDescending ? "descending" : "ascending"); 
                }

            }

            return source.OrderBy(orderByString);
        }
    }
}
