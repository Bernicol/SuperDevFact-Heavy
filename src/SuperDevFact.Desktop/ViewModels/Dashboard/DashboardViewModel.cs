using System.Collections.ObjectModel;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using Material.Icons;
using SkiaSharp;
using SuperDevFact.Application.Invoices;
using SuperDevFact.Application.Quotes;
using SuperDevFact.Desktop.Infrastructure;

namespace SuperDevFact.Desktop.ViewModels.Dashboard;

public enum RevenuePeriod { Days30, Quarter, Year }

/// <summary>
/// Tableau de bord : la porte d'entrée du logiciel. Compteurs (et leur évolution réelle
/// sur 30 jours) calculés à partir des données existantes, actions rapides (nouveau
/// devis) et liste des derniers devis, cliquable pour reprendre l'édition dans l'atelier.
/// </summary>
public sealed class DashboardViewModel(ScopedUseCaseRunner runner) : ViewModelBase
{
    public event Action<QuoteSummaryDto>? QuoteSelected;
    public event Action? NewQuoteRequested;

    private decimal _totalRevenueCollected;
    public decimal TotalRevenueCollected { get => _totalRevenueCollected; private set => SetField(ref _totalRevenueCollected, value); }

    private RelayCommand? _newQuoteCommand;
    public RelayCommand NewQuoteCommand => _newQuoteCommand ??= new RelayCommand(_ => NewQuoteRequested?.Invoke());

    private MetricTrend _draftTrend = new(0, 0, "Flat");
    public MetricTrend DraftTrend { get => _draftTrend; private set => SetField(ref _draftTrend, value); }

    private MetricTrend _sentTrend = new(0, 0, "Flat");
    public MetricTrend SentTrend { get => _sentTrend; private set => SetField(ref _sentTrend, value); }

    private MetricTrend _acceptedTrend = new(0, 0, "Flat");
    public MetricTrend AcceptedTrend { get => _acceptedTrend; private set => SetField(ref _acceptedTrend, value); }

    private MetricTrend _invoicedTrend = new(0, 0, "Flat");
    public MetricTrend InvoicedTrend { get => _invoicedTrend; private set => SetField(ref _invoicedTrend, value); }

    private MetricTrend _unpaidTrend = new(0, 0, "Flat");
    public MetricTrend UnpaidTrend { get => _unpaidTrend; private set => SetField(ref _unpaidTrend, value); }

    private MetricTrend _partiallyPaidTrend = new(0, 0, "Flat");
    public MetricTrend PartiallyPaidTrend { get => _partiallyPaidTrend; private set => SetField(ref _partiallyPaidTrend, value); }

    private MetricTrend _paidTrend = new(0, 0, "Flat");
    public MetricTrend PaidTrend { get => _paidTrend; private set => SetField(ref _paidTrend, value); }

    private MetricTrend _overdueTrend = new(0, 0, "Flat");
    public MetricTrend OverdueTrend { get => _overdueTrend; private set => SetField(ref _overdueTrend, value); }

    private int _invoiceTotalCount;
    public int InvoiceTotalCount { get => _invoiceTotalCount; private set => SetField(ref _invoiceTotalCount, value); }

    public ObservableCollection<MetricCardViewModel> MetricCards { get; } = new();

    public ObservableCollection<InvoiceStatusShareViewModel> InvoiceStatusShares { get; } = new();

    public ObservableCollection<QuoteSummaryDto> RecentQuotes { get; } = new();

    private ISeries[] _quoteStatusSeries = Array.Empty<ISeries>();
    public ISeries[] QuoteStatusSeries { get => _quoteStatusSeries; private set => SetField(ref _quoteStatusSeries, value); }

    public Axis[] QuoteStatusXAxes { get; } =
    [
        new Axis
        {
            Labels = ["Brouillons", "Envoyés", "Acceptés", "Facturés"],
            LabelsRotation = 0,
            TextSize = 13,
            SeparatorsPaint = null,
            ForceStepToMin = true,
        }
    ];

