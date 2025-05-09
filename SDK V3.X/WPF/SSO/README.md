![Rainbow](./../../../logo_rainbow.png)

# Rainbow CSharp SDK v3 - WPF - Single Sign On

This WPF application permits to understand how to use the package [Rainbow.CSharp.SDK](https://www.nuget.org/packages/Rainbow.CSharp.SDK).

Documentation is available [here](https://developers.openrainbow.com/doc/sdk/csharp/core/lts/guides/001_getting_started)

## Prerequisites

### Rainbow API HUB

This SDK is using the [Rainbow environment](https://developers.openrainbow.com/)
 
This environment is based on the [Rainbow service](https://www.openrainbow.com/) 

### Rainbow CSharp SDK

To have more info about the SDK:
- check [Getting started guide](https://developers.openrainbow.com/doc/sdk/csharp/core/lts/guides/001_getting_started)
- check [API documentation](https://developers.openrainbow.com/doc/sdk/csharp/core/lts/api/Rainbow.Application)

## Features

This sample show how to use **SSO** using SDK C# in **WPF**.

You must firsts ensure that your **SSO configuration** has been set correctly.

Then you need to define an **Authentication Redirection URL** for your Rainbow Application.

Finally you have to configure two files: **credentials.json** and **exeSettings.json**

## SSO Configuration

Before to use this sample, you must ensure to have a correct SSO configuration. 

A full documentation to set this configuration can be found here: https://support.openrainbow.com/hc/en-us/articles/360019304499-How-to-configure-SSO-in-Rainbow-admin-view-

Then to check if the configuration is correct, you can use the standard web client and try to connect to Rainbow with any user with a SSO configuration set.

## Define an Authentication Redirection URL

You need now to specify an **Authentication Redirection URL** for your Rainbow Application using the Rainbow Hub Web site: https://developers.openrainbow.com/applications

Use the "Edit" button for your application.

![RbApplicationEdit](../../../images/RbApplicationEdit.png)

Now go to the section **SINGLE SIGN ON CONFIGURATION** and specify a valid URI (for example **"myappscheme://callback/"**). 

It will be used in the SSO Sample application. Don't forget to save using the "Update" button.

![RbApplicationSSOConfig](../../../images/RbApplicationSSOConfig.png)

## File credentials.json

You need to set correctly the file "credentials.json" like described in chapter [File credentials.json](./../../ConfigurationFiles.md#credentials.json).

## File exeSettings.json

You need to set correctly the file "exeSettings.json" like described in chapter [File exeSettings.json](./../../ConfigurationFiles.md#exeSettings.json) - no need here to use external dependencies.

