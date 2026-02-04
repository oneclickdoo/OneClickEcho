using OneClickEcho.Persistence.Common.Filtering.Lexer.Enums;

namespace OneClickEcho.Persistence.Common.Filtering.Lexer.Tokens;

public class FilterTokenVariable : FilterToken
{
    public FilterTokenVariable(FilterTokenType tokenType, string value) : base(tokenType, value) { }

    public override string ToString()
    {
        return $"{Value}";
    }
}