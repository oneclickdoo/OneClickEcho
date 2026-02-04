using OneClickEcho.Persistence.Common.Filtering.Lexer.Tokens;

namespace OneClickEcho.Persistence.Common.Filtering.Parser.Nodes;

public class BinaryExpressionNode : ExpressionNode
{
    public ExpressionNode Left { get; set; } = default!;

    public FilterToken Operator { get; set; } = default!;

    public ExpressionNode Right { get; set; } = default!;
}