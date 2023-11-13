using PoEAARegistry.Example.Models;
namespace PoEAARegistry.Example.Finders;

public class NeverFindingPersonFinder : IPersonFinder
{
  public Person? FindByLastName(string lastName)
  {
    return null;
  }
}
