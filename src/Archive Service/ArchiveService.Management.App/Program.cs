using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Mvc.Authorization;
using Graph = Microsoft.Graph;
using ArchiveService.Management.App.Data;
using Microsoft.EntityFrameworkCore;
using ArchiveService.Management.App.Data.Models;
using MudBlazor.Services;
using Microsoft.Xrm.Sdk;
using Microsoft.PowerPlatform.Dataverse.Client;
using Azure.Identity;
using Microsoft.Extensions.Caching.Memory;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var initialScopes = builder.Configuration["DownstreamApi:Scopes"]?.Split(' ') ?? builder.Configuration["MicrosoftGraph:Scopes"]?.Split(' ');

builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAd"))
        .EnableTokenAcquisitionToCallDownstreamApi(initialScopes)
            .AddMicrosoftGraph(builder.Configuration.GetSection("MicrosoftGraph"))
            .AddInMemoryTokenCaches();
builder.Services.AddControllersWithViews()
    .AddMicrosoftIdentityUI();
builder.Services.AddAuthorization(options =>
{
    // By default, all incoming requests will be authorized according to the default policy
    options.FallbackPolicy = options.DefaultPolicy;
});


builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor()
    .AddMicrosoftIdentityConsentHandler();
builder.Services.AddScoped<SettingsService>();
builder.Services.AddScoped<TablesService>();
builder.Services.AddDbContext<ArchiveServiceContext>(option =>
    option
        .UseSqlServer(builder.Configuration.GetConnectionString("ArchiveServiceDb"))
        .AddInterceptors(new AadAuthenticationInterceptor())
);

builder.Services.AddMemoryCache();
builder.Services.AddSingleton(new DefaultAzureCredential());
builder.Services.AddMudServices();
builder.Services.AddSingleton<IOrganizationService, ServiceClient>(provider =>
{
    var managedIdentity = provider.GetRequiredService<DefaultAzureCredential>();
    var environment = builder.Configuration.GetValue<string>("dataverseurl", "https://orgb569cb44.crm.dynamics.com/");
    var cache = provider.GetService<IMemoryCache>();
    return new ServiceClient(
            tokenProviderFunction: f => ArchiveService.Management.App.Helper.TokenHelper.GetToken(environment, managedIdentity, cache),
            instanceUrl: new Uri(environment),
            useUniqueInstance: true);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
