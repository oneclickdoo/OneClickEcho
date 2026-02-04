namespace OneClickEcho.Domain.Common.Shared;

public class Error : IEquatable<Error>
{
    public static readonly Error None = new(string.Empty, string.Empty);
    public static readonly Error NullValue = new("Error.NullValue", "The specified result value is null.");

    public Error(string code, string message)
    {
        Code = code;
        Message = message;
    }

    public string Code { get; }

    public string Message { get; }

    public static implicit operator string(Error error)
    {
        return error.Code;
    }

    public static bool operator ==(Error? a, Error? b)
    {
        return (a is null && b is null) || (a is not null && b is not null && a.Equals(b));
    }

    public static bool operator !=(Error? a, Error? b)
    {
        return !(a == b);
    }

    public virtual bool Equals(Error? other)
    {
        return other is not null && Code == other.Code && Message == other.Message;
    }

    public override bool Equals(object? obj)
    {
        return obj is Error error && Equals(error);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Code, Message);
    }

    public override string ToString()
    {
        return Code;
    }
}
