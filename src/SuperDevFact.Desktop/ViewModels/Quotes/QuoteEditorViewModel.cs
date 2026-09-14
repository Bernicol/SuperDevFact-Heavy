using System.Collections.ObjectModel;
using SuperDevFact.Application.Customers;
using SuperDevFact.Application.Invoices;
using SuperDevFact.Application.Quotes;
using SuperDevFact.Desktop.Infrastructure;
using SuperDevFact.Domain.Common;
using SuperDevFact.Domain.Documents;
using SuperDevFact.Domain.Quotes;

namespace SuperDevFact.Desktop.ViewModels.Quotes;

/// <summary>
/// ViewModel de l'écran signature : l'éditeur de devis avec aperçu live. Les totaux sont
/// recalculés localement à chaque frappe via le même moteur de calcul que le Domain
/// (<see cref="DocumentTotalsCalculator"/>) pour un retour instantané sans aller-retour
/// base de données ; la persistance elle-même ne passe que par les cas d'usage
/// Application, jamais directement par EF Core ou par le Domain.
/// </summary>
public sealed class QuoteEditorViewModel : ViewModelBase
{
    private readonly ScopedUseCaseRunner _runner;
    private readonly List<Guid> _removedLineIds = new();

    public QuoteEditorViewModel(ScopedUseCaseRunner runner)
    {
        _runner = runner;
        Lines = new ObservableCollection<QuoteLineRowViewModel>();
        TaxBreakdown = new ObservableCollection<TaxBreakdownRowViewModel>();
        PaymentTermsOptions = new ObservableCollection<PaymentTermsOption>
        {
            new("Comptant", 0),
            new("30 jours net", 30),
            new("45 jours net", 45),
            new("60 jours net", 60),
        };

        AddLineCommand = new RelayCommand(_ => AddLine());
        RemoveLineCommand = new RelayCommand(p => RemoveLine((QuoteLineRowViewModel)p!));
        SaveCommand = new AsyncRelayCommand(_ => SaveAsync(), _ => IsDraft && !IsBusy);
        SendCommand = new AsyncRelayCommand(_ => SendAsync(), _ => IsDraft && !IsBusy && Lines.Count > 0);
        AcceptCommand = new AsyncRelayCommand(_ => AcceptAsync(), _ => StatusRaw == "Sent" && !IsBusy);
        DeclineCommand = new AsyncRelayCommand(_ => DeclineAsync(), _ => StatusRaw == "Sent" && !IsBusy);
        ConvertToInvoiceCommand = new AsyncRelayCommand(_ => ConvertToInvoiceAsync(), _ => CanConvertToInvoice && !IsBusy);

        RecomputeTotals();
    }

    public Guid QuoteId { get; private set; }

    private string _number = string.Empty;
    public string Number { get => _number; private set => SetField(ref _number, value); }

    private string _statusRaw = "Draft";
    public string StatusRaw
    {
        get => _statusRaw;
        private set
        {
            if (SetField(ref _statusRaw, value))
            {
                OnPropertyChanged(nameof(IsDraft));
                OnPropertyChanged(nameof(CanConvertToInvoice));
                OnPropertyChanged(nameof(WorkflowStepIndex));
            }
        }
    }

    public bool IsDraft => StatusRaw == "Draft";

    private Guid? _convertedInvoiceId;
    public Guid? ConvertedInvoiceId
    {
        get => _convertedInvoiceId;
        private set
        {
            if (SetField(ref _convertedInvoiceId, value))
            {
                OnPropertyChanged(nameof(CanConvertToInvoice));
                OnPropertyChanged(nameof(WorkflowStepIndex));
            }
        }
    }

    public bool CanConvertToInvoice => StatusRaw == "Accepted" && ConvertedInvoiceId is null;

    /// <summary>Position dans le fil Brouillon-Envoyé-Accepté-Facturé-Payé (0 à 4).</summary>
    public int WorkflowStepIndex => StatusRaw switch
    {
        "Draft" => 0,
        "Sent" => 1,
        "Accepted" => ConvertedInvoiceId is null ? 2 : 3,
        _ => 0
    };

    private Guid _customerId;
    public Guid CustomerId { get => _customerId; set => SetField(ref _customerId, value); }

    private string _customerName = string.Empty;
    public string CustomerName { get => _customerName; private set => SetField(ref _customerName, value); }

    private string _customerAddressLine = string.Empty;
    public string CustomerAddressLine { get => _customerAddressLine; private set => SetField(ref _customerAddressLine, value); }

    private string _customerContactLine = string.Empty;
    public string CustomerContactLine { get => _customerContactLine; private set => SetField(ref _customerContactLine, value); }

