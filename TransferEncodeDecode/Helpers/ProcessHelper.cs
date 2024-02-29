using System.Security.Principal;

namespace TransferEncodeDecode.Helpers
{
    internal class ProcessHelper
    {
        internal static bool IsRunningAsAdministrator()
        {
            var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
    }
}
