using Testify.Shared.Enums;

namespace Testify.Client.Shared.Services;

public class ConfirmDialogService
{
    public event Action<ConfirmDialogOptions>? OnShow;

    public void Show(ConfirmDialogOptions options)
    {
        OnShow?.Invoke(options);
    }

    public void ShowDelete(string itemName, Func<Task> onConfirm)
    {
        Show(new ConfirmDialogOptions
        {
            Title = "Delete Confirmation",
            Message = $"Are you sure you want to delete '{itemName}'? This action cannot be undone.",
            ConfirmText = "Delete",
            CancelText = "Cancel",
            Type = ConfirmDialogType.Danger,
            OnConfirm = onConfirm
        });
    }
}

public class ConfirmDialogOptions
{
    public string Title { get; set; } = "Are you sure?";
    public string Message { get; set; } = "This action cannot be undone.";
    public string ConfirmText { get; set; } = "Confirm";
    public string CancelText { get; set; } = "Cancel";
    public ConfirmDialogType Type { get; set; } = ConfirmDialogType.Danger;
    public Func<Task>? OnConfirm { get; set; }
    public Func<Task>? OnCancel { get; set; }
}