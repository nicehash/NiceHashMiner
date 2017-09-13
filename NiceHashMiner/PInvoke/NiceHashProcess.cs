using System;
using System.Diagnostics;
using System.Runtime.InteropServices;


namespace NiceHashMiner
{
	public class NiceHashProcess : Process
	{
		// Ctrl+C
		public enum CtrlTypes
		{
			CTRL_C_EVENT = 0,
			CTRL_BREAK_EVENT,
			CTRL_CLOSE_EVENT,
			CTRL_LOGOFF_EVENT = 5,
			CTRL_SHUTDOWN_EVENT
		}
		
		delegate bool HandlerRoutine(CtrlTypes CtrlType);
		
		[DllImport("kernel32.dll", SetLastError = true)]
		static extern bool AttachConsole(uint dwProcessId);

		[DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
		static extern bool FreeConsole();

		[DllImport("kernel32.dll", SetLastError = true)]
		static extern bool GenerateConsoleCtrlEvent(CtrlTypes dwCtrlEvent, uint dwProcessGroupId);

		[DllImport("Kernel32", SetLastError = true)]
		static extern bool SetConsoleCtrlHandler(HandlerRoutine handler, bool add);

		bool signalCtrlС(uint dwProcessId)
		{
			bool success = false;
			int errorCode;
			bool attaced = false;
			bool consoleDetached = FreeConsole();
			if (!consoleDetached) {
				errorCode = Marshal.GetLastWin32Error();
				if (errorCode == 87) {
					consoleDetached = true;
				}
			}
			if (consoleDetached) {
				attaced = AttachConsole(dwProcessId);
				if (attaced) {
					// Add a fake Ctrl-C handler for avoid instant kill is this console
					// WARNING: do not revert it or current program will be also killed
					SetConsoleCtrlHandler(null, true);
					success = GenerateConsoleCtrlEvent(CtrlTypes.CTRL_C_EVENT, 0);
					FreeConsole();
					WaitForExit(10000);
				}
				SetConsoleCtrlHandler(null, false);
			}
			return success;
		}
		
		public bool StartProcces()
		{
			bool success = false;
			Start();
			WaitForExit(100);
			success = !HasExited;
			return success;
		}
		
		public void EndProcces(int waitms)
		{
			Helpers.ConsolePrint("my", "EndProcess: ");// FIXME:
			if (!HasExited) {
				signalCtrlС((uint)Id);
				if (!WaitForExit(waitms)) {
					Helpers.ConsolePrint("my", "Waring CTRL_C_EVENT Error: ");// FIXME:
					CloseMainWindow();
					if (!WaitForExit(waitms)) {
						Kill();
						Helpers.ConsolePrint("my", "Waring Terminate process: ");// FIXME:
						if (!WaitForExit(waitms)) {
							Helpers.ConsolePrint("my", "Error Terminated process: ");// FIXME:
						}
					}
//				} else {
//					Helpers.ConsolePrint("my", "CTRL_C_EVENT Work!!!");// FIXME:
				}
			}
			Close();
		}
	}
}
