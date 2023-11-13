[![Continuous Integration](https://github.com/kaiosilveira/poeaa-registry/actions/workflows/dotnet.yml/badge.svg)](https://github.com/kaiosilveira/poeaa-registry/actions/workflows/dotnet.yml)

ℹ️ _This repository is part of my "Patterns of Enterprise Application Architecture" (PoEAA) catalog, based on Martin Fowler's book with the same title. For my full work on the topic, see [kaiosilveira/poeaa](https://github.com/kaiosilveira/patterns-of-enterprise-application-architecture)_

---

# Registry

A well-known object that other objects can use to find common objects and services.

## Implementation example

Sticking to the book's example, our implementation contains a `Registry` responsible for holding a `PersonFinder` which, in turn, queries the database for a person based on some criteria. As database connections are commonly thread-scoped, a `ThreadLocalRegistry` was also implemented to deal with multi-thread concerns.

## Implementation considerations

To implement a Registry, we often need a global reference to a single, well-known object in the system. Therefore, a [Singleton](https://github.com/kaiosilveira/design-patterns/tree/main/singleton) is often a good choice.

As for the multi-threaded code, we need to make sure that we have a construct that stores references only inside the current thread, so we can have parallel executions of the same code with isolated values. For that, we can resort to C#'s `ThreadLocal` construct.

## Test suite

In a good [TDD](https://github.com/kaiosilveira/test-driven-development) fashion, unit tests were used to guide this implementation. Testing the standard `Registry` implementation is straightforward — we just need to make sure that the instance values are the same:

```csharp
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
    var personFinder1 = Registry.GetPersonFinder();
    var personFinder2 = Registry.GetPersonFinder();

    Assert.Equal(personFinder1, personFinder2);
  }
}
```

The same can't be said about testing the multi-threaded solution, though. We need to find a way to prove that something is particular to a thread, so here's where things start to get creative. We can add a `tag` to our `ThreadLocalRegistry` instances, using the value from `Environment.CurrentManagedThreadId`, so we can later read this value and compare it to the external identifier of our current execution thread. Our test looks like the code below:

```csharp
[Fact]
public void HasDifferentValuesForDifferentThreads()
{
    int firstThreadId = 0;
    int secondThreadId = 0;
    var mainProcessId = Environment.CurrentManagedThreadId;

    Thread thread1 = new(() =>
    {
        firstThreadId = Environment.CurrentManagedThreadId;

        ThreadLocalRegistry.Initialize();
        var instance = ThreadLocalRegistry.GetInstance();

        Assert.Equal(firstThreadId, Convert.ToInt32(instance.Tag));
    });

    Thread thread2 = new(() =>
    {
        secondThreadId = Environment.CurrentManagedThreadId;

        ThreadLocalRegistry.Initialize();
        var instance = ThreadLocalRegistry.GetInstance();

        Assert.Equal(secondThreadId, Convert.ToInt32(instance.Tag));
    });

    thread1.Start();
    thread2.Start();

    thread1.Join();
    thread2.Join();

    Assert.NotEqual(firstThreadId, secondThreadId);
}
```

We can also inexpensively cover the other, single-threaded case:

```csharp
[Fact]
public void ReturnsTheSameInstanceInTheSameThread()
{
    var registry1 = ThreadLocalRegistry.GetInstance();
    var registry2 = ThreadLocalRegistry.GetInstance();

    Assert.Equal(registry1, registry2);
}
```

The full test suite for both registries are available at [RegistryTests.cs](./PoEAARegistry.Tests/Registry/RegistryTests.cs) and [ThreadLocalRegistryTests.cs](./PoEAARegistry.Tests/Registry/ThreadLocalRegistryTests.cs).

## Implementation details

Let's now dive a little deeper into the implementation that satisfies the unit tests above.

### Singleton considerations

Our approach here is to eagerly initialize the `Instance` on the class definition:

```csharp
public class Registry
{
  private static Registry Instance = new();
}
```

Then, we can provide a way to access the instance:

```csharp
public class Registry
{
  private static Registry soleInstance = new();

  public static Registry GetInstance() => Instance;
}
```

And to provide a way of reinitializing it:

```csharp
public class Registry
{
  private static Registry soleInstance = new();

  public static Registry GetInstance() => Instance;

  public static void Initialize() => Instance = new();
}
```

### PersonFinder property

With the singleton considerations in place, we can move on to storing our `PersonFinder` object, specifying ways of getting and setting it:

```csharp
public class Registry
{
  public IPersonFinder PersonFinder { get; set; }

  // more code
}
```

And initializing it directly in the constructor:

```csharp
public class Registry
{
  public IPersonFinder PersonFinder { get; set; }

  private Registry() => PersonFinder = new AlwaysFindingPersonFinder();
  // more code
}
```

For simplicity, we're using an `AlwaysFindingPersonFinder` instance.

### Multi-thread ready Registry

For our multi-thread-ready Registry, we're going to implement a `ThreadLocalRegistry`. The steps are pretty much the same as the above, except that the instance of the singleton is stored inside a `ThreadLocal` construct, the tagging mentioned above is applied, and we have some validation when returning the thread's stored value:

```csharp
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
```

And that's it!
