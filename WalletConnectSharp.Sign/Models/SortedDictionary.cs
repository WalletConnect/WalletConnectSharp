using WalletConnectSharp.Common.Utils;

namespace WalletConnectSharp.Sign.Models;

public abstract class SortedDictionary<TKey, TValue> : Dictionary<TKey, TValue>, IEquatable<SortedDictionary<TKey, TValue>>
{
    public abstract IEqualityComparer<TValue> Comparer { get; }

    protected SortedDictionary() : base()
    {
    }

    protected SortedDictionary(IDictionary<TKey, TValue> dictionary) : base(dictionary)
    {
    }

    private List<TKey> _orderedKeys = new();

    public List<TKey> OrderedKeys => _orderedKeys;

    public TValue this[TKey key]
    {
        get
        {
            return base[key];
        }
        set
        {
            if (base[key] == null && value != null)
            {
                _orderedKeys.Add(key);
            }
            else if (base[key] != null && value == null)
            {
                _orderedKeys.Remove(key);
            }
        }
    }
    
    public new void Add(TKey key, TValue value)
    {
        base.Add(key, value);
        _orderedKeys.Add(key);
    }

    public new void Remove(TKey key)
    {
        base.Remove(key);
        _orderedKeys.Remove(key);
    }
    
    public bool Equals(SortedDictionary<TKey, TValue> other)
    {
        return new DictionaryComparer<TKey, TValue>(Comparer).Equals(this, other);
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj))
        {
            return false;
        }

        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        if (obj.GetType() != this.GetType())
        {
            return false;
        }

        return Equals((SortedDictionary<TKey, TValue>)obj);
    }

    public override int GetHashCode()
    {
        throw new NotImplementedException();
    }


    public bool Equals(SortedDictionary<TKey, TValue> x, SortedDictionary<TKey, TValue> y)
    {
        return new DictionaryComparer<TKey, TValue>(Comparer).Equals(x, y);
    }

    public int GetHashCode(SortedDictionary<TKey, TValue> obj)
    {
        throw new NotImplementedException();
    }
}
