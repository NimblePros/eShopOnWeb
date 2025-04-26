---
title: "Architecture"
parent: Explore
---

The main application in the eShopOnWeb solution is a monolithic ASP.NET Core web app. It is organized according to Clean Architecture principles, such that dependencies on infrastructure concerns are minimized throughout the application. Business concepts and domain model concerns are kept in the ApplicationCore project, which other projects depend on.

## Clean Architecture References

These are resources for learning more about Clean Architecture.

### .NET Conf Appearances

- ▶️ [Clean Architecture with ASP.NET Core 9 (.NET Conf 2024)](https://www.youtube.com/watch?v=zw-ZtB1BNl8)
- ▶️ [Clean Architecture with ASP.NET Core 8 (.NET Conf 2023)](https://www.youtube.com/watch?v=yF9SwL0p0Y0)
- ▶️ [Clean Architecture with ASP.NET Core 7 (.NET Conf 2022)](https://www.youtube.com/watch?v=j6u7Pw6dyUw)
- ▶️ [Clean Architecture with ASP.NET Core 6 (.NET Conf 2021)](https://www.youtube.com/watch?v=lkmvnjypENw&ab_channel=dotNET)

### ![image](https://github.com/user-attachments/assets/b8ac5eef-61c2-46b0-8c44-18215647bc65) NimblePros Trainings

- 🎓 [Introducing Clean Architecture (NimblePros Academy)](https://academy.nimblepros.com/p/learn-clean-architecture)
- ▶️ [Clean Architecture with .NET 9 webinar](https://mailchi.mp/nimblepros/clean-architecture-dotnet-9-recording)
- ▶️ [Clean Architecture with .NET 8 webinar](https://mailchi.mp/nimblepros/clean-architecture-dotnet-8-recording)
