# New features in C# 12

## Requirements
* .Net SDK 8.0.100 or higher
* Visual Studio 17.8.0 or higher
* The following csproj properties:
```
<PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <LangVersion>12.0</LangVersion>
  </PropertyGroup>
```


## Primary Constructors
* Primary constructors for classes
```
public class Person(string name) // here hidden constructor
{
    public void PrintName()
    {
        Console.WriteLine(name);
    }
}
```

## Collection Expressions
* Additional functionalities for collection initializations

```
ImmutableList<decimal> extraGrades = [5, 3, 4, 1];

// .... extraGrades -> SPREADS collection merging
student.SetGrades([3, 4, 2, 5, .. extraGrades]);

int[] a = [1, 2, 3, 4, 5, 8];

Span<int> b = ['a', 'b', 'c'];

int[][] twoD = [[1, 2], [3, 4], [5, 6]];

int[] row0 = [1, 2, 3];
int[] row1 = [4, 5, 6];
int[] row2 = [7, 8, 9];
```

## Alias any type
* Aliases can now be created for value types and tuples also
```
//Alias for any types and tuples
using Grade = decimal;
using GradeTuple = (string, decimal);

//Alias for pointer types only in Unsafe Mode
//using Grade = decimal*;

```

## Ref readonly
* Can only be used on parameters
* Used to pass a reference to a value type but the value itself cannot be modified
* Used to optimize performance by avoiding copying of large value types
* Can be used with both value types and reference types

```

public void Print(ref readonly int grade)
    {
        Console.WriteLine(grade);
    }

var grade = 5;
student.Print(in grade);
```

## Default lambda parameters
* Lambda expression parameters can now have a default value

```
public Action<string> ReplyWithName => (string personName = "Person") =>
    {
        Console.WriteLine($"Hello {personName}.");
        Console.WriteLine($"Nice to meet you.");
        Console.WriteLine($"My name is {Name}");
    };
```

## Experimental flag
* Can mark features as experimental

```
using System.Diagnostics.CodeAnalysis;

[Experimental("PRINTLASTGRADE01", UrlFormat = "some url for more information")]
    public void PrintLastGrade()
    {
        Console.WriteLine(grades.Last());
    }
```

* When used must be suppressed
```
#pragma warning disable PRINTLASTGRADE01 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
student.PrintLastGrade();
#pragma warning restore PRINTLASTGRADE01 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
```

## Interceptors (preview feature as of 14-Nov-2023 subject to be removed)

## Documentation

:link: [C# 12 Blog post](https://devblogs.microsoft.com/dotnet/announcing-csharp-12/)

:link: [.NET 8 Blog Post](https://devblogs.microsoft.com/dotnet/announcing-dotnet-8/)
