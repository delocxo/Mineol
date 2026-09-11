using System;
using System.Collections.Generic;
using System.Text;

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
    Position _position;

    public Error(string message, Position position) : base(message)
    {
        _position = position;
    }

    public void Exit()
    {
        Console.Error.WriteLine($"Error: {_position.Line}:{_position.Column}:{_position.Source}: {Message}");
        Environment.Exit(1);
    }
}