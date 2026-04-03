using OneClickEcho.Persistence.Common.Filtering.Lexer;
using System.Linq.Expressions;

namespace OneClickEcho.Persistence.Common.Filtering;

public class Filtering<TEntity>
{
    public static Expression<Func<TEntity, bool>> ApplyFilter(string filter)
    {
        FilterLexer lexer = new(filter);

        Parser.Parser parse = new(lexer.FilterTokens);

        Parser.Nodes.ExpressionNode astRootNode = parse.Parse();

        return ExpressionGenerator.ExpressionGenerator.Generate<TEntity>(astRootNode);
    }
}