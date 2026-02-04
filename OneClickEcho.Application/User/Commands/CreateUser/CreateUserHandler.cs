using OneClickEcho.Application.Common.Messaging;
using OneClickEcho.Domain.Common.Identity;
using OneClickEcho.Domain.Common.Shared;
using OneClickEcho.Domain.CompanyAggregate.ValueObjects;
using System.Text;

namespace OneClickEcho.Application.User.Commands.CreateUser;

public class CreateUserHandler(IUserManager userManager)
    : ICommandHandler<CreateUserCommand, CreateUserResponse>
{
    private readonly IUserManager _userManager = userManager;

    public async Task<Result<CreateUserResponse>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        if (request.Password is null)
        {
            string generatedPassword = PasswordGenerator.Generate();

            await _userManager.CreateUser(
                request.Email,
                generatedPassword,
                CompanyId.Create(request.CompanyId),
                request.Role);

            return new CreateUserResponse(generatedPassword);
        }

        // @TODO: Check if user already exists
        // @TODO: Use try/catch
        await _userManager.CreateUser(
            request.Email,
            request.Password,
            CompanyId.Create(request.CompanyId),
            request.Role);

        return new CreateUserResponse(null);
    }
}

public class PasswordGenerator
{
    private static readonly Random Random = new();
    private const string LowercaseChars = "abcdefghijklmnopqrstuvwxyz";
    private const string UppercaseChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private const string NumericChars = "0123456789";
    private const string NonAlphanumericChars = "!@#$%^&*()-_+=<>?";

    public static string Generate()
    {
        StringBuilder passwordBuilder = new();

        // Ensure at least one lowercase letter
        passwordBuilder.Append(GetRandomCharacter(LowercaseChars));

        // Ensure at least one uppercase letter
        passwordBuilder.Append(GetRandomCharacter(UppercaseChars));

        // Ensure at least one numeric character
        passwordBuilder.Append(GetRandomCharacter(NumericChars));

        // Ensure at least one non-alphanumeric character
        passwordBuilder.Append(GetRandomCharacter(NonAlphanumericChars));

        // Fill the rest with random characters from all sets until the length requirement is met
        const int passwordLength = 12;

        string allChars = LowercaseChars + UppercaseChars + NumericChars + NonAlphanumericChars;

        while (passwordBuilder.Length < passwordLength)
        {
            passwordBuilder.Append(GetRandomCharacter(allChars));
        }

        // Shuffle the password to ensure randomness
        char[] passwordArray = passwordBuilder.ToString().ToCharArray();

        return new string(passwordArray.OrderBy(c => Random.Next()).ToArray());
    }

    private static char GetRandomCharacter(string characterSet)
    {
        int index = Random.Next(characterSet.Length);

        return characterSet[index];
    }
}