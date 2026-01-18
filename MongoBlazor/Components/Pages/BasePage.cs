using Microsoft.AspNetCore.Components;
using MongoBlazor.Services;

namespace MongoBlazor.Components.Pages
{
    public class BasePage : ComponentBase
    {
        [Inject]
        protected AuthService AuthService { get; set; } = default!;

        [Inject]
        protected SessionService SessionService { get; set; } = default!;

        [Inject]
        protected NavigationManager Navigation { get; set; } = default!;

        protected override async Task OnInitializedAsync()
        {
            await CheckAuthentication();
            await base.OnInitializedAsync();
        }

        protected async Task CheckAuthentication()
        {
            if (AuthService.IsLoginEnabled)
            {
                var isAuthenticated = await SessionService.IsAuthenticatedAsync();
                if (!isAuthenticated)
                {
                    Navigation.NavigateTo("/login", true);
                }
            }
        }
    }
}