using OneClickEcho.Application.Lead.Commands.CreateLead;
using OneClickEcho.Domain.CompanyAggregate.ValueObjects;
using OneClickEcho.Domain.LeadAggregate.Repositories;

namespace OneClickEcho.Application.Common.Helpers
{
    public class LeadHelper
    {
        public static async Task<Domain.LeadAggregate.Lead> CreateOrUpdate(
            CreateLeadCommand command,
            CompanyId companyId,
            ILeadRepository leadRepository)
        {
            // check if lead with phone number already exists
            string standardizedPhoneNumber = PhoneNumberHelper.Standardize(command.PhoneNumber);

            Domain.LeadAggregate.Lead? existingLead = await leadRepository
                .GetByPhoneNumberAsync(standardizedPhoneNumber, companyId);

            if (existingLead != null)
            {
                // if lead exists, update data
                existingLead.FirstName = command.FirstName ?? existingLead.FirstName;
                existingLead.LastName = command.LastName ?? existingLead.LastName;
                existingLead.Gender = command.Gender ?? existingLead.Gender;
                existingLead.Email = command.Email ?? existingLead.Email;
                existingLead.DateOfBirth = command.DateOfBirth ?? existingLead.DateOfBirth;
                existingLead.City = command.City ?? existingLead.City;
                existingLead.State = command.State ?? existingLead.State;
                existingLead.Country = command.Country ?? existingLead.Country;

                return existingLead;
            }
            else
            {
                // if lead doesn't exist, create new lead
                Domain.LeadAggregate.Lead lead = new(
                    companyId: CompanyId.Create(command.CompanyId),
                    phoneNumber: standardizedPhoneNumber,
                    firstName: command.FirstName,
                    lastName: command.LastName,
                    gender: command.Gender,
                    email: command.Email,
                    dateOfBirth: command.DateOfBirth,
                    city: command.City,
                    state: command.State,
                    country: command.Country
                );

                leadRepository.Add(lead);

                return lead;
            }
        }
        
        public static async Task<Domain.LeadAggregate.Lead> CreateOrUpdateAndBlacklist(
            CreateLeadCommand command,
            CompanyId companyId,
            ILeadRepository leadRepository)
        {
            // check if lead with phone number already exists
            string standardizedPhoneNumber = PhoneNumberHelper.Standardize(command.PhoneNumber);

            Domain.LeadAggregate.Lead? existingLead = await leadRepository
                .GetByPhoneNumberAsync(standardizedPhoneNumber, companyId);

            if (existingLead != null)
            {
                // if lead exists, update data
                existingLead.FirstName = command.FirstName ?? existingLead.FirstName;
                existingLead.LastName = command.LastName ?? existingLead.LastName;
                existingLead.Gender = command.Gender ?? existingLead.Gender;
                existingLead.Email = command.Email ?? existingLead.Email;
                existingLead.DateOfBirth = command.DateOfBirth ?? existingLead.DateOfBirth;
                existingLead.City = command.City ?? existingLead.City;
                existingLead.State = command.State ?? existingLead.State;
                existingLead.Country = command.Country ?? existingLead.Country;
                existingLead.IsBlacklisted = true;

                return existingLead;
            }
            
            // if lead doesn't exist, create new lead
            Domain.LeadAggregate.Lead lead = new(
                companyId: CompanyId.Create(command.CompanyId),
                phoneNumber: standardizedPhoneNumber,
                firstName: command.FirstName,
                lastName: command.LastName,
                gender: command.Gender,
                email: command.Email,
                dateOfBirth: command.DateOfBirth,
                city: command.City,
                state: command.State,
                country: command.Country
            );
            lead.IsBlacklisted = true;

            leadRepository.Add(lead);

            return lead;
        }
    }
}
