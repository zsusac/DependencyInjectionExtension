# .NET Core Dependency injection Extension

The main idea behind this project is to implement an extension that will allowed us to register (configure) services in yaml files.
Extension is called at startup in ConfigureServices method.
Extension searches recursively for all *services.yml* files placed somewhere in project root directory.

If we want to register new service in DI container using yaml file we need to:

* Create service interface
* Create service class that implements service interface
* Create *services.yml* file 
* Add service configuration to *services.yml* file

*services.yml* file structure should look like this:
```
services:
    <service name>:
        class: <type name qualified by its namespace>
        interface: <type name qualified by its namespace>
        lifetime: <service lifetime>

    ...
```

All services should be placed under *services* root node. Every service should be configured with following properties:

Property | Required | Default value
--- | --- | ---
service name | true | null
class | true | null
interface | false | null
lifetime | false | singleton


Here is an example of service configuration in *services.yml* file
```
services:
    app.clock:
        class: PartyInvites.Services.Clock
        interface: PartyInvites.Services.IClock
        lifetime: singleton
```

Because Clock class and IClock interface are in current assembly, we need to add type name qualified by its namespace for class and interface configs.
If we want to add another service, we need to create service class, service interface and add service configuration to existing (or new) *services.yml* file.
If we add configuration to existing *services.yml* file it will look like this:
```
services:
    app.clock:
        class: PartyInvites.Services.Clock
        interface: PartyInvites.Services.IClock
        lifetime: singleton
    app.newService:
        class: PartyInvites.Services.NewService
        interface: PartyInvites.Services.INewService
        lifetime: transient
```

To list all registred services go to http://localhost:5200/allservices
At the end of the page you can see all registered services using yaml configuration files:
```
PartyInvites.Services.IClock                            Singleton  PartyInvites.Services.Clock
PartyInvites.HelloWorldDomain.Services.IHelloWorld      Scoped     PartyInvites.HelloWorldDomain.Services.HelloWorld
PartyInvites.OnlyClassDomain.Services.OnlyClassService  Singleton	
PartyInvites.StartupDomain.Services.IUptimeService      Transient  PartyInvites.StartupDomain.Services.UptimeService
```

Extension will throw an error if we want to register two services with same name or same class type.
