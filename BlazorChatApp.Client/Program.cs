using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using BlazorChatApp.Services;
using Microsoft.Extensions.Configuration;
using System.Reflection;
using System.Net.Http;

namespace BlazorChatApp
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("app");
            builder.Services.AddTransient(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
            builder.Services.AddSingleton<ChatService>();

            var assembly = Assembly.GetExecutingAssembly();
            var resource = assembly.GetName().Name + ".Config.ConnectionSettings.json";            
            var stream = assembly.GetManifestResourceStream(resource);
            
            var config = new ConfigurationBuilder()  
                    .AddJsonStream(stream)
                    .Build();

            builder.Configuration.AddConfiguration(config);

            await builder.Build().RunAsync();
        }
    }
}