    private DateTime _issueDate = DateTime.Today;
    public DateTime IssueDate { get => _issueDate; set => SetField(ref _issueDate, value); }

    private int _validityDays = 30;
    public int ValidityDays { get => _validityDays; set => SetField(ref _validityDays, value); }

    public ObservableCollection<PaymentTermsOption> PaymentTermsOptions { get; }

    private int _selectedPaymentTermsDays = 30;
    public int SelectedPaymentTermsDays { get => _selectedPaymentTermsDays; set => SetField(ref _selectedPaymentTermsDays, value); }

    private string? _clientReference;
    public string? ClientReference { get => _clientReference; set => SetField(ref _clientReference, value); }

    private string? _internalNotes;
    public string? InternalNotes { get => _internalNotes; set => SetField(ref _internalNotes, value); }

    private string? _clientMessage;
    public string? ClientMessage { get => _clientMessage; set => SetField(ref _clientMessage, value); }

    public ObservableCollection<QuoteLineRowViewModel> Lines { get; }

    private decimal _globalDiscountRatePercent;
    public decimal GlobalDiscountRatePercent
    {
        get => _globalDiscountRatePercent;
        set { if (SetField(ref _globalDiscountRatePercent, value)) RecomputeTotals(); }
    }

    private decimal _subtotalHt;
    public decimal SubtotalHt { get => _subtotalHt; private set => SetField(ref _subtotalHt, value); }

    private decimal _globalDiscountAmount;
    public decimal GlobalDiscountAmount { get => _globalDiscountAmount; private set => SetField(ref _globalDiscountAmount, value); }

    private decimal _netHt;
    public decimal NetHt { get => _netHt; private set => SetField(ref _netHt, value); }

    private decimal _totalTax;
    public decimal TotalTax { get => _totalTax; private set => SetField(ref _totalTax, value); }

    private decimal _totalTtc;
    public decimal TotalTtc { get => _totalTtc; private set => SetField(ref _totalTtc, value); }

    public ObservableCollection<TaxBreakdownRowViewModel> TaxBreakdown { get; }

    private bool _isBusy;
    public bool IsBusy { get => _isBusy; private set => SetField(ref _isBusy, value); }

    private string? _lastCreatedInvoiceNumber;
    public string? LastCreatedInvoiceNumber { get => _lastCreatedInvoiceNumber; private set => SetField(ref _lastCreatedInvoiceNumber, value); }

    private Guid? _lastCreatedInvoiceId;
    public Guid? LastCreatedInvoiceId { get => _lastCreatedInvoiceId; private set => SetField(ref _lastCreatedInvoiceId, value); }

    private DateTime? _lastSavedAtUtc;
    public DateTime? LastSavedAtUtc { get => _lastSavedAtUtc; private set => SetField(ref _lastSavedAtUtc, value); }

    public RelayCommand AddLineCommand { get; }
    public RelayCommand RemoveLineCommand { get; }
    public AsyncRelayCommand SaveCommand { get; }
    public AsyncRelayCommand SendCommand { get; }
    public AsyncRelayCommand AcceptCommand { get; }
    public AsyncRelayCommand DeclineCommand { get; }
    public AsyncRelayCommand ConvertToInvoiceCommand { get; }

    public async Task InitializeAsync()
    {
        var customers = await _runner.RunAsync<SearchCustomersUseCase, IReadOnlyList<CustomerSummaryDto>>(
            uc => uc.ExecuteAsync(null));

        var customer = customers.FirstOrDefault();
        if (customer is null)
        {
            customer = await _runner.RunAsync<CreateCustomerUseCase, CustomerSummaryDto>(uc => uc.ExecuteAsync(
                new CreateCustomerRequest(
                    "Dupont Architecture", "12 rue des Arts", "75011", "Paris", "France",
                    "Sophie Martin", "s.martin@dupont-architecture.fr", "01 42 56 78 90",
                    "812 345 678", "FR12345678901", 30)));
        }

        var existingDrafts = await _runner.RunAsync<SearchQuotesUseCase, IReadOnlyList<QuoteSummaryDto>>(
            uc => uc.ExecuteAsync(null, QuoteStatus.Draft));

        QuoteDetailsDto dto;
        if (existingDrafts.Count > 0)
        {
            dto = await _runner.RunAsync<GetQuoteForEditingUseCase, QuoteDetailsDto>(
                uc => uc.ExecuteAsync(existingDrafts[0].Id));
        }
        else
        {
            dto = await _runner.RunAsync<CreateQuoteUseCase, QuoteDetailsDto>(
                uc => uc.ExecuteAsync(new CreateQuoteRequest(customer.Id)));
        }

        LoadFromDto(dto);
    }

