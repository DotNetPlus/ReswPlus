using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace ReswPlus.Core.ResourceParser;

public sealed class VariantedReswItems
{
    public List<ReswItem> Items { get; }
    public string Key { get; }
    public bool SupportPlural { get; }
    public bool SupportVariants { get; }

    public VariantedReswItems(List<ReswItem> items, string key, bool supportPlural, bool supportVariants)
    {
        Items = items;
        Key = key;
        SupportPlural = supportPlural;
        SupportVariants = supportVariants;
    }
}

public static class ReswFilters
{
    private static readonly Regex regexPluralVariantItems = new("(?:_(?<variant>Variant\\-?\\d+))?(?:_(?<plural>Zero|One|Other|Many|Few|None))?$");

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

            yield return new VariantedReswItems(
                variantedItem.Select(i => i.item).ToList(),
                variantedItem.Key,
                variantedItem.First().isPlural,
                variantedItem.First().isVariant
            );
        }
    }
}
