using Testify.Shared.Models;

namespace Testify.Client.Shared.Services
{
    public class ModalService
    {
        // Generic modal event
        public event Func<ModalOptions, Task<ModalResult>>? OnShow;

        #region Generic Method

        public Task<ModalResult> ShowAsync(ModalOptions options)
        {
            return OnShow?.Invoke(options) ?? Task.FromResult(new ModalResult { Confirmed = false });
        }

        #endregion

        #region Convenience Methods

        public Task<ModalResult> ShowInfoAsync(string title, string message)
        {
            return ShowAsync(new ModalOptions
            {
                Title = title,
                Message = message,
                Type = ModalType.Info,
                Buttons = ModalButton.OK,
                Icon = HeroIcons.InformationCircle // Updated
            });
        }

        public Task<ModalResult> ShowSuccessAsync(string title, string message)
        {
            return ShowAsync(new ModalOptions
            {
                Title = title,
                Message = message,
                Type = ModalType.Success,
                Buttons = ModalButton.OK,
                Icon = HeroIcons.CheckCircle // Updated
            });
        }

        public Task<ModalResult> ShowWarningAsync(string title, string message, bool showCancel = false)
        {
            return ShowAsync(new ModalOptions
            {
                Title = title,
                Message = message,
                Type = ModalType.Warning,
                Buttons = showCancel ? ModalButton.OKCancel : ModalButton.OK,
                Icon = HeroIcons.ExclamationTriangle // Updated
            });
        }

        public Task<ModalResult> ShowErrorAsync(string title, string message)
        {
            return ShowAsync(new ModalOptions
            {
                Title = title,
                Message = message,
                Type = ModalType.Error,
                Buttons = ModalButton.OK,
                Icon = HeroIcons.XCircle // Updated
            });
        }

        public Task<ModalResult> ShowConfirmationAsync(string title, string message, string? confirmText = null, string? cancelText = null)
        {
            return ShowAsync(new ModalOptions
            {
                Title = title,
                Message = message,
                Type = ModalType.Confirmation,
                Buttons = ModalButton.YesNo,
                ConfirmButtonText = confirmText ?? "Yes",
                CancelButtonText = cancelText ?? "No",
                Icon = HeroIcons.QuestionMarkCircle // Updated
            });
        }

        public async Task<bool> ShowDeleteConfirmation(string title, string message)
        {
            var result = await ShowAsync(new ModalOptions
            {
                Title = title,
                Message = message,
                Type = ModalType.Warning,
                Buttons = ModalButton.YesNo,
                ConfirmButtonText = "Delete",
                CancelButtonText = "Cancel",
                Icon = HeroIcons.Trash, // Updated
                AllowBackdropClick = false
            });

            return result.Confirmed;
        }

        public async Task<bool> ShowStartExecutionWarningAsync()
        {
            var result = await ShowAsync(new ModalOptions
            {
                Title = "Start Test Execution",
                Message = "After starting, you cannot change the Test Plan. Are you sure?",
                Type = ModalType.Warning,
                Buttons = ModalButton.YesNo,
                ConfirmButtonText = "Start Execution",
                CancelButtonText = "Cancel",
                Icon = HeroIcons.PlayCircle, // Updated
                AllowBackdropClick = false,
                ShowCloseButton = false
            });

            return result.Confirmed;
        }

        #endregion
    }

    /// <summary>
    /// Static collection of Heroicon SVG path strings (24x24 Outline)
    /// </summary>
    public static class HeroIcons
    {
        public const string InformationCircle = "M11.25 11.25l.041-.02a.75.75 0 011.063.852l-.708 2.836a.75.75 0 001.063.853l.041-.021M21 12a9 9 0 11-18 0 9 9 0 0118 0zm-9-3.75h.008v.008H12V8.25z";
        public const string CheckCircle = "M9 12.75L11.25 15 15 9.75M21 12a9 9 0 11-18 0 9 9 0 0118 0z";
        public const string ExclamationTriangle = "M12 9v3.75m-9.303 3.376c-.866 1.5.217 3.374 1.948 3.374h14.71c1.73 0 2.813-1.874 1.948-3.374L13.949 3.378c-.866-1.5-3.032-1.5-3.898 0L2.697 16.126zM12 15.75h.007v.008H12v-.008z";
        public const string XCircle = "M9.75 9.75l4.5 4.5m0-4.5l-4.5 4.5M21 12a9 9 0 11-18 0 9 9 0 0118 0z";
        public const string QuestionMarkCircle = "M9.879 7.519c1.171-1.025 3.071-1.025 4.242 0 1.172 1.025 1.172 2.687 0 3.712-.203.179-.43.326-.67.442-.745.361-1.45.999-1.45 1.827v.75M21 12a9 9 0 11-18 0 9 9 0 0118 0zm-9 5.25h.008v.008H12v-.008z";
        public const string Trash = "M14.74 9l-.346 9m-4.788 0L9.26 9m9.968-3.21c.342.052.682.107 1.022.166m-1.022-.165L18.16 19.673a2.25 2.25 0 01-2.244 2.077H8.084a2.25 2.25 0 01-2.244-2.077L4.772 5.79m14.456 0a48.108 48.108 0 00-3.478-.397m-12 .562c.34-.059.68-.114 1.022-.165m0 0a48.11 48.11 0 013.478-.397m7.5 0v-.916c0-1.18-.91-2.164-2.09-2.201a51.964 51.964 0 00-3.32 0c-1.18.037-2.09 1.022-2.09 2.201v.916m7.5 0a48.667 48.667 0 00-7.5 0";
        public const string PlayCircle = "M21 12a9 9 0 11-18 0 9 9 0 0118 0zM15.91 11.672a.375.375 0 010 .656l-5.603 3.113a.375.375 0 01-.557-.328V8.887c0-.286.307-.466.557-.327l5.603 3.112z";
    }
}