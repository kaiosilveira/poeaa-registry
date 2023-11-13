using PoEAARegistry.Example.Finders;

namespace PoEAARegistry.Example.Registry;

public class ThreadLocalRegistry
{
  private static readonly ThreadLocal<ThreadLocalRegistry> threadLocalInstance = new(
      () => new ThreadLocalRegistry(tag: Environment.CurrentManagedThreadId.ToString())
  );

  public readonly string Tag;
  public IPersonFinder PersonFinder { get; set; }

  public ThreadLocalRegistry(string tag)
  {
    Tag = tag;
    PersonFinder = new AlwaysFindingPersonFinder();
  }

  public static void Initialize()
  {
    var tag = Environment.CurrentManagedThreadId.ToString();
    threadLocalInstance.Value = new ThreadLocalRegistry(tag);
  }

  public static ThreadLocalRegistry GetInstance()
  {
    return threadLocalInstance.Value ?? throw new NullReferenceException();
  }
}
