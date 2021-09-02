/*
 * SpiderBanner plugin for NSIS
 *
 * 2006-2007, 2010-2011, 2013-2014, 2016 MouseHelmet Software.
 *
 * Created By Jason Ross aka JasonFriday13 on the forums
 *
 * Displays a custom dialog that shows the information
 * on the InstFiles page of the installer.
 *
 * Originally coded for the InstallSpider Interface, but
 * can be used for any installer.
 *
 * SpiderBanner.c
 */

/*  Include relevent files. */
#include <windows.h>
#include "nsis\pluginapi.h" /* This means NSIS 2.42 or higher is required. */

#include <commctrl.h> /* For ProgressBar control only.*/
#include "ProgressClass.h"
#include "resource.h"

/*  Global declarations. */
typedef enum {
  ACT_UN = 0,
  ACT_TL,
  ACT_TR,
  ACT_BL,
  ACT_BR,
  ACT_CE,
  ACT_MO,
  ACT_IC
} ARGUMENTCONFIGTYPE;

HANDLE hIcon;
HINSTANCE hInstance;
HWND hParent, hBanner, hInstBar, hProgBar, hBannerBar, hDetailBar, hDetailInstBar;
RECT rectOriginalPos;
int iModern, iPBOnly, iShb; /* This is for preventing two calls to 'show' causing a crash. */

/*  These are for storing the vars from the stack so that we
can use them at any time. */
ARGUMENTCONFIGTYPE actLocation;
int iMargin;

/*  Original WndProc variables. */
LONG_PTR lpOriginalParentProc, lpOriginalPosProc, lpOriginalDetailProc, lpOriginalDetailInstProc;

ARGUMENTCONFIGTYPE TransformArgument(TCHAR *arg)
{
  if (0 == lstrcmpi(arg, TEXT("/TL"))) return ACT_TL;
  if (0 == lstrcmpi(arg, TEXT("/TR"))) return ACT_TR;
  if (0 == lstrcmpi(arg, TEXT("/BL"))) return ACT_BL;
  if (0 == lstrcmpi(arg, TEXT("/BR"))) return ACT_BR;
  if (0 == lstrcmpi(arg, TEXT("/CENTER"))) return ACT_CE;
  if (0 == lstrcmpi(arg, TEXT("/MODERN"))) return ACT_MO;
  if (0 == lstrcmpi(arg, TEXT("/ICON"))) return ACT_IC;
  return ACT_UN;
}

/*  This routine shows and hides all the extra items behind our child window so
they don't interfere with the painting of our child window. */
void WindowDisplay(int state)
{
  int show = state ? SW_SHOW : SW_HIDE;

  ShowWindow(FindWindowEx(hParent, 0, TEXT("#32770"), 0), show);	
  ShowWindow(GetDlgItem(hParent, 1034), show);
  ShowWindow(GetDlgItem(hParent, 1035), show);
  ShowWindow(GetDlgItem(hParent, 1036), show);
  ShowWindow(GetDlgItem(hParent, 1037), show);
  ShowWindow(GetDlgItem(hParent, 1038), show);
  ShowWindow(GetDlgItem(hParent, 1039), show);
  ShowWindow(GetDlgItem(hParent, 1046), show); /* ISUI only. */
}

/*  This brings back the original InstFiles page that was included with NSIS. */
void CenterParentWindow(void)
{
  SetWindowPos(
    hParent,
    0,
    (GetSystemMetrics(SM_CXSCREEN)/2)-(rectOriginalPos.right/2)-(GetSystemMetrics(SM_CXFIXEDFRAME)*2),
    (GetSystemMetrics(SM_CYSCREEN)/2)-(rectOriginalPos.bottom/2)-GetSystemMetrics(SM_CYCAPTION)-(GetSystemMetrics(SM_CXFIXEDFRAME)*2),
    rectOriginalPos.right+(GetSystemMetrics(SM_CXFIXEDFRAME)*2),
    rectOriginalPos.bottom+GetSystemMetrics(SM_CYCAPTION)+(GetSystemMetrics(SM_CXFIXEDFRAME)*2),
    SWP_NOZORDER
    );
}

