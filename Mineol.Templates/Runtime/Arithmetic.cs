static class Arithmetic
{
    public static Value Add(Value left, Value right, Position position)
    {
        if (left.IsNumber() && right.IsNumber())
        {
            if (left.IsFloat() || right.IsFloat())
                return new Value(left.AsFloat() + right.AsFloat());

            try
            {
                return new Value(unchecked(left.Int + right.Int));
            }
            catch (OverflowException)
            {
                return new Value(left.AsFloat() + right.AsFloat());
            }
        }

        else if (left.IsString() || right.IsString())
            return new Value(left.ToString() + right.ToString());

        throw BinaryError(left, right, "+", position);
    }

    public static Value Sub(Value left, Value right, Position position)
    {
        if (left.IsNumber() && right.IsNumber())
        {
            if (left.IsFloat() || right.IsFloat())
                return new Value(left.AsFloat() - right.AsFloat());

            try
            {
                return new Value(unchecked(left.Int - right.Int));
            }
            catch (OverflowException)
            {
                return new Value(left.AsFloat() - right.AsFloat());
            }
        }

        throw BinaryError(left, right, "-", position);
    }

    public static Value Mul(Value left, Value right, Position position)
    {
        if (left.IsNumber() && right.IsNumber())
        {
            if (left.IsFloat() || right.IsFloat())
                return new Value(left.AsFloat() * right.AsFloat());

            try
            {
                return new Value(unchecked(left.Int * right.Int));
            }
            catch (OverflowException)
            {
                return new Value(left.AsFloat() * right.AsFloat());
            }
        }

        throw BinaryError(left, right, "*", position);
    }

    public static Value Div(Value left, Value right, Position position)
    {
        if (left.IsNumber() && right.IsNumber())
        {
            if (left.IsFloat() || right.IsFloat())
                return new Value(left.AsFloat() / right.AsFloat());

            try
            {
                return new Value(unchecked(left.Int / right.Int));
            }
            catch (DivideByZeroException)
            {
                throw new Error("Cannot divide an int by zero", position);
            }
            catch (OverflowException)
            {
                return new Value(left.AsFloat() / right.AsFloat());
            }
        }

        throw BinaryError(left, right, "/", position);
    }

    public static Value Mod(Value left, Value right, Position position)
    {
        if (left.IsNumber() && right.IsNumber())
        {
            if (left.IsFloat() || right.IsFloat())
                return new Value(left.AsFloat() % right.AsFloat());

            try
            {
                return new Value(unchecked(left.Int % right.Int));
            }
            catch (DivideByZeroException)
            {
                throw new Error("Cannot modulo an int by zero", position);
            }
            catch (OverflowException)
            {
                return new Value(left.AsFloat() % right.AsFloat());
            }
        }

        throw BinaryError(left, right, "%", position);
    }

    public static Value Less(Value left, Value right, Position position)
    {
        if (left.IsNumber() && right.IsNumber())
        {
            if (left.IsFloat() || right.IsFloat())
                return new Value(left.AsFloat() < right.AsFloat());

            return new Value(left.Int < right.Int);
        }

        throw BinaryError(left, right, "<", position);
    }

    public static Value Greater(Value left, Value right, Position position)
    {
        if (left.IsNumber() && right.IsNumber())
        {
            if (left.IsFloat() || right.IsFloat())
                return new Value(left.AsFloat() > right.AsFloat());

            return new Value(left.Int > right.Int);
        }

        throw BinaryError(left, right, ">", position);
    }

    public static Value LessEqual(Value left, Value right, Position position)
    {
        if (left.IsNumber() && right.IsNumber())
        {
            if (left.IsFloat() || right.IsFloat())
                return new Value(left.AsFloat() <= right.AsFloat());

            return new Value(left.Int <= right.Int);
        }

        throw BinaryError(left, right, "<=", position);
    }

    public static Value GreaterEqual(Value left, Value right, Position position)
    {
        if (left.IsNumber() && right.IsNumber())
        {
            if (left.IsFloat() || right.IsFloat())
                return new Value(left.AsFloat() >= right.AsFloat());

            return new Value(left.Int >= right.Int);
        }

        throw BinaryError(left, right, ">=", position);
    }

    public static Value Equals(Value left, Value right, Position position)
    {
        return new Value(left.CheckEquality(right));
    }

    public static Value NotEquals(Value left, Value right, Position position)
    {
        return new Value(!left.CheckEquality(right));
    }

    public static Value Negate(Value right, Position position)
    {
        if (right.IsInt())
            return new Value(-right.Int);

        if (right.IsFloat())
            return new Value(-right.Float);

        throw UnaryError(right, "-", position);
    }

    public static Value Flip(Value right)
    {
        return new Value(!right.IsTruthy());
    }

    public static Value And(Value left, Func<Value> right)
    {
        return left.IsTruthy() ? right() : left;
    }

    public static Value Or(Value left, Func<Value> right)
    {
        return left.IsTruthy() ? left : right();
    }

    public static Error UnaryError(Value value, string op, Position position) =>
        new Error($"Cannot apply '{op}' to {value.KindName}", position);

    public static Error BinaryError(Value left, Value right, string op, Position position) =>
        new Error($"Cannot apply '{op}' to {left.KindName} and {right.KindName}", position);
}