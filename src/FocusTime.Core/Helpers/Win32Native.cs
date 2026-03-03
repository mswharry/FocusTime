using System.Runtime.InteropServices;
using System.Text;

namespace FocusTime.Core.Helpers;

/// <summary>
/// Windows API functions for foreground window tracking
/// </summary>
public static class Win32Native
{
    [DllImport("user32.dll")]
    public static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll", SetLastError = true)]
    public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern IntPtr OpenProcess(uint processAccess, bool bInheritHandle, uint processId);

    [DllImport("psapi.dll", SetLastError = true)]
    public static extern uint GetModuleBaseName(IntPtr hProcess, IntPtr hModule, StringBuilder lpBaseName, uint nSize);

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool CloseHandle(IntPtr hObject);

    public const uint PROCESS_QUERY_INFORMATION = 0x0400;
    public const uint PROCESS_VM_READ = 0x0010;

    /// <summary>
    /// Get the process name of the foreground window
    /// </summary>
    public static string GetForegroundProcessName()
    {
        try
        {
            IntPtr hwnd = GetForegroundWindow();
            if (hwnd == IntPtr.Zero)
                return string.Empty;

            GetWindowThreadProcessId(hwnd, out uint processId);
            IntPtr hProcess = OpenProcess(PROCESS_QUERY_INFORMATION | PROCESS_VM_READ, false, processId);
            
            if (hProcess == IntPtr.Zero)
                return string.Empty;

            StringBuilder processName = new StringBuilder(256);
            GetModuleBaseName(hProcess, IntPtr.Zero, processName, (uint)processName.Capacity);
            CloseHandle(hProcess);

            return processName.ToString();
        }
        catch
        {
            return string.Empty;
        }
    }
}
