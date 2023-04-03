# Steamfinity Cloud Contributing Guidelines
Thank you for considering contributing to Steamfinity Cloud! To ensure that the codebase remains high quality and secure, we ask that all contributors follow the guidelines below. 

> **Warning**<br>
> Before you start, please make sure that the feature, issue, or change is related to the Steamfinity Cloud and not the [Steamfinity](https://github.com/steamfinity/steamfinity) app, 
which is located in a separate repository.

# Microsoft C# Code Style and Naming Conventions
Please follow the official Microsoft C# code style and naming conventions, which can be found at:
https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions

# High-Quality Code
Write high-quality code that is well-commented, documented, and tested. When writing code, please keep in mind that others may need to read and understand it in the future. As a general rule, we ask that all contributions meet the following criteria:

- Code is readable, maintainable, and easy to understand
- Code is well-commented, including explanations of any non-obvious or complex logic
- Code is well-documented, including API references and any usage notes
- Code is tested thoroughly, and all tests pass before submission

# Security and Performance
When making changes to the Steamfinity Cloud codebase, please keep security and performance in mind. This includes:

- Avoiding the use of untrusted inputs or code
- Ensuring that all user inputs are validated and sanitized
- Avoiding any unnecessary or expensive operations
- Optimizing code for performance where possible

# ASP.NET Core and Entity Framework Core
Steamfinity Cloud is an ASP.NET Core project using Entity Framework Core. When making changes to the codebase, please use built-in features and frameworks provided by these technologies where applicable.

# Prefer Doing Things Client-Side
Please avoid using the cloud for things that can be done client-side. This helps to keep the server load to a minimum, ensuring a fast and responsive experience for all users.

# Commit Messages
Please use the title case for commit messages. This makes them easier to read and understand. Keep commits short and descriptive.
