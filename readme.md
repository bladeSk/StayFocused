# Stay Focused

[💾 Download](https://github.com/bladeSk/StayFocused/releases)

Stay Focused prevents the annoying "focus stealing" that's been plaguing Windows since Windows 2000. Focus stealing occurs when an app pops up unexpectedly, like when you're in the middle of writing a sentence in another window.

This utility prevents focus stealing by injecting a DLL into offending apps. The DLL "hooks" the process' system calls to SetForegroundWindow to an empty function.

![Stay Focused Screenshot](https://user-images.githubusercontent.com/4263742/99443147-b3f50580-291a-11eb-80dc-fbe448a1895a.png)

## Usage

Extract the app somewhere and run it, it will automatically hook whitelisted apps and watch for newly created ones. Closing the window minimizes the app to tray. To exit and unload it, click the unload button.

Configure which apps you want to hook by editing the .ini file, exit and re-run Stay Focused to apply changes. You may wanto to turn on the blacklist mode to hook everything excluding system apps.

When running in the blacklist mode, you shouldn't run this with administrator privileges, system processes are not very safe to hook.

Get the binary here https://github.com/bladeSk/StayFocused/releases

## Building

The building process is a bit involved at the moment, this should be streamlined in the future.

* Open `PreventFocusStealing\PreventFocusStealing.vcxproj` and build both 32 and 64-bit versions. These are the DLLs that get injected into apps and they're responsible for preventing focus stealing.

* Open `StayFocused.sln`, set the configuration to `x86` and build the `32BitHelper` project. This is a helper utility that's used to get handles of 32-bit processes from a 64-bit process.

* Set the configuration to `x64` and build the `StayFocused` project. This is the main part of the app.

* Go to `StayFocused\bin\x64\Release`, create a folder named `helpers` and copy the files generated in the first two steps there. The structure should look like this:  
`StayFocused\bin\x64\Release\helpers\PreventFocusStealing32.dll`  
`StayFocused\bin\x64\Release\helpers\PreventFocusStealing64.dll`  
`StayFocused\bin\x64\Release\helpers\32BitHelper.exe`

* That's it, you should be able to run the main project now.
