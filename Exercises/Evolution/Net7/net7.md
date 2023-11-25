# New features in C# 11

## Requirements
* SDK version 7.0.100 or higher
* Visual Studio 17.4 or higher
```
<PropertyGroup>
    <TargetFramework>net7.0</TargetFramework>
    <LangVersion>11.0</LangVersion>
</PropertyGroup>
```

## UTF8 String Literals
* New UTF8 string literal keyword `u8`
* Default c# strings are UTF16

```
var utf8string = "Good morming!"u8;
```

## Raw string literals
* String values can be written that do not need to escape values
such as slash `\`, double-quoutes `"` and brackets ```{ }```.

```
var rawText1 = """ This is a raw string literal with "content"! """;
Console.WriteLine(rawText1);
```

* String must start with at least 3 `"`. Less will not work.

The following doest not compile:
```
var rawText3 = "" This is a raw string literal with "content"! "";
Console.WriteLine(rawText1);
```

* If the text has more `"` values, then more `"` characters must be added to the surrounding quoutes
```
var rawText4 = """"" single " double "" triple """ quadruple """"  """"  """"";
```

* Multi-line raw string literals remove white spaces 
```
var rawMultiLine = """
    <html style="blog">
      <body>
        The 4 white spaces will be truncated.
      </body>
    </html>
    """;
```
## Abstract static members OR generic math support
* New interface member keyword combination`static abstract`
* Interface members can be declared as `static abstract`
* `static abstract` cannot be made virtual

```
public interface INumber<T> where T : INumber<T> {
   static abstract T operator +(T a, T b);
   static abstract T operator /(T a, T b);
   static abstract T One { get; }
   static abstract T Zero { get; }
}

int[] intValues = new int[] { 1, 2, 3, 4, 5 };
double[] doubleValues = new double[] { 1.1, 2.2, 3.3, 4.4, 5.5 };

Console.WriteLine($"Sum of int array:{AddAll(intValues)}");
Console.WriteLine($"Sum of double array:{AddAll(doubleValues)}");

//Same function used for both int and double
T AddAll<T>(T[] values) where T : INumber<T>
{
    T result = T.Zero;  // <- INumber has a static abstract member here
    foreach(T value in values)
    {
        result += result;
    }

    return result;
}
```

## List Patterns
* New pattern matching for lists
* Possibility to match an array or a list with a sequence of patterns
* List patterns are recursive patterns, they can be nested inside of themselves
```
listAlpha is []   //matches empty array
listAlpha is [1, 2, 3] //matches exact values  or "Lowering"
listAlpha is [0 or 1, <= 2, >= 3] //matches or condition and greater or lower than
listAlpha is [var first, _, _]  //matches first and sets as variable  or "Subsumption checking"
listAlpha is [var first, var second] //matches first and second, only two items
listAlpha is [var firstAlpha, .. var restAlpha] //matches first and rest of items or "Slice pattern"

matrix is [[1, 2, 3], [4, 5, 6], [7, 8, 9]] //matches recursive pattern
```

## Generic attributes
* Attributes can now be created in a way so that they can be applied to any type.
```
public interface IValidator{}
public class UserValidator : IValidator{}

public class Validator<TValidator> : Attribute
    where TValidator : IValidator
{
    public Type ValidatorType { get; }
    public Validator()    { ValidatorType = typeof(TValidator); }
}

[Validator<UserValidator>]
public class User{}
```

## Required members
* New keyword `required` for class members
* Makes a property mandatory to be passed in to a class

```
public class Book
{
    public required string Name { get; init; }

    public required string AuthorName { get; set; }
}
```

## Extended nameOf() scope
* Functionality of nameOf() has been extened to parameters and 
```
[Name(nameof(name))]  // <- extended nameof scope
public Animal(string name)
{
}
```

## File scope access modifier
* New scope keyword `file`
* Restricts type to file 

```
file class Book
{
    public required string Name { get; init; }

    public required string AuthorName { get; set; }
}
```

## Method group optimization
* Method group functions are now also cached
* Before only lambda functions were cached

```
//Cached 
var filtered = Numbers.Where(x => Filter(x)).ToList();

//Not cached before C# 11, was slower performance
var filtered2 = Numbers.Where(Filter).ToList();
```

## ref fields and ref scoped variables and scoped arguments
* Updates to 'Low Level Struct Improvements'
* New `scoped` keyword for parameters and variables
* Structs and struct fields can be declared as `ref`

```
//ref scoped parameter
pan<int> CreateEmptySpan(scoped ref int initialValue)
{
    return Span<int>.Empty;
}

//scoped variable
Span<int> CreateSpan()
{
    scoped Span<int> span = Span<int>.Empty;
    return Span<int>.Empty;
}

//scoped parameter
Span<char> CreateFromExisting(scoped Span<char> Values)
{
    return Span<char>.Empty;
}

//ref struct and ref field
public ref struct RefStruct
{
    private ref int refField;
}


//allocated on the stack
Span<char> values = stackalloc char[3] { 'T', 'o', 'm' };
//stack value passed here
CreateFromExisting(values);
```

## Aliases for numeric IntPtr and UIntPtr
* Represents an integer where the bit-width is the same as a pointer
* New alias `nint` and `nuint`


## Documentation

:link: [C# 11 Blog post](https://devblogs.microsoft.com/dotnet/welcome-to-csharp-11/)

:link: [C# 11 Guide](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-11)

:link: [.NET 7 Blog Post](https://devblogs.microsoft.com/dotnet/announcing-dotnet-7/)
