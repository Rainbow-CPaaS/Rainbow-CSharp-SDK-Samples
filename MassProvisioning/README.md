![Rainbow](../logo_rainbow.png)

 
# Rainbow CSharp SDK Sample - Mass Provisioning
---

This sample demonstrates how to use the SDK in a mass provisioning process.

It permits also to understand how to login and use SDK API which are asynchornous in a synchronous way?

This scenario is used in this sample:

Using an Organization admin: 
- We create a company (for a specific school)
- We create a company admin (the responsible of teachers and students)
  
Using the Company Admin createD just before:
- We create teachers
- We create students

Using each teachers previously created:
- We create X bubbles (used as classrooms)
- In each bubbles/classrooms we add Y students 

The creation (or deletion) of teachers / students are made in parallel (25 by default - see variable: nbRbUsersCreatedOrDeletedInSameTime in file "MassProvisioningForm.cs")


## Rainbow API HUB
---

This SDK is using the [Rainbow Hub environment](https://hub.openrainbow.com/)
 
This environment is based on the [Rainbow service](https://www.openrainbow.com/) 


## Rainbow CSharp SDK
---

To have more info about the SDK:
- check [Getting started guide](https://hub.openrainbow.com/#/documentation/doc/sdk/csharp/core/lts/guides/001_getting_started)
- check [API documentation](https://hub.openrainbow.com/#/documentation/doc/sdk/csharp/core/lts/api/Rainbow.Application)
