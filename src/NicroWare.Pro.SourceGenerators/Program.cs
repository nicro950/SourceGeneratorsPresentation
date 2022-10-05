// See https://aka.ms/new-console-template for more information

using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace NicroWare.Pro.SourceGenerators;

public partial class Program
{
    public static void Main()
    {
        HelloFrom("Somewhere");

        var person = new Person("First", "Last");

        person.PropertyChanged += Person_PropertyChanged  ;

        person.FirstName = "bob";
		person.Age = 25;

	}

    private static void Person_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        

		Console.WriteLine($"Someone changed {e.PropertyName} new value is '{(sender as Person).GetValueForProperty(e.PropertyName)}'!");
    }

    static partial void HelloFrom(string name);
}

public partial class Person : INotifyPropertyChanged
{
    public Person(string firstName, string lastName)
    {
        this.firstName = firstName;
        this.lastName = lastName;
    }

    private string firstName;
    private string? middleName;
    private string lastName;

    private int age;
}