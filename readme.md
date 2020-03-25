# Stay Focused

Stay Focused prevents the annoying "focus stealing" that's been plaguing Windows since Windows 2000. Focus stealing occurs when an app pops up unexpectedly, like when you're in the middle of writing a sentence in another window.

This utility prevents focus stealing by injecting a DLL into offending apps. The DLL "hooks" the process' system calls to SetForegroundWindow to an empty function.

## Usage

Extract the app somewhere and run it, it will automatically hook all the running apps (except Explorer and Chrome) and watch for newly created ones. Closing the window minimizes the app to tray. To exit and unload it, click the unload button.

Don't run this with administrator privileges, system processes are not very safe to manipulate this way.

Get the binary here https://blade.sk/stay-focused/

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
