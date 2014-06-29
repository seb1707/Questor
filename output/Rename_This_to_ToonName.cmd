@Echo off
cls
Set InjectorEXE=Adapteve.exe
Set iniFile=Rename_This_To_ToonName.ini
::
:: we eventually need to also have things like AccountName, AccountPassword, CharacterName (the login info that is in Schedules.XML)
::

Echo Injector Details Are as Follows: 
Echo.
Echo InjectorEXE - This does the actual injection [ %InjectorEXE% ] 
Echo iniFile                                      [ %iniFile% ]
Echo.
Echo Running [ %injectorexe% %inifile% ]
%injectorexe% %inifile%
pause