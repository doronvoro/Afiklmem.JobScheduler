# 

**Afikmlem Job Scheduler**

**Overview**

The **Afikmlem Job Scheduler** is a flexible and persistent job scheduling system designed to manage and execute jobs based on user-defined schedules. It supports tasks like daily report generation and system backups, with options for persistence across system restarts and occurrence limiting.

**Features**

-   **Add Job**: Schedule new jobs with specific execution times.
-   **Delete Job**: Remove jobs by their unique ID.
-   **Retrieve Jobs**: View all registered jobs or query specific jobs by their ID.
-   **Concurrent Execution**: Run multiple jobs concurrently, ensuring system efficiency.
-   **Persistence**: All registered jobs are persisted and survive system restarts.
-   **Occurrence Limiting**: Jobs can be set to run a limited number of times.
-   **Job Types**: Supports different types of jobs (e.g., Reporting, Maintenance).
-   **Logging**: Provides detailed logs for tracking job execution and errors.

**Requirements**

-   .NET 6.0 or later
-   SQLite (for production) or InMemory database (for testing)

**Getting Started**

**Prerequisites**

Ensure that you have installed the following:

-   [.NET 6.0 SDK](https://dotnet.microsoft.com/download/dotnet/6.0)
-   SQLite (for database persistence)

**Installation**

1.  **Clone the Repository**:

 ```bash
 
git clone https://github.com/doronvoro/Afiklmem.JobScheduler.git

cd Afiklmem.JobScheduler

2.  **Install Dependencies**:  
    Navigate to the solution folder and restore the project dependencies:

 ```bash

dotnet restore

3.  **Run the Application**:  
    To start the application:

bash

Copy code

dotnet run --project Afimilk.JobScheduler.API

4.  **Run the Unit Tests**:  
    Ensure that all tests pass by running the following command:

bash

Copy code

dotnet test

**Usage**

**Registering a Job**

You can register a job to run at a specific time. For example, to schedule a maintenance backup every day at 01:00 AM:

csharp

Copy code

var jobRequest = new JobRequest

{

    DailyExecutionTime = TimeSpan.FromHours(1),

    Occurrences = 5, // Run 5 times

    Type = "MaintenanceJob"

};

await \_schedulerController.AddJob(jobRequest);

**Viewing All Jobs**

Retrieve a list of all registered jobs:

csharp

Copy code

var jobs = await \_schedulerController.GetJobs();

**Deleting a Job**

Remove a job by its ID:

csharp

Copy code

await \_schedulerController.DeleteJob(jobId);

**Design Overview**

**Job Scheduler**

The JobScheduler class manages the execution of scheduled jobs. It retrieves jobs from the database and runs them when their scheduled time is due. The system uses dependency injection (DI) to manage services and job handlers, making it modular and testable.

**Persistence**

Jobs are stored in an SQLite database, ensuring they persist across application restarts. The system supports both SQLite for production and InMemory database for testing.

**Dependency Injection**

The scheduler and job handlers are registered using Microsoft’s built-in DI container. This allows different modules to be easily injected and swapped, following SOLID principles.

**Unit Tests**

The solution includes a comprehensive set of unit tests for core functionality. These tests ensure that:

-   Jobs can be added, updated, and deleted.
-   The scheduler correctly handles job execution times.
-   Jobs are persisted across restarts.
-   Jobs run the correct number of times based on the occurrence limit.

Run the tests using:

bash

Copy code

dotnet test

**Future Enhancements**

-   **Job Retry Mechanism**: Add retry logic for failed jobs.
-   **Advanced Scheduling**: Add support for more complex schedules (e.g., weekly or monthly jobs).
-   **Improved Error Handling**: Implement more robust error handling for job execution.

**License**

This project is licensed under the MIT License - see the LICENSE file for details.