/*  This moves the parent window based on the variables given to us 
from the SetWindowParams routine. */
void MoveParentTo(ARGUMENTCONFIGTYPE corner, int dist)
{
  RECT rect;
    
  GetClientRect(hBanner, &rect);

  switch (corner)
  {
  case ACT_TL:
    rect.left = dist;
    rect.top = dist;
    break;
  case ACT_TR:
    rect.left = (GetSystemMetrics(SM_CXSCREEN)-dist)-rect.right-(GetSystemMetrics(SM_CXFIXEDFRAME)*2);
    rect.top = dist;
    break;
  case ACT_BL:
    rect.left = dist;
    rect.top = ((GetSystemMetrics(SM_CYSCREEN)-dist)-rect.bottom-GetSystemMetrics(SM_CYCAPTION)-(GetSystemMetrics(SM_CXFIXEDFRAME)*2));
    break;
  case ACT_BR:
    rect.left = (GetSystemMetrics(SM_CXSCREEN)-dist)-rect.right-(GetSystemMetrics(SM_CXFIXEDFRAME)*2);
    rect.top = (GetSystemMetrics(SM_CYSCREEN)-dist)-rect.bottom-GetSystemMetrics(SM_CYCAPTION)-(GetSystemMetrics(SM_CXFIXEDFRAME)*2);
    break;
  default:
    rect.left = (GetSystemMetrics(SM_CXSCREEN)/2)-(rect.right/2)-(GetSystemMetrics(SM_CXFIXEDFRAME)*2);
    rect.top = (GetSystemMetrics(SM_CYSCREEN)/2)-(rect.bottom/2)-GetSystemMetrics(SM_CYCAPTION)-(GetSystemMetrics(SM_CXFIXEDFRAME)*2);
  }
  /*  Apply window position. */
  SetWindowPos(hParent, 0, rect.left, rect.top, 0, 0, SWP_NOSIZE|SWP_NOZORDER);
}

/*  This routine gets the data from the installer and parses it. */
void SetWindowParams(void)
{
  ARGUMENTCONFIGTYPE arg;
  TCHAR *buf = GlobalAlloc(GPTR, sizeof(TCHAR)*g_stringsize);
  TCHAR *tmp = GlobalAlloc(GPTR, sizeof(TCHAR)*g_stringsize);

  do
  {
    /*  Pop, pop, pop it off the stack. */
    if (popstring(buf) > 0) break;
    arg = TransformArgument(buf);
    switch (arg)
    {
    case ACT_TL:
    case ACT_TR:
    case ACT_BL:
    case ACT_BR:
      if (0 == popstring(tmp) && myatoi(tmp) >= 0)
      {
        iMargin = myatoi(tmp);
      }
    case ACT_CE:
      actLocation = arg;
      break;
    case ACT_MO:
      iModern = 1;
      break;
    case ACT_IC:
      if (0 == popstring(tmp))
      {
        if (hIcon) DestroyIcon(hIcon), hIcon = NULL;
        hIcon = LoadImage(NULL, tmp, IMAGE_ICON, 32, 32, LR_LOADFROMFILE);
      }
      break;
    case ACT_UN:
      /*  Push the last string back onto the stack. */
      pushstring(buf);
      break;
    }
  } while (arg != ACT_UN);

  if (hIcon == NULL) hIcon = LoadImage(GetModuleHandle(NULL), MAKEINTRESOURCE(103), IMAGE_ICON, 32, 32, LR_DEFAULTCOLOR);

  GlobalFree(buf);
  GlobalFree(tmp);
}

/********************************************************
The following Procedures are for duplicating the info
on the installer and applying it to our child window.
*********************************************************/

/*  This is here only to repaint our progress bar when the
window position changes to prevent it disappearing behind
other windows on the parent.*/
LRESULT CALLBACK ParentProc(HWND hwnd, UINT msg, WPARAM wParam, LPARAM lParam)
{
  if (hwnd == hParent && msg == WM_PAINT)
  {
    if (hBanner)
    {			
      InvalidateRect(hBanner, NULL, TRUE);
      UpdateWindow(hBanner);
    }
    if (hProgBar)
    {
      /*  HAS to have both or it doesn't work. */
      InvalidateRect(hProgBar, NULL, TRUE);
      UpdateWindow(hProgBar);
    }		
  }
  return CallWindowProc((WNDPROC)lpOriginalParentProc, hwnd, msg, wParam, lParam);
}

/*  The new proc for the progressbar control. */
LRESULT CALLBACK PosProc(HWND hwnd, UINT msg, WPARAM wParam, LPARAM lParam)
{
  if (msg == PBM_SETPOS)
    SendMessage(hProgBar ? hProgBar : hBannerBar, PBM_SETPOS, wParam, 0);

  return CallWindowProc((WNDPROC)lpOriginalPosProc, hwnd, msg, wParam, lParam);
}

