# Plural rules

`CldrPluralRules.cs` holds the plural rules of Unicode CLDR **as objects**: which languages share a set of
rules, the categories a translator has to supply for them, and the conditions themselves — `CldrAnyOf`,
`CldrAllOf`, `CldrRelation`. It is data, not code.

`CldrEmitter` turns those conditions into the C# of a provider while a project is generated. Keeping the two
apart means a CLDR release arrives as the relations that moved, and how a relation is written as C# stays one
decision in one place rather than being baked into a table.

**`CldrPluralRules.cs` is generated. Do not edit it by hand.** It is written by `tools/CldrRuleImporter`,
which reads the rules CLDR publishes and works the grouping out from them:

```powershell
dotnet run --project tools\CldrRuleImporter
```

See [`tools/CldrRuleImporter/README.md`](../../../tools/CldrRuleImporter/README.md) for what is derived, how to
refresh CLDR, and what to do when a test fails afterwards.
