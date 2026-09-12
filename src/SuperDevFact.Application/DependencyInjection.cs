using Microsoft.Extensions.DependencyInjection;
using SuperDevFact.Application.Customers;
using SuperDevFact.Application.Invoices;
using SuperDevFact.Application.Quotes;

namespace SuperDevFact.Application;

/// <summary>
/// Enregistre les cas d'usage de l'Application. Ce sont de simples classes avec
/// injection de constructeur (pas de médiateur) : la seule chose que ce fichier fait
/// est de les rendre résolvables par le conteneur DI choisi par l'hôte (le Desktop, ici).
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services) => services
        .AddTransient<SearchCustomersUseCase>()
        .AddTransient<CreateCustomerUseCase>()
        .AddTransient<CreateQuoteUseCase>()
        .AddTransient<AddQuoteLineUseCase>()
        .AddTransient<UpdateQuoteLineUseCase>()
        .AddTransient<RemoveQuoteLineUseCase>()
        .AddTransient<ApplyGlobalDiscountUseCase>()
        .AddTransient<UpdateQuoteGeneralInfoUseCase>()
        .AddTransient<UpdateQuoteNotesUseCase>()
        .AddTransient<SendQuoteUseCase>()
        .AddTransient<AcceptQuoteUseCase>()
        .AddTransient<DeclineQuoteUseCase>()
        .AddTransient<GetQuoteForEditingUseCase>()
        .AddTransient<SearchQuotesUseCase>()
        .AddTransient<ConvertQuoteToInvoiceUseCase>()
        .AddTransient<RecordPaymentUseCase>()
        .AddTransient<GetInvoiceForViewingUseCase>()
        .AddTransient<SearchInvoicesUseCase>();
}
