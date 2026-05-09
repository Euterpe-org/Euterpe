namespace Euterpe.Abstractions;

public interface IDialog<T>
{
    event EventHandler<T>? RequestClose;
    void Close(T result);
}