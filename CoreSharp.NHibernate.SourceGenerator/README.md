CoreSharp NHibernate Source Generator
=====================================

Extension methods generator for CoreSharp NHibernate entities.\
This package generates Add/Remove/Clear methods for each collection property.

Requirements
------------

.NET 5

Installation
------------

Add `CoreSharp.NHibernate.SourceGenerator` reference to csproj:

```json
<ItemGroup>
    ...
    <PackageReference Include="CoreSharp.NHibernate.SourceGenerator" Version="0.1.1" />
    ...
</ItemGroup>
```

Add `analyzers.config` to root of project:

```xml
<?xml version="1.0" encoding="utf-8" ?>
<Analyzers>
  <ValidTypes>
    <ValidType>CoreSharp.DataAccess.IEntity</ValidType>
    <ValidType>CoreSharp.DataAccess.ICodeList</ValidType>
  </ValidTypes>

  <VirtualModifierAnalyzer>

  </VirtualModifierAnalyzer>
  <PropertyOrderAnalyzer>

  </PropertyOrderAnalyzer>
</Analyzers>
```
