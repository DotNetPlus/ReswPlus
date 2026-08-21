# Plural rules

`CldrPluralRules.g.cs` holds the plural rules of Unicode CLDR as C#: which languages share a set of rules, the
categories a translator has to supply for them, and the source of the class deciding a quantity by them. The
generator does no more than look a language up in it.

**It is generated. Do not edit it by hand.** It is written by `tools/CldrRuleImporter`, which reads the rules
CLDR publishes and works the rest out from them:

```powershell
dotnet run --project tools\CldrRuleImporter
```

See [`tools/CldrRuleImporter/README.md`](../../../tools/CldrRuleImporter/README.md) for what is derived, how to
refresh CLDR, and what to do when a test fails afterwards.
