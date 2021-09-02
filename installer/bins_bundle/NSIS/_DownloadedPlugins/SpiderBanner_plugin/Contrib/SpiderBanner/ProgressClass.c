/*
 * SpiderBanner plugin for NSIS
 *
 * 2006-2007, 2010-2011, 2013-2014, 2016 MouseHelmet Software.
 *
 * This file defines the custom progress bar
 * control, so I can get around the theme problem
 * on Windows XP where the progress bar is not
 * smooth when xp themes are turned on.
 *
 * ProgressClass.c
 */

#include <windows.h>
#include <commctrl.h> /* For ProgressBar control only. */

unsigned int gpos, top;
int _fltused = 0; /* Prevents a linker error for floats or doubles. */

PPAINTSTRUCT pPs;
TCHAR *ptString;

/*  Our callback proc for our custom progressbar control. */
void PaintBar(HWND hwnd)
{
  /*  This routine paints our custom progressbar, as well as moving it
  to the correct place. The majority of the painting was ported
  from the ZealProgressBar ActiveX Control, which was coded in VB.
  I had to port it to C for it to work the way I wanted it to. */

  RECT vp, rc;
  SIZE txtSize;
  unsigned int pbPercent;
  COLORREF BackColor, ForeColor;
  float tmpfloat;

  HDC hdc;
  LOGBRUSH lh;
  HBRUSH brush;
  HFONT newFont;

  /*  Start our painting. */
  hdc = BeginPaint(hwnd, pPs);

  GetClientRect(hwnd, &vp);

  /*  Create the font that we are going to use. These are not exact, they are rounded. */
  newFont = CreateFont(12, 7, 0, 0, 400, 0, 0, 0,
    0,
    OUT_DEFAULT_PRECIS,
    CLIP_DEFAULT_PRECIS,
    DEFAULT_QUALITY,
    FF_DONTCARE,
    TEXT("MS SANS SERIF")
    );

  /*  Set our colours, Blue ...*/
  ForeColor = 0x00FF0000; //RGB(0,0,255);
  /*  ...and White. */
  BackColor = 0x00FFFFFF; //RGB(255,255,255);

  /*  Caculate the percent. */
  tmpfloat = (float)gpos/top;
  pbPercent = (unsigned int)(tmpfloat*100);

  /*  Set the string to be displayed. */
  wsprintf(ptString, TEXT("%u%%"), pbPercent);

  /*  Get the size of our new font so we can position it correctly. */
  SelectObject(hdc, newFont);
  GetTextExtentPoint32(hdc, ptString, 4, &txtSize);
  txtSize.cy--; /* This moves the text down one pixel (looks better). */

  /*  Set our brush up for painting a rectangle with the ForeColor first (blue). */
  lh.lbColor = BS_SOLID;
  lh.lbColor = ForeColor;
  lh.lbHatch = 0;

  /*  Store our first half of the rectangle to be painted. */
  rc.right = (unsigned int)(vp.right*tmpfloat);
  rc.left = 0;
  rc.top = 0;
  rc.bottom = vp.bottom;

  /*  Create the brush object, draw our rect, then delete it. */
  brush = CreateBrushIndirect(&lh);
  FillRect(hdc, &rc, brush);
  DeleteObject(brush); 

  /*  Set our colors for use by ExtTextOut. */
  SetBkColor(hdc, ForeColor);
  SetTextColor(hdc, BackColor);
  ExtTextOut(hdc, (vp.right-txtSize.cx)/2, (vp.bottom-txtSize.cy)/2, ETO_CLIPPED|ETO_OPAQUE, &rc, ptString, lstrlen(ptString), 0);

  /*  Draw the second part, which is the opposite of what we just did, which
  gives us the nice percent color change as the bar increases. */
  rc.left = rc.right;
  rc.right = vp.right;

  /*  We only have to change the colour here as the the other two values
  haven't changed from before. */
  lh.lbColor = BackColor;

  /*  Draw the second part. */
  brush = CreateBrushIndirect(&lh);
  FillRect(hdc, &rc, brush);
  DeleteObject(brush); 

  /*  Set our colours the opposite way from before. */
  SetBkColor(hdc, BackColor);
  SetTextColor(hdc, ForeColor);
  ExtTextOut(hdc, (vp.right-txtSize.cx)/2, (vp.bottom-txtSize.cy)/2, ETO_CLIPPED|ETO_OPAQUE, &rc, ptString, lstrlen(ptString), 0);

  /*  Draw a nice edge to make it look more retro. */
  DrawEdge(hdc, &vp, BDR_SUNKENOUTER, BF_RECT);

  /*	Delete our font (cleanup). */
	DeleteObject(newFont);

  /*  End our painting session. */
  EndPaint(hwnd, pPs);
}

/*  Simple message processing, Windows does the rest. */
LRESULT CALLBACK StaticProc(HWND hwnd, UINT msg, WPARAM wParam, LPARAM lParam)
{
  if (msg == WM_CREATE)
    ShowWindow(hwnd, SW_SHOW);

  if (msg == PBM_SETPOS)
    gpos = (unsigned int)wParam, InvalidateRect(hwnd, NULL, TRUE);

  if (msg == PBM_SETRANGE)
    top = HIWORD(lParam);

  if (msg == WM_PAINT)
    PaintBar(hwnd);

  if (msg == WM_CLOSE)
  {
    GlobalFree(pPs);
    GlobalFree(ptString);
    DestroyWindow(hwnd);
  }

  return DefWindowProc(hwnd, msg, wParam, lParam); 
}

/*  Lets create our custom progress bar, shall we?
I'm using a static control as the base, making
it a child window and painting directly onto 
the background. The dialog is the owner, not
hwndparent.*/
HWND CreateProgressBar(HINSTANCE hInst, HWND owner)
{
  HWND tmp;

  /*  Create our window. */
  tmp = CreateWindowEx(0, TEXT("static"), 0, WS_CHILD,
    0, 0, 2, 2, owner, NULL, hInst, NULL);
  if (!tmp)
    return 0;

  /*  Needs to be allocated, if static it causes a string check cookie to be inserted when using /Gs. */
  pPs = GlobalAlloc(GPTR, sizeof(PAINTSTRUCT));
  ptString = GlobalAlloc(GPTR, sizeof(TCHAR)*16); /* Should be big enough for a printed int. */

  /*  Give it a WNDPROC so we can use it. */
  SetWindowLongPtr(tmp, GWLP_WNDPROC, (LONG_PTR)StaticProc);
  SetWindowPos(tmp, HWND_TOP, 0, 0, 0, 0, SWP_NOSIZE|SWP_NOMOVE|SWP_FRAMECHANGED);

  /*  Return the handle so it can be resized. */
  return tmp;
}