using OneClickEcho.Persistence.Common.Filtering.Lexer.Tokens;

namespace OneClickEcho.Persistence.Common.Filtering.Parser.Nodes;

public class ValueNode : ExpressionNode
{
    public FilterToken Variable { get; set; } = default!;

    public FilterToken Operator { get; set; } = default!;

    public FilterToken Constant { get; set; } = default!;
}