    // Axe Y masqué : la valeur de chaque barre est déjà lisible via son étiquette,
    // un axe numérique n'apporterait que du bruit visuel pour des compteurs aussi petits.
    public Axis[] QuoteStatusYAxes { get; } =
    [
        new Axis { IsVisible = false, MinLimit = 0 }
    ];

    private ISeries[] _invoiceStatusSeries = Array.Empty<ISeries>();
    public ISeries[] InvoiceStatusSeries { get => _invoiceStatusSeries; private set => SetField(ref _invoiceStatusSeries, value); }

    // --- Évolution du CA (report de la version web) : dégradé sous une courbe, avec
    // sélecteur de période. Toujours calculé à partir des vrais paiements enregistrés.
    private IReadOnlyList<PaymentListItemDto> _payments = Array.Empty<PaymentListItemDto>();

    private RevenuePeriod _selectedPeriod = RevenuePeriod.Days30;
    public RevenuePeriod SelectedPeriod
    {
        get => _selectedPeriod;
        set { if (SetField(ref _selectedPeriod, value)) { BuildRevenueChart(); OnPropertyChanged(nameof(IsDays30)); OnPropertyChanged(nameof(IsQuarter)); OnPropertyChanged(nameof(IsYear)); } }
    }

    public bool IsDays30 => SelectedPeriod == RevenuePeriod.Days30;
    public bool IsQuarter => SelectedPeriod == RevenuePeriod.Quarter;
    public bool IsYear => SelectedPeriod == RevenuePeriod.Year;

    private RelayCommand? _setPeriodCommand;
    public RelayCommand SetPeriodCommand => _setPeriodCommand ??= new RelayCommand(p => SelectedPeriod = (RevenuePeriod)p!);

    private ISeries[] _revenueSeries = Array.Empty<ISeries>();
    public ISeries[] RevenueSeries { get => _revenueSeries; private set => SetField(ref _revenueSeries, value); }

    private Axis[] _revenueXAxes = [new Axis { IsVisible = false }];
    public Axis[] RevenueXAxes { get => _revenueXAxes; private set => SetField(ref _revenueXAxes, value); }

    public Axis[] RevenueYAxes { get; } = [new Axis { IsVisible = false }];

    // --- Taux de transformation devis → facture : donut avec pourcentage au centre.
    private decimal _conversionRatePercent;
    public decimal ConversionRatePercent { get => _conversionRatePercent; private set => SetField(ref _conversionRatePercent, value); }

    private ISeries[] _conversionSeries = Array.Empty<ISeries>();
    public ISeries[] ConversionSeries { get => _conversionSeries; private set => SetField(ref _conversionSeries, value); }

    /// <summary>Simulation décorative : un aperçu de gestion de rappels personnels (aucune donnée réelle, aucune persistance).</summary>
    public ObservableCollection<TaskPulseItemViewModel> TaskPulseItems { get; } =
    [
        new("Relancer Camping du Lac Bleu (devis en attente)", MaterialIconKind.BellRingOutline, "#3B82F6"),
        new("Revoir les prix sur la facture en retard", MaterialIconKind.FileEditOutline, "#FBBF24"),
        new("Organiser une réunion sur la TVA installation", MaterialIconKind.CalendarClockOutline, "#A78BFA"),
        new("Préparer le prochain devis Résidence Les Tilleuls", MaterialIconKind.FileEditOutline, "#34D399"),
    ];

    private RelayCommand? _selectQuoteCommand;
    public RelayCommand SelectQuote => _selectQuoteCommand ??= new RelayCommand(p =>
    {
        if (p is QuoteSummaryDto quote)
            QuoteSelected?.Invoke(quote);
    });

