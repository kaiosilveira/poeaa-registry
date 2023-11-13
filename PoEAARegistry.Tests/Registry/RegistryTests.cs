using PoEAARegistry.Example.Registry;

namespace PoEAARegistry.Tests;

public class RegistryTests
{
  [Fact]
  public void TestReturnsTheSameInstance()
  {
    var registry1 = Registry.GetInstance();
    var registry2 = Registry.GetInstance();

    Assert.Equal(registry1, registry2);
  }

  [Fact]
  public void TestReturnsTheSamePersonFinder()
  {
    var personFinder1 = Registry.GetInstance().PersonFinder;
    var personFinder2 = Registry.GetInstance().PersonFinder;

    Assert.Equal(personFinder1, personFinder2);
  }
}
