using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SuperDevFact.Desktop.Behaviors;

/// <summary>
/// Comportement attaché : sélectionne tout le texte d'un TextBox dès qu'il reçoit le
/// focus, pour que l'utilisateur puisse remplacer une valeur par défaut (ex. "unité")
/// simplement en tapant, sans avoir à l'effacer au préalable au clavier.
/// </summary>
public static class TextBoxBehaviors
{
    public static readonly DependencyProperty SelectAllOnFocusProperty = DependencyProperty.RegisterAttached(
        "SelectAllOnFocus", typeof(bool), typeof(TextBoxBehaviors), new PropertyMetadata(false, OnSelectAllOnFocusChanged));

    public static void SetSelectAllOnFocus(DependencyObject element, bool value) => element.SetValue(SelectAllOnFocusProperty, value);

    public static bool GetSelectAllOnFocus(DependencyObject element) => (bool)element.GetValue(SelectAllOnFocusProperty);

    private static void OnSelectAllOnFocusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not TextBox textBox || e.NewValue is not true)
            return;

        textBox.GotFocus += (_, _) => textBox.SelectAll();

        // Sans ceci, le clic qui donne le focus positionne aussi le curseur et annule
        // la sélection faite dans GotFocus : on intercepte le clic pour focus d'abord,
        // sélectionner ensuite, sans laisser le TextBox gérer lui-même le positionnement.
        textBox.PreviewMouseLeftButtonDown += (sender, args) =>
        {
            var box = (TextBox)sender!;
            if (!box.IsKeyboardFocusWithin)
            {
                args.Handled = true;
                box.Focus();
            }
        };
    }
}
