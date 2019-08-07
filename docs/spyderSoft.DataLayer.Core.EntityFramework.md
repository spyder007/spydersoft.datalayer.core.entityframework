## `TypeExtensions`

Class TypeExtensions.  This class is a helper class to assist in reflection against generic methods, which  the .NET Type.GetMethod does not handle well.
```csharp
public static class spyderSoft.DataLayer.Core.EntityFramework.TypeExtensions

```

Static Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `MethodInfo` | GetGenericMethod(this `Type` genericType, `String` name, `Type[]` parameterTypes) | This extension method gets a particular generic method based on the calling type  and the name and parameter types of the desired variant of the method. | 


