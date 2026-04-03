namespace OneClickEcho.Persistence.Common.Filtering.Lexer.Enums;

public enum FilterTokenOperationType
{
    Eq,  // eq: Equals (e.g., Price eq 20).
    Ne,  // ne: Not Equals (e.g., Price ne 20).
    Gt,  // gt: Greater Than (e.g., Price gt 20).
    Ge,  // ge: Greater Than or Equal (e.g., Price ge 20).
    Lt,  // lt: Less Than (e.g., Price lt 20).
    Le,  // le: Less Than or Equal (e.g., Price le 20).

    Co,  // co: Variable(string) contains constant (e.g., Book co 'Book')
    In,  // in: Variable(enum) in list of constants (e.g., Book in 1, 2)

    And, // and: And
    Or   // or: Or
}
