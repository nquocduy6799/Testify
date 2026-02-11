using System;
using System.Collections.Generic;
using System.Text;

namespace Testify.Shared.Models
{
    public class ModalOptions
    {
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public ModalType Type { get; set; } = ModalType.Info;
        public ModalButton Buttons { get; set; } = ModalButton.OK;

        // Custom button text
        public string? ConfirmButtonText { get; set; }
        public string? CancelButtonText { get; set; }

        // Additional options
        public bool AllowBackdropClick { get; set; } = true;
        public bool ShowCloseButton { get; set; } = true;
        public string? Icon { get; set; }
        public string? CssClass { get; set; }
    }

    public class ModalResult
    {
        public bool Confirmed { get; set; }
        public ModalResultType ResultType { get; set; }
        public object? Data { get; set; }
    }

    public enum ModalResultType
    {
        None,
        OK,
        Cancel,
        Yes,
        No
    }

    public enum ModalType
    {
        Info,
        Success,
        Warning,
        Error,
        Confirmation
    }

    public enum ModalButton
    {
        OK,
        OKCancel,
        YesNo,
        YesNoCancel,
        Custom
    }

}
