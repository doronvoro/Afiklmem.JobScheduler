Afimilk Job Scheduler API
This project is a Job Scheduling API designed for handling and executing scheduled jobs with various job types (e.g., Reporting, Maintenance). The system is built on top of .NET and Entity Framework, using dependency injection, in-memory database support for testing, and asynchronous job execution.

Table of Contents
Features
Technologies
Getting Started
Usage
Endpoints
Testing
Contributing
License
Features
Job Management: Supports adding, deleting, and retrieving jobs.
Daily Job Execution: Supports daily task scheduling, with persistence.
Custom Job Types: Easily extendable to support custom job types.
Dependency Injection: Uses Microsoft's DI for job handler creation.
In-Memory Testing: Supports in-memory databases for testing.
Concurrent Job Execution: Executes multiple jobs concurrently with logging support.
Technologies
.NET 6
Entity Framework Core
SQLite/In-Memory Databases
ASP.NET Core Web API
MoQ for Unit Testing
Swagger for API Documentation
Getting Started
Prerequisites
.NET 6 SDK installed on your machine
SQLite for persistence
Installation
Clone the repository:

bash
Copy code
git clone https://github.com/your-repo/Afimilk.JobScheduler
cd Afimilk.JobScheduler
Install dependencies:

bash
Copy code
dotnet restore
Run migrations (if using SQLite):

bash
Copy code
dotnet ef migrations add InitialCreate
dotnet ef database update
Run the application:

bash
Copy code
dotnet run
View Swagger documentation: Open your browser at http://localhost:5000/swagger

Usage
This project provides a REST API for managing jobs and executing them.

Adding a Job
To add a job, send a POST request to the /api/scheduler/add-job endpoint with the job details in the body. For example:

json
Copy code
{
  "dailyExecutionTime": "14:30:00",
  "occurrences": 5,
  "type": "ReportingJob"
}
Executing Jobs
Jobs are automatically executed based on the specified DailyExecutionTime. You can check the currently running jobs by calling the /api/scheduler/running-jobs endpoint.

Job Types
Two default job types are provided: ReportingJob and MaintenanceJob. You can easily add new job types by creating new classes implementing the IJobHandler interface and registering them in the DI container.

Endpoints
POST /api/scheduler/add-job: Add a new job.
DELETE /api/scheduler/delete-job/{id}: Delete a job by ID.
GET /api/scheduler/get-all-jobs: Retrieve all jobs.
GET /api/scheduler/get-job/{id}: Retrieve a job by ID.
GET /api/scheduler/running-jobs: Retrieve currently running jobs.
GET /api/scheduler/get-all-job-types: Retrieve available job types.
Testing
This project includes unit tests for the API controllers and services.

Run the tests:

bash
Copy code
dotnet test
In-memory testing: The project uses an in-memory database for unit tests. The JobRepository class has been adapted to work with both SQLite and in-memory databases.

Contributing
Feel free to contribute to this project by creating pull requests or raising issues. Please ensure your code follows the established coding style, and all tests pass before submitting.

License
This project is licensed under the MIT License.