    public async Task InitializeAsync()
    {
        var quotes = await runner.RunAsync<SearchQuotesUseCase, IReadOnlyList<QuoteSummaryDto>>(uc => uc.ExecuteAsync(null, null));
        var invoices = await runner.RunAsync<SearchInvoicesUseCase, IReadOnlyList<InvoiceSummaryDto>>(uc => uc.ExecuteAsync(null, null));
        var payments = await runner.RunAsync<ListPaymentsUseCase, IReadOnlyList<PaymentListItemDto>>(uc => uc.ExecuteAsync());
        _payments = payments;

        TotalRevenueCollected = payments.Sum(p => p.Amount);

        var today = DateOnly.FromDateTime(DateTime.Today);
        var since30Days = today.AddDays(-30);
        var since60Days = today.AddDays(-60);

        DraftTrend = ComputeQuoteTrend(quotes, "Draft", since30Days, since60Days);
        SentTrend = ComputeQuoteTrend(quotes, "Sent", since30Days, since60Days);
        AcceptedTrend = ComputeQuoteTrend(quotes, "Accepted", since30Days, since60Days);
        InvoicedTrend = MetricTrend.Compute(
            invoices.Count(i => i.IssueDate >= since30Days),
            invoices.Count(i => i.IssueDate >= since60Days && i.IssueDate < since30Days),
            invoices.Count);

        UnpaidTrend = ComputeInvoiceTrend(invoices, "Sent", since30Days, since60Days);
        PartiallyPaidTrend = ComputeInvoiceTrend(invoices, "PartiallyPaid", since30Days, since60Days);
        PaidTrend = ComputeInvoiceTrend(invoices, "Paid", since30Days, since60Days);

        var overdueCount = invoices.Count(i => i.IsOverdue);
        OverdueTrend = new MetricTrend(overdueCount, 0, "Flat");

        MetricCards.Clear();
        MetricCards.Add(new MetricCardViewModel("Brouillons", MaterialIconKind.FileDocumentOutline, "#94A3B8", DraftTrend));
        MetricCards.Add(new MetricCardViewModel("Devis envoyés", MaterialIconKind.SendOutline, "#3B82F6", SentTrend));
        MetricCards.Add(new MetricCardViewModel("Devis acceptés", MaterialIconKind.CheckCircleOutline, "#34D399", AcceptedTrend));
        MetricCards.Add(new MetricCardViewModel("Devis facturés", MaterialIconKind.ReceiptTextOutline, "#A78BFA", InvoicedTrend));
        MetricCards.Add(new MetricCardViewModel("Factures en attente", MaterialIconKind.ClockOutline, "#FBBF24", UnpaidTrend));
        MetricCards.Add(new MetricCardViewModel("Partiellement payées", MaterialIconKind.CashClock, "#FBBF24", PartiallyPaidTrend));
        MetricCards.Add(new MetricCardViewModel("Payées", MaterialIconKind.CashCheck, "#34D399", PaidTrend));
        MetricCards.Add(new MetricCardViewModel("En retard", MaterialIconKind.AlertCircleOutline, "#FB7185", OverdueTrend));

        RecentQuotes.Clear();
        foreach (var quote in quotes.OrderByDescending(q => q.IssueDate).Take(8))
            RecentQuotes.Add(quote);

        BuildCharts(invoices);
        BuildRevenueChart();
        BuildConversionChart(quotes);
    }

    private static MetricTrend ComputeQuoteTrend(IReadOnlyList<QuoteSummaryDto> quotes, string status, DateOnly since30, DateOnly since60)
    {
        var matching = quotes.Where(q => q.Status == status).ToList();
        return MetricTrend.Compute(
            matching.Count(q => q.IssueDate >= since30),
            matching.Count(q => q.IssueDate >= since60 && q.IssueDate < since30),
            matching.Count);
    }

