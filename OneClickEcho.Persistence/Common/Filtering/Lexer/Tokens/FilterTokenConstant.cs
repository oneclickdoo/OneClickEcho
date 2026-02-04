using OneClickEcho.Persistence.Common.Filtering.Lexer.Enums;
using OneClickEcho.Persistence.Common.Filtering.Lexer.Tokens.Interfaces;

namespace OneClickEcho.Persistence.Common.Filtering.Lexer.Tokens;

public class FilterTokenConstant<T> : FilterToken, IFilterTokenConstant
{
    public FilterTokenConstant(
        FilterTokenType tokenType,
        string value,
        FilterTokenConstantType filterTokenConstantType,
        T constantValue) : base(tokenType, value)
    {
        FilterTokenConstantType = filterTokenConstantType;
        ConstantValue = constantValue;
    }

    public FilterTokenConstantType FilterTokenConstantType { get; set; }
    public T ConstantValue { get; set; }

    public override string ToString()
    {
        return $"{FilterTokenConstantType}({ConstantValue})";
    }

    object IFilterTokenConstant.Value => ConstantValue!;
}