/*  The new proc for the detail control. */
LRESULT CALLBACK DetailProc(HWND hwnd, UINT msg, WPARAM wParam, LPARAM lParam)
{
  if (msg == WM_SETTEXT)
    /*  Send the text to our dialog, then return to the original proc. */
    SendDlgItemMessage(hBanner, 1003, WM_SETTEXT, 0, lParam);

  return CallWindowProc((WNDPROC)lpOriginalDetailProc, hwnd, msg, wParam, lParam);
}

/*  The new proc for the detail control. */
LRESULT CALLBACK DetailInstProc(HWND hwnd, UINT msg, WPARAM wParam, LPARAM lParam)
{
  if (msg == WM_SETTEXT)
    /*  Send the text to our dialog, then return to the original proc. */
    SendDlgItemMessage(hBanner, 1002, WM_SETTEXT, 0, lParam);  

  return CallWindowProc((WNDPROC)lpOriginalDetailInstProc, hwnd, msg, wParam, lParam);
}

/*******************************
End of procedure hacking :).
********************************/

/*  The banner procedure for our child window. */
LRESULT CALLBACK BannerProc(HWND hDlg, UINT msg, WPARAM wParam, LPARAM lParam)
{
  if (msg == WM_INITDIALOG)
  {
    RECT tmp;
    /*  This is to restore the window (if it's minimized) so we can
    change the dimensions of the window. */
    if (GetActiveWindow() != hParent)
      ShowWindow(hParent, SW_RESTORE);

    /*  This next line is here to stop someone trying to crash the installer 
    if they are moving the window around when WM_CLOSE is called. */
    ShowWindow(hParent, SW_HIDE);
    SetWindowPos(hParent, 0, 0, 0, 0, 0, SWP_SHOWWINDOW|SWP_NOSIZE|SWP_NOMOVE|SWP_NOZORDER);

    /*  Get the dialog dimensions so that I can resize and move the dialog. */
    GetClientRect(hDlg, &tmp);
    GetClientRect(hParent, &rectOriginalPos);

    /*  Set the icon on our dialog. It retrieves the icon from the installer resources. */
    SendDlgItemMessage(hDlg, 1025, STM_SETICON, (WPARAM)hIcon, 0);

    /*  Resize the parent window to our custom dialog. */
    SetWindowPos(hParent, 0, 0, 0, tmp.right+(GetSystemMetrics(SM_CXFIXEDFRAME)*2), tmp.bottom+GetSystemMetrics(SM_CYCAPTION)+(GetSystemMetrics(SM_CXFIXEDFRAME)*2), SWP_NOMOVE|SWP_NOZORDER);

    /*  Hide the labels on the parent window. */
    WindowDisplay(0);
  }
  if (msg == WM_CLOSE) 
  {		
    /*  This is to restore the window (if it's minimized) so we can
    change the dimensions of the window. */
    if (GetActiveWindow() != hParent)
      ShowWindow(hParent, SW_RESTORE);

    /*  This next line is here to stop someone trying to crash the installer 
    if they are moving the window around when WM_CLOSE is called. */
    ShowWindow(hParent, SW_HIDE);
    SetWindowPos(hParent, 0, 0, 0, 0, 0, SWP_SHOWWINDOW|SWP_NOSIZE|SWP_NOMOVE|SWP_NOZORDER);

    /*  Destroy our child window, restore the hidden items on the original
    page, and center the parent window and make it visible again. */
    DestroyWindow(hDlg);
    WindowDisplay(1);
    CenterParentWindow();
  }
  return 0;
}

/*  Our banner thread, so that we can process messages
without hogging the CPU speed. */
DWORD WINAPI BannerThread(LPVOID lpParameter)
{
  MSG msg;

  /*  Create the dialog. */
  if (!(hBanner = CreateDialog(hInstance, MAKEINTRESOURCE(IDD_DIALOG), hParent, BannerProc)))
    return 0;

  /*  Store the banner's progressbar handle for use later. */
  hBannerBar = GetDlgItem(hBanner, 1001);

  /*  Set the range using our current progressbar. */
  SendMessage(hBannerBar, PBM_SETRANGE, 0, MAKELPARAM(0, SendMessage(hInstBar, PBM_GETRANGE, FALSE, 0)));

  /*  Our message loop for the banner only. */
  while (IsWindow(hBanner))
    PeekMessage(&msg, hBanner, 0, 0, PM_REMOVE) ? DispatchMessage(&msg) : WaitMessage();

  /*  Wipe the handle because the window doesn't exist anymore. */
  hBanner = 0;

  return 0;
}

