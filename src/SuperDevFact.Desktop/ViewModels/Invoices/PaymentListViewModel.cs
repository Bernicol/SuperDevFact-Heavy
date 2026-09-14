using System.Collections.ObjectModel;
using SuperDevFact.Application.Invoices;
using SuperDevFact.Desktop.Infrastructure;

namespace SuperDevFact.Desktop.ViewModels.Invoices;

/// <summary>Liste de tous les paiements enregistrés, cliquables pour ouvrir la facture correspondante.</summary>
public sealed class PaymentListViewModel(ScopedUseCaseRunner runner) : ViewModelBase
{
    public event Action<Guid>? InvoiceSelected;

    public ObservableCollection<PaymentListItemDto> Payments { get; } = new();

    private RelayCommand? _selectPaymentCommand;
    public RelayCommand SelectPayment => _selectPaymentCommand ??= new RelayCommand(p =>
    {
        if (p is PaymentListItemDto payment)
            InvoiceSelected?.Invoke(payment.InvoiceId);
    });

    public async Task InitializeAsync()
    {
        var payments = await runner.RunAsync<ListPaymentsUseCase, IReadOnlyList<PaymentListItemDto>>(uc => uc.ExecuteAsync());

        Payments.Clear();
        foreach (var payment in payments)
            Payments.Add(payment);
    }
}
