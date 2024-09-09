using Afimilk.JobScheduler.BL;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class SchedulerController : ControllerBase
{
    private readonly JobScheduler _jobScheduler;
    private readonly IJobRepository _jobRepository;
    private readonly IJobHandlerFactory _jobHandlerFactory;

    public SchedulerController(JobScheduler jobScheduler,
                               IJobRepository jobRepository,
                               IJobHandlerFactory jobHandlerFactory)
    {
        _jobScheduler = jobScheduler;
        _jobRepository = jobRepository;
        _jobHandlerFactory = jobHandlerFactory;
    }

    // New: Add Job
    [HttpPost("add-job")]
    public async Task<IActionResult> AddJob([FromBody] JobRequest jobRequest)
    {
        // Check if model is valid based on data annotations (like Occurrences validation)
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (!_jobHandlerFactory.GetJobTypeNames().Contains(jobRequest.Type))
        {
            return BadRequest($"Job type {jobRequest.Type} does not exist.");
        }

        var job = new Job
        {
            DailyExecutionTime = jobRequest.DailyExecutionTime,
            Occurrences = jobRequest.Occurrences,
            RemainingOccurrences = jobRequest.Occurrences,
            Type = jobRequest.Type,
        };

        await _jobRepository.AddJobAsync(job);
        
        return Ok("Job added successfully");
    }

    // New: Delete Job
    [HttpDelete("delete-job/{id}")]
    public async Task<IActionResult> DeleteJob(int id)
    {
        var job = await _jobRepository.GetJobByIdAsync(id);
        if (job == null)
        {
            return NotFound("Job not found");
        }
        await _jobRepository.DeleteJobAsync(id);
        return Ok("Job deleted successfully");
    }

    // New: Get all jobs
    [HttpGet("get-all-jobs")]
    public async Task<IActionResult> GetAllJobs()
    {
        var jobs = await _jobRepository.GetAllJobsAsync();
        return Ok(jobs);
    }

    // New: Get one job
    [HttpGet("get-job/{id}")]
    public async Task<IActionResult> GetJobById(int id)
    {
        var job = await _jobRepository.GetJobByIdAsync(id);
        if (job == null)
        {
            return NotFound("Job not found");
        }
        return Ok(job);
    }

    // Existing: Get currently running jobs
    [HttpGet("running-jobs")]
    public IActionResult GetRunningJobs()
    {
        var runningJobs = _jobScheduler.GetCurrentlyRunningJobs();
        return Ok(runningJobs);
    }

    [HttpGet("get-all-job-types")]
    public IActionResult GetJobTypes()
    {
        var jobTypeNames = _jobHandlerFactory.GetJobTypeNames();
        return Ok(jobTypeNames);
    }
}