/*  Our progress bar thread, so that we can process messages
without hogging the CPU speed. */
DWORD WINAPI ProgBarThread(LPVOID lpParameter)
{
  MSG msg;

  if (!(hProgBar = CreateProgressBar(hInstance, iPBOnly ? FindWindowEx(hParent, 0, TEXT("#32770"), 0) : hBanner)))
    return 0;

  /*  Set the range on our new progressbar. */
  SendMessage(hProgBar, PBM_SETRANGE, 0, MAKELPARAM(0, SendMessage(hInstBar, PBM_GETRANGE, FALSE, 0)));

  /*  Pre-paint our control. */
  SendMessage(hProgBar, PBM_SETPOS, 1, 0);

  /*  Our message loop for the progressbar only. */
  while (IsWindow(hProgBar))
    PeekMessage(&msg, hProgBar, 0, 0, PM_REMOVE) ? DispatchMessage(&msg) : WaitMessage();

  /*  Wipe the handle because the window doesn't exist anymore. */
  hProgBar = 0;

  return 0;
}

/*  This is the Show routine that is exported so our installer can call it. */
void ShowInit(HWND owner)
{
  DWORD dwThreadId1, dwThreadId2;
  HANDLE hThread;
  WINDOWPLACEMENT wp;
  RECT rect;

  /*  This is to prevent a second call to the show routine crashing us. */
  if (iShb)
    return;
  else
  {
    hParent = owner;
    hBanner = 0;
    hProgBar = 0;
    actLocation = ACT_CE;
    iMargin = 0;
    iModern = 0;
    hIcon = NULL;

    /*  Pop our settings off the stack and apply them. */
    SetWindowParams();
    wp.length = sizeof(WINDOWPLACEMENT);

    /*  Store the reqired handles so that it is easier to code. */
    hInstBar = GetDlgItem(FindWindowEx(hParent, 0, TEXT("#32770"), 0), 1004);
    hDetailBar = GetDlgItem(FindWindowEx(hParent, 0, TEXT("#32770"), 0), 1006);

    if (GetDlgItem(FindWindowEx(hParent, 0, TEXT("#32770"), 0), 1011)) /* For ISUI only. */
      hDetailInstBar = GetDlgItem(FindWindowEx(hParent, 0, TEXT("#32770"), 0), 1011);

    if (!iPBOnly)
    {
      /*  Create our dedicated thread so we can do other things at the same time. */
      hThread = CreateThread(0, 0, BannerThread, (LPVOID)hParent, 0, &dwThreadId1);

      /*  Wait for the window to initalize and for the stack operations to finish. */
      while (hThread && !hBanner)
        Sleep(10);

      if (!hThread)
        return;

      /*  Security measure, close the handle so people can't do anything with it. */
      CloseHandle(hThread);

      /*  Redraw our window to get rid of any graphical glitches. */
      SendMessage(hBanner, WM_SETREDRAW, TRUE, 0);
      InvalidateRect(hBanner, NULL, TRUE);

      /*  Subclass the last of our controls. */
      lpOriginalPosProc = SetWindowLongPtr(hInstBar, GWLP_WNDPROC, (LONG_PTR)PosProc);
      lpOriginalDetailProc = SetWindowLongPtr(hDetailBar, GWLP_WNDPROC, (LONG_PTR)DetailProc);
      lpOriginalDetailInstProc = SetWindowLongPtr(hDetailInstBar, GWLP_WNDPROC, (LONG_PTR)DetailInstProc);
    }
    if (!iModern)
    {
      /*  Create our dedicated thread so we can do other things at the same time. */
      hThread = CreateThread(0, 0, ProgBarThread, (LPVOID)hParent, 0, &dwThreadId2);

      /*  Wait for the window to initalize and for the stack operations to finish. */
      while (hThread && !hProgBar)
        Sleep(10);

      if (!hThread)
        return;

      /*  Security measure, close the handle so people can't do anything with it. */
      CloseHandle(hThread);
    }
    if (iPBOnly)
    {
      lpOriginalPosProc = SetWindowLongPtr(hInstBar, GWLP_WNDPROC, (LONG_PTR)PosProc);

      GetWindowPlacement(hInstBar, &wp);
      GetClientRect(hInstBar, &rect);

      /*  Hide the progressbar so that our custom one isn't fighting
      with it for painting priority. */
      ShowWindow(hInstBar, SW_HIDE);
      SetWindowPos(hProgBar, 0, wp.rcNormalPosition.left, wp.rcNormalPosition.top, rect.right, rect.bottom, SWP_NOZORDER|SWP_SHOWWINDOW);
    }
    else
    {
      MoveParentTo(actLocation, iMargin);
      if (!iModern)
      {
        /*  Hide the progressbar so that our custom one isn't fighting
        with it for painting priority. */
        ShowWindow(hBannerBar, SW_HIDE);

        GetWindowPlacement(hBannerBar, &wp);
        GetClientRect(hBannerBar, &rect);

        SetWindowPos(hProgBar, 0, wp.rcNormalPosition.left, wp.rcNormalPosition.top, rect.right, rect.bottom, SWP_NOZORDER|SWP_SHOWWINDOW);
        InvalidateRect(hBanner, NULL, TRUE);
        UpdateWindow(hBanner);
      }
    } 

    /*  Subclass hwndparent so we can repaint our progressbar
    after the window changes state. */
    lpOriginalParentProc = SetWindowLongPtr(hParent, GWLP_WNDPROC, (LONG_PTR)ParentProc);
    SetWindowPos(hParent, 0, 0, 0, 0, 0, SWP_NOSIZE|SWP_NOMOVE|SWP_NOZORDER|SWP_NOACTIVATE|SWP_FRAMECHANGED);
  }

  /*  Our anti-crashing variable. */
  iShb = 1;
}

