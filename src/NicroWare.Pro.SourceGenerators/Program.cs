// See https://aka.ms/new-console-template for more information

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;

namespace NicroWare.Pro.SourceGenerators;

public static partial class Program
{
    public static void Main()
    {
        Console.WriteLine("Hello, World!");

        var person = new Person("a", "b");

        person.PropertyChanged += Person_PropertyChanged;

        person.FirstName = "b";
    }

    private static void Person_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        Console.WriteLine(e.PropertyName);
    }
}

public partial class Person : INotifyPropertyChanged
{

    public Person(string firstName, string lastName)
    {
        this.firstName = firstName;
        this.lastName = lastName;
    }

    private string firstName;
    private string lastName;
    private int age;
    private string? middleName;
}