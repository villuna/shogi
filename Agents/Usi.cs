using System;
using System.Linq;

public abstract class UsiMessage
{
    private static string[] commands = ["id", "usiok", "bestmove", "info"];

    public static UsiMessage Parse(String message)
    {
        string[] tokens = message.Split(' ');
        if (tokens.Length == 0)
        {
            throw new ArgumentException("empty command");
        }

        int i = 0;
        while (!commands.Contains(tokens[i]))
        {
            i++;
            if (i == commands.Length)
            {
                throw new ArgumentException("unrecognised command");
            }
        }

        String command = tokens[i];
        switch (command)
        {
            case "id":
                return new Id(tokens[i..]);
            case "usiok":
                if (i == tokens.Length - 1)
                {
                    return new UsiOk();
                }
                else
                {
                    throw new ArgumentException("invalid argument for command \"usiok\"");
                }
            case "info":
                return new Info(tokens[i..]);
            case "bestmove":
                return new BestMove(tokens[i..]);
            default:
                throw new ArgumentException("unrecognised command");
        }
    }
}

public class UsiOk : UsiMessage
{ }

public class Id : UsiMessage
{
    public enum IdType
    {
        Name,
        Author,
    }
    public IdType type;
    public String value;

    public Id(string[] tokens)
    {
        if (tokens.Length < 3)
        {
            throw new ArgumentException("not enough parameters for command \"id\"");
        }
        string type = tokens[1];

        switch (type)
        {
            case "name":
                this.type = IdType.Name;
                break;
            case "author":
                this.type = IdType.Author;
                break;
            default:
                throw new ArgumentException("uncrecognised parameter for command \"id\"");
        }

        string val = tokens[2];

        for (int i = 2; i < tokens.Length; i++)
        {
            val += " " + tokens[i];
        }

        this.value = val;
    }

    public Id(IdType type, String value)
    {
        this.type = type;
        this.value = value;
    }
}

public class Info : UsiMessage
{
    public String info;

    public Info(string[] tokens)
    {
        this.info = "";
        for (int i = 1; i < tokens.Length; i++)
        {
            if (i != 1)
            {
                this.info += " ";
            }
            this.info += tokens[i];
        }
    }
}

public class BestMove : UsiMessage
{
    public string bestMove;

    public BestMove(string[] tokens)
    {
        if (tokens.Length <= 1)
        {
            throw new ArgumentException("not enough parameters for command \"bestmove\"");
        }
        bestMove = tokens[1];
    }
}
