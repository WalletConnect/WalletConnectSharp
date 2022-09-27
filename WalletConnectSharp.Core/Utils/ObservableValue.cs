namespace WalletConnectSharp.Core.Utils;

public class ObservableValue<T> : DisposableBase, IObservable<T>
{
    public event EventHandler<T> OnValueChanged; 

    private T _value;
    public T Value
    {
        get
        {
            return _value;
        }
        set
        {
            _value = value;

            if (OnValueChanged != null)
                OnValueChanged(this, value);
        }
    }

    public IDisposable Subscribe(IObserver<T> observer)
    {
        OnValueChanged += (sender, e) =>
        {
            observer.OnNext(e);
        };

        return this;
    }

    protected override void DisposeManaged()
    {
        OnValueChanged = null;
        _value = default;
    }
}
