using OneClickEcho.Persistence.Common.Filtering.Lexer.Enums;
using System.Collections;

namespace OneClickEcho.Persistence.Common.Filtering.Lexer.Tokens;

public class FilterTokenConstantList : FilterToken
{
    public FilterTokenConstantList(
        FilterTokenType tokenType,
        FilterTokenConstantType filterTokenConstantType,
        IList constantValueList) : base(tokenType, "")
    {
        FilterTokenConstantType = filterTokenConstantType;
        ConstantValueList = constantValueList;
    }

    public FilterTokenConstantType FilterTokenConstantType { get; set; }
    public IList ConstantValueList { get; set; }

    public override string ToString()
    {
        string combinedList = "";
        for (int i = 0; i < ConstantValueList.Count; i++)
        {
            combinedList += $"{FilterTokenConstantType}({ConstantValueList[i]})";
            if (i < ConstantValueList.Count - 1)
                combinedList += ",";
        }

        return combinedList;
    }
}