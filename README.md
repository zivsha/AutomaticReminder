# AutomaticReminder
A C# Windows Service that allows the automatic sending of emails and text message reminders to a given set of contacts according to a given set of events.

This service was originally created as a Happy-Hour reminder for my team at work, and some parts of it remained specific for that cause (but most of it is configurable for general purpose reminders).

The application is configurable using a single configuration file (explained later).

Emails are sent using SMTP server with the use of [`System.Net.Mail`](https://docs.microsoft.com/en-us/dotnet/api/system.net.mail?view=netframework-4.7.1).

Text messages are sent using a given SMS gateway. The current implementation of the [`SMSHelper.SendSMS`](./AutomaticReminderCommon/SMSHelper.cs) function is specific to the gateway I was using so changes there might be required.

### Reminders

Currently the service will check when the next event is scheduled and will sent the following reminders:

* An email reminder, 1 Week prior to event
* An email reminder, 2 days prior to event
* An email reminder, 1 day prior to event
* An SMS reminder, 1 day prior to event (If a phone number is provided to the contact that should be reminded)
* An email reminder, on the day of the event (Currently will only work if the event is after ~8:00am)

## AutomaticReminderService

A C# service application that compiles to a .NET executable and should be registered as a Windows service.
This project depends on the `AutomaticReminderCommon` project. 

### To register as service:

1. Build the executable (and the assembly for `AutomaticReminderCommon`)
2. Go to output directory
3. Install the service with `InstallUtil AutomaticReminderService.exe` [(Microsoft guide)](https://docs.microsoft.com/en-us/dotnet/framework/windows-services/how-to-install-and-uninstall-services)
4. Start the service

### To unregister the service:

1. Stop the service
2. Uninstall the service with `InstallUtil /u AutomaticReminderService.exe` [(Microsoft guide)](https://docs.microsoft.com/en-us/dotnet/framework/windows-services/how-to-install-and-uninstall-services)


## AutomaticReminderCommon

A collection of common utilities used by AutomaticReminderService and AutomaticReminderController.
Includes utilities such as:

* Database access
* Events
* eMail
* Notifications
* Date and Time
* Logger

In addition the file contains the `UserConfiguration.cs` file which reads the user's configuration file that you should add  (read about the [User Configuration](<TODO>)).

## AutomaticReminderController

A C#-WPF tray-icon application that allows you to test the the reminder service, and control the service.

*You Should run this application as administrator*


## UserConfigurations.txt

The user configuration file holds most of the application's configurable variables to allow for this service to be a general purpose reminder service.
An example file exists next to this README and it contains explanations inside it.

The easiest way to allow for proper configuration file to work is edit the following line in `UserConfiguration.cs`:
```csharp
private const string filePath = @"UserConfigurations.txt";
```

And provide an absolute path to the configuration file, for example:
```csharp
private const string filePath = @"C:\Users\user\source\repos\AutomaticReminder\UserConfigurations.txt";
```

## AutomaticReminderDatabase.txt

This file holds the database for contacts and events for the reminder.
Open the files to learn more.