void DestroyMe(void)
{
  /*  Restore all our subclasses. */
  if (lpOriginalPosProc)
    SetWindowLongPtr(hInstBar, GWLP_WNDPROC, lpOriginalPosProc);

  if (lpOriginalParentProc)
    SetWindowLongPtr(hParent, GWLP_WNDPROC, lpOriginalParentProc);

  if (lpOriginalDetailProc)
    SetWindowLongPtr(hDetailBar, GWLP_WNDPROC, lpOriginalDetailProc);

  if (lpOriginalDetailInstProc)
    SetWindowLongPtr(hDetailInstBar, GWLP_WNDPROC, lpOriginalDetailInstProc);

  if (GetActiveWindow() != hParent)
    ShowWindow(hParent, SW_RESTORE);

  if (hProgBar)
  {	
    SendMessage(hProgBar, WM_CLOSE, 0, 0);
    hProgBar = 0;

    ShowWindow(hInstBar, SW_SHOW);
  }

  if (hIcon)
    DestroyIcon(hIcon), hIcon = NULL;

  /*  And finally, destroy our window. */
  if (hBanner)
  {
    SendMessage(hBanner, WM_CLOSE, 0, 0);
    hBanner = 0;
  }
}

/*  Our callback function so that our dll stays loaded. */
UINT_PTR __cdecl NSISPluginCallback(enum NSPIM Event) 
{
  if (Event == NSPIM_GUIUNLOAD)
    DestroyMe();

  return 0;
}

/*  This routine displays the custom dialog. */
__declspec(dllexport) void Show(HWND hwndParent, int string_size, TCHAR *variables, stack_t **stacktop, extra_parameters* xp)
{
  /*  Initialize the stack so we can access it from our DLL using 
  popstring and pushstring. */
  EXDLL_INIT();
  xp->RegisterPluginCallback(hInstance, NSISPluginCallback);

  iPBOnly = 0;
  ShowInit(hwndParent);
}

/*  This routine displays just our custom progress bar in place of the default one. */
__declspec(dllexport) void ShowPBOnly(HWND hwndParent, int string_size, TCHAR *variables, stack_t **stacktop, extra_parameters* xp)
{
  /*  Initialize the stack so we can access it from our DLL using 
  popstring and pushstring. */
  EXDLL_INIT();
  xp->RegisterPluginCallback(hInstance, NSISPluginCallback);

  iPBOnly = 1;
  ShowInit(hwndParent);
}

/*  This exported routine destroys the windows and restores the parent window. */
__declspec(dllexport) void Destroy(HWND hwndParent, int string_size, TCHAR *variables, stack_t **stacktop, extra_parameters* xp) 
{
  iShb = 0; /* Our anti-crashing variable, although it technically doesn't need to be here. */
  DestroyMe();
}

/*  Our DLL entry point, this is called when we first load up our DLL. */
BOOL WINAPI _DllMainCRTStartup(HINSTANCE hInst, DWORD ul_reason_for_call, LPVOID lpReserved)
{
  hInstance = hInst;
  iShb = 0; /* Initialize our anti-crashing variable. */
  
  if (ul_reason_for_call == DLL_PROCESS_DETACH)
    DestroyMe();

  return TRUE;
}