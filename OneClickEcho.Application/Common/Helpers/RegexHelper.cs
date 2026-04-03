namespace OneClickEcho.Application.Common.Helpers;

public class RegexHelper
{
    // public static readonly string PHONE_NUMBER_REGEX = @"^(\+3816|3816|06)(([0-6]|[8-9])\d{6,7}|(77|78)\d{5,6})$";
    public static readonly string PHONE_NUMBER_REGEX = @"^\+?[1-9]\d{1,14}$";
}