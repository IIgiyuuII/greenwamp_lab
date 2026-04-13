using GreenSwamp.Pages;
using Microsoft.AspNetCore.Mvc;

namespace GreenSwamp.Services
{
    public interface IContactService
    {
        Task SaveContactAsync(ContactFormModel model);
    }
}