    public async Task LoadAsync(Guid quoteId)
    {
        var dto = await _runner.RunAsync<GetQuoteForEditingUseCase, QuoteDetailsDto>(uc => uc.ExecuteAsync(quoteId));
        LoadFromDto(dto);
    }

    /// <summary>Crée systématiquement un nouveau devis en brouillon (contrairement à <see cref="InitializeAsync"/>, qui réutilise un brouillon existant).</summary>
    public async Task CreateNewAsync(Guid customerId)
    {
        var dto = await _runner.RunAsync<CreateQuoteUseCase, QuoteDetailsDto>(
            uc => uc.ExecuteAsync(new CreateQuoteRequest(customerId)));
        LoadFromDto(dto);
    }

    private void LoadFromDto(QuoteDetailsDto dto)
    {
        QuoteId = dto.Id;
        Number = dto.Number;
        StatusRaw = dto.Status;
        ConvertedInvoiceId = dto.ConvertedInvoiceId;

        CustomerId = dto.Customer.Id;
        CustomerName = dto.Customer.CompanyName;
        CustomerAddressLine = $"{dto.Customer.Street}, {dto.Customer.PostalCode} {dto.Customer.City}";
        CustomerContactLine = string.IsNullOrWhiteSpace(dto.Customer.ContactName)
            ? dto.Customer.ContactEmail
            : $"{dto.Customer.ContactName} · {dto.Customer.ContactEmail}";

        IssueDate = dto.IssueDate.ToDateTime(TimeOnly.MinValue);
        ValidityDays = dto.ValidityDays;
        ClientReference = dto.ClientReference;
        InternalNotes = dto.InternalNotes;
        ClientMessage = dto.ClientMessage;
        GlobalDiscountRatePercent = dto.GlobalDiscountRatePercent;

        var matchingOption = PaymentTermsOptions.FirstOrDefault(o => dto.PaymentTermsLabel.Contains(o.Days.ToString()))
            ?? PaymentTermsOptions.First();
        SelectedPaymentTermsDays = matchingOption.Days;

        foreach (var row in Lines)
            row.PropertyChanged -= OnLinePropertyChanged;
        Lines.Clear();
        _removedLineIds.Clear();

        foreach (var line in dto.Lines.OrderBy(l => l.Position))
        {
            var row = new QuoteLineRowViewModel
            {
                Id = line.Id,
                Position = line.Position,
                Description = line.Description,
                Detail = line.Detail,
                Quantity = line.Quantity,
                Unit = line.Unit,
                UnitPriceHt = line.UnitPriceHt,
                DiscountRatePercent = line.DiscountRatePercent,
                TaxRatePercent = line.TaxRatePercent,
                IsDirty = false,
            };
            row.PropertyChanged += OnLinePropertyChanged;
            Lines.Add(row);
        }

        LastCreatedInvoiceNumber = null;
        RecomputeTotals();
    }

    private void AddLine()
    {
        var row = new QuoteLineRowViewModel { Position = Lines.Count, TaxRatePercent = 20m, Quantity = 1m, Unit = "unité" };
        row.PropertyChanged += OnLinePropertyChanged;
        Lines.Add(row);
        RecomputeTotals();
    }

    private void RemoveLine(QuoteLineRowViewModel row)
    {
        row.PropertyChanged -= OnLinePropertyChanged;
        Lines.Remove(row);
        if (row.Id is { } id)
            _removedLineIds.Add(id);
        RecomputeTotals();
    }

