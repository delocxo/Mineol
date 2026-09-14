delegate Value MemberGetter(Value target, string name, Position position);
delegate bool ExtensionMemberGetter(Value target, string name, Position position, out Value value);
delegate void MemberSetter(Value target, string name, Value value, Position position);
delegate Value IndexGetter(Value target, Value index, Position position);
delegate void IndexSetter(Value target, Value index, Value value, Position position);
delegate bool KindEquality(Value left, Value right);
delegate string KindToString(Value target);
delegate int KindHash(Value target);
delegate List<Value> KindIterable(Value target);

class KindOperations
{
    Dictionary<int, MemberGetter> _memberGetters = new Dictionary<int, MemberGetter>();
    Dictionary<int, MemberSetter> _memberSetters = new Dictionary<int, MemberSetter>();
    Dictionary<int, IndexGetter> _indexGetters = new Dictionary<int, IndexGetter>();
    Dictionary<int, IndexSetter> _indexSetters = new Dictionary<int, IndexSetter>();

    Dictionary<int, KindEquality> _equalities = new Dictionary<int, KindEquality>();
    Dictionary<int, KindToString> _toStrings = new Dictionary<int, KindToString>();
    Dictionary<int, KindHash> _kindHashes = new Dictionary<int, KindHash>();
    Dictionary<int, KindIterable> _kindIterables = new Dictionary<int, KindIterable>();

    public List<ExtensionMemberGetter> ExtensionMemberGetters { get; } = new List<ExtensionMemberGetter>();

    public Value GetMember(Value target, string name, Position position)
    {
        foreach (ExtensionMemberGetter extensionMemberGetter in ExtensionMemberGetters)
        {
            if (extensionMemberGetter(target, name, position, out Value value))
                return value;
        }

        if (_memberGetters.TryGetValue(target.Kind, out var getter))
            return getter(target, name, position);

        throw new Error($"{target.KindName} does not contain '{name}'", position);
    }

    public void SetMember(Value target, string name, Value value, Position position)
    {
        if (_memberSetters.TryGetValue(target.Kind, out var setter))
        {
            setter(target, name, value, position);
            return;
        }

        throw new Error($"{target.KindName} does not contain '{name}'", position);
    }

    public Value GetIndex(Value target, Value index, Position position)
    {
        if (_indexGetters.TryGetValue(target.Kind, out var getter))
            return getter(target, index, position);

        throw new Error($"{target.KindName} cannot be indexed", position);
    }

    public void SetIndex(Value target, Value index, Value value, Position position)
    {
        if (_indexSetters.TryGetValue(target.Kind, out var setter))
        {
            setter(target, index, value, position);
            return;
        }

        throw new Error($"{target.KindName} does not support index assignment", position);
    }

    public bool Equals(Value left, Value right)
    {
        if (_equalities.TryGetValue(left.Kind, out var equality))
            return equality(left, right);

        return false;
    }

    public string ToString(Value target)
    {
        if (_toStrings.TryGetValue(target.Kind, out var toString))
            return toString(target);

        return target.KindName;
    }

    public int GetHash(Value target, Position position)
    {
        if (_kindHashes.TryGetValue(target.Kind, out var hash))
            return hash(target);

        throw new Error($"{target.KindName} is not hashable", position);
    }

    public bool TryGetHash(Value target, out int hashCode)
    {
        if (_kindHashes.TryGetValue(target.Kind, out var hash))
        {
            hashCode = hash(target);
            return true;
        }

        hashCode = 0;
        return false;
    }

    public List<Value> GetIterable(Value target, Position position)
    {
        if (_kindIterables.TryGetValue(target.Kind, out var iterable))
            return iterable(target);

        throw new Error($"{target.KindName} is not iterable", position);
    }

    public bool HasIterable(int kind)
    {
        return _kindIterables.ContainsKey(kind);
    }

    public void AddMemberGetter(int kind, MemberGetter memberGetter)
    {
        _memberGetters[kind] = memberGetter;
    }

    public void AddMemberSetter(int kind, MemberSetter memberSetter)
    {
        _memberSetters[kind] = memberSetter;
    }

    public void AddIndexGetter(int kind, IndexGetter indexGetter)
    {
        _indexGetters[kind] = indexGetter;
    }

    public void AddIndexSetter(int kind, IndexSetter indexSetter)
    {
        _indexSetters[kind] = indexSetter;
    }

    public void AddEquality(int kind, KindEquality equality)
    {
        _equalities[kind] = equality;
    }

    public void AddDefaultEquality<T>(int kind)
    {
        _equalities[kind] = (left, right) =>
        {
            if (!right.KindIs(kind))
                return false;

            T leftObj = left.As<T>();
            T rightObj = right.As<T>();

            return EqualityComparer<T>.Default.Equals(leftObj, rightObj);
        };
    }

    public void AddToString(int kind, KindToString toString)
    {
        _toStrings[kind] = toString;
    }

    public void AddHash(int kind, KindHash kindHash)
    {
        _kindHashes[kind] = kindHash;
    }

    public void AddDefaultHash(int kind)
    {
        _kindHashes[kind] = (target) =>
        {
            return target.Object!.GetHashCode();
        };
    }

    public void AddIterable(int kind, KindIterable kindIterable)
    {
        _kindIterables[kind] = kindIterable;
    }

    public void AddExtensionMemberGetter(ExtensionMemberGetter memberGetter)
    {
        ExtensionMemberGetters.Add(memberGetter);
    }
}