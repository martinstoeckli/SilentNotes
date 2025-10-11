using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using TG.Blazor.IndexedDB;

namespace SilentNotes.Pwa
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");
            builder.RootComponents.Add<HeadOutlet>("head::after");

            builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
            builder.Services.AddIndexedDB(dbStore =>
            {
                dbStore.DbName = "ch.martinstoeckli.silentnotes";
                dbStore.Version = 1;
                dbStore.Stores.Add(new StoreSchema
                {
                    Name = "files",
                    PrimaryKey = new IndexSpec { Name = "path", KeyPath = "path", Unique = true, Auto = false },
                });
            });

            await builder.Build().RunAsync();
        }
    }
}
