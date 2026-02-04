using OneClickEcho.Persistence.Common.Filtering.Lexer.Enums;

namespace OneClickEcho.Persistence.Common.Filtering.Lexer.Tokens;

public abstract class FilterToken(
    FilterTokenType tokenType,
    string value)
{
    public string Value { get; set; } = value;

    public FilterTokenType TokenType { get; set; } = tokenType;
}