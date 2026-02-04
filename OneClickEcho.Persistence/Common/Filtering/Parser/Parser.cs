using OneClickEcho.Persistence.Common.Filtering.Lexer.Tokens;
using OneClickEcho.Persistence.Common.Filtering.Parser.Nodes;

namespace OneClickEcho.Persistence.Common.Filtering.Parser;

public class Parser(List<FilterToken> tokens)
{
    private readonly List<FilterToken> _tokens = tokens;
    private int _position = 0;

    public ExpressionNode Parse()
    {
        ExpressionNode left = ParseTerm;

        while (_position < _tokens.Count)
        {
            FilterToken opToken = _tokens[_position++];
            ExpressionNode right = ParseTerm;

            left = new BinaryExpressionNode
            {
                Left = left,
                Operator = opToken,
                Right = right
            };
        }

        return left;
    }

    private ExpressionNode ParseTerm
    {
        get
        {
            FilterToken varToken = _tokens[_position++];
            FilterToken opToken = _tokens[_position++];
            FilterToken constToken = _tokens[_position++];

            return new ValueNode
            {
                Variable = varToken,
                Operator = opToken,
                Constant = constToken
            };
        }
    }
}
