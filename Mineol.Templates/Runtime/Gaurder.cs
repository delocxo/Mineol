static class Gaurder
{
    public static T Gaurd<T>(Position position, Func<T> func)
    {
        try
        {
            return func();
        }
        catch (Error)
        {
            throw;
        }
        catch (Exception e)
        {
            throw new Error(e.Message, position);
        }
    }

    public static void Gaurd(Position position, Action action)
    {
        try
        {
            action();
        }
        catch (Error)
        {
            throw;
        }
        catch (Exception e)
        {
            throw new Error(e.Message, position);
        }
    }
}