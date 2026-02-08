namespace Testify.Client.Shared.Services
{
    public class ModalService
    {
        public event Func<string, string, Task<bool>> OnShow;

        public Task<bool> ShowConfirmation(string title, string message)
        {
            return OnShow?.Invoke(title, message);
        }
    }
}
