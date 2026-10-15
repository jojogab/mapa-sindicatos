using CnesMapaSindicatos.Components;
using CnesMapaSindicatos.Services;

var builder = WebApplication.CreateBuilder(args);

// Adiciona os serviços do Razor Components com Interatividade Server
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Registra o serviço de manipulação e importação dos sindicatos (Planilha MTE)
builder.Services.AddSingleton<SindicatoService>();

var app = builder.Build();

// Configuração do pipeline de requisições HTTP
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();