namespace OneClickEcho.Application.Common.Helpers
{
    public class PhoneNumberHelper
    {
        public static string Standardize(string phoneNumber)
        {
            // remove all non-digit characters
            string standardizedPhoneNumber = new(phoneNumber
                .Where(Char.IsDigit)
                .ToArray());

            // // convert phone number to international format
            // if (standardizedPhoneNumber.StartsWith('0'))
            // {
            //     standardizedPhoneNumber = $"+381{standardizedPhoneNumber[1..]}";
            // }
            if (standardizedPhoneNumber.StartsWith("381"))
            {
                standardizedPhoneNumber = $"+{standardizedPhoneNumber}";
            }

            return standardizedPhoneNumber;
        }
    }
}
