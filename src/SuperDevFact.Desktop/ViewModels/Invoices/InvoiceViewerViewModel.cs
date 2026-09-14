using System.Collections.ObjectModel;
using SuperDevFact.Application.Invoices;
using SuperDevFact.Desktop.Infrastructure;
using SuperDevFact.Domain.Invoices;

namespace SuperDevFact.Desktop.ViewModels.Invoices;

/// <summary>
/// ViewModel de l'atelier en mode facture : les lignes sont en lecture seule (une
/// facture émise ne se modifie plus, garantie du domaine), les actions possibles sont
/// l'enregistrement d'un paiement et l'annulation.
/// </summary>
public sealed class InvoiceViewerViewModel(ScopedUseCaseRunner runner) : ViewModelBase
{
    public Guid InvoiceId { get; private set; }

    private string _number = string.Empty;
    public string Number { get => _number; private set => SetField(ref _number, value); }

    private string _statusRaw = "Sent";
    public string StatusRaw { get => _statusRaw; private set { if (SetField(ref _statusRaw, value)) OnPropertyChanged(nameof(WorkflowStepIndex)); } }

    private bool _isOverdue;
    public bool IsOverdue { get => _isOverdue; private set => SetField(ref _isOverdue, value); }

    /// <summary>0 à 4 : ici toujours au moins "Facturé" (3), "Payé" (4) si soldée.</summary>
    public int WorkflowStepIndex => StatusRaw == "Paid" ? 4 : 3;

    public string DisplayStatus => IsOverdue && StatusRaw != "Paid" ? "Overdue" : StatusRaw;

    private string _customerName = string.Empty;
    public string CustomerName { get => _customerName; private set => SetField(ref _customerName, value); }

    private string _customerAddressLine = string.Empty;
    public string CustomerAddressLine { get => _customerAddressLine; private set => SetField(ref _customerAddressLine, value); }

    private DateOnly _issueDate;
    public DateOnly IssueDate { get => _issueDate; private set => SetField(ref _issueDate, value); }

    private DateOnly _dueDate;
    public DateOnly DueDate { get => _dueDate; private set => SetField(ref _dueDate, value); }

    public ObservableCollection<InvoiceLineDto> Lines { get; } = new();
    public ObservableCollection<PaymentDto> Payments { get; } = new();

    private decimal _subtotalHt, _totalTax, _totalTtc, _amountPaid, _balance;
    public decimal SubtotalHt { get => _subtotalHt; private set => SetField(ref _subtotalHt, value); }
    public decimal TotalTax { get => _totalTax; private set => SetField(ref _totalTax, value); }
    public decimal TotalTtc { get => _totalTtc; private set => SetField(ref _totalTtc, value); }
    public decimal AmountPaid { get => _amountPaid; private set => SetField(ref _amountPaid, value); }
    public decimal Balance { get => _balance; private set => SetField(ref _balance, value); }

    private decimal _newPaymentAmount;
    public decimal NewPaymentAmount { get => _newPaymentAmount; set => SetField(ref _newPaymentAmount, value); }

    private DateTime _newPaymentDate = DateTime.Today;
    public DateTime NewPaymentDate { get => _newPaymentDate; set => SetField(ref _newPaymentDate, value); }

    public IReadOnlyList<PaymentMethod> PaymentMethods { get; } = Enum.GetValues<PaymentMethod>();

    private PaymentMethod _newPaymentMethod = PaymentMethod.BankTransfer;
    public PaymentMethod NewPaymentMethod { get => _newPaymentMethod; set => SetField(ref _newPaymentMethod, value); }

    private bool _isBusy;
    public bool IsBusy { get => _isBusy; private set => SetField(ref _isBusy, value); }

    public bool CanRecordPayment => StatusRaw is "Sent" or "PartiallyPaid";

    private AsyncRelayCommand? _recordPaymentCommand;
    public AsyncRelayCommand RecordPaymentCommand => _recordPaymentCommand ??= new AsyncRelayCommand(_ => RecordPaymentAsync(), _ => CanRecordPayment && !IsBusy);

    public async Task LoadAsync(Guid invoiceId)
    {
        var dto = await runner.RunAsync<GetInvoiceForViewingUseCase, InvoiceDetailsDto>(uc => uc.ExecuteAsync(invoiceId));
        LoadFromDto(dto);
    }

    private void LoadFromDto(InvoiceDetailsDto dto)
    {
        InvoiceId = dto.Id;
        Number = dto.Number;
        StatusRaw = dto.Status;
        IsOverdue = dto.IsOverdue;
        CustomerName = dto.Customer.CompanyName;
        CustomerAddressLine = $"{dto.Customer.Street}, {dto.Customer.PostalCode} {dto.Customer.City}";
        IssueDate = dto.IssueDate;
        DueDate = dto.DueDate;

        Lines.Clear();
        foreach (var line in dto.Lines.OrderBy(l => l.Position))
            Lines.Add(line);

        Payments.Clear();
        foreach (var payment in dto.Payments)
            Payments.Add(payment);

        SubtotalHt = dto.Totals.SubtotalHt;
        TotalTax = dto.Totals.TotalTax;
        TotalTtc = dto.Totals.TotalTtc;
        AmountPaid = dto.AmountPaid;
        Balance = dto.Balance;

        NewPaymentAmount = Balance;
        OnPropertyChanged(nameof(DisplayStatus));
        OnPropertyChanged(nameof(CanRecordPayment));
    }

    private async Task RecordPaymentAsync()
    {
        IsBusy = true;
        try
        {
            var dto = await runner.RunAsync<RecordPaymentUseCase, InvoiceDetailsDto>(uc => uc.ExecuteAsync(
                InvoiceId, new RecordPaymentRequest(NewPaymentAmount, DateOnly.FromDateTime(NewPaymentDate), NewPaymentMethod, null)));
            LoadFromDto(dto);
        }
        finally
        {
            IsBusy = false;
        }
    }
}
