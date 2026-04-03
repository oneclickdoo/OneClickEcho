using OneClickEcho.Persistence.Common.Filtering.Lexer.Enums;
using OneClickEcho.Persistence.Common.Filtering.Lexer.Tokens;
using System.Collections;
using System.Globalization;

namespace OneClickEcho.Persistence.Common.Filtering.Lexer;

public class FilterLexer
{
    private string FilterText { get; }

    public List<FilterToken> FilterTokens { get; } = [];

    public FilterLexer(string filterText)
    {
        FilterText = filterText;
        Loop();
    }

    private void Loop()
    {
        FilterTokenType? previousToken = null;

        FilterTokenOperationType? previousOperation = null;

        for (int i = 0; i < FilterText.Length; i++)
        {
            switch (previousToken)
            {
                case null:
                    string variableName = GetVariableName(ref i);
                    FilterTokens.Add(new FilterTokenVariable(FilterTokenType.Variable, variableName));
                    previousToken = FilterTokenType.Variable;
                    break;
                case FilterTokenType.Variable:
                    string operatorName = GetOperatorType(ref i, out FilterTokenOperationType operationType1);
                    FilterTokens.Add(new FilterTokenOperation(FilterTokenType.Operation, operatorName, operationType1));
                    previousToken = FilterTokenType.Operation;
                    previousOperation = operationType1;
                    break;
                case FilterTokenType.Operation:
                    if (FilterTokens[^1].TokenType == FilterTokenType.Operation &&
                        previousOperation == FilterTokenOperationType.In)
                    {
                        IList operatorList = HandleInOperatorList(GenerateInOperatorList(ref i), out FilterTokenConstantType constantType1);
                        FilterTokens.Add(new FilterTokenConstantList(FilterTokenType.Constant, constantType1, operatorList));
                        previousToken = FilterTokenType.Constant;
                        break;
                    }
                    string constantValue = GetConstantValue(ref i, out FilterTokenConstantType? constantType);
                    AddConstantToken(constantValue, constantType);
                    previousToken = FilterTokenType.Constant;
                    break;
                case FilterTokenType.Constant:
                    operatorName = GetOperatorType(ref i, out FilterTokenOperationType operationType2);
                    FilterTokens.Add(new FilterTokenOperation(FilterTokenType.Operation, operatorName, operationType2));
                    previousToken = null;
                    previousOperation = operationType2;
                    break;
            }
        }
    }

    private string GetVariableName(ref int i)
    {
        string operatorName = "";

        char currentChar;

        do
        {
            currentChar = FilterText[i];

            if (currentChar == ' ')
            {
                break;
            }

            operatorName += currentChar;

            i++;
        } while (currentChar != ' ' && i < FilterText.Length);

        return operatorName;
    }

    private string GetOperatorType(ref int i, out FilterTokenOperationType operationType)
    {

        string operatorName = "";
        char currentChar;

        do
        {
            currentChar = FilterText[i];

            if (currentChar == ' ')
            {
                break;
            }

            operatorName += currentChar;

            i++;
        } while (currentChar != ' ' && i < FilterText.Length);

        operationType = operatorName switch
        {
            "eq" => FilterTokenOperationType.Eq,
            "ne" => FilterTokenOperationType.Ne,
            "gt" => FilterTokenOperationType.Gt,
            "ge" => FilterTokenOperationType.Ge,
            "lt" => FilterTokenOperationType.Lt,
            "le" => FilterTokenOperationType.Le,
            "co" => FilterTokenOperationType.Co,
            "in" => FilterTokenOperationType.In,
            "and" => FilterTokenOperationType.And,
            "or" => FilterTokenOperationType.Or,
            _ => throw new ArgumentException($"Unknown operator name: {operatorName}"),
        };

        return operatorName;
    }

    private string GetConstantValue(ref int i, out FilterTokenConstantType? constantType)
    {
        char currentChar = FilterText[i];

        // Hanlde strings
        if (currentChar == '\'')
        {
            constantType = FilterTokenConstantType.String;
            return HandleString(ref i);
        }

        // Handle all other types
        string constantValue = "";
        do
        {
            currentChar = FilterText[i];

            if (currentChar == ' ')
            {
                break;
            }

            constantValue += currentChar;

            i++;
        } while (currentChar != ' ' && i < FilterText.Length);

        constantType = null;

        return constantValue;
    }

