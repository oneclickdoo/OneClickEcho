namespace OneClickEcho.Persistence.Common.Filtering.Lexer.Enums;

public enum FilterTokenConstantType
{
    Int,       // 123                                                       123
    Decimal,   // 123.45                                                    123.45
    DateTime,  // DateTime.Parse("2023-10-20")                              2023-10-21T18:07:00.000Z
    Guid,      // Guid.Parse("123e4567-e89b-12d3-a456-426614174000")        123e4567-e89b-12d3-a456-426614174000
    String     // "example text"                                            'example "text"'   (Escape ' with double '')
}
