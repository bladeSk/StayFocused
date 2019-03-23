//Copyright (c) 2007-2008, Marton Anka
//
//Permission is hereby granted, free of charge, to any person obtaining a 
//copy of this software and associated documentation files (the "Software"), 
//to deal in the Software without restriction, including without limitation 
//the rights to use, copy, modify, merge, publish, distribute, sublicense, 
//and/or sell copies of the Software, and to permit persons to whom the 
//Software is furnished to do so, subject to the following conditions:
//
//The above copyright notice and this permission notice shall be included 
//in all copies or substantial portions of the Software.
//
//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS 
//OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
//FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
//THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
//LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING 
//FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS 
//IN THE SOFTWARE.

#include "stdafx.h"
#include "mhook-lib/mhook.h"

//=========================================================================
// Define _NtOpenProcess so we can dynamically bind to the function
//
typedef struct _CLIENT_ID {
	DWORD_PTR UniqueProcess;
	DWORD_PTR UniqueThread;
} CLIENT_ID, *PCLIENT_ID;

typedef ULONG (WINAPI* _NtOpenProcess)(OUT PHANDLE ProcessHandle, 
	     IN ACCESS_MASK AccessMask, IN PVOID ObjectAttributes, 
		 IN PCLIENT_ID ClientId ); 

//=========================================================================
// Define _SelectObject so we can dynamically bind to the function
typedef HGDIOBJ (WINAPI* _SelectObject)(HDC hdc, HGDIOBJ hgdiobj); 

//=========================================================================
// Get the current (original) address to the functions to be hooked
//
_NtOpenProcess TrueNtOpenProcess = (_NtOpenProcess)
	GetProcAddress(GetModuleHandle(L"ntdll"), "NtOpenProcess");

_SelectObject TrueSelectObject = (_SelectObject)
	GetProcAddress(GetModuleHandle(L"gdi32"), "SelectObject");

//=========================================================================
// This is the function that will replace NtOpenProcess once the hook 
// is in place
//
ULONG WINAPI HookNtOpenProcess(OUT PHANDLE ProcessHandle, 
							   IN ACCESS_MASK AccessMask, 
							   IN PVOID ObjectAttributes, 
							   IN PCLIENT_ID ClientId)
{
	printf("***** Call to open process %d\n", ClientId->UniqueProcess);
	return TrueNtOpenProcess(ProcessHandle, AccessMask, 
		ObjectAttributes, ClientId);
}

//=========================================================================
// This is the function that will replace SelectObject once the hook 
// is in place
//
HGDIOBJ WINAPI HookSelectobject(HDC hdc, HGDIOBJ hgdiobj)
{
	printf("***** Call to SelectObject(0x%p, 0x%p)\n", hdc, hgdiobj);
	return TrueSelectObject(hdc, hgdiobj);
}

//=========================================================================
// This is where the work gets done.
//
int wmain(int argc, WCHAR* argv[])
{
	HANDLE hProc = NULL;

	// Set the hook
	if (Mhook_SetHook((PVOID*)&TrueNtOpenProcess, HookNtOpenProcess)) {
		// Now call OpenProcess and observe NtOpenProcess being redirected
		// under the hood.
		hProc = OpenProcess(PROCESS_ALL_ACCESS, 
			FALSE, GetCurrentProcessId());
		if (hProc) {
			printf("Successfully opened self: %p\n", hProc);
			CloseHandle(hProc);
		} else {
			printf("Could not open self: %d\n", GetLastError());
		}
		// Remove the hook
		Mhook_Unhook((PVOID*)&TrueNtOpenProcess);
	}

	// Call OpenProces again - this time there won't be a redirection as
	// the hook has bee removed.
	hProc = OpenProcess(PROCESS_ALL_ACCESS, FALSE, GetCurrentProcessId());
	if (hProc) {
		printf("Successfully opened self: %p\n", hProc);
		CloseHandle(hProc);
	} else {
		printf("Could not open self: %d\n", GetLastError());
	}

	// Test another hook, this time in SelectObject
	// (SelectObject is interesting in that on XP x64, the second instruction
	// in the trampoline uses IP-relative addressing and we need to do some
	// extra work under the hood to make things work properly. This really
	// is more of a test case rather than a demo.)
	if (Mhook_SetHook((PVOID*)&TrueSelectObject, HookSelectobject)) {
		// error checking omitted for brevity. doesn't matter much 
		// in this context anyway.
		HDC hdc = GetDC(NULL);
		HDC hdcMem = CreateCompatibleDC(hdc);
		HBITMAP hbm = CreateCompatibleBitmap(hdc, 32, 32);
		HBITMAP hbmOld = (HBITMAP)SelectObject(hdcMem, hbm);
		SelectObject(hdcMem, hbmOld);
		DeleteObject(hbm);
		DeleteDC(hdcMem);
		ReleaseDC(NULL, hdc);
		// Remove the hook
		Mhook_Unhook((PVOID*)&TrueSelectObject);
	}

	return 0;
}

