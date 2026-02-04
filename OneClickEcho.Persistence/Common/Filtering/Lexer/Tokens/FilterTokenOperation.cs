using OneClickEcho.Persistence.Common.Filtering.Lexer.Enums;

namespace OneClickEcho.Persistence.Common.Filtering.Lexer.Tokens;

public class FilterTokenOperation : FilterToken
{
    public FilterTokenOperation(
        FilterTokenType tokenType,
        string value,
        FilterTokenOperationType filterTokenOperationType) : base(tokenType, value)
    {
        CurrentFilterTokenOperationType = filterTokenOperationType;
    }

    public FilterTokenOperationType? CurrentFilterTokenOperationType { get; set; }

    public override string ToString()
    {
        return CurrentFilterTokenOperationType switch
        {
            FilterTokenOperationType.Eq => "==",
            FilterTokenOperationType.Ne => "!=",
            FilterTokenOperationType.Gt => ">",
            FilterTokenOperationType.Ge => ">=",
            FilterTokenOperationType.Lt => "<",
            FilterTokenOperationType.Le => "<=",
            FilterTokenOperationType.Co => "contains",
            FilterTokenOperationType.In => "in",
            FilterTokenOperationType.And => "&&",
            FilterTokenOperationType.Or => "||",
            _ => throw new InvalidOperationException($"Unsupported operation: {CurrentFilterTokenOperationType}")
        };
    }
}