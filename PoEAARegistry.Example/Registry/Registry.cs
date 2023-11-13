using PoEAARegistry.Example.Finders;

namespace PoEAARegistry.Example.Registry;

public class Registry
{
  private static Registry Instance = new();
  public IPersonFinder PersonFinder { get; set; }

  private Registry() => PersonFinder = new AlwaysFindingPersonFinder();

  public static void Initialize() => Instance = new();

  public static Registry GetInstance() => Instance;
}