    private static MetricTrend ComputeInvoiceTrend(IReadOnlyList<InvoiceSummaryDto> invoices, string status, DateOnly since30, DateOnly since60)
    {
        var matching = invoices.Where(i => i.Status == status).ToList();
        return MetricTrend.Compute(
            matching.Count(i => i.IssueDate >= since30),
            matching.Count(i => i.IssueDate >= since60 && i.IssueDate < since30),
            matching.Count);
    }

    private void BuildCharts(IReadOnlyList<InvoiceSummaryDto> invoices)
    {
        QuoteStatusSeries =
        [
            Bar(DraftTrend.Value, 0, "#94A3B8", "Brouillons"),
            Bar(SentTrend.Value, 1, "#3B82F6", "Envoyés"),
            Bar(AcceptedTrend.Value, 2, "#34D399", "Acceptés"),
            Bar(InvoicedTrend.Value, 3, "#A78BFA", "Facturés"),
        ];

        // Répartition mutuellement exclusive pour éviter tout double comptage dans le donut
        // (une facture en retard peut être "Envoyée" ou "Partiellement payée" à la base).
        var overdue = invoices.Count(i => i.IsOverdue);
        var sentOnTime = invoices.Count(i => i.Status == "Sent" && !i.IsOverdue);
        var partiallyPaidOnTime = invoices.Count(i => i.Status == "PartiallyPaid" && !i.IsOverdue);
        var paid = invoices.Count(i => i.Status == "Paid");

        InvoiceTotalCount = overdue + sentOnTime + partiallyPaidOnTime + paid;

        InvoiceStatusSeries =
        [
            Slice(paid, "#34D399", "Payées"),
            Slice(partiallyPaidOnTime, "#FBBF24", "Partiellement payées"),
            Slice(sentOnTime, "#3B82F6", "En attente"),
            Slice(overdue, "#FB7185", "En retard"),
        ];

        InvoiceStatusShares.Clear();
        var total = Math.Max(InvoiceTotalCount, 1);
        InvoiceStatusShares.Add(new InvoiceStatusShareViewModel("Payées", "#34D399", paid, paid * 100m / total));
        InvoiceStatusShares.Add(new InvoiceStatusShareViewModel("Partiellement payées", "#FBBF24", partiallyPaidOnTime, partiallyPaidOnTime * 100m / total));
        InvoiceStatusShares.Add(new InvoiceStatusShareViewModel("En attente", "#3B82F6", sentOnTime, sentOnTime * 100m / total));
        InvoiceStatusShares.Add(new InvoiceStatusShareViewModel("En retard", "#FB7185", overdue, overdue * 100m / total));
    }

    /// <summary>Courbe d'évolution du CA encaissé (cumulé), reconstruite à chaque changement de période.</summary>
    private void BuildRevenueChart()
    {
        var (days, bucketCount) = SelectedPeriod switch
        {
            RevenuePeriod.Quarter => (90, 13),
            RevenuePeriod.Year => (365, 12),
            _ => (30, 30),
        };

        var today = DateOnly.FromDateTime(DateTime.Today);
        var start = today.AddDays(-days);
        var bucketSizeDays = days / (double)bucketCount;

        // On n'affiche qu'une étiquette sur N (les autres restent vides) : avec 30 points
        // journaliers, afficher chaque date ferait un fouillis illisible sur l'axe.
        var labelStep = Math.Max(1, bucketCount / 6);

        var values = new double[bucketCount];
        var labels = new string[bucketCount];
        for (var i = 0; i < bucketCount; i++)
        {
            var bucketEnd = start.AddDays((int)Math.Round((i + 1) * bucketSizeDays));
            values[i] = (double)_payments.Where(p => p.PaymentDate <= bucketEnd).Sum(p => p.Amount);
            var showLabel = i % labelStep == 0 || i == bucketCount - 1;
            labels[i] = showLabel ? (SelectedPeriod == RevenuePeriod.Year ? bucketEnd.ToString("MMM") : bucketEnd.ToString("dd/MM")) : string.Empty;
        }

        var blue = SKColor.Parse("#3B82F6");
        RevenueSeries =
        [
            new LineSeries<double>
            {
                Values = values,
                Name = "Chiffre d'affaires",
                Fill = new LinearGradientPaint([blue.WithAlpha(90), blue.WithAlpha(0)], new SKPoint(0.5f, 0f), new SKPoint(0.5f, 1f)),
                Stroke = new SolidColorPaint(blue, 3),
                GeometrySize = 0,
                LineSmoothness = 0.5,
            }
        ];

        RevenueXAxes =
        [
            new Axis
            {
                Labels = labels,
                TextSize = 10,
                LabelsPaint = new SolidColorPaint(SKColor.Parse("#94A3B8")),
                SeparatorsPaint = null,
            }
        ];
    }

