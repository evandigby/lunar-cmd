using Microsoft.Extensions.DependencyInjection;

namespace Client.State
{
    public static class StateContainerServicesExtension
    {
        public static void AddState(this IServiceCollection services, StateContainer state)
        {
            services.AddSingleton(state);
            services.AddSingleton(state.Api);
            services.AddSingleton(state.Mission);
            services.AddSingleton(state.User);
            services.AddSingleton(state.Attachments);
        }
    }
}
