enum BinaryOperation
{
    Add,
    Sub,
    Mul,
    Div,
    Mod,
    Less,
    LessEqual,
    Greater,
    GreaterEqual,
}

enum UnaryOperation
{
    Negate,
    Flip
}

static class Arithmetic
{
    public static Value Add(Value left, Value right, Position position)
    {
        return Gaurder.Gaurd(position, () =>
        {
            if (left.IsNumber() && right.IsNumber())
            {
                if (left.IsFloat() || right.IsFloat())
                    return new Value(left.AsFloat() + right.AsFloat());

                try
                {
                    return new Value(checked(left.Int + right.Int));
                }
                catch (OverflowException)
                {
                    return new Value(left.AsFloat() + right.AsFloat());
                }
            }

            else if (left.IsString() || right.IsString())
                return new Value(left.ToString(position) + right.ToString(position));

            return Globals.KindOperations.GetBinary(left, right, BinaryOperation.Add, "+", position);
        });
    }

    public static Value Sub(Value left, Value right, Position position)
    {
        return Gaurder.Gaurd(position, () =>
        {
            if (left.IsNumber() && right.IsNumber())
            {
                if (left.IsFloat() || right.IsFloat())
                    return new Value(left.AsFloat() - right.AsFloat());

                try
                {
                    return new Value(checked(left.Int - right.Int));
                }
                catch (OverflowException)
                {
                    return new Value(left.AsFloat() - right.AsFloat());
                }
            }

            return Globals.KindOperations.GetBinary(left, right, BinaryOperation.Sub, "-", position);
        });
    }

    public static Value Mul(Value left, Value right, Position position)
    {
        return Gaurder.Gaurd(position, () =>
        {
            if (left.IsNumber() && right.IsNumber())
            {
                if (left.IsFloat() || right.IsFloat())
                    return new Value(left.AsFloat() * right.AsFloat());

                try
                {
                    return new Value(checked(left.Int * right.Int));
                }
                catch (OverflowException)
                {
                    return new Value(left.AsFloat() * right.AsFloat());
                }
            }

            return Globals.KindOperations.GetBinary(left, right, BinaryOperation.Mul, "*", position);
        });
    }

    public static Value Div(Value left, Value right, Position position)
    {
        return Gaurder.Gaurd(position, () =>
        {
            if (left.IsNumber() && right.IsNumber())
            {
                if (left.IsFloat() || right.IsFloat())
                    return new Value(left.AsFloat() / right.AsFloat());

                try
                {
                    return new Value(checked(left.Int / right.Int));
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

            return Globals.KindOperations.GetBinary(left, right, BinaryOperation.Div, "/", position);
        });
    }

    public static Value Mod(Value left, Value right, Position position)
    {
        return Gaurder.Gaurd(position, () =>
        {
            if (left.IsNumber() && right.IsNumber())
            {
                if (left.IsFloat() || right.IsFloat())
                    return new Value(left.AsFloat() % right.AsFloat());

                try
                {
                    return new Value(checked(left.Int % right.Int));
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

            return Globals.KindOperations.GetBinary(left, right, BinaryOperation.Mod, "%", position);
        });
    }

    public static Value Less(Value left, Value right, Position position)
    {
        return Gaurder.Gaurd(position, () =>
        {
            if (left.IsNumber() && right.IsNumber())
            {
                if (left.IsFloat() || right.IsFloat())
                    return new Value(left.AsFloat() < right.AsFloat());

                return new Value(left.Int < right.Int);
            }

            return Globals.KindOperations.GetBinary(left, right, BinaryOperation.Less, "<", position);
        });
    }

    public static Value Greater(Value left, Value right, Position position)
    {
        return Gaurder.Gaurd(position, () =>
        {
            if (left.IsNumber() && right.IsNumber())
            {
                if (left.IsFloat() || right.IsFloat())
                    return new Value(left.AsFloat() > right.AsFloat());

                return new Value(left.Int > right.Int);
            }

            return Globals.KindOperations.GetBinary(left, right, BinaryOperation.Greater, ">", position);
        });
    }

    public static Value LessEqual(Value left, Value right, Position position)
    {
        return Gaurder.Gaurd(position, () =>
        {
            if (left.IsNumber() && right.IsNumber())
            {
                if (left.IsFloat() || right.IsFloat())
                    return new Value(left.AsFloat() <= right.AsFloat());

                return new Value(left.Int <= right.Int);
            }

            return Globals.KindOperations.GetBinary(left, right, BinaryOperation.LessEqual, "<=", position);
        });
    }

    public static Value GreaterEqual(Value left, Value right, Position position)
    {
        return Gaurder.Gaurd(position, () =>
        {
            if (left.IsNumber() && right.IsNumber())
            {
                if (left.IsFloat() || right.IsFloat())
                    return new Value(left.AsFloat() >= right.AsFloat());

                return new Value(left.Int >= right.Int);
            }

            return Globals.KindOperations.GetBinary(left, right, BinaryOperation.GreaterEqual, ">=", position);
        });
    }

    public static Value Equals(Value left, Value right, Position position)
    {
        return Gaurder.Gaurd(position, () =>
        {
            return new Value(left.CheckEquality(right, position));
        });
    }

    public static Value NotEquals(Value left, Value right, Position position)
    {
        return Gaurder.Gaurd(position, () =>
        {
            return new Value(!left.CheckEquality(right, position));
        });
    }

    public static Value Negate(Value right, Position position)
    {
        return Gaurder.Gaurd(position, () =>
        {
            if (right.IsInt())
                return new Value(-right.Int);

            else if (right.IsFloat())
                return new Value(-right.Float);

            else if (right.IsRecord())
                return Globals.KindOperations.GetUnary(right, UnaryOperation.Negate, "-", position);

            throw UnaryError(right, "-", position);
        });
    }

    public static Value Flip(Value right, Position position)
    {
        return Gaurder.Gaurd(position, () =>
        {
            if (right.IsRecord())
                if (Globals.KindOperations.TryGetUnary(right, UnaryOperation.Flip, "!", position, out Value value))
                    return value;

            return new Value(!right.IsTruthy());
        });
    }

    public static Value And(Value left, Position position, Func<Value> right)
    {
        return Gaurder.Gaurd(position, () =>
        {
            return left.IsTruthy() ? right() : left;
        });
    }

    public static Value Or(Value left, Position position, Func<Value> right)
    {
        return Gaurder.Gaurd(position, () =>
        {
            return left.IsTruthy() ? left : right();
        });
    }

    public static Error UnaryError(Value value, string op, Position position) =>
        new Error($"Cannot apply '{op}' to {value.KindName}", position);

    public static Error BinaryError(Value left, Value right, string op, Position position) =>
        new Error($"Cannot apply '{op}' to {left.KindName} and {right.KindName}", position);
}
