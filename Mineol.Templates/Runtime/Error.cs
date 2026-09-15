struct Position
{
    public Position(int line, int column, string source)
    {
        Line = line;
        Column = column;
        Source = source;
    }

    public int Line { get; set; }
    public int Column { get; set; }
    public string Source { get; }
}

class Error : Exception
{
    public Value ErrorValue { get; } = Value.Null;

    Position _position;

    public Error(string message, Position position) : base($"{position.Line}:{position.Column}:{position.Source}: {message}")
    {
        _position = position;
    }

    public Error(string message, Value errorValue, Position position) : base($"{position.Line}:{position.Column}:{position.Source}: {message}")
    {
        ErrorValue = errorValue;
        _position = position;
    }

    public void Exit()
    {
        Console.Error.WriteLine(Message);
        Environment.Exit(1);
    }
}
