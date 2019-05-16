[![Build status](https://ci.appveyor.com/api/projects/status/sfrglj9cgb54vfqh?svg=true)](https://ci.appveyor.com/project/EMG/lambda-local-runner) [![EMG Lambda LocalRunner](https://img.shields.io/nuget/v/EMG.Lambda.LocalRunner.svg)](https://www.nuget.org/packages/EMG.Lambda.LocalRunner)

# AWS Lambda Local Runner
This library helps you create a console application that hosts your AWS Lambda function so that you can easily debug its execution. This is important especially when working on a distributed system composed by several services and applications.

**Note**: this works only with AWS Lambda functions based on the `dotnetcore2.0` runtime.

## How it works?
The library creates a small ASP.NET Core application that listens to a port and forwards every call to an web handler whose responsibility is to deserialize the incoming request's body to a well known type, execute the function handler, and deserialize the response, if any.

## How to use it?
The best way to run your Lambda functions locally is to create a new console application in your solution and add a reference to the project containing the function.

Once you have added the [`EMG.Lambda.LocalRunner`](https://www.nuget.org/packages/EMG.Lambda.LocalRunner/) package to your application, you can simply replace your `Main` function with something similar to the following snippet.

```csharp
LambdaRunner.Create()
            .Receives<string>()
            .Returns<string>()
            .UsesFunction<Function>((function, input, context) => function.FunctionHandler(input, context))
            .Build()
            .RunAsync()
            .Wait();
```

If your function returns a `Task<T>`, you can use the `UsesAsyncFunction` method.

```csharp
LambdaRunner.Create()
            .Receives<string>()
            .Returns<string>()
            .UsesAsyncFunction<Function>((function, input, context) => function.FunctionHandlerAsync(input, context))
            .Build()
            .RunAsync()
            .Wait();
```

If your function has no return type, you skip the `Returns<T>()` and use `UsesFunctionWithNoResult` or `UsesAsyncFunctionWithNoResult`

```csharp
LambdaRunner.Create()
            .Receives<string>()
            .UsesFunctionWithNoResult<Function>((function, input, context) => function.FunctionHandler(input, context))
            .Build()
            .RunAsync()
            .Wait();

LambdaRunner.Create()
            .Receives<string>()
            .UsesAsyncFunctionWithNoResult<Function>((function, input, context) => function.FunctionHandlerAsync(input, context))
            .Build()
            .RunAsync()
            .Wait();
```

[Here](https://github.com/emgdev/lambda-local-runner/wiki/Tutorial) there is a tutorial that helps you creating your first local runner application.

## Optional parameters
When constructing your runner, you can customize it by providing two optional parameters: the port to bind and the serializer to use.

### Customizing the port to bind
If you plan to locally run more than one Lambda function, you need to use bind different ports.
This can be easily done by using the `UsePort(int port)` method. If omitted, the default port 5000 will be used.

```csharp
LambdaRunner.Create().UsePort(5001) ...
```

### Customizing the response `Content-Type` header
You can use the `WithResponseContentType(string contentType)` method to set a value that will be returned
in the `Content-Type` header. If omitted the default `application/json` will be used.

```csharp
LambdaRunner.Create()
            .Receives<string>()
            .Returns<Foo>()
            .WithResponseContentType("application/xml")
            .UsesFunction<Function>((function, input, context) => function.FunctionHandler(input, context))
            .Build()
```

### Customizing the serializer
If you need to use a different serialization strategy, you can specify the serializer while building the runner by using the `UseSerializer<T>(Func<T> factory)`.
For consistency with the AWS Lambda SDK for .NET, the custom serializer you use must implement the `ILambdaSerializer` interface.
If omitted, the default `Amazon.Lambda.Serialization.Json.JsonSerializer` will be used.

```csharp
LambdaRunner.Create().UseSerializer(() => new MyCustomSerializer()) ...
```

## Notes on C# 7.1
C# 7.1 introduced the support to [async/await directly in the Main method of a console application](https://docs.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-7-1#async-main).

Once your console application has an Async Main method, you can rewrite your console application so that it looks like the following snippet
```csharp
static async Task Main(string[] args)
{
    await LambdaRunner.Create()
                        .UsePort(5001)
                        .Receives<string>()
                        .UsesAsyncFunctionWithNoResult<Function>((function, input, context) => function.FunctionHandlerAsync(input, context))
                        .Build()
                        .RunAsync();
}
```

By default, features introduced in minor versions of the compilers are turned off. To enable these features, you can simply modify your project file.

Adding the following setting in your project file will enable the latest available feature.
```xml
<LangVersion>latest</LangVersion>
```
Please, make sure to have the latest version of the C# compiler installed on your machine or any build server you might be using.

## How to build

This project uses [Cake](https://cakebuild.net/) as a build engine.

If you would like to build Nybus locally, just execute the `build.cake` script.

You can do it by using the .NET tool created by CAKE authors and use it to execute the build script.
```powershell
dotnet tool install -g Cake.Tool
dotnet cake
```