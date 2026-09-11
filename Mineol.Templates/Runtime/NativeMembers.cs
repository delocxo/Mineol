delegate Value MemberGetter(Value target, string name, Position position);
delegate void MemberSetter(Value target, string name, Value value, Position position);

class NativeMembers
{
    Dictionary<int, MemberGetter> _getters = new Dictionary<int, MemberGetter>();
    Dictionary<int, MemberSetter> _setters = new Dictionary<int, MemberSetter>();

    public Value Get(Value target, string name, Position position)
    {
        if (_getters.TryGetValue(target.Kind, out var getter))
            return getter(target, name, position);

        throw new Error($"{target.KindName} does not contain '{name}'", position);
    }

    public void Set(Value target, string name, Value value, Position position)
    {
        if (_setters.TryGetValue(target.Kind, out var setter))
        {
            setter(target, name, value, position);
            return;
        }

        throw new Error($"{target.KindName} does not contain '{name}'", position);
    }
}