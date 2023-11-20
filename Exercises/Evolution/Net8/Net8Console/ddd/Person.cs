namespace Net8Console.ddd;

/// <summary>
/// Primary constructor for class.
/// </summary>
public class Person(string name)
{
    public void PrintName()
    {
        Console.WriteLine(name);
    }
}


record User(string Name);