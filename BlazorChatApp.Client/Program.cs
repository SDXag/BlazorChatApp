using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using BlazorChatApp.Services;
using Microsoft.Extensions.Configuration;
using System.Reflection;

namespace BlazorChatApp
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("app");
            builder.Services.AddBaseAddressHttpClient();
            builder.Services.AddSingleton<ChatService>();

            var assembly = Assembly.GetExecutingAssembly();
            var resource = assembly.GetName().Name + ".Config.ConnectionSettings.json";            
            var stream = assembly.GetManifestResourceStream(resource);
            
            var config = new ConfigurationBuilder()  
                    .AddJsonStream(stream)
                    .Build();

            builder.Configuration.AddConfiguration(config);

            builder.Services.AddMsalAuthentication(options =>
            {
                options.ProviderOptions.Authentication.Authority = "https://login.microsoftonline.com/common";
                options.ProviderOptions.Authentication.ClientId = "b6240c79-58b6-4e27-a8d4-719c5a866312";
            });

            await builder.Build().RunAsync();
        }
    }
}
