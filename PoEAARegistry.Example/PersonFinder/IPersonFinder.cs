using PoEAARegistry.Example.Models;
namespace PoEAARegistry.Example.Finders;

public interface IPersonFinder
{
  public Person? FindByLastName(string lastName);
}
