
//UTF8 string literals -> becomes ReadOnlySpan<byte>
using System.Numerics;
using System.Text;

ReadOnlySpan<byte> utf8string = "Good morming!"u8;
ReadOnlySpan<byte> utf16String = Encoding.Unicode.GetBytes("Good morming!");

//Console.WriteLine(utf8string); <-- directly doesn't work
Console.WriteLine(utf8string.ToString());

//Raw string literals
var rawText1 = """ This is a raw string literal with "content"! """;
Console.WriteLine(rawText1);

//var rawText2 = "" This is a raw string literal with "content"! ""; <- with 2 " values it will not compile

//Raw string literals with 3 "
var rawText3 = """"  single " double "" triple """  """";
Console.WriteLine(rawText3);

//Raw string literals with 4 "
var rawText4 = """"" single " double "" triple """ quadruple """"  """"  """"";
Console.WriteLine(rawText4);

//Multi-line raw string literals
var rawMultiLine = """
    <html style="blog">
      <body>
        The 4 white spaces will be truncated.
      </body>
    </html>
    """;
Console.WriteLine(rawMultiLine);


//List patterns
int[] listAlpha = { 1, 2, 3 };

Console.WriteLine($"Does the list contain 1,2,3: {listAlpha is [1, 2, 3]}");
Console.WriteLine($"Does the list contain 1,2,4: {listAlpha is [1, 2, 4]}");
Console.WriteLine($"Does the list contain 1,2,3,4: {listAlpha is [1, 2, 3, 4]}");

Console.WriteLine($"Does the list contain 1,2,3,4: {listAlpha is [0 or 1, <= 2, >= 3]}");

if (listAlpha is [var first, _, _])
{
    Console.WriteLine($"List contains 3 values and the first value is: {first}");
}

if (listAlpha is [var firstAlpha, .. var restAlpha])
{
    Console.WriteLine($"List contains 3 values and the first value is: {firstAlpha}");
    Console.WriteLine($"The rest of teh values are: {string.Join(", ", restAlpha)}");
}

var emptyArray = Array.Empty<int>();
var oneItemArray = new[] { 20 };
var twoItemArray = new[] { 60, 90 };
var moreValuesArray = new[] { 60, 90, 120, 124, 656 };

var itemChecker = (int[] array) =>
{
    return array switch
    {
        [] => "Array is empty",
        [var oneValue] => $"Array has one value {oneValue}",
        [var first, var second] => $"Array has two values, first value {first} and the second is {second}.",
        [var first, .. var rest] => $"Array has two values, first value {first} and the rest are {string.Join(",", rest)}.",
    };
};

Console.WriteLine(itemChecker(emptyArray));
Console.WriteLine(itemChecker(oneItemArray));
Console.WriteLine(itemChecker(twoItemArray));
Console.WriteLine(itemChecker(moreValuesArray));

var row1 = new[] { 1, 2, 3 };
var row2 = new[] { 4, 5, 6 };
var row3 = new[] { 7, 8, 9 };

var matrix = new[] { row1, row2, row3 };

if(matrix is [[1, 2, 3], [4, 5, 6], [7, 8, 9]])
{
    foreach (var matrixRow in matrix)
    {
        Console.WriteLine(string.Join(",", matrixRow));
    }
}

//Abstract static members or generic math support
int[] intValues = new int[] { 1, 2, 3, 4, 5 };
double[] doubleValues = new double[] { 1.1, 2.2, 3.3, 4.4, 5.5 };

Console.WriteLine($"Sum of int array:{AddAll(intValues)}");
Console.WriteLine($"Sum of double array:{AddAll(doubleValues)}");


//Aliases for Numeric IntPtr and UIntPtr
nint intPtr = IntPtr.Zero;
nuint uIntPtr = UIntPtr.Zero;

// ref fields and ref scoped variables and scoped arguments
//allocated on the stack
Span<char> values = stackalloc char[3] { 'T', 'o', 'm' };
//stack value passed here
CreateFromExisting(values);

Console.ReadKey();

//Abstract static members or generic math support
T AddAll<T>(T[] values) where T : INumber<T>
{
    T result = T.Zero;  // <- INumber has a static abstract member here
    foreach(T value in values)
    {
        result += result;
    }

    return result;
}

// ref fields and ref scoped variables and scoped arguments
Span<int> CreateEmptySpan(scoped ref int initialValue)
{
    return Span<int>.Empty;
}

Span<int> CreateSpan()
{
    scoped Span<int> span = Span<int>.Empty;
    return Span<int>.Empty;
}

Span<char> CreateFromExisting(scoped Span<char> Values)
{
    return Span<char>.Empty;
}

public ref struct RefStruct
{
    private ref int refField;
}


    //Generic attributes
    public interface IValidator
{

}

public class UserValidator : IValidator
{

}

public class Validator<TValidator> : Attribute
    where TValidator : IValidator
{
    public Type ValidatorType { get; }

    public Validator()
    {
        ValidatorType = typeof(TValidator);
    }
}

[Validator<UserValidator>]
public class User
{

}


//Extended nameof scope
public class Name: Attribute
{
    public string NameValue { get; }

    public Name(string name)
    {
        NameValue = name;
    }
}

public class Animal
{
    [Name(nameof(name))]  // <- extended nameof scope
    public Animal(string name)
    {
    }
}


//Required members and file scoped class
file class Book
{
    public required string Name { get; init; }

    public required string AuthorName { get; set; }
}

