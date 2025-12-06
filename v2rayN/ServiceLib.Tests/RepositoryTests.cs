using ServiceLib.Repositories;

namespace ServiceLib.Tests;

public class RepositoryTests
{
    [Fact]
    public void ProfileRepository_Instance_ShouldBeSingleton()
    {
        var instance1 = ProfileRepository.Instance;
        var instance2 = ProfileRepository.Instance;

        Assert.Same(instance1, instance2);
    }

    [Fact]
    public void SubItemRepository_Instance_ShouldBeSingleton()
    {
        var instance1 = SubItemRepository.Instance;
        var instance2 = SubItemRepository.Instance;

        Assert.Same(instance1, instance2);
    }

    [Fact]
    public void RoutingRepository_Instance_ShouldBeSingleton()
    {
        var instance1 = RoutingRepository.Instance;
        var instance2 = RoutingRepository.Instance;

        Assert.Same(instance1, instance2);
    }

    [Fact]
    public void DNSRepository_Instance_ShouldBeSingleton()
    {
        var instance1 = DNSRepository.Instance;
        var instance2 = DNSRepository.Instance;

        Assert.Same(instance1, instance2);
    }
}
