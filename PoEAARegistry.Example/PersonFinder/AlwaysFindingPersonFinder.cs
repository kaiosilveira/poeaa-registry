using PoEAARegistry.Example.Models;
namespace PoEAARegistry.Example.Finders;

public class AlwaysFindingPersonFinder : IPersonFinder
{
  public Person? FindByLastName(string lastName)
  {
    return new Person(firstName: "John", lastName);
  }
}
