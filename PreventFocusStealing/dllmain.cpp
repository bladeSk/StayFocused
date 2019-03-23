#include "stdafx.h"
#include "mhook-2.2/mhook-lib/mhook.h"

typedef NTSTATUS( WINAPI* PNT_QUERY_SYSTEM_INFORMATION ) ( 
  __in       SYSTEM_INFORMATION_CLASS SystemInformationClass,     
	__inout    PVOID SystemInformation, 
	__in       ULONG SystemInformationLength, 
	__out_opt  PULONG ReturnLength    
);

// Originals
PNT_QUERY_SYSTEM_INFORMATION OriginalFlashWindow   = 
  (PNT_QUERY_SYSTEM_INFORMATION)::GetProcAddress( 
  ::GetModuleHandle( L"user32" ), "FlashWindow" );

PNT_QUERY_SYSTEM_INFORMATION OriginalFlashWindowEx = 
  (PNT_QUERY_SYSTEM_INFORMATION)::GetProcAddress( 
  ::GetModuleHandle( L"user32" ), "FlashWindowEx" );

PNT_QUERY_SYSTEM_INFORMATION OriginalSetForegroundWindow = 
  (PNT_QUERY_SYSTEM_INFORMATION)::GetProcAddress( 
  ::GetModuleHandle( L"user32" ), "SetForegroundWindow" );

// Hooks
BOOL WINAPI
HookedFlashWindow(
	__in  HWND hWnd,
  __in  BOOL bInvert
  ) {
  return 0;
}

BOOL WINAPI 
HookedFlashWindowEx(
	__in  PFLASHWINFO pfwi
  ) {
  return 0;
}

BOOL WINAPI 
HookedSetForegroundWindow(
  __in  HWND hWnd
  ) {
  // Pretend window was brought to foreground
  return 1;
}


BOOL APIENTRY 
DllMain( 
  HMODULE hModule,
  DWORD   ul_reason_for_call,
  LPVOID  lpReserved
  ) {
	switch( ul_reason_for_call ) {
	  case DLL_PROCESS_ATTACH:
		  Mhook_SetHook( (PVOID*)&OriginalFlashWindow,         HookedFlashWindow );
      Mhook_SetHook( (PVOID*)&OriginalFlashWindowEx,       HookedFlashWindowEx );
      Mhook_SetHook( (PVOID*)&OriginalSetForegroundWindow, HookedSetForegroundWindow );
		  break;

	  case DLL_PROCESS_DETACH:
		  Mhook_Unhook( (PVOID*)&OriginalFlashWindow );
      Mhook_Unhook( (PVOID*)&OriginalFlashWindowEx );
      Mhook_Unhook( (PVOID*)&OriginalSetForegroundWindow );
		  break;
	}
	return TRUE;
}

