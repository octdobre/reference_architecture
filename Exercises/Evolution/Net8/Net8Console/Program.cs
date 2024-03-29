﻿using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

//Alias for any types and tuples
using Grade = decimal;
using GradeTuple = (string, decimal);

//Alias for pointer types only in Unsafe Mode
//using Grade = decimal*;

//Primary constructor for classes
var student = new Student(1, "John", 22);

//Console.WriteLine(student.name); <- only parameter

Console.WriteLine(student.Id);
Console.WriteLine(student.Name);
Console.WriteLine(student.Age);

//ref readonly
var grade = 5;
student.Print(in grade);

//Collection expressions
ImmutableList<decimal> extraGrades = [5, 3, 4, 1];

// .... extraGrades -> SPREADS collection merging
student.SetGrades([3, 4, 2, 5, .. extraGrades]);

int[] a = [1, 2, 3, 4, 5, 8];

Span<int> b = ['a', 'b', 'c'];

int[][] twoD = [[1, 2], [3, 4], [5, 6]];

int[] row0 = [1, 2, 3];
int[] row1 = [4, 5, 6];
int[] row2 = [7, 8, 9];

//Spreads collection
int[] twoDFromVariables = [.. row0, .. row1, .. row2];

//experimental feature
#pragma warning disable PRINTLASTGRADE01 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
student.PrintLastGrade();
#pragma warning restore PRINTLASTGRADE01 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

Console.ReadKey();

//Primary constructor for classes
public class Person(string name)
{
    public void DisplayName()
    {
        Console.WriteLine(name);
    }
}

public class Student(int id, string name, int age) : Person(name)
{
    public string Name { get; set; } = name; //auto property initialization

    private readonly int id = id; //field stashes value of input

    public int Id => id; //from class member;

    public int Age => age = 42; //compiler stashes the id as a lambda capture

    private readonly string name = name; //get out of the auto lambda capture situation

    public void DisplayAge()
    {
        Console.WriteLine(Age);
    }

    public void DisplayNameAndAge()
    {
        //if name is not a field in the class then it becomes a captured property
        //comment the name property to check
        Console.WriteLine(name + Age);
    }

    //Collection expressions
    private ImmutableArray<decimal> grades;

    public void SetGrades(ImmutableArray<decimal> grades)
    {
        this.grades = grades;
    }

    public decimal GPA => grades switch
    {
        [] => 4.0m, //no grades
        [var grade] => grade, //one grade
        [.. var all] => all.Average() //multiple grades
    };

    // default lambda params
    public Action<string> ReplyWithName => (string personName = "Person") =>
    {
        Console.WriteLine($"Hello {personName}.");
        Console.WriteLine($"Nice to meet you.");
        Console.WriteLine($"My name is {Name}");
    };

    // ref readonly (value types and reference types)
    public void Print(ref readonly int grade)
    {
        Console.WriteLine(grade);
    }

    // experimental attribute
    [Experimental("PRINTLASTGRADE01", UrlFormat = "some url for more information")]
    public void PrintLastGrade()
    {
        Console.WriteLine($"The last grade is :{grades.Last()}");
    }
}