    /// <summary>Taux de transformation devis (hors brouillons) → facture, en donut avec pourcentage central.</summary>
    private void BuildConversionChart(IReadOnlyList<QuoteSummaryDto> quotes)
    {
        var eligible = quotes.Where(q => q.Status != "Draft").ToList();
        var converted = eligible.Count(q => q.ConvertedInvoiceId is not null);
        ConversionRatePercent = eligible.Count == 0 ? 0 : Math.Round(converted * 100m / eligible.Count, 1);

        var blue = SKColor.Parse("#3B82F6");
        ConversionSeries =
        [
            new PieSeries<double>
            {
                Values = [(double)ConversionRatePercent],
                Fill = new SolidColorPaint(blue),
                InnerRadius = 60,
                MaxRadialColumnWidth = 16,
                DataLabelsPaint = null,
            },
            new PieSeries<double>
            {
                Values = [(double)(100 - ConversionRatePercent)],
                Fill = new SolidColorPaint(SKColor.Parse("#1FFFFFFF")),
                InnerRadius = 60,
                MaxRadialColumnWidth = 16,
                DataLabelsPaint = null,
            },
        ];
    }

    private static ISeries Bar(int value, int position, string hexColor, string name)
    {
        var values = new double?[4];
        values[position] = value;

        var baseColor = SKColor.Parse(hexColor);

        return new ColumnSeries<double?>
        {
            Values = values,
            Name = name,
            // Dégradé vertical (couleur pleine en haut, plus douce en bas) pour un rendu
            // moins "à plat" que des barres unies.
            Fill = new LinearGradientPaint(
                [baseColor, baseColor.WithAlpha(120)],
                new SKPoint(0.5f, 0f), new SKPoint(0.5f, 1f)),
            MaxBarWidth = 54,
            Rx = 12,
            Ry = 12,
            DataLabelsPaint = new SolidColorPaint(SKColor.Parse("#F1F5F9")),
            DataLabelsSize = 15,
            DataLabelsPosition = LiveChartsCore.Measure.DataLabelsPosition.Top,
            DataLabelsFormatter = point => point.Coordinate.PrimaryValue > 0 ? point.Coordinate.PrimaryValue.ToString("N0") : string.Empty,
        };
    }

    private static ISeries Slice(int value, string hexColor, string name)
    {
        var baseColor = SKColor.Parse(hexColor);

        return new PieSeries<double>
        {
            Values = [value],
            Name = name,
            Fill = new LinearGradientPaint([baseColor, baseColor.WithAlpha(180)], new SKPoint(0, 0), new SKPoint(1, 1)),
            InnerRadius = 62,
            HoverPushout = 14,
            DataLabelsPaint = value > 0 ? new SolidColorPaint(SKColors.White) : null,
            DataLabelsSize = 14,
            DataLabelsPosition = LiveChartsCore.Measure.PolarLabelsPosition.Middle,
            DataLabelsFormatter = point => point.Coordinate.PrimaryValue > 0 ? point.Coordinate.PrimaryValue.ToString("N0") : string.Empty,
        };
    }
}
