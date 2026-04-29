namespace Euterpe.Abstractions;

public interface IAsyncInitializable
{
    Task InitializeAsync();
}