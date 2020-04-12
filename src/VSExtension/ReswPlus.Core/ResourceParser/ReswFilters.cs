// Copyright (c) Rudy Huyn. All rights reserved.
// Licensed under the MIT License.
// Source: https://github.com/DotNetPlus/ReswPlus

using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace ReswPlus.Core.ResourceParser
{
    public class VariantedReswItems
    {
        public List<ReswItem> Items { get; set; }
        public string Key { get; set; }
        public bool SupportPlural { get; set; }
        public bool SupportVariants { get; set; }
    }

    public static class ReswFilters
    {
        private static Regex regexPluralVariantItems = new Regex("(?:_(?<variant>Variant\\-?\\d+))?(?:_(?<plural>Zero|One|Other|Many|Few|None))?$");

        public static IEnumerable<VariantedReswItems> GetItemsWithVariantOrPlural(this IEnumerable<ReswItem> reswItems)
        {

            var variantedItems = from item in reswItems
                               where item.Key.Contains("_")
                               let match = regexPluralVariantItems.Match(item.Key)
                               where match.Success && !string.IsNullOrEmpty(match.Value)
                               let commonKey = item.Key.Substring(0, item.Key.Length - match.Length)
                               group (item: item, isVariant: match.Groups["variant"].Length > 0, isPlural: match.Groups["plural"].Length > 0)
                               by commonKey into gr
                               select gr;
            foreach (var variantedItem in variantedItems)
            {
                // all items must have the same type (pluralization, variants or both), if one of them is different, we reject all of them
                var checkPlurals = variantedItem.Select(i => i.isPlural).Distinct();
                var itemsAllPlurals = checkPlurals.Count() == 1 && checkPlurals.First();
                var checkVariants = variantedItem.Select(i => i.isVariant).Distinct();
                var itemsAllVariants = checkVariants.Count() == 1 && checkVariants.First();

                if (!itemsAllPlurals && !itemsAllVariants)
                {
                    // Ignore if items don't have the same form
                    continue;
                }

                yield return new VariantedReswItems()
                {
                    Key = variantedItem.Key,
                    Items = variantedItem.Select(i => i.item).ToList(),
                    SupportPlural = variantedItem.First().isPlural,
                    SupportVariants = variantedItem.First().isVariant,
                };
            }
        }
    }
}
