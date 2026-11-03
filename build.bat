@echo off
rem Builds MashaPoster.exe using the C# compiler built into Windows (no installs needed)
set CSC=%WINDIR%\Microsoft.NET\Framework64\v4.0.30319\csc.exe
if not exist "%CSC%" set CSC=%WINDIR%\Microsoft.NET\Framework\v4.0.30319\csc.exe

"%CSC%" /nologo /target:winexe /out:MashaPoster.exe /r:System.Windows.Forms.dll /r:System.Drawing.dll /resource:poster.jpg,poster.jpg Program.cs

if exist MashaPoster.exe (echo Done: MashaPoster.exe) else (echo Build failed)
pause
