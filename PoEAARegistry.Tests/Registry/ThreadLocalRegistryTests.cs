using PoEAARegistry.Example.Registry;

namespace PoEAARegistry.Tests;

public class ThreadLocalRegistryTests
{
    [Fact]
    public void ReturnsTheSameInstanceInTheSameThread()
    {
        var registry1 = ThreadLocalRegistry.GetInstance();
        var registry2 = ThreadLocalRegistry.GetInstance();

        Assert.Equal(registry1, registry2);
    }

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
}
