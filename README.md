# Afikmlem Job Scheduler

## Overview

The Afikmlem Job Scheduler is a robust and flexible job scheduling system designed to manage and execute jobs based on defined schedules. The system includes functionalities for adding, updating, deleting, and retrieving jobs. It also supports concurrent execution of jobs with logging and error handling.

## Features

- **Add Job**: Create new jobs with specified execution times and types.
- **Delete Job**: Remove jobs by their ID.
- **Get Jobs**: Retrieve all jobs or a specific job by ID.
- **Get Running Jobs**: Check the list of currently running jobs.
- **Job Types**: Supports different types of jobs (e.g., Reporting, Maintenance).

## Getting Started

### Prerequisites

- .NET 6.0 or later
- SQLite (or InMemory database for testing)

### Installation

1. **Clone the Repository**:
   ```bash
   git clone https://github.com/doronvoro/Afiklmem.JobScheduler.git