    private string HandleString(ref int i)
    {
        i++;
        string constantName = "";

        do
        {
            char currentChar = FilterText[i];

            if (currentChar == '\'')
            {
                int numberOfApostrophes = 0;

                while (i < FilterText.Length && FilterText[i] == '\'')
                {
                    numberOfApostrophes++;
                    i++;
                }

                for (int j = 0; j < numberOfApostrophes / 2; j++) constantName += '\''; // Add escaped apostrophes

                if (numberOfApostrophes % 2 == 1)
                {
                    break; // If the number of apostrophes is uneven, end the string
                }
            }
            else
            {
                constantName += currentChar;
                i++;
            }
        } while (i < FilterText.Length);

        return constantName;
    }

    private void AddConstantToken(string constantValue, FilterTokenConstantType? constantType)
    {
        // String was handled previously, I just decided to handle all adding here.
        if (constantType == FilterTokenConstantType.String)
        {
            FilterTokens.Add(new FilterTokenConstant<string>(FilterTokenType.Constant, constantValue, FilterTokenConstantType.String, constantValue));
            return;
        }

        // Try to parse all other constants
        if (Guid.TryParse(constantValue, out Guid parsedGuid))
        {
            FilterTokens.Add(new FilterTokenConstant<Guid>(FilterTokenType.Constant, constantValue, FilterTokenConstantType.Guid, parsedGuid));
            return;
        }

        if (DateTime.TryParse(constantValue, null, DateTimeStyles.RoundtripKind, out DateTime parsedDateTime))
        {
            parsedDateTime = DateTime.SpecifyKind(parsedDateTime, DateTimeKind.Utc);
            FilterTokens.Add(new FilterTokenConstant<DateTime>(FilterTokenType.Constant, constantValue, FilterTokenConstantType.DateTime, parsedDateTime));
            return;
        }

        if (int.TryParse(constantValue, out int parsedInt))
        {
            FilterTokens.Add(new FilterTokenConstant<int>(FilterTokenType.Constant, constantValue, FilterTokenConstantType.Int, parsedInt));
            return;
        }

        if (decimal.TryParse(constantValue, out decimal parsedDecimal))
        {
            FilterTokens.Add(new FilterTokenConstant<Decimal>(FilterTokenType.Constant, constantValue, FilterTokenConstantType.Decimal, parsedDecimal));
            return;
        }

        FilterTokens.Add(new FilterTokenConstant<string>(FilterTokenType.Constant, constantValue, FilterTokenConstantType.String, constantValue));
    }

    private List<string> GenerateInOperatorList(ref int i)
    {
        List<string> constantValues = [];

        string substring = FilterText[i..].Split(' ')[0];

        foreach (string value in substring.Split(',', StringSplitOptions.RemoveEmptyEntries))
        {
            constantValues.Add(value);
        }

        i += substring.Length;

        return constantValues;
    }

    private IList HandleInOperatorList(List<string> constantValues, out FilterTokenConstantType constantType)
    {
        if (constantValues.Count == 0)
        {
            throw new Exception("Constant Values List can't be empty");
        }

        constantType = IntOrGuid(constantValues[0]);

        if (constantType == FilterTokenConstantType.Guid)
        {
            List<Guid> guids = [];

            foreach (string constantValue in constantValues)
            {
                Guid.TryParse(constantValue, out Guid tempGuid);
                guids.Add(tempGuid);
            }

            return guids;
        }

        if (constantType == FilterTokenConstantType.Int)
        {
            List<int> ints = [];

            foreach (string constantValue in constantValues)
            {
                int.TryParse(constantValue, out int tempInt);
                ints.Add(tempInt);
            }

            return ints;
        }

        throw new Exception("Constant Values List can only be Int or Guid");
    }

    private FilterTokenConstantType IntOrGuid(string constantValue)
    {
        return Guid.TryParse(constantValue, out _)
            ? FilterTokenConstantType.Guid
            : int.TryParse(constantValue, out _) ? FilterTokenConstantType.Int : throw new Exception("Unsupported Constant Type");
    }

    public override string ToString()
    {
        string displayCommands = "";

        for (int i = 0; i < FilterTokens.Count; i++)
        {
            FilterToken currentToken = FilterTokens[i];

            displayCommands += currentToken + " ";

            if (currentToken.TokenType == FilterTokenType.Constant)
            {
                displayCommands += "\n";
            }
        }

        return displayCommands;
    }
}