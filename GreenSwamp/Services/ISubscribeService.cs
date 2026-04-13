using GreenSwamp.Pages;
using Microsoft.AspNetCore.Mvc;

namespace GreenSwamp.Services
{
    public interface ISubscribeService
    {
        Task SaveSubscriberAsync(string email);
    }
}