    private void OnLinePropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e) => RecomputeTotals();

    private void RecomputeTotals()
    {
        var domainLines = new List<DocumentLine>();

        foreach (var row in Lines)
        {
            if (!TryBuildDomainLine(row, out var line))
            {
                row.LineTotalHt = 0m;
                continue;
            }

            row.LineTotalHt = line.NetAmountHt.Amount;
            domainLines.Add(line);
        }

        var globalDiscount = TryCreatePercentage(GlobalDiscountRatePercent);
        var totals = DocumentTotalsCalculator.Calculate(domainLines, globalDiscount);

        SubtotalHt = totals.SubtotalHt.Amount;
        GlobalDiscountAmount = totals.GlobalDiscountAmount.Amount;
        NetHt = totals.NetHt.Amount;
        TotalTax = totals.TotalTax.Amount;
        TotalTtc = totals.TotalTtc.Amount;

        TaxBreakdown.Clear();
        foreach (var breakdown in totals.TaxBreakdown)
            TaxBreakdown.Add(new TaxBreakdownRowViewModel($"TVA ({breakdown.TaxRate.Rate})", breakdown.TaxAmount.Amount));
    }

    private static bool TryBuildDomainLine(QuoteLineRowViewModel row, out DocumentLine line)
    {
        line = null!;
        if (string.IsNullOrWhiteSpace(row.Description) || row.Quantity <= 0 || string.IsNullOrWhiteSpace(row.Unit))
            return false;

        try
        {
            line = new QuoteLine(
                row.Description, row.Detail, row.Quantity, row.Unit,
                new Money(row.UnitPriceHt), TryCreatePercentage(row.DiscountRatePercent), TaxRate.FromPercent(row.TaxRatePercent));
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static Percentage TryCreatePercentage(decimal value)
    {
        try { return new Percentage(Math.Clamp(value, 0m, 100m)); }
        catch { return Percentage.Zero; }
    }

    private async Task SaveAsync()
    {
        IsBusy = true;
        try
        {
            foreach (var lineId in _removedLineIds)
                await _runner.RunAsync<RemoveQuoteLineUseCase>(uc => uc.ExecuteAsync(QuoteId, lineId));
            _removedLineIds.Clear();

            foreach (var row in Lines.Where(r => r.IsNew))
            {
                var updated = await _runner.RunAsync<AddQuoteLineUseCase, QuoteDetailsDto>(uc => uc.ExecuteAsync(
                    QuoteId, new QuoteLineRequest(row.Description, row.Detail, row.Quantity, row.Unit, row.UnitPriceHt, row.DiscountRatePercent, row.TaxRatePercent)));
                var persisted = updated.Lines.OrderBy(l => l.Position).Last();
                row.Id = persisted.Id;
                row.IsDirty = false;
            }

            foreach (var row in Lines.Where(r => !r.IsNew && r.IsDirty))
            {
                await _runner.RunAsync<UpdateQuoteLineUseCase, QuoteDetailsDto>(uc => uc.ExecuteAsync(
                    QuoteId, row.Id!.Value, new QuoteLineRequest(row.Description, row.Detail, row.Quantity, row.Unit, row.UnitPriceHt, row.DiscountRatePercent, row.TaxRatePercent)));
                row.IsDirty = false;
            }

            await _runner.RunAsync<ApplyGlobalDiscountUseCase, QuoteDetailsDto>(
                uc => uc.ExecuteAsync(QuoteId, GlobalDiscountRatePercent));

            await _runner.RunAsync<UpdateQuoteGeneralInfoUseCase, QuoteDetailsDto>(uc => uc.ExecuteAsync(
                QuoteId, new UpdateQuoteGeneralInfoRequest(
                    CustomerId, DateOnly.FromDateTime(IssueDate), ValidityDays, SelectedPaymentTermsDays, ClientReference)));

            var final = await _runner.RunAsync<UpdateQuoteNotesUseCase, QuoteDetailsDto>(
                uc => uc.ExecuteAsync(QuoteId, InternalNotes, ClientMessage));

            LoadFromDto(final);
            LastSavedAtUtc = DateTime.UtcNow;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task SendAsync()
    {
        IsBusy = true;
        try
        {
            await SaveAsync();
            var dto = await _runner.RunAsync<SendQuoteUseCase, QuoteDetailsDto>(uc => uc.ExecuteAsync(QuoteId));
            LoadFromDto(dto);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task AcceptAsync()
    {
        IsBusy = true;
        try
        {
            var dto = await _runner.RunAsync<AcceptQuoteUseCase, QuoteDetailsDto>(uc => uc.ExecuteAsync(QuoteId));
            LoadFromDto(dto);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task DeclineAsync()
    {
        IsBusy = true;
        try
        {
            var dto = await _runner.RunAsync<DeclineQuoteUseCase, QuoteDetailsDto>(uc => uc.ExecuteAsync(QuoteId));
            LoadFromDto(dto);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task ConvertToInvoiceAsync()
    {
        IsBusy = true;
        try
        {
            var invoice = await _runner.RunAsync<ConvertQuoteToInvoiceUseCase, InvoiceDetailsDto>(uc => uc.ExecuteAsync(QuoteId));
            LastCreatedInvoiceNumber = invoice.Number;
            LastCreatedInvoiceId = invoice.Id;

            var dto = await _runner.RunAsync<GetQuoteForEditingUseCase, QuoteDetailsDto>(uc => uc.ExecuteAsync(QuoteId));
            LoadFromDto(dto);
            LastCreatedInvoiceNumber = invoice.Number; // LoadFromDto le remet à null : on le restaure après
        }
        finally
        {
            IsBusy = false;
        }
    }
}
